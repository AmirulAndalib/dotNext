Alloc vs Pooling
=====
.NET allows to rent a memory instead of allocation using **new** keyword. It is useful in many cases especially when you need a large block of memory or large array. There a many articles describing benefits of this approach.
* [Pooling large arrays with ArrayPool](https://adamsitnik.com/Array-Pool/)
* [Avoid GC Pressure](https://michaelscodingspot.com/avoid-gc-pressure/)

The memory can be rented using [ArrayPool](https://docs.microsoft.com/en-us/dotnet/api/system.buffers.arraypool-1) or [MemoryPool](https://docs.microsoft.com/en-us/dotnet/api/system.buffers.memorypool-1) but their direct usage has several inconveniences:
* Not possible to use **using** statement to return rented array back to the pool in case of `ArrayPool<T>`
* It's hard to mix the code when rental is optional. For instance, in case of small block of memory you can use **stackalloc** instead of renting memory
* The returned memory or array can have larger size so you need to control bounds by yourself

.NEXT offers convenient wrappers that simplify the rental process and handle situations when renting is optional:
* [ArrayRental&lt;T&gt;](https://sakno.github.io/dotNext/api/DotNext.Buffers.ArrayRental-1.html) if you need to work with arrays
* [MemoryRental&lt;T&gt;](https://sakno.github.io/dotNext/api/DotNext.Buffers.ArrayRental-1.html) if you need to work with [Span&lt;T&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.span-1)

# ArrayRental
[ArrayRental&lt;T&gt;](https://sakno.github.io/dotNext/api/DotNext.Buffers.ArrayRental-1.html) allows to rent the array using array pool and supports **using** statement.
```csharp
using DotNext.Buffers;

using var array = new ArrayRental<byte>(10);
Memory<byte> mem = array.Memory;
Span<byte> span = array.Span;
ArraySegment<byte> segment = array.Segment;

//the code is equivalent to
using System.Buffers;

var array = ArrayPool<byte>.Shared.Rent(10);
try
{
}
finally
{
  Array<byte>.Shared.Return(array);
}
```
`ArrayRental` provides several accessor to the rented array using `Memory`, `Span` and `Segment` properties. All these properties return the representation of the rented array with exact size that was initially requested.

The type supports custom array pool that can be passed to the constructor. In some advanced scenarios, you may have already allocated array so you don't to rent a new one from the pool. It is possible to pass such array as an argument of `ArrayRental` constructor.

# MemoryRental
[MemoryRental&lt;T&gt;](https://sakno.github.io/dotNext/api/DotNext.Buffers.ArrayRental-1.html) is more specialized version of `ArrayRental` which is useful in hybrid scenarios when renting can be replaced with stack allocation. This type is **ref**-like value type so it cannot be stored in fields or used inside of **async** methods. The rented memory is only accessible using [Span&lt;T&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.span-1) data type.

The following example demonstrates how to reverse a string and choose temporary buffers allocation method depending on the size of the string.
```csharp
using DotNext.Buffers;

public static unsafe string Reverse(this string str)
{
  if (str.Length == 0) return str;
  using MemoryRental<char> result = str.Length <= 1024 ? stackalloc char[str.Length] : new MemoryRental<char>(str.Length);
  str.AsSpan().CopyTo(result.Span);
  result.Span.Reverse();
  fixed (char* ptr = result.Span)
    return new string(ptr, 0, result.Length);
} 
```
In constrast to `ArrayRental<T>`, this type uses [MemoryPool](https://docs.microsoft.com/en-us/dotnet/api/system.buffers.memorypool-1). It is possible to pass custom memory pool the constructor.

The type is typically used in unsafe context when you need a temporary buffer to perform in-memory transformations. If you don't have intentions to use **stackalloc** then choose `ArrayRental<T>` instead.

# MemoryOwner
.NET offers two different models for memory pooling: [MemoryPool&lt;T&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.buffers.memorypool-1) class and [ArrayPool&lt;T&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.buffers.arraypool-1) class. Both are abstract classes so it's not possible to unify memory pooling API. For instance, [configuration model](https://docs.microsoft.com/en-us/dotnet/api/system.io.pipes.pipeoptions) for I/O pipe from .NET expecting `MemoryPool<T>` instance. If you want to use custom `ArrayPool<T>` then you need to write wrapper for it.

.NEXT library contains [MemoryOwner&lt;T&gt;](https://sakno.github.io/dotNext/api/DotNext.Buffers.MemoryOwner-1.html) value type that represents rented memory regardless of the rental method:
```csharp
using DotNext.Buffers;
using System.Buffers;

using var rentedArray = new MemoryOwner<byte>(ArrayPool<byte>.Shared, 10);
using var rentedMemory = new MemoryOwner<byte>(MemoryPool<byte>.Shared, 10);
rentedArray.Memory.Slice(0, 5);
rentedMemory.Memory.Slice(0, 5);
```
The value type implements [IMemoryOwner&lt;T&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.buffers.imemoryowner-1) interface so you can easly access pooled memory in a uniform way.

However, `MemoryOwner` provides subset of functionality available in `ArrayRental` and `MemoryRental` which are corner cases and specialized for particular pooling mechanism.

Additionally, .NEXT offers special abstraction layer for memory pooling which is compatible with existing mechanisms in .NET. [MemoryAllocator&lt;T&gt;](https://sakno.github.io/dotNext/api/DotNext.Buffers.MemoryAllocator-1.html) delegate represents universal way to rent the memory. The consumer of your library can supply concrete instance of this delegate to supply appropriate allocation mechanism. [MemoryAllocator](https://sakno.github.io/dotNext/api/DotNext.Buffers.MemoryAllocator.html) static class provides extension methods for interop between memory allocator and existing .NET memory pooling APIs.

# RentedMemoryStream
Another way to represent the rented memory is [RentedMemoryStream](https://sakno.github.io/dotNext/api/DotNext.IO.RentedMemoryStream.html) that allows to work with pooled memory in [stream](https://docs.microsoft.com/en-us/dotnet/api/system.io.stream)-like manner. The stream is available for read/write operations. However, it's capacity is fixed and cannot grow. Therefore, you need to request sufficient capacity.
```csharp
using DotNext.IO;

using var stream = new RentedMemoryStream(1024);  //rent 1 KB of memory and wrap it to stream
stream.Write(new byte[512], 0, 512);
```

# Growable Buffers
If size of the required buffer is not known and can grow dynamically then you need to use [Dynamic Buffers](./buffers.md) that are based on memory pooling mechanism as well.

Dynamic buffers can be combined with [streams](https://docs.microsoft.com/en-us/dotnet/api/system.io.stream) easily using extension methods from [StreamSource](https://sakno.github.io/dotNext/api/DotNext.IO.StreamSource.html) class, so you can avoid limitation of `RentedMemoryStream` class. With [PooledArrayBufferWriter&lt;T&gt;](https://sakno.github.io/dotNext/api/DotNext.Buffers.PooledArrayBufferWriter-1.html) class, it's possible to read/write bytes using stream and utilize memory pooling:
```csharp
using DotNext.Buffers;
using DotNext.IO;

using var writer = new PooledBufferWriter<byte>(ArrayPool<byte>.Shared);

// write bytes using stream
using Stream writeStream = StreamSource.AsStream(writer);
writeStream.Write(new byte[1024]);

// read bytes using stream
using Stream readStream = StreamSource.GetWrittenBytesAsStream(writer);
```