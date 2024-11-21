using System;

namespace UGameCore.Utilities
{
    public class DeadUnityObjectException : Exception
    {
        public static void ThrowIfDead(UnityEngine.Object obj)
        {
            if (object.ReferenceEquals(obj, null))
                throw new DeadUnityObjectException("Specified Unity Object is dead (null)");

            if (null == obj)
                throw new DeadUnityObjectException($"Specified Unity Object is dead ({obj.GetType().Name})");
        }

        public DeadUnityObjectException()
            : base("Specified Unity Object is dead")
        {
        }

        public DeadUnityObjectException(string message)
            : base(message)
        {
        }
    }
}
