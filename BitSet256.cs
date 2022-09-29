namespace UGameCore.Utilities
{
	/// <summary>
	/// Bit set (bit field) capable of storing 256 bits, without allocating any memory.
	/// </summary>
	public struct BitSet256
	{
		private ulong m_bits0, m_bits1, m_bits2, m_bits3;

		public void Set(byte index)
		{
			if (index >= 192)
				m_bits3 |= ((ulong)1 << (index - 192));
			else if (index >= 128)
				m_bits2 |= ((ulong)1 << (index - 128));
			else if (index >= 64)
				m_bits1 |= ((ulong)1 << (index - 64));
			else
				m_bits0 |= ((ulong)1 << index);
		}

		public bool IsSet(byte index)
		{
			if (index >= 192)
				return (m_bits3 & ((ulong)1 << (index - 192))) != 0;
			if (index >= 128)
				return (m_bits2 & ((ulong)1 << (index - 128))) != 0;
			if (index >= 64)
				return (m_bits1 & ((ulong)1 << (index - 64))) != 0;
			return (m_bits0 & ((ulong)1 << index)) != 0;
		}
	}
}
