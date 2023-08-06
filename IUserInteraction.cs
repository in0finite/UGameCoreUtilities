using System.Collections;

namespace UGameCore.Utilities
{
    /// <summary>
    /// Allows for basic interaction with user, in the form of showing messages, or asking for input.
    /// </summary>
    public interface IUserInteraction
    {
        /// <summary>
        /// Shows message to the user. Note that this function does not have to be synchronous (it can return immediately).
        /// </summary>
        void ShowMessage(string title, string message) => ShowMessageAsync(title, message).EnumerateToEnd();

        /// <summary>
        /// Shows message to the user and return <see cref="IEnumerator"/> which finishes when user closes the message.
        /// </summary>
        IEnumerator ShowMessageAsync(string title, string message);

        /// <summary>
        /// Is Confirm supported by this implementation ?
        /// </summary>
        bool SupportsConfirm { get; }

        /// <summary>
        /// Asks user to confirm, using 2 options (Ok and Cancel).
        /// Result will be true if user agreed, false if disagreed.
        /// </summary>
        IEnumerator ConfirmAsync(Ref<bool> bResultRef, string title, string message, string ok = "Ok", string cancel = "Cancel");
    }
}
