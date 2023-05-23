using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UGameCore.Utilities
{
    public static class ComponentExtensions
    {

        public static T GetComponentWithName<T>(this Component root, string name) where T : Component
        {
            return root.GetComponentsInChildren<T>().FirstOrDefault(x => x.name == name);
        }

        public static T GetComponentOrThrow<T>(this Component comp)
        {
            return comp.gameObject.GetComponentOrThrow<T>();
        }

        public static T GetComponentOrLogError<T>(this Component comp)
        {
            return comp.gameObject.GetComponentOrLogError<T>();
        }

        public static Transform GetTransformOrNull(this Component component)
        {
            return (component != null) ? component.transform : null;
        }
    }
}
