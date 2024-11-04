using UnityEngine;

namespace UGameCore.Utilities
{
    public static class ClipboardUtility
    {
#if UNITY_WEBGL || UNITY_EDITOR
        const string PluginName = "__Internal";

        [System.Runtime.InteropServices.DllImport(PluginName)]
        static extern void ClipboardUtility_SetText(byte[] stringByteArray, int stringByteArrayLength);
#endif


        public static string GetClipboardText()
        {
            // On Web platform, there is `Promise<string> window.navigator.clipboard.readText()`, but it returns Promise.
            // We could return Task<string> and pass int64 as id of operation to JS, but that's too much ...
            // So we simply use GUIUtility.systemCopyBuffer.

            return GUIUtility.systemCopyBuffer;
        }

        public static void SetClipboardText(string text)
        {
            GUIUtility.systemCopyBuffer = text;

#if UNITY_WEBGL
            byte[] stringByteArray = System.Text.Encoding.UTF8.GetBytes(text);
            ClipboardUtility_SetText(stringByteArray, stringByteArray.Length);
#endif
        }
    }
}
