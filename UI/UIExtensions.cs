using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGameCore.Utilities
{
    public static class UIExtensions
    {
        public static RectTransform GetRectTransform(this Component component)
        {
            return component.transform as RectTransform;
        }

        public static RectTransform GetRectTransform(this GameObject go)
        {
            return go.transform as RectTransform;
        }

        public static void BringToFront(this RectTransform rectTransform)
        {
            rectTransform.SetAsLastSibling();
        }

        public static void BringToFront(this UIBehaviour uiBehaviour)
        {
            uiBehaviour.GetRectTransform().BringToFront();
        }

        public static Vector2 GetPreferredSize<TLayoutElement>(this TLayoutElement layoutElement)
            where TLayoutElement : ILayoutElement
        {
            return new Vector2(layoutElement.preferredWidth, layoutElement.preferredHeight);
        }

        public static void SetColorAlpha(this Graphic graphic, float alpha)
        {
            Color color = graphic.color.WithAlpha(alpha);
            graphic.color = color; // it will check for changes itself
        }

        public static void SetStringIfChanged(this TextMeshProUGUI textComponent, string value)
        {
            if (textComponent.text != value)
                textComponent.SetText(value); // use SetText() because it doesn't check for changes - it's faster
        }

        public static void SetStringIfChanged(this TextMeshProUGUI textComponent, ReadOnlySpan<char> value)
        {
            if (!textComponent.text.AsSpan().SequenceEqual(value))
                textComponent.SetText(new string(value)); // use SetText() because it doesn't check for changes - it's faster
        }

        public static void SetStringIfChanged(this Text textComponent, string value)
        {
            textComponent.text = value; // it will check for changes itself
        }

        public static void SetStringIfChanged(this Text textComponent, ReadOnlySpan<char> value)
        {
            if (textComponent.text.AsSpan().SequenceEqual(value))
                return;
            textComponent.text = new string(value);
        }

        public static void SetIntIfChanged(this TextMeshProUGUI textComponent, int value)
        {
            if (int.TryParse(textComponent.text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int currentValue))
            {
                if (currentValue == value)
                    return;
            }

            // use SetText() because it doesn't check for changes - it's faster
            textComponent.SetText(value.ToString(CultureInfo.InvariantCulture));
        }

        public static void SetIntIfChanged(this Text textComponent, int value)
        {
            if (int.TryParse(textComponent.text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int currentValue))
            {
                if (currentValue == value)
                    return;
            }

            textComponent.text = value.ToString(CultureInfo.InvariantCulture);
        }

        public static TextMeshProUGUI GetTextComponentOrThrow(this Button button)
        {
            return button.GetComponentInChildrenOrThrow<TextMeshProUGUI>();
        }

        public static Button WithText(this Button button, string text)
        {
            button.GetTextComponentOrThrow().text = text;
            return button;
        }

        public static Color GetTextColor(this Button button)
        {
            return button.GetTextComponentOrThrow().color;
        }

        public static Button WithTextColor(this Button button, Color color)
        {
            button.GetTextComponentOrThrow().color = color;
            return button;
        }

        public static Color GetBackgroundColor(this Button button)
        {
            return button.GetComponentOrThrow<Image>().color;
        }

        public static Button WithBackgroundColor(this Button button, Color color)
        {
            button.GetComponentOrThrow<Image>().color = color;
            return button;
        }

        public static string RemoveNoParseForTMP(string str)
        {
            // Removing characters doesn't help (attacker can still inject stuff),
            // but adding new ones or replacing existing ones does work.
            // We only need to make sure that <noparse> can not be closed by attacker, everything else doesn't matter
            // because TextMeshPro ignores all other tags when inside <noparse> tag.

            return str.Replace("</noparse>", "</ noparse>", System.StringComparison.OrdinalIgnoreCase);
        }

        public static string EscapeStringForTMP(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return str;

            return "<noparse>" + RemoveNoParseForTMP(str) + "</noparse>";
        }

        public static string SurroundTextWithColor(string text, Color color)
        {
            Span<char> chars = stackalloc char[text.Length + 32];
            SpanCharStream spanCharStream = new(chars);
            SurroundTextWithColor(text, ColorUtility.ToHtmlStringRGBA(color), ref spanCharStream);
            return new string(spanCharStream.AsSpan);
        }

        public static void SurroundTextWithColor(
            ReadOnlySpan<char> text, ReadOnlySpan<char> htmlColorString, System.Text.StringBuilder sb)
        {
            Span<char> chars = stackalloc char[text.Length + 32];
            SpanCharStream spanCharStream = new(chars);
            SurroundTextWithColor(text, htmlColorString, ref spanCharStream);
            sb.Append(spanCharStream.AsSpan);
        }

        public static void SurroundTextWithColor(
            ReadOnlySpan<char> text,
            ReadOnlySpan<char> htmlColorString,
            ref SpanCharStream dest)
        {
            dest.WriteString("<color=#");
            dest.WriteString(htmlColorString);
            dest.WriteString(">");
            dest.WriteString(text);
            dest.WriteString("</color>");
        }
    }
}
