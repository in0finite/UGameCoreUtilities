using System;
using System.Collections.Generic;
using UnityEngine;

namespace UGameCore.Utilities
{
    public static class EditorApplicationEvents
    {
        private struct ObjectData
        {
            public MonoBehaviour monoBehaviour;
            public bool hadFirstUpdate;
            public System.Reflection.MethodInfo startMethod;
            public System.Reflection.MethodInfo updateMethod;
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
                return;

            s_subscribersUpdateBuffer.AddRange(s_subscribers);
            s_subscribersUpdateBuffer.TrimExcessSmart();
            s_subscribersUpdateBuffer.ForEach(DispatchUpdateToSingleObject);
            s_subscribersUpdateBuffer.Clear();
        }

        private static void DispatchUpdateToSingleObject(ObjectData objectData, int index)
        {
            if (!objectData.monoBehaviour.isActiveAndEnabled)
                return;

            if (!objectData.hadFirstUpdate)
            {
                objectData.hadFirstUpdate = true;
                s_subscribers[index] = objectData;

                if (objectData.startMethod != null)
                    F.RunExceptionSafe(() => objectData.startMethod.Invoke(objectData.monoBehaviour, Array.Empty<object>()));
            }
            
            if (objectData.updateMethod != null)
                F.RunExceptionSafe(() => objectData.updateMethod.Invoke(objectData.monoBehaviour, Array.Empty<object>()));
        }

        public static void Register(MonoBehaviour subscriber)
        {
#if UNITY_EDITOR

            ObjectData objectData = new ObjectData { monoBehaviour = subscriber };

            Type type = subscriber.GetType();

            System.Reflection.BindingFlags bindingFlags =
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

            objectData.startMethod = type.GetMethod("Start", bindingFlags);
            objectData.updateMethod = type.GetMethod("Update", bindingFlags);

            s_subscribers.Add(objectData);
#endif
        }

        public static bool RegisterIfNotExists(MonoBehaviour subscriber)
        {
            if (s_subscribers.Exists(o => o.monoBehaviour == subscriber))
                return false;

            Register(subscriber);
            return true;
        }
    }
}
