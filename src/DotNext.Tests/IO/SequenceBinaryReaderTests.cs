using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DotNext.IO
{
    using Buffers;
    using static Pipelines.PipeExtensions;

    public sealed class SequenceBinaryReaderTests : Test
    {
        [Fact]
        public static async Task ReadMemory()
        {
            var sequence = new ChunkSequence<byte>(new byte[] { 1, 5, 8, 9 }, 2).ToReadOnlySequence();
            False(sequence.IsSingleSegment);
            var result = new byte[3];
            IAsyncBinaryReader reader = IAsyncBinaryReader.Create(sequence);
            await reader.ReadAsync(result);
            Equal(1, result[0]);
            Equal(5, result[1]);
            Equal(8, result[2]);
        }

        [Fact]
        public static async Task CopyToStream()
        {
            var content = new byte[] { 1, 5, 8, 9 };
            IAsyncBinaryReader reader = IAsyncBinaryReader.Create(new ChunkSequence<byte>(content, 2).ToReadOnlySequence());
            using var ms = new MemoryStream();
            await reader.CopyToAsync(ms);
            ms.Position = 0;
            Equal(content, ms.ToArray());
        }

        [Fact]
        public static async Task CopyToPipe()
        {
            var expected = new byte[] { 1, 5, 8, 9 };
            IAsyncBinaryReader reader = IAsyncBinaryReader.Create(new ChunkSequence<byte>(expected, 2).ToReadOnlySequence());
            var pipe = new Pipe();
            await reader.CopyToAsync(pipe.Writer);
            await pipe.Writer.CompleteAsync();
            var actual = new byte[expected.Length];
            await pipe.Reader.ReadBlockAsync(actual);
            Equal(expected, actual);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public static void ReadBlittableType(bool littleEndian)
        {
            var writer = new ArrayBufferWriter<byte>();
            writer.Write(10M);
            writer.WriteInt64(42L, littleEndian);
            writer.WriteUInt64(43UL, littleEndian);
            writer.WriteInt32(44, littleEndian);
            writer.WriteUInt32(45U, littleEndian);
            writer.WriteInt16(46, littleEndian);
            writer.WriteUInt16(47, littleEndian);

            var reader = IAsyncBinaryReader.Create(writer.WrittenMemory);
            Equal(10M, reader.Read<decimal>());
            Equal(42L, reader.ReadInt64(littleEndian));
            Equal(43UL, reader.ReadUInt64(littleEndian));
            Equal(44, reader.ReadInt32(littleEndian));
            Equal(45U, reader.ReadUInt32(littleEndian));
            Equal(46, reader.ReadInt16(littleEndian));
            Equal(47, reader.ReadUInt16(littleEndian));
        }

        private static async Task ReadWriteStringUsingEncodingAsync(string value, Encoding encoding, StringLengthEncoding? lengthEnc)
        {
            using var ms = new MemoryStream();
            await ms.WriteStringAsync(value.AsMemory(), encoding, lengthEnc);
            ms.Position = 0;
            IAsyncBinaryReader reader = IAsyncBinaryReader.Create(ms.ToArray());
            var result = await (lengthEnc is null ?
                reader.ReadStringAsync(encoding.GetByteCount(value), encoding) :
                reader.ReadStringAsync(lengthEnc.GetValueOrDefault(), encoding));
            Equal(value, result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(StringLengthEncoding.Compressed)]
        [InlineData(StringLengthEncoding.Plain)]
        [InlineData(StringLengthEncoding.PlainLittleEndian)]
        [InlineData(StringLengthEncoding.PlainBigEndian)]
        public static async Task ReadWriteBufferedStringAsync(StringLengthEncoding? lengthEnc)
        {
            const string testString1 = "Hello, world!&*(@&*(fghjwgfwffgw";
            await ReadWriteStringUsingEncodingAsync(testString1, Encoding.UTF8, lengthEnc);
            await ReadWriteStringUsingEncodingAsync(testString1, Encoding.Unicode, lengthEnc);
            await ReadWriteStringUsingEncodingAsync(testString1, Encoding.UTF7, lengthEnc);
            await ReadWriteStringUsingEncodingAsync(testString1, Encoding.UTF32, lengthEnc);
            await ReadWriteStringUsingEncodingAsync(testString1, Encoding.ASCII, lengthEnc);
            const string testString2 = "������, ���!";
            await ReadWriteStringUsingEncodingAsync(testString2, Encoding.UTF8, lengthEnc);
            await ReadWriteStringUsingEncodingAsync(testString2, Encoding.Unicode, lengthEnc);
            await ReadWriteStringUsingEncodingAsync(testString2, Encoding.UTF7, lengthEnc);
            await ReadWriteStringUsingEncodingAsync(testString2, Encoding.UTF32, lengthEnc);
        }
    }
}