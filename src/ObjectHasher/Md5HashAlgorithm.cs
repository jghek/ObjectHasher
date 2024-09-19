using System;
using System.Security.Cryptography;

public class Md5HashAlgorithm : IHashAlgorithm
{
	private readonly MD5 _md5 = MD5.Create();

	public void Append(ReadOnlySpan<byte> data) => _md5.TransformBlock(data.ToArray(), 0, data.Length, null, 0);
	public byte[] GetHashAndReset()
	{
		_md5.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
		return _md5.Hash;
	}
	public void Reset() => _md5.Initialize();
}
