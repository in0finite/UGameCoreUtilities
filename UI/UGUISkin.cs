using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGameCore.Utilities
{
    [CreateAssetMenu(fileName = nameof(UGUISkin), menuName = F.kEditorMenuName + "/" + nameof(UGUISkin))]
    public class UGUISkin : ScriptableObject
    {
        [Serializable]
        public class ElementStyle
        {
            public Color backgroundColor = Color.white;
            public Color textColor = Color.black;
            public int fontSize = 14;
            public Font font;
            public bool useSprite;
            public Sprite sprite;
        }

        public ElementStyle button, text, image, inputField, scrollRect, scrollBar;

        [Space(15)]
        [Tooltip("Drag a GameObject here to apply skin to it")]
        [SerializeField]
        GameObject m_appliedGameObject;


        public void Apply(GameObject go, bool includeChildren)
        {
            UIBehaviour[] uIBehaviours = includeChildren
                ? go.GetComponentsInChildren<UIBehaviour>()
                : go.GetComponents<UIBehaviour>();

            Apply(uIBehaviours);
        }

        public void Apply(UIBehaviour[] uiBehaviours)
        {
            // first apply to Text and Image components, because they can be overriden later

            foreach (UIBehaviour uiBehaviour in uiBehaviours)
            {
                if (uiBehaviour is not Text or Image)
                    continue;

                Apply(uiBehaviour);
            }

            foreach (UIBehaviour uiBehaviour in uiBehaviours)
            {
                if (uiBehaviour is Text or Image)
                    continue;

                Apply(uiBehaviour);
            }
        }

        public void Apply(UIBehaviour uiBehaviour)
        {
            if (uiBehaviour is Button button)
            {
                if (button.targetGraphic != null)
                    ApplyToGraphicComponent(button.targetGraphic, this.button);
                
                var text = button.GetComponentInChildren<Text>();
                if (text != null)
                    ApplyToTextComponent(text, this.button);
            }
            else if (uiBehaviour is Text text)
            {
                ApplyToTextComponent(text, this.text);
            }
            else if (uiBehaviour is Image image)
            {
                ApplyToGraphicComponent(image, this.image);
            }
            else if (uiBehaviour is InputField inputField)
            {
                if (inputField.targetGraphic != null)
                    ApplyToGraphicComponent(inputField.targetGraphic, this.inputField);

                if (inputField.textComponent != null)
                    ApplyToTextComponent(inputField.textComponent, this.inputField);
            }
            else if (uiBehaviour is ScrollRect scrollRect)
            {
                if (scrollRect.TryGetComponent<Image>(out var imageComponent))
                    ApplyToGraphicComponent(imageComponent, this.scrollRect);
            }
            else if (uiBehaviour is Scrollbar scrollBar)
            {
                if (scrollBar.targetGraphic != null)
                    ApplyToGraphicComponent(scrollBar.targetGraphic, this.scrollBar);
            }
        }

        void ApplyToGraphicComponent(Graphic graphic, ElementStyle elementStyle)
        {
            graphic.color = elementStyle.backgroundColor;
            if (elementStyle.useSprite && graphic is Image image)
                image.sprite = elementStyle.sprite;
        }

        void ApplyToTextComponent(Text text, ElementStyle elementStyle)
        {
            text.color = elementStyle.textColor;
            if (elementStyle.font != null)
                text.font = elementStyle.font;
            if (elementStyle.fontSize > 0)
                text.fontSize = elementStyle.fontSize;
        }

        [ContextMenu("Apply skin")]
        void ApplyContextMenu()
        {
            if (null == m_appliedGameObject)
                return;

            Apply(m_appliedGameObject, true);
            EditorUtilityEx.MarkObjectAsDirty(m_appliedGameObject);
        }
    }
}
