using System;
using System.Security.Cryptography;

public class Sha512HashAlgorithm : IHashAlgorithm
{
	private readonly SHA512 _sha512 = SHA512.Create();

	public void Append(ReadOnlySpan<byte> data) => _sha512.TransformBlock(data.ToArray(), 0, data.Length, null, 0);
	public byte[] GetHashAndReset()
	{
		_sha512.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
		return _sha512.Hash;
	}
	public void Reset() => _sha512.Initialize();
}
