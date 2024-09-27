using System;
using System.IO.Hashing;

namespace ObjectHasher;

public class XxHash128Algorithm : IHashAlgorithm
{
	private readonly XxHash128 _xxHash128 = new XxHash128();

	public void Append(ReadOnlySpan<byte> data) => _xxHash128.Append(data);
	public byte[] GetHashAndReset() => _xxHash128.GetCurrentHash();
	public void Reset() => _xxHash128.Reset();
}
