using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

public class TypeConfiguration<T>
{
	public Dictionary<Expression<Func<T, object>>, PropertyConfig> PropertyConfigs { get; } = new();

	public TypeConfiguration()
	{
		var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
		var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);

		foreach (var prop in properties)
			PropertyConfigs.Add(Expression.Lambda<Func<T, object>>(Expression.Property(Expression.Parameter(typeof(T)), prop), Expression.Parameter(typeof(T))), new PropertyConfig());

		foreach (var field in fields)
			PropertyConfigs.Add(Expression.Lambda<Func<T, object>>(Expression.Field(Expression.Parameter(typeof(T)), field), Expression.Parameter(typeof(T))), new PropertyConfig());

	}

	public TypeConfiguration<T> Ignore(Expression<Func<T, object>> selector)
	{
		PropertyConfigs.Remove(selector); // Removes if it exists
		return this;
	}

	public TypeConfiguration<T> Configure(Expression<Func<T, object>> selector, Encoding encoding)
	{
		PropertyConfigs[selector] = new PropertyConfig { Encoding = encoding };
		return this;
	}
}
