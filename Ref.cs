namespace UGameCore.Utilities
{
    /// <summary>
    /// Reference to an object.
    /// </summary>
    public sealed class Ref<T>
    {
        public T value;

        public Ref()
        {
        }

        public Ref(T value)
        {
            this.value = value;
        }
    }
}
