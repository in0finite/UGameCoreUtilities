namespace UGameCore.Utilities
{
    /// <summary>
    /// General-purpose structure that holds serializable data.
    /// </summary>
    [System.Serializable]
    public struct SerializableBag
    {
        public long longValue;
        public double doubleValue;
        public string stringValue;
        public UnityEngine.Object objectValue;
    }
}
