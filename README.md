# ObjectHasher

## Introduction
The **ObjectHash** library provides an easy and flexible way to compute cryptographic hashes for objects using custom configurations. It allows developers to register type-specific configurations to control how object properties are handled during hashing. The library works with any hash algorithm that implements the `IHashAlgorithm` interface.

## Prerequisites
This package targets .NET 8.0, to include the latest hashers and improvements.

## Installation
Use NuGet to install the package. Use can use the UI, or use the following command in the package manager console:
```
Install-Package ObjectHasher
```

## Contributing
If you want to contribute, please create a pull request. I will review it as soon as possible.
Use visual studio 2022 version 17.8 or later to build this library. The main library and the tests use .NET 8.0.

## Author
This library was created by Jan Geert Hek, a software developer from the Netherlands. You can find more information about me on my [LinkedIn](https://www.linkedin.com/in/jghek/) page.

## License
This library is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

# The manual
The **ObjectHash** library provides an easy and flexible way to compute cryptographic hashes for objects using custom configurations. It allows developers to register type-specific configurations to control how object properties are handled during hashing. The library works with any hash algorithm that implements the `IHashAlgorithm` interface.

## Features
- **Type-Specific Configuration**: You can register configurations for specific types, control which properties or fields to include/exclude, and specify custom encodings for string properties.
- **Supports Complex Types**: The library supports various types including primitive types, collections, strings, and user-defined objects.
- **Customizable Hash Algorithms**: You can use any hashing algorithm that implements the `IHashAlgorithm` interface.

## Use Cases

### 1. Basic Hashing for Objects
You can compute the hash for an object with the default behavior, which includes all public properties and fields.

### 2. Custom Type Configuration
You can register custom configurations for object types to ignore certain properties or customize the encoding of string properties.

### 3. Use of Different Hash Algorithms
The library is algorithm-agnostic and can work with different hash algorithms as long as they implement the `IHashAlgorithm` interface.

## Examples

### 1. Compute a Basic Hash for an Object

```csharp
public class User
{
    public string Username { get; set; }
    public int Age { get; set; }
}

var sha256 = new Sha256HashAlgorithm(); // Implement IHashAlgorithm with SHA-256.
var objectHasher = new ObjectHash(sha256);

var user = new User { Username = "JohnDoe", Age = 30 };
byte[] hash = objectHasher.ComputeHash(user);

Console.WriteLine(BitConverter.ToString(hash));
```

### 2. Register Custom Configuration for a Type

In this example, we register a configuration for the `User` class to ignore the `Age` property and apply custom encoding to the `Username` property.

```csharp
objectHasher.Register<User>(config =>
{
    config.Ignore(u => u.Age)                  // Ignore the Age property.
          .Configure(u => u.Username, Encoding.UTF8); // Use UTF-8 encoding for Username.
});

byte[] customHash = objectHasher.ComputeHash(user);

Console.WriteLine(BitConverter.ToString(customHash));
```

### 3. Static Hashing with a Custom Algorithm

You can also compute a hash statically by providing an object and a hash algorithm.

```csharp
var hash = ObjectHash.ComputeHash(user, sha256);
Console.WriteLine(BitConverter.ToString(hash));
```

### 4. Hashing Collections

The library supports hashing of collections (e.g., arrays, lists). It will iterate through each element and compute the combined hash.

```csharp
public class Order
{
    public string OrderId { get; set; }
    public List<string> Items { get; set; }
}

var order = new Order { OrderId = "12345", Items = new List<string> { "Apple", "Banana", "Orange" } };
byte[] orderHash = objectHasher.ComputeHash(order);

Console.WriteLine(BitConverter.ToString(orderHash));
```

---

## How It Works

### Type Configuration

When you register a type using the `Register<T>()` method, you provide an action that configures which properties should be hashed, ignored, or have custom encoding. The configuration is stored internally and applied whenever that type is hashed.

### Hash Calculation

The `ComputeHash()` method uses reflection to dynamically inspect the properties and fields of the object. It recursively processes complex types and collections, applying the registered configurations where applicable.

### Supported Types

- **Primitive types**: `int`, `long`, `float`, `double`, `bool`, etc.
- **Special types**: `decimal`, `DateTime`, `Guid`
- **Collections**: `IEnumerable` types (e.g., arrays, lists)
- **Complex types**: Custom objects with properties and fields

## Extending with Custom Hash Algorithms

You can extend the functionality by providing your own hash algorithm. Implement the `IHashAlgorithm` interface for custom behavior. As an example, here's the MD5 algorithm: (it is already implemented in the library))

```csharp
public class Md5HashAlgorithm : IHashAlgorithm
{
    private MD5 _md5 = MD5.Create();
    private MemoryStream _stream = new MemoryStream();

    public void Append(ReadOnlySpan<byte> data)
    {
        _stream.Write(data.ToArray(), 0, data.Length);
    }

    public byte[] GetHashAndReset()
    {
        byte[] hash = _md5.ComputeHash(_stream.ToArray());
        _stream.SetLength(0);  // Reset stream
        return hash;
    }

    public void Reset()
    {
        _stream.SetLength(0);
    }
}
```

You can then use this custom algorithm with the `ObjectHash` class:

```csharp
var md5Algorithm = new Md5HashAlgorithm();
var md5Hasher = new ObjectHash(md5Algorithm);

byte[] hash = md5Hasher.ComputeHash(user);
Console.WriteLine(BitConverter.ToString(hash));
```

## Credits

The icon used has been designed by Flaticon.com