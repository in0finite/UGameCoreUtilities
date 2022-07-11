using System.Collections.Generic;
using UnityEngine;

namespace UGameCore.Utilities
{
    public interface IEditorApplicationEventsReceiver
    {
        void OnFirstEditorUpdate();

        void OnEditorUpdate();
    }

#if UNITY_EDITOR
    public static class EditorApplicationEvents
    {
        private struct ObjectData
        {
            public MonoBehaviour monoBehaviour;
            public bool hadFirstUpdate;
        }

        private readonly static List<ObjectData> s_subscribers = new List<ObjectData>();
        private readonly static List<ObjectData> s_subscribersUpdateBuffer = new List<ObjectData>();



        static EditorApplicationEvents()
        {
            UnityEditor.EditorApplication.update -= EditorUpdate;
            UnityEditor.EditorApplication.update += EditorUpdate;
        }

        private static void EditorUpdate()
        {
            if (s_subscribers.Count == 0)
                return;

            s_subscribers.RemoveAll(o => null == o.monoBehaviour);

            if (s_subscribers.Count == 0)
                return;

            s_subscribersUpdateBuffer.AddRange(s_subscribers);
            s_subscribersUpdateBuffer.ForEach(DispatchToSingeObject);
            s_subscribersUpdateBuffer.Clear();
        }

        private static void DispatchToSingeObject(ObjectData objectData, int index)
        {
            var eventsReceiver = (IEditorApplicationEventsReceiver)objectData.monoBehaviour;
            if (objectData.hadFirstUpdate)
                F.RunExceptionSafe(() => eventsReceiver.OnEditorUpdate());
            else
            {
                F.RunExceptionSafe(() => eventsReceiver.OnFirstEditorUpdate());
                objectData.hadFirstUpdate = true;
                s_subscribers[index] = objectData;
            }
        }

        public static void Register<T>(T subscriber)
            where T : MonoBehaviour, IEditorApplicationEventsReceiver
        {
            s_subscribers.Add(new ObjectData { monoBehaviour = subscriber });
        }

        public static void RegisterIfNotExists<T>(T subscriber)
            where T : MonoBehaviour, IEditorApplicationEventsReceiver
        {
            if (s_subscribers.Exists(o => o.monoBehaviour == subscriber))
                return;

            Register(subscriber);
        }
    }
#endif
}
