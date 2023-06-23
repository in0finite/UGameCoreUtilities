using System;
using System.Collections.Generic;
using UnityEngine;

namespace UGameCore.Utilities
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public static class EditorApplicationEvents
    {
        private struct ObjectData
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
        }
#endif

        private static void EditorUpdate()
        {
            if (s_subscribers.Count == 0)
                return;

            s_subscribers.RemoveAll(o => null == o.monoBehaviour);
            s_subscribers.TrimExcessSmart();

            if (s_subscribers.Count == 0)
                return;

            if (Application.isPlaying)
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
                        s_subscribers[i] = subscriber;
                    }
                }

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
            s_subscribers[index] = objectData;
            s_subscribersUpdateBuffer[index] = objectData;

            if (objectData.awakeMethod != null)
                F.RunExceptionSafe(() => objectData.awakeMethod.Invoke(objectData.monoBehaviour, Array.Empty<object>()));
            
        }

        private static void DispatchUpdateToSingleObject(ObjectData objectData, int index)
        {
            if (!objectData.monoBehaviour.isActiveAndEnabled)
                return;

            if (!objectData.hadFirstUpdate)
            {
                objectData.hadFirstUpdate = true;
                s_subscribers[index] = objectData;
                s_subscribersUpdateBuffer[index] = objectData;

                if (objectData.startMethod != null)
                    F.RunExceptionSafe(() => objectData.startMethod.Invoke(objectData.monoBehaviour, Array.Empty<object>()));
            }
            
            if (objectData.updateMethod != null)
                F.RunExceptionSafe(() => objectData.updateMethod.Invoke(objectData.monoBehaviour, Array.Empty<object>()));
        }

        private static void DispatchMethodToSingleObject(
            ObjectData objectData, System.Reflection.MethodInfo methodInfo)
        {
            if (!objectData.monoBehaviour.isActiveAndEnabled)
                return;

            if (methodInfo != null)
                F.RunExceptionSafe(() => methodInfo.Invoke(objectData.monoBehaviour, Array.Empty<object>()));
        }

        public static bool Register(MonoBehaviour subscriber)
        {
#if UNITY_EDITOR

            if (s_subscribers.Exists(o => o.monoBehaviour == subscriber))
                return false;

            ObjectData objectData = new ObjectData { monoBehaviour = subscriber };

            Type type = subscriber.GetType();

            System.Reflection.BindingFlags bindingFlags =
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

            objectData.awakeMethod = type.GetMethod("Awake", bindingFlags);
            objectData.startMethod = type.GetMethod("Start", bindingFlags);
            objectData.updateMethod = type.GetMethod("Update", bindingFlags);
            objectData.lateUpdateMethod = type.GetMethod("LateUpdate", bindingFlags);

            s_subscribers.Add(objectData);

            return true;
#else
            return false;
#endif
        }
    }
}
