using System;
using System.Security.Cryptography;

// Wrappers for cryptographic hash algorithms
public class Sha1HashAlgorithm : IHashAlgorithm
{
	private readonly SHA1 _sha1 = SHA1.Create();

	public void Append(ReadOnlySpan<byte> data) => _sha1.TransformBlock(data.ToArray(), 0, data.Length, null, 0);
	public byte[] GetHashAndReset()
	{
		_sha1.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
		return _sha1.Hash;
	}
	public void Reset() => _sha1.Initialize();
}
