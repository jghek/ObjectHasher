using System;
using System.IO.Hashing;

public class XxHash32Algorithm : IHashAlgorithm
{
	private readonly XxHash32 _xxHash32 = new XxHash32();

	public void Append(ReadOnlySpan<byte> data) => _xxHash32.Append(data);
	public byte[] GetHashAndReset() => _xxHash32.GetCurrentHash();
	public void Reset() => _xxHash32.Reset();
}
