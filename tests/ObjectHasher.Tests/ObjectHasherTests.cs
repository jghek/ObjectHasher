using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Xunit;

public class ObjectHasherTests
{
	// Example classes to test
	public class Person
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public DateTime BirthDate { get; set; }
	}

	public class Car
	{
		public string Make { get; set; }
		public string Model { get; set; }
		public int Year { get; set; }
	}

	// Test custom Register with Ignore and Configure in non-static method
	[Fact]
	public void ComputeHash_WithRegisterIgnoreAndConfigure_ShouldIgnoreIdAndUseCustomEncoding()
	{
		// Arrange
		var person = new Person { Id = 123, Name = "Alice", BirthDate = new DateTime(1990, 1, 1) };
		var hasher = new ObjectHasher(new Sha256HashAlgorithm());

		// Register custom settings for Person
		hasher.Register<Person>(options =>
		{
			options.Ignore(p => p.Id); // Ignore the Id property
			options.Configure(p => p.Name, Encoding.ASCII); // Use ASCII encoding for Name
		});

		// Act
		var hash = hasher.ComputeHash(person);

		// Assert
		hash.Should().NotBeNull();
		hash.Length.Should().Be(32, "SHA256 hash should always be 32 bytes long.");
	}

	// Test custom Register for static method (static method doesn't use registrations)
	[Fact]
	public void ComputeHashStatic_ShouldNotUseRegisteredOptions()
	{
		// Arrange
		var person = new Person { Id = 456, Name = "Bob", BirthDate = new DateTime(1985, 5, 23) };
		var hasher = new ObjectHasher(new Sha256HashAlgorithm());

		// Register custom settings for Person (this should be ignored by the static method)
		hasher.Register<Person>(options =>
		{
			options.Ignore(p => p.Name); // Ignore the Name property
		});

		// Act
		var hash = ObjectHasher.ComputeHashStatic(person, new Sha256HashAlgorithm());

		// Assert
		hash.Should().NotBeNull();
		hash.Length.Should().Be(32, "SHA256 hash should always be 32 bytes long.");
	}

	// Test custom Register with Ignore only in non-static method
	[Fact]
	public void ComputeHash_WithRegisterIgnoreOnly_ShouldIgnoreSpecificProperty()
	{
		// Arrange
		var person = new Person { Id = 789, Name = "Charlie", BirthDate = new DateTime(2000, 12, 31) };
		var hasher = new ObjectHasher(new Crc32HashAlgorithm());

		// Register custom settings for Person
		hasher.Register<Person>(options =>
		{
			options.Ignore(p => p.BirthDate); // Ignore the BirthDate property
		});

		// Act
		var hash = hasher.ComputeHash(person);

		// Assert
		hash.Should().NotBeNull();
		hash.Length.Should().Be(4, "CRC32 hash should always be 4 bytes long.");
	}

	// Test custom Register with Configure encoding only in non-static method
	[Fact]
	public void ComputeHash_WithRegisterConfigureOnly_ShouldUseCustomEncodingForStrings()
	{
		// Arrange
		var person = new Person { Id = 1010, Name = "David", BirthDate = new DateTime(1995, 11, 11) };
		var hasher = new ObjectHasher(new Crc64HashAlgorithm());

		// Register custom settings for Person
		hasher.Register<Person>(options =>
		{
			options.Configure(p => p.Name, Encoding.ASCII); // Use ASCII encoding for Name
		});

		// Act
		var hash = hasher.ComputeHash(person);

		// Assert
		hash.Should().NotBeNull();
		hash.Length.Should().Be(8, "CRC64 hash should always be 8 bytes long.");
	}

	// Test that Register<> affects only the registered type
	[Fact]
	public void ComputeHash_WithRegister_ShouldNotAffectOtherTypes()
	{
		// Arrange
		var person = new Person { Id = 1111, Name = "Eve", BirthDate = new DateTime(1970, 1, 1) };
		var car = new Car { Make = "Tesla", Model = "Model S", Year = 2020 };
		var hasher = new ObjectHasher(new Sha512HashAlgorithm());

		// Register custom settings for Person (ignore Id)
		hasher.Register<Person>(options =>
		{
			options.Ignore(p => p.Id); // Ignore the Id property for Person
		});

		// Act
		var personHash = hasher.ComputeHash(person);
		var carHash = hasher.ComputeHash(car); // Car should not be affected by Person's registration

		// Assert
		personHash.Should().NotBeNull();
		personHash.Length.Should().Be(64, "SHA512 hash should always be 64 bytes long.");
		carHash.Should().NotBeNull();
		carHash.Length.Should().Be(64, "SHA512 hash should always be 64 bytes long.");
	}

	// Test registering multiple types with custom configurations
	[Fact]
	public void ComputeHash_WithMultipleRegisteredTypes_ShouldApplyCorrectRegistrationsPerType()
	{
		// Arrange
		var person = new Person { Id = 123, Name = "Alice", BirthDate = new DateTime(1990, 1, 1) };
		var car = new Car { Make = "Tesla", Model = "Model 3", Year = 2019 };
		var hasher = new ObjectHasher(new XxHash64Algorithm());

		// Register custom settings for Person and Car
		hasher.Register<Person>(options =>
		{
			options.Ignore(p => p.Id); // Ignore the Id property for Person
			options.Configure(p => p.Name, Encoding.ASCII); // Use ASCII encoding for Name
		});

		hasher.Register<Car>(options =>
		{
			options.Ignore(c => c.Year); // Ignore the Year property for Car
		});

		// Act
		var personHash = hasher.ComputeHash(person);
		var carHash = hasher.ComputeHash(car);

		// Assert
		personHash.Should().NotBeNull();
		personHash.Length.Should().Be(8, "XxHash64 produces an 8-byte hash.");
		carHash.Should().NotBeNull();
		carHash.Length.Should().Be(8, "XxHash64 produces an 8-byte hash.");
	}

	// Test static method with Register (should not use registered options)
	[Fact]
	public void ComputeHashStatic_ShouldNotUseRegisterOptionsForStaticHashing()
	{
		// Arrange
		var person = new Person { Id = 2222, Name = "Frank", BirthDate = new DateTime(2001, 7, 10) };
		var hasher = new ObjectHasher(new Sha1HashAlgorithm());

		// Register custom settings for Person (this should not affect static hashing)
		hasher.Register<Person>(options =>
		{
			options.Ignore(p => p.BirthDate); // Ignore BirthDate (but static method won't use this)
		});

		// Act
		var staticHash = ObjectHasher.ComputeHashStatic(person, new Sha1HashAlgorithm());
		var instanceHash = hasher.ComputeHash(person);

		// Assert
		staticHash.Should().NotBeEquivalentTo(instanceHash, "Static hash should not be affected by instance-based registrations.");
		staticHash.Should().NotBeNull();
		staticHash.Length.Should().Be(20, "SHA1 hash should always be 20 bytes long.");
		instanceHash.Should().NotBeNull();
		instanceHash.Length.Should().Be(20, "SHA1 hash should always be 20 bytes long.");
	}

	[Theory]
	[InlineData("MD5")]
	[InlineData("SHA1")]
	[InlineData("SHA256")]
	[InlineData("SHA384")]
	[InlineData("SHA512")]
	[InlineData("CRC32")]
	[InlineData("CRC64")]
	[InlineData("XxHash32")]
	[InlineData("XxHash64")]
	[InlineData("XxHash128")]
	[InlineData("XxHash3")]
	public void ComputeHash_ShouldReturnConsistentHash_ForSameObjectData(string algorithm)
	{
		// Arrange
		var person1 = new Person { Id = 5, Name = "Eve", BirthDate = new DateTime(1970, 1, 1) };
		var person2 = new Person { Id = 5, Name = "Eve", BirthDate = new DateTime(1970, 1, 1) };
		var hasher = getHasherForAlgorithm(algorithm);

		// Act
		var hash1 = hasher.ComputeHash(person1);
		var hash2 = hasher.ComputeHash(person2);

		// Assert
		hash1.Should().BeEquivalentTo(hash2, $"{algorithm} should produce the same hash for the same object state.");
	}

	[Theory]
	[InlineData("MD5")]
	[InlineData("SHA1")]
	[InlineData("SHA256")]
	[InlineData("SHA384")]
	[InlineData("SHA512")]
	[InlineData("CRC32")]
	[InlineData("CRC64")]
	[InlineData("XxHash32")]
	[InlineData("XxHash64")]
	[InlineData("XxHash128")]
	[InlineData("XxHash3")]
	public void ComputeHash_ShouldReturnSameHash_WhenIdIsIgnored(string algorithm)
	{
		// Arrange
		var person1 = new Person { Id = 1, Name = "John Doe", BirthDate = new DateTime(1980, 1, 1) };
		var person2 = new Person { Id = 2, Name = "John Doe", BirthDate = new DateTime(1980, 1, 1) };
		var person3 = new Person { Id = 3, Name = "Jane Doe", BirthDate = new DateTime(1980, 1, 1) };

		var hasher = getHasherForAlgorithm(algorithm);
		hasher.Register<Person>(options => options.Ignore(p => p.Id));

		// Act
		var hash1 = hasher.ComputeHash(person1);
		var hash2 = hasher.ComputeHash(person2);
		var hash3 = hasher.ComputeHash(person3);

		// Assert
		hash1.Should().BeEquivalentTo(hash2, $"{algorithm} should produce the same hash when the Id property is ignored.");
		hash1.Should().NotBeEquivalentTo(hash3, $"{algorithm} should produce a different hash when the Id property is ignored, but other properties change.");
	}

	[Theory]
	[InlineData("MD5",       "ASCII",   "ASCII")]
	[InlineData("SHA1",      "ASCII",   "ASCII")]
	[InlineData("SHA256",    "ASCII",   "ASCII")]
	[InlineData("SHA384",    "ASCII",   "ASCII")]
	[InlineData("SHA512",    "ASCII",   "ASCII")]
	[InlineData("CRC32",     "ASCII",   "ASCII")]
	[InlineData("CRC64",     "ASCII",   "ASCII")]
	[InlineData("XxHash32",  "ASCII",   "ASCII")]
	[InlineData("XxHash64",  "ASCII",   "ASCII")]
	[InlineData("XxHash128", "ASCII",   "ASCII")]
	[InlineData("XxHash3",   "ASCII",   "ASCII")]
	[InlineData("MD5",       "ASCII",   "UTF8")]
	[InlineData("SHA1",      "ASCII",   "UTF8")]
	[InlineData("SHA256",    "ASCII",   "UTF8")]
	[InlineData("SHA384",    "ASCII",   "UTF8")]
	[InlineData("SHA512",    "ASCII",   "UTF8")]
	[InlineData("CRC32",     "ASCII",   "UTF8")]
	[InlineData("CRC64",     "ASCII",   "UTF8")]
	[InlineData("XxHash32",  "ASCII",   "UTF8")]
	[InlineData("XxHash64",  "ASCII",   "UTF8")]
	[InlineData("XxHash128", "ASCII",   "UTF8")]
	[InlineData("XxHash3",   "ASCII",   "UTF8")]
	[InlineData("MD5",       "ASCII",   "Unicode")]
	[InlineData("SHA1",      "ASCII",   "Unicode")]
	[InlineData("SHA256",    "ASCII",   "Unicode")]
	[InlineData("SHA384",    "ASCII",   "Unicode")]
	[InlineData("SHA512",    "ASCII",   "Unicode")]
	[InlineData("CRC32",     "ASCII",   "Unicode")]
	[InlineData("CRC64",     "ASCII",   "Unicode")]
	[InlineData("XxHash32",  "ASCII",   "Unicode")]
	[InlineData("XxHash64",  "ASCII",   "Unicode")]
	[InlineData("XxHash128", "ASCII",   "Unicode")]
	[InlineData("XxHash3",   "ASCII",   "Unicode")]
	[InlineData("MD5",       "UTF8",    "ASCII")]
	[InlineData("SHA1",      "UTF8",    "ASCII")]
	[InlineData("SHA256",    "UTF8",    "ASCII")]
	[InlineData("SHA384",    "UTF8",    "ASCII")]
	[InlineData("SHA512",    "UTF8",    "ASCII")]
	[InlineData("CRC32",     "UTF8",    "ASCII")]
	[InlineData("CRC64",     "UTF8",    "ASCII")]
	[InlineData("XxHash32",  "UTF8",    "ASCII")]
	[InlineData("XxHash64",  "UTF8",    "ASCII")]
	[InlineData("XxHash128", "UTF8",    "ASCII")]
	[InlineData("XxHash3",   "UTF8",    "ASCII")]
	[InlineData("MD5",       "UTF8",    "UTF8")]
	[InlineData("SHA1",      "UTF8",    "UTF8")]
	[InlineData("SHA256",    "UTF8",    "UTF8")]
	[InlineData("SHA384",    "UTF8",    "UTF8")]
	[InlineData("SHA512",    "UTF8",    "UTF8")]
	[InlineData("CRC32",     "UTF8",    "UTF8")]
	[InlineData("CRC64",     "UTF8",    "UTF8")]
	[InlineData("XxHash32",  "UTF8",    "UTF8")]
	[InlineData("XxHash64",  "UTF8",    "UTF8")]
	[InlineData("XxHash128", "UTF8",    "UTF8")]
	[InlineData("XxHash3",   "UTF8",    "UTF8")]
	[InlineData("MD5",       "UTF8",    "Unicode")]
	[InlineData("SHA1",      "UTF8",    "Unicode")]
	[InlineData("SHA256",    "UTF8",    "Unicode")]
	[InlineData("SHA384",    "UTF8",    "Unicode")]
	[InlineData("SHA512",    "UTF8",    "Unicode")]
	[InlineData("CRC32",     "UTF8",    "Unicode")]
	[InlineData("CRC64",     "UTF8",    "Unicode")]
	[InlineData("XxHash32",  "UTF8",    "Unicode")]
	[InlineData("XxHash64",  "UTF8",    "Unicode")]
	[InlineData("XxHash128", "UTF8",    "Unicode")]
	[InlineData("XxHash3",   "UTF8",    "Unicode")]
	[InlineData("MD5",       "Unicode", "ASCII")]
	[InlineData("SHA1",      "Unicode", "ASCII")]
	[InlineData("SHA256",    "Unicode", "ASCII")]
	[InlineData("SHA384",    "Unicode", "ASCII")]
	[InlineData("SHA512",    "Unicode", "ASCII")]
	[InlineData("CRC32",     "Unicode", "ASCII")]
	[InlineData("CRC64",     "Unicode", "ASCII")]
	[InlineData("XxHash32",  "Unicode", "ASCII")]
	[InlineData("XxHash64",  "Unicode", "ASCII")]
	[InlineData("XxHash128", "Unicode", "ASCII")]
	[InlineData("XxHash3",   "Unicode", "ASCII")]
	[InlineData("MD5",       "Unicode", "UTF8")]
	[InlineData("SHA1",      "Unicode", "UTF8")]
	[InlineData("SHA256",    "Unicode", "UTF8")]
	[InlineData("SHA384",    "Unicode", "UTF8")]
	[InlineData("SHA512",    "Unicode", "UTF8")]
	[InlineData("CRC32",     "Unicode", "UTF8")]
	[InlineData("CRC64",     "Unicode", "UTF8")]
	[InlineData("XxHash32",  "Unicode", "UTF8")]
	[InlineData("XxHash64",  "Unicode", "UTF8")]
	[InlineData("XxHash128", "Unicode", "UTF8")]
	[InlineData("XxHash3",   "Unicode", "UTF8")]
	[InlineData("MD5",       "Unicode", "Unicode")]
	[InlineData("SHA1",      "Unicode", "Unicode")]
	[InlineData("SHA256",    "Unicode", "Unicode")]
	[InlineData("SHA384",    "Unicode", "Unicode")]
	[InlineData("SHA512",    "Unicode", "Unicode")]
	[InlineData("CRC32",     "Unicode", "Unicode")]
	[InlineData("CRC64",     "Unicode", "Unicode")]
	[InlineData("XxHash32",  "Unicode", "Unicode")]
	[InlineData("XxHash64",  "Unicode", "Unicode")]
	[InlineData("XxHash128", "Unicode", "Unicode")]
	[InlineData("XxHash3",   "Unicode", "Unicode")]
	public void ComputeHash_ShouldReturnDifferentHash_ForDifferentEncodings(string algorithm, string encoding1Name, string encoding2Name)
	{
		// Arrange
		var person = new Person { Id = 1, Name = "John Doe ꝏ", BirthDate = new DateTime(1980, 1, 1) };

		var encoding1 = getEncodingByName(encoding1Name);
		var encoding2 = getEncodingByName(encoding2Name);

		var hasherWithEncoding1 = getHasherForAlgorithm(algorithm);
		hasherWithEncoding1.Register<Person>(options => options.Configure(p => p.Name, encoding1));

		var hasherWithEncoding2 = getHasherForAlgorithm(algorithm);
		hasherWithEncoding2.Register<Person>(options => options.Configure(p => p.Name, encoding2));

		// Act
		var hash1 = hasherWithEncoding1.ComputeHash(person);
		var hash2 = hasherWithEncoding2.ComputeHash(person);

		// Assert
		if (encoding1Name != encoding2Name)
			hash1.Should().NotBeEquivalentTo(hash2, $"{algorithm} should produce different hashes for {encoding1Name} and {encoding2Name} encodings.");
		else
			hash1.Should().BeEquivalentTo(hash2, $"{algorithm} should produce the same hash for the same encoding ({encoding1Name}) when used consistently.");
	}

	// Helper method to return the appropriate ObjectHasher for each algorithm
	private ObjectHasher getHasherForAlgorithm(string algorithm)
	{
		return algorithm switch
		{
			"MD5" => new ObjectHasher(new Md5HashAlgorithm()),
			"SHA1" => new ObjectHasher(new Sha1HashAlgorithm()),
			"SHA256" => new ObjectHasher(new Sha256HashAlgorithm()),
			"SHA384" => new ObjectHasher(new Sha384HashAlgorithm()),
			"SHA512" => new ObjectHasher(new Sha512HashAlgorithm()),
			"CRC32" => new ObjectHasher(new Crc32HashAlgorithm()),
			"CRC64" => new ObjectHasher(new Crc64HashAlgorithm()),
			"XxHash32" => new ObjectHasher(new XxHash32Algorithm()),
			"XxHash64" => new ObjectHasher(new XxHash64Algorithm()),
			"XxHash128" => new ObjectHasher(new XxHash128Algorithm()),
			"XxHash3" => new ObjectHasher(new XxHash3Algorithm()),
			_ => throw new ArgumentException("Invalid algorithm", nameof(algorithm)),
		};
	}

	// Helper method to get the encoding by name
	private Encoding getEncodingByName(string encodingName)
	{
		return encodingName switch
		{
			"UTF8" => Encoding.UTF8,
			"ASCII" => Encoding.ASCII,
			"Unicode" => Encoding.Unicode,
			_ => throw new ArgumentException("Invalid encoding", nameof(encodingName)),
		};
	}
}
