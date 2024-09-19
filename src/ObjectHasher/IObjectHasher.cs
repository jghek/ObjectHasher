using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Hashing;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

public interface IObjectHasher
{
	void Register<T>(Action<TypeConfiguration<T>> configure) where T: class, new();
	byte[] ComputeHash<T>(T obj) where T : class;
}
