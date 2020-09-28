using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;

namespace DotNext
{
    using ChunkSequence = Buffers.ChunkSequence;

    [ExcludeFromCodeCoverage]
    [LogBeforeAfterTest]
    public abstract class Test : Assert
    {
        private protected static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(20);

        private protected static T SerializeDeserialize<T>(T value)
            where T : ISerializable
        {
            IFormatter formatter = new BinaryFormatter();
            using var ms = new MemoryStream(2048);
            formatter.Serialize(ms, value);
            ms.Position = 0;
            return (T)formatter.Deserialize(ms);
        }

        private static IEnumerable<ReadOnlyMemory<T>> Split<T>(ReadOnlyMemory<T> memory, int chunkSize)
        {
            var startIndex = 0;
            var length = Math.Min(chunkSize, memory.Length);

            do
            {
                yield return memory.Slice(startIndex, length);
                startIndex += chunkSize;
                length = Math.Min(memory.Length - startIndex, chunkSize);
            }
            while (startIndex < memory.Length);
        }

        private protected static ReadOnlySequence<T> ToReadOnlySequence<T>(ReadOnlyMemory<T> memory, int chunkSize)
            => ChunkSequence.ToReadOnlySequence(Split(memory, chunkSize));
    }
}