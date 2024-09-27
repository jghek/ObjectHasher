using System;
using System.IO.Hashing;

namespace ObjectHasher;

public class Crc64HashAlgorithm : IHashAlgorithm
{
	private readonly Crc64 _crc64 = new Crc64();

	public void Append(ReadOnlySpan<byte> data) => _crc64.Append(data);
	public byte[] GetHashAndReset() => _crc64.GetCurrentHash();
	public void Reset() => _crc64.Reset();
}
