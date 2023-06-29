using System.Collections;

namespace UGameCore.Utilities
{
    /// <summary>
    /// Stream which can read data in an async way.
    /// </summary>
    public interface IAsyncStream
    {
        IEnumerator ReadAsync(Ref<int> numReadRef, byte[] buffer, int offset, int count);
    }
}
