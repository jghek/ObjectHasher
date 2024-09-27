using System;

namespace ObjectHasher;

public interface IObjectHash
{
	void Register<T>(Action<TypeConfiguration<T>> configure);
	byte[] ComputeHash(object obj);
}
