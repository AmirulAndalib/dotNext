Benchmarks
====
Microbenchmarks are important part of DotNext library to prove than important features can speed up performance of your application or, at least, is not slowing down it.

The configuration of all benchmarks:

| Parameter | Configuration |
| ---- | ---- |
| Host | .NET Core 3.1.9 (CoreCLR 4.700.20.47201, CoreFX 4.700.20.47203), X64 RyuJIT |
| Job | .NET Core 3.1.9 (CoreCLR 4.700.20.47201, CoreFX 4.700.20.47203), X64 RyuJIT |
| LaunchCount | 1 |
| RunStrategy | Throughput |
| OS | Ubuntu 20.04.1 |
| CPU | Intel Core i7-6700HQ CPU 2.60GHz (Skylake) |
| Number of CPUs | 1 |
| Physical Cores | 4 |
| Logical Cores | 8 |
| RAM | 24 GB |

You can run benchmarks using `Bench` build configuration as follows:
```bash
cd <dotnext-clone-path>/src/DotNext.Benchmarks
dotnet run -c Bench
```

# Bitwise Equality
[This benchmark](https://github.com/sakno/DotNext/blob/master/src/DotNext.Benchmarks/BitwiseEqualityBenchmark.cs) compares performance of [BitwiseComparer&lt;T&gt;.Equals](./api/DotNext.BitwiseComparer-1.yml) with overloaded equality `==` operator. Testing data types: [Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid) and custom value type with multiple fields.

| Method | Mean | Error | StdDev | Median |
| ---- | ---- | ---- | ---- | ---- |
| `BitwiseComparer<Guid>.Equals` | 4.0946 ns | 0.1103 ns | 0.1546 ns | 4.0572 ns |
| `Guid.Equals` | 2.6408 ns | 0.0679 ns | 0.0567 ns | 2.6404 ns |
| `ReadOnlySpan.SequenceEqual` for `Guid` | 4.7230 ns | 0.1173 ns | 0.1992 ns | 4.6229 ns |
| `BitwiseComparer<LargeStruct>.Equals` | 20.8664 ns | 0.5215 ns | 1.4537 ns |    21.0593 ns |
| `LargeStruct.Equals` | 44.2600 ns | 0.8155 ns | 0.7230 ns | 44.4745 ns |
| `ReadOnlySpan.SequenceEqual` for `LargeStruct` | 24.2414 ns | 0.3574 ns | 0.2984 ns | 24.2423 ns |

Bitwise equality method has the better performance than field-by-field equality check especially for large value types because `BitwiseEquals` utilizes low-level optimizations performed by .NET Core according with underlying hardware such as SIMD. Additionally, it uses [aligned memory access](https://en.wikipedia.org/wiki/Data_structure_alignment) in constrast to [SequenceEqual](https://docs.microsoft.com/en-us/dotnet/api/system.memoryextensions.sequenceequal) method.

# Equality of Arrays
[This benchmark](https://github.com/sakno/DotNext/blob/master/src/DotNext.Benchmarks/ArrayEqualityBenchmark.cs) compares performance of [ReadOnlySpan.SequenceEqual](https://docs.microsoft.com/en-us/dotnet/api/system.memoryextensions.sequenceequal#System_MemoryExtensions_SequenceEqual__1_System_ReadOnlySpan___0__System_ReadOnlySpan___0__), [OneDimensionalArray.BitwiseEquals](./api/DotNext.OneDimensionalArray.yml) and manual equality check between two arrays using `for` loop. The benchmark is applied to the array of [Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid) elements.

| Method | Mean | Error | StdDev | Median  |
| ---- | ---- | ---- | ---- | ---- |
| `Guid[].BitwiseEquals`, small arrays (~10 elements) | 10.13 ns | 0.083 ns | 0.077 ns | 10.15 ns |
| `ReadOnlySpan<Guid>.SequenceEqual`, small arrays (~10 elements) | 46.68 ns | 0.200 ns | 0.187 ns | 46.69 ns |
| `for` loop, small arrays (~10 elements) | 61.23 ns | 0.144 ns | 0.127 ns | 61.27 ns |
| `Guid[].BitwiseEquals`, large arrays (~100 elements) | 52.30 ns | 0.130 ns | 0.121 ns | 52.25 ns |
| `ReadOnlySpan<Guid>.SequenceEqual`, large arrays (~100 elements) | 437.96 ns | 8.669 ns | 8.109 ns | 444.44 ns |
| `for` loop, large arrays (~100 elements) | 607.54 ns | 2.572 ns | 2.406 ns | 607.48 ns |

Bitwise equality is an absolute winner for equality check between arrays of any size.

# Bitwise Hash Code
[This benchmark](https://github.com/sakno/DotNext/blob/master/src/DotNext.Benchmarks/BitwiseHashCodeBenchmark.cs) compares performance of [BitwiseComparer&lt;T&gt;.GetHashCode](./api/DotNext.BitwiseComparer-1.yml) and `GetHashCode` instance method for the types [Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid) and custom value type with multiple fields.

| Method | Mean | Error | StdDev |
| ---- | ---- | ---- | ---- |
| `Guid.GetHashCode` | 1.446 ns | 0.0137 ns | 0.0128 ns |
| `BitwiseComparer<Guid>.GetHashCode` | 5.646 ns | 0.0256 ns | 0.0214 ns |
| `BitwiseComparer<LargeStructure>.GetHashCode` | 40.257 ns | 0.1833 ns | 0.1714 ns |
| `LargeStructure.GetHashCode` | 33.694 ns | 0.7107 ns | 1.5749 ns |

Bitwise hash code algorithm is slower than JIT optimizations introduced by .NET Core 3.1 but still convenient in complex cases.

# Bytes to Hex
[This benchmark](https://github.com/sakno/DotNext/blob/master/src/DotNext.Benchmarks/HexConversionBenchmark.cs) demonstrates performance of `DotNext.Span.ToHex` extension method that allows to convert arbitrary set of bytes into hexadecimal form. It is compatible with`Span<T>` data type in constrast to [BitConverter.ToString](https://docs.microsoft.com/en-us/dotnet/api/system.bitconverter.tostring) method.

| Method | Num of Bytes | Mean | Error | StdDev |
| ---- | ---- | ---- | ---- | ---- |
| `BitConverter.ToString` | 16 bytes |  66.22 ns | 0.481 ns | 0.427 ns |
| `Span.ToHex` | 16 bytes | 78.10 ns | 0.583 ns | 0.487 ns |
| `BitConverter.ToString` | 64 bytes | 219.96 ns | 1.742 ns | 1.454 ns |
| `Span.ToHex` | 64 bytes | 158.60 ns | 0.966 ns | 0.904 ns |
| `BitConverter.ToString` | 128 bytes | 447.40 ns |  3.989 ns | 3.331 ns |
| `Span.ToHex` | 128 bytes | 258.58 ns | 1.359 ns | 1.205 ns |
| `BitConverter.ToString` | 256 bytes | 838.54 ns | 12.750 ns | 9.955 ns |
| `Span.ToHex` | 256 bytes | 496.05 ns |  3.077 ns | 2.402 ns |

`Span.ToHex` demonstrates the best performance especially for large arrays.

# Fast Reflection
The next series of benchmarks demonstrate performance of strongly typed reflection provided by DotNext Reflection library.

## Property Getter
[This benchmark](https://github.com/sakno/DotNext/blob/master/src/DotNext.Benchmarks/Reflection/PropertyGetterReflectionBenchmark.cs) demonstrates overhead of getting instance property value caused by different mechanisms:
1. Using [FastMember](https://github.com/mgravell/fast-member) library
1. Using strongly typed reflection from DotNext Reflection library: `Type<IndexOfCalculator>.Property<int>.RequireGetter`
1. Using strongly typed reflection from DotNext Reflection library using special delegate type `Function<object, ValueTuple, object>`. It is assumed that instance type and property type is not known at compile type (th) so the delegate performs type check on every call.
1. Classic .NET reflection

| Method | Mean | Error | StdDev |
| ---- | ---- | ---- | ---- |
| Direct call | 11.04 ns | 0.056 ns | 0.049 ns |
| Reflection with DotNext using delegate type `MemberGetter<IndexOfCalculator, int>` | 12.74 ns | 0.150 ns | 0.133 ns |
| Reflection with DotNext using `DynamicInvoker` | 21.35 ns | 0.083 ns | 0.073 ns |
| Reflection with DotNext using delegate type `Function<object, ValueTuple, object>` | 22.47 ns | 0.067 ns | 0.060 ns |
| `ObjectAccess` class from _FastMember_ library | 44.78 ns | 0.116 ns | 0.109 ns |
| .NET reflection | 169.14 ns | 0.466 ns | 0.413 ns |

Strongly typed reflection provided by DotNext Reflection library has the same performance as direct call.

## Instance Method Call
[This benchmark](https://github.com/sakno/DotNext/blob/master/src/DotNext.Benchmarks/Reflection/StringMethodReflectionBenchmark.cs) demonstrates overhead of calling instance method `IndexOf` of type **string** caused by different mechanisms:
1. Using strongly typed reflection from DotNext Reflection library: `Type<string>.Method<char, int>.Require<int>(nameof(string.IndexOf))`
1. Using strongly typed reflection from DotNext Reflection library using special delegate type: `Type<string>.RequireMethod<(char, int), int>(nameof(string.IndexOf));`
1. Using strongly typed reflection from DotNext Reflection library using special delegate type: `Function<object, (object, object), object>`. It is assumed that types of all parameters are not known at compile time.
1. Classic .NET reflection

The benchmark uses series of different strings to run the same set of tests. Worst case means that character lookup is performed for a string that doesn't contain the given character. Best case means that character lookup is performed for a string that has the given character.

| Method | Condition | Mean | Error | StdDev | Median |
| ---- | ---- | ---- | ---- | ---- | ---- |
| Direct call | Empty String | 5.027 ns | 0.0192 ns | 0.0179 ns | 5.029 ns |
| Direct call | Best Case | 11.265 ns | 0.2595 ns | 0.5473 ns | 11.038 ns |
| Direct call | Worst Case | 13.702 ns | 0.0381 ns | 0.0357 ns | 13.701 ns |
| Reflection with DotNext using delegate type `Func<string, char, int, int>` | Empty String | 8.292 ns | 0.1272 ns | 0.1062 ns | 8.292 ns |
| Reflection with DotNext using delegate type `Func<string, char, int, int>` | Best Case | 10.786 ns | 0.0263 ns |  0.0233 ns |  10.787 ns |
| Reflection with DotNext using delegate type `Func<string, char, int, int>` | Worst Case | 16.156 ns | 0.0532 ns | 0.0445 ns | 16.164 ns |
| Reflection with DotNext using delegate type `Function<string, (char, int), int>` | Empty String | 12.920 ns | 0.0802 ns | 0.0711 ns | 12.912 ns |
| Reflection with DotNext using delegate type `Function<string, (char, int), int>` | Best Case | 16.764 ns | 0.3341 ns | 0.6357 ns | 16.547 ns |
| Reflection with DotNext using delegate type `Function<string, (char, int), int>` | Worst Case | 19.723 ns | 0.0467 ns | 0.0437 ns | 19.718 ns |
| Reflection with DotNext using delegate type `Function<object, (object, object), object>` | Empty String | 30.837 ns | 0.9323 ns | 2.6749 ns | 29.444 ns |
| Reflection with DotNext using delegate type `Function<object, (object, object), object>` | Best Case | 34.313 ns | 0.6684 ns | 1.1168 ns | 34.725 ns |
| Reflection with DotNext using delegate type `Function<object, (object, object), object>` | Worst Case | 38.278 ns | 0.2308 ns | 0.2159 ns | 38.261 ns |
| .NET reflection | Empty String | 330.086 ns | 6.4941 ns | 10.4867 ns | 325.026 ns |
| .NET reflection | Best Case | 332.212 ns | 6.6117 ns | 11.5798 ns | 326.741 ns |
| .NET reflection | Worst Case | 339.153 ns | 2.0429 ns | 1.7059 ns | 339.822 ns |

DotNext Reflection library offers the best result in case when delegate type exactly matches to the reflected method with small overhead measured in a few nanoseconds.

## Static Method Call
[This benchmark](https://github.com/sakno/DotNext/blob/master/src/DotNext.Benchmarks/Reflection/TryParseReflectionBenchmark.cs) demonstrates overhead of calling static method `TryParse` of type **decimal** caused by different mechanisms:
1. Using strongly typed reflection from DotNext Reflection library: `Type<decimal>.Method.Get<TryParseDelegate>(nameof(decimal.TryParse), MethodLookup.Static)`. The delegate type exactly matches to the reflected method signature: `delegate bool TryParseDelegate(string text, out decimal result)`
1. Using strongly typed reflection from DotNext Reflection library using special delegate type: `Function<(string text, decimal result), bool>`
1. Using strongly typed reflection from DotNext Reflection library using special delegate type: `Function<(object text, object result), object>`. It is assumed that types of all parameters are not known at compile time.
1. Classic .NET reflection

| Method | Mean | Error | StdDev | Median |
| ---- | ---- | ---- | ---- | ---- |
| Direct call | 127.5 ns | 0.74 ns | 0.65 ns | 127.2 ns |
| Reflection with DotNext using delegate type `TryParseDelegate` | 127.4 ns | 0.41 ns | 0.36 ns | 127.1 ns |
| Reflection with DotNext using delegate type `Function<(string text, decimal result), bool>` | 142.3 ns | 0.50 ns | 0.42 ns | 142.9 ns |
| Reflection with DotNext using delegate type `Function<(object text, object result), object>` | 154.8 ns | 2.70 ns | 2.40 ns | 155.1 ns |
| .NET reflection | 516.0 ns | 4.61 ns | 4.09 ns | 514.13 ns |

Strongly typed reflection provided by DotNext Reflection library has the same performance as direct call.

# Atomic Access to Arbitrary Value Type
[This benchmark](https://github.com/sakno/DotNext/blob/master/src/DotNext.Benchmarks/Threading/AtomicContainerBenchmark.cs) compares performance of [Atomic&lt;T&gt;](./api/DotNext.Threading.Atomic-1.yml) and Synchronized methods. The implementation of the benchmark contains concurrent read/write threads to ensure that lock contention is in place.

| Method | Mean | Error | StdDev | Median |
| ---- | ---- | ---- | ---- | ---- |
| Atomic | 352.8 us | 10.00 us | 93.91 us | 341.1 us |
| Synchronized | 993.8 us | 11.41 us | 104.88 us | 982.4 us |
| SpinLock | 1,539.2 us | 38.05 us | 337.18 us | 1,603.6 us |

# Value Delegate
[This benchmark](https://github.com/sakno/DotNext/blob/master/src/DotNext.Benchmarks/FunctionPointerBenchmark.cs) compares performance of indirect method call using classic delegates from .NET and [value delegates](./features/core/valued.md).

| Method | Mean | Error | StdDev |
| ---- | ---- | ---- | ---- |
| Instance method, regular delegate, has implicit **this** | 0.9273 ns | 0.0072 ns | 0.0060 ns |
| Instance method, Value Delegate, has implicit **this** | 1.8824 ns | 0.0495 ns | 0.0463 ns |
| Static method, regular delegate, large size of param type, no implicitly captured object | 14.5560 ns | 0.0440 ns | 0.0367 ns |
| Static method, Value Delegate, large size of param type, no implicitly captured object | 15.7549 ns | 0.0731 ns | 0.0684 ns |
| Static method, regular delegate, small size of param type, no implicitly captured object | 23.2037 ns | 0.3844 ns | 0.3408 ns |
| Static method, Value Delegate, small size of param type, no implicitly captured object | 21.8213 ns | 0.1073 ns | 0.0896 ns |

_Large size of param type_ means that the type of the parameter is larger than 64 bit.

Interpretation of benchmark results:
* _Proxy_ mode of Value Delegate adds a small overhead in comparison with regular delegate
* If the type of the parameter is less than or equal to the size of CPU register then Value Delegate offers the best performance
* If the type of the parameter is greater than the size of CPU register then Value Delegate is slower than regular delegate

# File-buffering Writer
[This benchmark](https://github.com/sakno/dotNext/blob/master/src/DotNext.Benchmarks/IO/FileBufferingWriterBenchmark.cs) compares performance of [FileBufferingWriteStream](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.webutilities.filebufferingwritestream) from ASP.NET Core and [FileBufferingWriter](./api/DotNext.IO.FileBufferingWriter.yml) from .NEXT library.

Both classes switching from in-memory buffer to file-based buffer during benchmark execution. Note that benchmark result highly depends on disk I/O performance. The following results were obtained using NVMe SSD.

| Method | Mean | Error | StdDev |
| ---- | ---- | ---- | ---- |
| `FileBufferingWriter` in synchronous mode | 1.001 ms | 0.0111 ms | 0.0104 ms |
| `FileBufferingWriteStream` in synchronous mode | 26.690 ms | 1.4974 ms | 4.4151 ms |
| `FileBufferingWriter` in asynchronous mode | 8.947 ms | 0.2014 ms | 0.5412 ms |
| `FileBufferingWriteStream` in asynchronous mode | 19.300 ms | 1.2528 ms | 3.6546 ms |

`FileBufferingWriter` is a winner in synchronous scenario because it has native support for synchronous mode in contrast to `FileBufferingWriteStream`.
