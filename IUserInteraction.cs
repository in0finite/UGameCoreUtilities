using System.Collections;

namespace UGameCore.Utilities
{
    /// <summary>
    /// Allows for basic interaction with user, in the form of showing messages, or asking for input.
    /// </summary>
    public interface IUserInteraction
    {
        /// <summary>
        /// Shows message to the user.
        /// </summary>
        IEnumerator ShowMessageAsync(string title, string message);

        /// <summary>
        /// Asks user to confirm, using 2 options (Ok and Cancel).
        /// Result will be true if user agreed, false if disagreed.
        /// </summary>
        IEnumerator ConfirmAsync(Ref<bool> bResultRef, string title, string message, string ok = "Ok", string cancel = "Cancel");
    }
}
