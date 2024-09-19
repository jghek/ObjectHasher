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
	private readonly Dictionary<Type, TypeConfiguration> _configurations = new Dictionary<Type, TypeConfiguration>();
	private readonly IHashAlgorithm _hashAlgorithm;
	public Encoding DefaultEncoding { get; set; } = Encoding.Unicode;

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
	public void Register<T>(Action<TypeConfiguration> configure) where T : class
	{
		var typeConfig = new TypeConfiguration(typeof(T));
		configure(typeConfig);
		_configurations[typeof(T)] = typeConfig;
	}


	/// <summary>
	/// Computes the hash for the specified object.
	/// </summary>
	/// <typeparam name="T">The type of the object.</typeparam>
	/// <param name="obj">The object to compute the hash for.</param>
	/// <returns>A byte array representing the computed hash.</returns>
	public byte[] ComputeHash(object obj)
		=> computeHash(obj, _hashAlgorithm, _configurations);

	/// <summary>
	/// Computes the hash for the specified object using a static method.
	/// </summary>
	/// <typeparam name="T">The type of the object.</typeparam>
	/// <param name="obj">The object to compute the hash for.</param>
	/// <param name="hashAlgorithm">The hash algorithm to use for computing the hash.</param>
	/// <returns>A byte array representing the computed hash.</returns>
	public static byte[] ComputeHash(object obj, IHashAlgorithm hashAlgorithm)
		=> computeHash(obj, hashAlgorithm, new());

	private static byte[] computeHash(object obj, IHashAlgorithm hashAlgorithm, Dictionary<Type, TypeConfiguration> configurations)
	{
		hashAlgorithm.Reset();

		processType(obj, hashAlgorithm, configurations);

		//if (configurations.TryGetValue(typeof(T), out var configObj) && configObj is TypeConfiguration<T> config)
		//	computeHashWithConfig(obj, hashAlgorithm, config);
		//else
		//	computeHashFallback(obj, hashAlgorithm);

		return hashAlgorithm.GetHashAndReset();
	}

	private static void processType<T>(T obj, IHashAlgorithm hashAlgorithm, Dictionary<Type, TypeConfiguration> configurations) where T : class
	{
		ArgumentNullException.ThrowIfNull(obj);

		if (configurations is not null && configurations.TryGetValue(typeof(T), out TypeConfiguration config))
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
					hashAlgorithm.Append(stringBytes);
				}
				else
					processMemberValue(value.GetType(), hashAlgorithm, value);
			}
		}
		else 
		{
			var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
			var fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

			foreach (var prop in properties)
			{
				var value = prop.GetValue(obj);
				if (value == null) continue;
				processMemberValue(prop.PropertyType, hashAlgorithm, value);
			}

			foreach (var field in fields)
			{
				var value = field.GetValue(obj);
				if (value == null) continue;
				processMemberValue(field.FieldType, hashAlgorithm, value);
			}
		}
	}

	private static void processMemberValue<T>(string name, object value, IHashAlgorithm hashAlgorithm, Dictionary<Type, TypeConfiguration> configurations, Encoding defaultEncoding) where T: class
	{
		var memberType = value.GetType();

		PropertyConfig? config = null;

		if (configurations.TryGetValue(typeof(T), out TypeConfiguration v))
			if (v.PropertyConfigs.TryGetValue(name, out PropertyConfig c))
				config = c;

		if (memberType == typeof(string))
		{
			hashAlgorithm.Append(config?.Encoding?.GetBytes((string)value) ?? defaultEncoding.GetBytes((string)value));
		}
		else if (memberType.IsPrimitive || isSupportedPrimitiveType(memberType))
		{
			hashAlgorithm.Append(getBytesForPrimitive(value));
		}
		else if (memberType.IsValueType)
		{
			hashAlgorithm.Append(getBytesForValueType(value));
		}
		else if (typeof(IEnumerable).IsAssignableFrom(memberType))
		{
			IEnumerable collection = (IEnumerable)value;
			foreach (var item in collection)
				if (item != null)
					processType(item, hashAlgorithm, configurations);
		}
		else
			processType(value, hashAlgorithm, configurations);
	}


	//private static byte[] computeHashFallback<T>(T obj, IHashAlgorithm hashAlgorithm)
	//{

	//	var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
	//	var fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

	//	foreach (var prop in properties)
	//	{
	//		var value = prop.GetValue(obj);
	//		if (value == null) continue;
	//		processMemberValue(hashAlgorithm, prop.PropertyType, value);
	//	}

	//	foreach (var field in fields)
	//	{
	//		var value = field.GetValue(obj);
	//		if (value == null) continue;
	//		processMemberValue(hashAlgorithm, field.FieldType, value);
	//	}

	//	return hashAlgorithm.GetHashAndReset();
	//}

	//private static byte[] computeHashWithConfig<T>(T obj, IHashAlgorithm hashAlgorithm, TypeConfiguration<T> config) where T : class
	//{
	//	foreach (var propertyConfig in config.PropertyConfigs)
	//	{
	//		if (propertyConfig.Value.Ignore)
	//			continue;

	//		var value = propertyConfig.Value.Selector(obj);
	//		
	//		if (value == null) 
	//			continue;

	//		if (propertyConfig.Value.Encoding != null && value is string strValue)
	//		{
	//			var stringBytes = propertyConfig.Value.Encoding.GetBytes(strValue);
	//			hashAlgorithm.Append(stringBytes);
	//		}
	//		else
	//			processMemberValue(hashAlgorithm, value.GetType(), value);
	//	}

	//	return hashAlgorithm.GetHashAndReset();
	//}


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
