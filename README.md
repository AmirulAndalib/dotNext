.NEXT
====
[![Build Status](https://dev.azure.com/rvsakno/dotNext/_apis/build/status/sakno.dotNext?branchName=master)](https://dev.azure.com/rvsakno/dotNext/_build/latest?definitionId=1&branchName=master)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/sakno/dotNext/blob/master/LICENSE)
![Test Coverage](https://img.shields.io/azure-devops/coverage/rvsakno/dotnext/1/master)
[![Join the chat](https://badges.gitter.im/dot_next/community.svg)](https://gitter.im/dot_next/community)

.NEXT (dotNext) is a set of powerful libraries aimed to improve development productivity and extend .NET API with unique features. Some of these features are planned in future releases of .NET platform but already implemented in the library:

| Proposal | Implementation |
| ---- | ---- |
| [Static Delegates](https://github.com/dotnet/csharplang/blob/master/proposals/static-delegates.md) | [Value Delegates](https://sakno.github.io/dotNext/features/core/valued.html) |
| [Operators for IntPtr and UIntPtr](https://github.com/dotnet/corefx/issues/32775) | [Extension methods](https://sakno.github.io/dotNext/api/DotNext.ValueTypeExtensions.html) for arithmetic, bitwise and comparison operations |
| [Enum API](https://github.com/dotnet/corefx/issues/34077) | [Documentation](https://sakno.github.io/dotNext/features/core/enum.html) |
| [Check if an instance of T is a default(T)](https://github.com/dotnet/corefx/issues/16209) | [IsDefault() method](https://sakno.github.io/dotNext/api/DotNext.Runtime.Intrinsics.html#DotNext_Runtime_Intrinsics_IsDefault__1___0_) |
| [Concept Types](https://github.com/dotnet/csharplang/issues/110) | [Documentation](https://sakno.github.io/dotNext/features/concept.html) |
| [Expression Trees covering additional language constructs](https://github.com/dotnet/csharplang/issues/158), i.e. `foreach`, `await`, patterns, multi-line lambda expressions | [Metaprogramming](https://sakno.github.io/dotNext/features/metaprogramming/index.html) |
| [Async Locks](https://github.com/dotnet/corefx/issues/34073) | [Documentation](https://sakno.github.io/dotNext/features/threading/index.html) |
| [High-performance general purpose Write-Ahead Log](https://github.com/dotnet/corefx/issues/25034) | [Persistent Log](https://sakno.github.io/dotNext/features/cluster/wal.html)  |
| [Memory-mapped file as Memory&lt;byte&gt;](https://github.com/dotnet/runtime/issues/37227) | [MemoryMappedFileExtensions](https://sakno.github.io/dotNext/features/io/mmfile.html) |
| [Memory-mapped file as ReadOnlySequence&lt;byte&gt;](https://github.com/dotnet/runtime/issues/24805) | [ReadOnlySequenceAccessor](https://sakno.github.io/dotNext/api/DotNext.IO.MemoryMappedFiles.ReadOnlySequenceAccessor.html) |

Quick overview of additional features:

* [Attachment of user data to arbitrary objects](https://sakno.github.io/dotNext/features/core/userdata.html)
* [Automatic generation of Equals/GetHashCode](https://sakno.github.io/dotNext/features/core/autoeh.html) for arbitrary type at runtime which is much better that Visual Studio compile-time helper for generating these methods
* Extended set of [atomic operations](https://sakno.github.io/dotNext/features/core/atomic.html). Inspired by [AtomicInteger](https://docs.oracle.com/javase/10/docs/api/java/util/concurrent/atomic/AtomicInteger.html) and friends from Java
* [Fast Reflection](https://sakno.github.io/dotNext/features/reflection/fast.html)
* Fast conversion of bytes to hexadecimal representation and vice versa using `ToHex` and `FromHex` methods from [Span](https://sakno.github.io/dotNext/api/DotNext.Span.html) static class
* `ManualResetEvent`, `ReaderWriterLockSlim` and other synchronization primitives now have their [asynchronous versions](https://sakno.github.io/dotNext/features/threading/rwlock.html)
* [Atomic](https://sakno.github.io/dotNext/features/core/atomic.html) memory access operations for arbitrary value types including enums
* [PipeExtensions](https://sakno.github.io/dotNext/api/DotNext.IO.Pipelines.PipeExtensions.html) provides high-level I/O operations for pipelines such as string encoding and decoding
* Fully-featured [Raft implementation](https://github.com/sakno/dotNext/tree/master/src/cluster)

All these things are implemented in 100% managed code on top of existing .NET Standard stack without modifications of Roslyn compiler or CoreFX libraries.

# Quick Links

* [Features](https://sakno.github.io/dotNext/features/core/index.html)
* [API documentation](https://sakno.github.io/dotNext/api/DotNext.html)
* [Benchmarks](https://sakno.github.io/dotNext/benchmarks.html)
* [NuGet Packages](https://www.nuget.org/profiles/rvsakno)

Documentation for older versions:
* [1.x](https://sakno.github.io/dotNext/versions/1.x/index.html)

# What's new
Release Date: 11-11-2020

<a href="https://www.nuget.org/packages/dotnext/2.12.0">DotNext 2.12.0</a>
* Added consuming enumerator for [IProducerConsumerCollection&lt;T&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.concurrent.iproducerconsumercollection-1)
* Introduced `ServiceProviderFactory` class and its factory methods for producing [Service Providers](https://docs.microsoft.com/en-us/dotnet/api/system.iserviceprovider)
* Significant performance improvements of `StringExtensions.Reverse` method
* Introduced a new class `SparseBufferWriter<T>` in addition to existing buffer writes which acts as a growable buffer without memory reallocations
* Updated dependencies

<a href="https://www.nuget.org/packages/dotnext.io/2.12.0">DotNext.IO 2.12.0</a>
* Introduced `TextBufferReader` class inherited from [TextReader](https://docs.microsoft.com/en-us/dotnet/api/system.io.textreader) that can be used to read the text from [ReadOnlySequence&lt;char&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.buffers.readonlysequence-1) or [ReadOnlyMemory&lt;char&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.readonlymemory-1)
* Added `SequenceBuilder<T>` type for building [ReadOnlySequence&lt;T&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.buffers.readonlysequence-1) instances from the chunk of memory blocks
* Added `GetWrittenContentAsStream` and `GetWrittenContentAsStreamAsync` methods to [FileBufferingWriter](https://sakno.github.io/dotNext/api/DotNext.IO.FileBufferingWriter.html) class
* Updated dependencies

<a href="https://www.nuget.org/packages/dotnext.metaprogramming/2.12.0">DotNext.Metaprogramming 2.12.0</a>
* Added support of `await using` statement
* Added support of `await foreach` statement
* Updated dependencies

<a href="https://www.nuget.org/packages/dotnext.reflection/2.12.0">DotNext.Reflection 2.12.0</a>
* More performance optimizations in code generation mechanism responsible for the method or constructor calls
* Added ability to reflect abstract and interface methods
* Added support of volatile access to the field via reflection

<a href="https://www.nuget.org/packages/dotnext.threading/2.12.0">DotNext.Threading 2.12.0</a>
* Added support of `Count` and `CanCount` properties inherited from [ChannelReader&lt;T&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.threading.channels.channelreader-1) by persistent channel reader
* Added support of diagnostics counters for persistent channel
* Fixed resuming of suspended callers in [AsyncTrigger](https://sakno.github.io/dotNext/api/DotNext.Threading.AsyncTrigger.html) class
* Updated dependencies

<a href="https://www.nuget.org/packages/dotnext.unsafe/2.12.0">DotNext.Unsafe 2.12.0</a>
* Fixed ignoring of array offset in `ReadFrom` and `WriteTo` methods of [Pointer&lt;T&gt;](https://sakno.github.io/dotNext/api/DotNext.Runtime.InteropServices.Pointer-1.html) type
* Added `ToArray` method to [Pointer&lt;T&gt;](https://sakno.github.io/dotNext/api/DotNext.Runtime.InteropServices.Pointer-1.html) type
* Added indexer property to [IUnmanagedArray&lt;T&gt;](https://sakno.github.io/dotNext/api/DotNext.Runtime.InteropServices.IUnmanagedArray-1.html) interface
* Updated dependencies

<a href="https://www.nuget.org/packages/dotnext.net.cluster/2.12.0">DotNext.Net.Cluster 2.12.0</a>
* Updated dependencies shipped with .NET Core 3.1.10

<a href="https://www.nuget.org/packages/dotnext.aspnetcore.cluster/2.12.0">DotNext.AspNetCore.Cluster 2.12.0</a>
* Updated dependencies shipped with .NET Core 3.1.10

Changelog for previous versions located [here](./CHANGELOG.md).

# Release & Support Policy
The libraries are versioned according with [Semantic Versioning 2.0](https://semver.org/).

| Version | .NET compatibility | Support Level |
| ---- | ---- | ---- |
| 0.x | .NET Standard 2.0 | Not Supported |
| 1.x | .NET Standard 2.0 | Not Supported (since 12/20/2020) |
| 2.x | .NET Standard 2.1 | Maintenance |
| 3.x | .NET Standard 2.1, .NET 5 | Active Development |

_Maintenance_ support level means that new releases will contain bug fixes only.

# Development Process
Philosophy of development process:
1. All libraries in .NEXT family based on .NET Standard to be available for wide range of .NET implementations: Mono, Xamarin, .NET Core
1. Compatibility with AOT compiler should be checked for every release
1. Minimize set of dependencies
1. Rely on .NET Standard specification
1. Provide high-quality documentation
1. Stay cross-platform
1. Provide benchmarks
