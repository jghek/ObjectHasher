using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

/// <summary>
/// Provides functionality to compute hashes for objects using a specified hash algorithm.
/// </summary
public class ObjectHasher : IObjectHasher
{
	private readonly Dictionary<Type, object> _configurations = new Dictionary<Type, object>();
	private readonly IHashAlgorithm _hashAlgorithm;

	/// <summary>
	/// Initializes a new instance of the <see cref="ObjectHasher"/> class with the specified hash algorithm.
	/// </summary>
	/// <param name="hashAlgorithm">The hash algorithm to use for computing hashes.</param>
	public ObjectHasher(IHashAlgorithm hashAlgorithm)
	{
		_hashAlgorithm = hashAlgorithm;
	}

	/// <summary>
	/// Registers a configuration for a specific type.
	/// </summary>
	/// <typeparam name="T">The type to register the configuration for.</typeparam>
	/// <param name="configure">An action to configure the type.</param>
	public void Register<T>(Action<TypeConfiguration<T>> configure) where T : class, new()
	{
		var typeConfig = new TypeConfiguration<T>();
		configure(typeConfig);
		_configurations[typeof(T)] = typeConfig;
	}

	/// <summary>
	/// Computes the hash for the specified object.
	/// </summary>
	/// <typeparam name="T">The type of the object.</typeparam>
	/// <param name="obj">The object to compute the hash for.</param>
	/// <returns>A byte array representing the computed hash.</returns>
	public byte[] ComputeHash<T>(T obj) where T: class
	{
		_hashAlgorithm.Reset();

		if (_configurations.TryGetValue(typeof(T), out var configObj) && configObj is TypeConfiguration<T> config)
			// Use registered configurations
			computeHashWithConfig(obj, config);
		else
		{
			// Fallback to reflection
			computeHashFallback(obj, _hashAlgorithm);
		}

		return _hashAlgorithm.GetHashAndReset();
	}

	/// <summary>
	/// Computes the hash for the specified object using a static method.
	/// </summary>
	/// <typeparam name="T">The type of the object.</typeparam>
	/// <param name="obj">The object to compute the hash for.</param>
	/// <param name="hashAlgorithm">The hash algorithm to use for computing the hash.</param>
	/// <returns>A byte array representing the computed hash.</returns>
	public static byte[] ComputeHashStatic<T>(T obj, IHashAlgorithm hashAlgorithm)
	{
		var hasher = new ObjectHasher(hashAlgorithm);
		return computeHashFallback(obj, hashAlgorithm);
	}

	private static byte[] computeHashFallback<T>(T obj, IHashAlgorithm hashAlgorithm)
	{
		if (obj == null)
			throw new ArgumentNullException(nameof(obj));

		var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
		var fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

		foreach (var prop in properties)
		{
			var value = prop.GetValue(obj);
			if (value == null) continue;
			processMemberValue(hashAlgorithm, prop.PropertyType, value);
		}

		foreach (var field in fields)
		{
			var value = field.GetValue(obj);
			if (value == null) continue;
			processMemberValue(hashAlgorithm, field.FieldType, value);
		}

		return hashAlgorithm.GetHashAndReset();
	}

	private void computeHashWithConfig<T>(T obj, TypeConfiguration<T> config) where T : class
	{
		foreach (var propertyConfig in config.PropertyConfigs)
		{
			if (propertyConfig.Value.Ignore)
				continue;

			var value = propertyConfig.Value.Selector(obj);
			
			if (value == null) 
				continue;

			if (propertyConfig.Value.Encoding != null && value is string strValue)
			{
				var stringBytes = propertyConfig.Value.Encoding.GetBytes(strValue);
				_hashAlgorithm.Append(stringBytes);
			}
			else
				processMemberValue(_hashAlgorithm, typeof(T), value);
		}
	}

	private static void processMemberValue(IHashAlgorithm hashAlgorithm, Type memberType, object value)
	{
		if (memberType == typeof(string))
		{
			string strValue = (string)value;
			byte[] stringBytes = Encoding.Unicode.GetBytes(strValue);
			hashAlgorithm.Append(stringBytes);
		}
		else if (memberType.IsPrimitive || isSupportedPrimitiveType(memberType))
		{
			byte[] primitiveBytes = getBytesForPrimitive(value);
			hashAlgorithm.Append(primitiveBytes);
		}
		else if (memberType.IsValueType)
		{
			byte[] structBytes = getBytesForValueType(value);
			hashAlgorithm.Append(structBytes);
		}
		else if (typeof(IEnumerable).IsAssignableFrom(memberType))
		{
			IEnumerable collection = (IEnumerable)value;
			foreach (var item in collection)
				if (item != null)
					processMemberValue(hashAlgorithm, item.GetType(), item);
		}
		else
			computeHashFallback(value, hashAlgorithm);
	}

	private static bool isSupportedPrimitiveType(Type type) => 
		type == typeof(decimal) || type == typeof(DateTime) || type == typeof(Guid);

	private static byte[] getBytesForPrimitive(object value) =>
		value switch
		{
			bool b => BitConverter.GetBytes(b),
			byte b => new[] { b },
			char c => BitConverter.GetBytes(c),
			short s => BitConverter.GetBytes(s),
			int i => BitConverter.GetBytes(i),
			long l => BitConverter.GetBytes(l),
			ushort us => BitConverter.GetBytes(us),
			uint ui => BitConverter.GetBytes(ui),
			ulong ul => BitConverter.GetBytes(ul),
			float f => BitConverter.GetBytes(f),
			double d => BitConverter.GetBytes(d),
			decimal dec => getBytesForDecimal(dec),
			DateTime dt => BitConverter.GetBytes(dt.Ticks),
			Guid g => g.ToByteArray(),
			_ => throw new NotImplementedException($"No byte conversion for type {value.GetType().Name}"),
		};

	private static byte[] getBytesForDecimal(decimal dec)
	{
		int[] bits = decimal.GetBits(dec);
		var bytes = new List<byte>();
		foreach (var bit in bits)
		{
			bytes.AddRange(BitConverter.GetBytes(bit));
		}
		return bytes.ToArray();
	}

	private static byte[] getBytesForValueType(object value)
	{
		if (value is ValueType && !value.GetType().IsPrimitive)
			return BitConverter.GetBytes(Convert.ToDouble(value));

		throw new NotImplementedException($"No byte conversion for type {value.GetType().Name}");
	}
}
