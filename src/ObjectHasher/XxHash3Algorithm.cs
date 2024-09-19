using System;
using System.IO.Hashing;

public class XxHash3Algorithm : IHashAlgorithm
{
	private readonly XxHash3 _xxHash3 = new XxHash3();

	public void Append(ReadOnlySpan<byte> data) => _xxHash3.Append(data);
	public byte[] GetHashAndReset() => _xxHash3.GetCurrentHash();
	public void Reset() => _xxHash3.Reset();
}
