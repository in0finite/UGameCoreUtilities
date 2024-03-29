using System;
using System.Collections.Generic;
using UnityEngine;

namespace UGameCore.Utilities
{
    /// <summary>
    /// Implement this interface to receive some Editor Application events.
    /// You are required to implement this interface if you are receiving messages that are not available as integrated
    /// Unity messages (Awake, Start, etc).
    /// Note that you also have to register the object, using <see cref="EditorApplicationEvents.Register"/>.
    /// </summary>
    public interface IEditorApplicationEventsReceiver
    {
        void OnExitPlayMode() { }
    }

    /// <summary>
    /// Dispathes some common Unity messages (Awake, Start, etc) and Editor Application messages to 
    /// registered objects, during edit-mode.
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public static class EditorApplicationEvents
    {
        private class ObjectData
        {
            public MonoBehaviour monoBehaviour;
            public bool awakeCalled;
            public bool hadFirstUpdate;
            public System.Reflection.MethodInfo awakeMethod;
            public System.Reflection.MethodInfo startMethod;
            public System.Reflection.MethodInfo updateMethod;
            public System.Reflection.MethodInfo lateUpdateMethod;
        }

        private readonly static List<ObjectData> s_subscribers = new List<ObjectData>();
        private readonly static List<ObjectData> s_subscribersUpdateBuffer = new List<ObjectData>();



#if UNITY_EDITOR
        static EditorApplicationEvents()
        {
            UnityEditor.EditorApplication.update -= EditorUpdate;
            UnityEditor.EditorApplication.update += EditorUpdate;

            UnityEditor.EditorApplication.playModeStateChanged -= PlayModeStateChanged;
            UnityEditor.EditorApplication.playModeStateChanged += PlayModeStateChanged;
        }
#endif

#if UNITY_EDITOR
        static void PlayModeStateChanged(UnityEditor.PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange != UnityEditor.PlayModeStateChange.EnteredEditMode)
                return;

            DispatchOnExitPlayMode();
        }
#endif

        private static void EditorUpdate()
        {
            s_subscribers.RemoveAll(o => null == o.monoBehaviour);
            s_subscribers.TrimExcessSmart();

            if (Application.isPlaying)
            {
                UpdateInPlayMode();
                return;
            }

            s_subscribersUpdateBuffer.Clear();
            s_subscribersUpdateBuffer.AddRange(s_subscribers);
            s_subscribersUpdateBuffer.TrimExcessSmart();
            s_subscribersUpdateBuffer.ForEachIndexed(DispatchAwakeToSingleObject);
            s_subscribersUpdateBuffer.ForEachIndexed(DispatchUpdateToSingleObject);
            s_subscribersUpdateBuffer.ForEachIndexed((objectData, i) => DispatchMethodToSingleObject(objectData, objectData.lateUpdateMethod));
            s_subscribersUpdateBuffer.Clear();
        }

        private static void DispatchAwakeToSingleObject(ObjectData objectData, int index)
        {
            if (objectData.awakeCalled)
                return;

            objectData.awakeCalled = true;

            if (objectData.awakeMethod != null)
            {
                bool awakeOk = F.RunExceptionSafe(() => objectData.awakeMethod.Invoke(objectData.monoBehaviour, Array.Empty<object>()));
                // mimic behavior from Unity : if Awake() throws exception, disable the script
                if (!awakeOk)
                    F.RunExceptionSafe(() => objectData.monoBehaviour.enabled = false);
            }
        }

        private static void DispatchUpdateToSingleObject(ObjectData objectData, int index)
        {
            if (null == objectData.monoBehaviour) // he could've been destroyed in Awake()
                return;

            if (!objectData.monoBehaviour.isActiveAndEnabled)
                return;

            if (!objectData.hadFirstUpdate)
            {
                objectData.hadFirstUpdate = true;
                
                if (objectData.startMethod != null)
                    F.RunExceptionSafe(() => objectData.startMethod.Invoke(objectData.monoBehaviour, Array.Empty<object>()));

                if (null == objectData.monoBehaviour) // he could've been destroyed in Start()
                    return;
            }
            
            if (objectData.updateMethod != null)
                F.RunExceptionSafe(() => objectData.updateMethod.Invoke(objectData.monoBehaviour, Array.Empty<object>()));
        }

        private static void DispatchMethodToSingleObject(
            ObjectData objectData, System.Reflection.MethodInfo methodInfo)
        {
            if (null == objectData.monoBehaviour) // he could've been destroyed in previous step
                return;

            if (!objectData.monoBehaviour.isActiveAndEnabled)
                return;

            if (methodInfo != null)
                F.RunExceptionSafe(() => methodInfo.Invoke(objectData.monoBehaviour, Array.Empty<object>()));
        }

        private static void DispatchOnExitPlayMode()
        {
            ObjectData[] copyArray = s_subscribers.ToArray(); // prevent concurrent modification
            foreach (ObjectData obj in copyArray)
            {
                if (obj.monoBehaviour != null && obj.monoBehaviour is IEditorApplicationEventsReceiver receiver)
                    F.RunExceptionSafe(receiver.OnExitPlayMode, obj.monoBehaviour);
            }
        }

        private static void UpdateInPlayMode()
        {
            // Make sure we don't call Awake() or Start() twice - this can happen when
            // exiting play-mode without assembly reload.
            // So, we assume that Unity called these methods in play-mode.
            for (int i = 0; i < s_subscribers.Count; i++)
            {
                var subscriber = s_subscribers[i];
                // Awake() should always be called by Unity
                bool awakeCalled = true;
                // we assume that if object is enabled, his Start() was called
                bool startCalled = subscriber.hadFirstUpdate || subscriber.monoBehaviour.enabled;
                if (subscriber.awakeCalled != awakeCalled || subscriber.hadFirstUpdate != startCalled)
                {
                    subscriber.awakeCalled = awakeCalled;
                    subscriber.hadFirstUpdate = startCalled;
                }
            }
        }

        public static void Register(MonoBehaviour subscriber)
        {
#if UNITY_EDITOR

            // Note: this function must not call any of Unity APIs, the reasons are listed below.

            // Do not use UnityEngine.Object's '==' operator here. When editing a prefab and saving it, Unity will try
            // to reset the object first, calling it's contructor, during which the MonoBehaviour '==' operator will
            // return null. This will cause some errors in the console, and sometimes will even crash the engine.
            if (object.ReferenceEquals(subscriber, null))
                throw new ArgumentNullException();
            
            // Here we have to use object.ReferenceEquals(), because something strange is happening.
            // When exiting playmode, Unity kills all objects and creates new objects that are part of the scene.
            // When this function is called from constructor of created object, his reference will point to
            // something different, but his MonoBehaviour will be equal to one of destroyed object's MonoBehaviour.

            if (s_subscribers.Exists(o => object.ReferenceEquals(o.monoBehaviour, subscriber)))
                throw new ArgumentException("Already registered");

            ObjectData objectData = new ObjectData { monoBehaviour = subscriber };

            Type type = subscriber.GetType();

            System.Reflection.BindingFlags bindingFlags =
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

            objectData.awakeMethod = type.GetMethod("Awake", bindingFlags);
            objectData.startMethod = type.GetMethod("Start", bindingFlags);
            objectData.updateMethod = type.GetMethod("Update", bindingFlags);
            objectData.lateUpdateMethod = type.GetMethod("LateUpdate", bindingFlags);

            s_subscribers.Add(objectData);
#endif
        }
    }
}
