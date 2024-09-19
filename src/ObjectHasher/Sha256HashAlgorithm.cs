using System;
using System.Security.Cryptography;

public class Sha256HashAlgorithm : IHashAlgorithm
{
	private readonly SHA256 _sha256 = SHA256.Create();

	public void Append(ReadOnlySpan<byte> data) => _sha256.TransformBlock(data.ToArray(), 0, data.Length, null, 0);
	public byte[] GetHashAndReset()
	{
		_sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
		return _sha256.Hash;
	}
	public void Reset() => _sha256.Initialize();
}
