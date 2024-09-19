using System;

// Interface for the different hash algorithms
public interface IHashAlgorithm
{
	void Append(ReadOnlySpan<byte> data);
	byte[] GetHashAndReset();
	void Reset();
}
