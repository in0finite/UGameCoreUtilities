using UnityEngine;

namespace UGameCore.Utilities
{
    public class UGUISkinApplier : MonoBehaviour
    {
        public UGUISkin skin;


        [ContextMenu("Apply skin")]
        void ApplyContextMenu()
        {
            if (null == this.skin)
                return;

            this.skin.Apply(this.gameObject, true);
            EditorUtilityEx.MarkObjectAsDirty(this.gameObject);
        }
    }
}
