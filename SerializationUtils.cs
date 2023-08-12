using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UGameCore.Utilities
{
    public static class SerializationUtils
    {
        public static IEnumerable<FieldInfo> GetSerializableFieldsBasedOnVisibilityOnly(Type type)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (FieldInfo field in fields)
            {
                if (field.IsInitOnly)
                    continue;

                if (!field.IsPublic && !field.IsDefined(typeof(SerializeField), true))
                    continue;
                
                if (field.IsDefined(typeof(NonSerializedAttribute), true))
                    continue;

                yield return field;
            }
        }

        /// <summary>
        /// Checks that all serializable <see cref="UnityEngine.Object"/> properties of this object are assigned.
        /// If not, an exception is thrown with details.
        /// </summary>
        public static void EnsureSerializableReferencesAssigned(this UnityEngine.Object obj)
        {
            Type objectType = typeof(UnityEngine.Object);

            var fields = GetSerializableFieldsBasedOnVisibilityOnly(obj.GetType())
                .Where(f => objectType.IsAssignableFrom(f.FieldType))
                .Where(f => ((UnityEngine.Object)f.GetValue(obj)) == null)
                .ToArray();

            if (fields.Length == 0)
                return;

            throw new UnassignedReferenceException($"Following references are not assigned on object '{obj.name}' ({obj.GetType().Name}): {string.Join(", ", fields.Select(f => f.Name))}");
        }
    }
}
