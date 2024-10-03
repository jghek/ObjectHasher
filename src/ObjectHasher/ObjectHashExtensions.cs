using System;

namespace ObjectHasher;

/// <summary>
/// Provides extension methods for converting byte arrays to various data types.
/// </summary>
public static class ObjectHashExtensions
{
	/// <summary>
	/// Converts a byte array to a 64-bit signed integer (Int64).
	/// </summary>
	/// <param name="bytes">The byte array to convert. Must be at least 8 bytes long.</param>
	/// <returns>A 64-bit signed integer (Int64) representation of the byte array.</returns>
	/// <exception cref="ArgumentException">Thrown if the byte array is too small for conversion.</exception>
	public static long ToInt64(this byte[] bytes)
		=> BitConverter.ToInt64(bytes, 0);

	/// <summary>
	/// Converts a byte array to a 32-bit signed integer (Int32).
	/// </summary>
	/// <param name="bytes">The byte array to convert. Must be at least 4 bytes long.</param>
	/// <returns>A 32-bit signed integer (Int32) representation of the byte array.</returns>
	/// <exception cref="ArgumentException">Thrown if the byte array is too small for conversion.</exception>
	public static long ToInt32(this byte[] bytes)
		=> BitConverter.ToInt32(bytes, 0);

	/// <summary>
	/// Converts a byte array to a <see cref="Guid"/>.
	/// </summary>
	/// <param name="bytes">The byte array to convert. Must be exactly 16 bytes long.</param>
	/// <returns>A <see cref="Guid"/> representation of the byte array.</returns>
	/// <exception cref="ArgumentException">Thrown if the byte array is not 16 bytes long.</exception>
	public static Guid ToGuid(this byte[] bytes)
		=> new Guid(bytes);

	/// <summary>
	/// Converts a byte array to its Base64-encoded string representation.
	/// </summary>
	/// <param name="bytes">The byte array to convert.</param>
	/// <returns>A Base64-encoded string representing the byte array.</returns>
	public static string ToBase64String(this byte[] bytes)
		=> Convert.ToBase64String(bytes);
}
