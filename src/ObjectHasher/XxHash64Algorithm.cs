using System;
using System.IO.Hashing;

public class XxHash64Algorithm : IHashAlgorithm
{
	private readonly XxHash64 _xxHash64 = new XxHash64();

	public void Append(ReadOnlySpan<byte> data) => _xxHash64.Append(data);
	public byte[] GetHashAndReset() => _xxHash64.GetCurrentHash();
	public void Reset() => _xxHash64.Reset();
}
