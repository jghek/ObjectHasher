using System;
using System.IO.Hashing;

// Wrappers for non-cryptographic hash algorithms
public class Crc32HashAlgorithm : IHashAlgorithm
{
	private readonly Crc32 _crc32 = new Crc32();

	public void Append(ReadOnlySpan<byte> data) => _crc32.Append(data);
	public byte[] GetHashAndReset() => _crc32.GetCurrentHash();
	public void Reset() => _crc32.Reset();
}
