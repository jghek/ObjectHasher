using System;

namespace ObjectHasher;

public interface IHashAlgorithm
{
	void Append(ReadOnlySpan<byte> data);
	byte[] GetHashAndReset();
	void Reset();
}
