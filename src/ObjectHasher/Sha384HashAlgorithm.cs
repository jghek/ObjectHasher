using System;
using System.Security.Cryptography;

namespace ObjectHasher;

public class Sha384HashAlgorithm : IHashAlgorithm
{
	private readonly SHA384 _sha384 = SHA384.Create();

	public void Append(ReadOnlySpan<byte> data) => _sha384.TransformBlock(data.ToArray(), 0, data.Length, null, 0);
	public byte[] GetHashAndReset()
	{
		_sha384.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
		return _sha384.Hash;
	}
	public void Reset() => _sha384.Initialize();
}
