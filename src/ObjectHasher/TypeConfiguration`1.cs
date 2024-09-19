using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;

public class TypeConfiguration
{
	public Dictionary<string, PropertyConfig> PropertyConfigs { get; } = new();

	public TypeConfiguration(Type type)
	{
		var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
		var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

		foreach (var prop in properties)
		{
			var parameter = Expression.Parameter(type);
			var propertyAccess = Expression.Property(parameter, prop);
			var castToObject = Expression.Convert(propertyAccess, typeof(object));
			var lambda = Expression.Lambda<Func<object, object>>(castToObject, parameter);
			PropertyConfigs.Add(prop.Name, new PropertyConfig() { Selector = lambda.Compile() });
		}

		foreach (var field in fields)
		{
			var parameter = Expression.Parameter(type);
			var fieldAccess = Expression.Field(parameter, field);
			var castToObject = Expression.Convert(fieldAccess, typeof(object));
			var lambda = Expression.Lambda<Func<object, object>>(castToObject, parameter);
			PropertyConfigs.Add(field.Name, new PropertyConfig() { Selector = lambda.Compile() });
		}
	}

	public TypeConfiguration Ignore(Expression<Func<object, object>> selector)
	{
		PropertyConfigs[getMemberName(selector)].Ignore = true;
		return this;
	}

	public TypeConfiguration Configure(Expression<Func<object, object>> selector, Encoding encoding)
	{
		PropertyConfigs[getMemberName(selector)].Encoding = encoding;
		return this;
	}

	private static string getMemberName(Expression<Func<object, object>> expression)
	{
		if (expression.Body is MemberExpression memberExpression)
			return memberExpression.Member.Name;
		else if (expression.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression operandMemberExpression)
			return operandMemberExpression.Member.Name;

		throw new ArgumentException("Invalid expression");
	}
}
