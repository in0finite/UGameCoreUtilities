const myClipboardUtilityLib = 
{
  $MarshalStringUtf8: function (stringPtr, stringArrayLength) {
    // If "Native C/C++ Multithreading" setting is enabled, Unity will use SharedArrayBuffer for WASM memory.
    // In this case, TextDecoder.decode() will throw exception.
    // That's why we have to copy WASM array into JS array.

    var strSubArray = HEAPU8.subarray(stringPtr, stringPtr + stringArrayLength);
    var jsArray = new Uint8Array(strSubArray);
    return new TextDecoder('UTF-8').decode(jsArray);
  },

  ClipboardUtility_SetText: function(stringPtr, stringArrayLength)
  {
    try
    {
      var jsString = MarshalStringUtf8(stringPtr, stringArrayLength);

      window.navigator.clipboard.writeText(jsString); // returns Promise
    }
    catch (error)
    {
      console.error(error);
      return;
    }
  },
};

autoAddDeps(myClipboardUtilityLib, "$MarshalStringUtf8");

mergeInto(LibraryManager.library, myClipboardUtilityLib);
