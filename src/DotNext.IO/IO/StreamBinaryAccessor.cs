using System;
using System.Buffers;
using System.Globalization;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace DotNext.IO
{
    using Text;
    using static Buffers.BufferReader;

    /// <summary>
    /// Represents binary reader for the stream.
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    internal readonly struct AsyncStreamBinaryAccessor : IAsyncBinaryReader, IAsyncBinaryWriter
    {
        private readonly Memory<byte> buffer;
        private readonly Stream stream;

        internal AsyncStreamBinaryAccessor(Stream stream, Memory<byte> buffer)
        {
            if (buffer.IsEmpty)
                throw new ArgumentException(ExceptionMessages.BufferTooSmall, nameof(buffer));
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            this.buffer = buffer;
        }

#region Reader
        public ValueTask<T> ReadAsync<T>(CancellationToken token = default)
            where T : unmanaged
            => StreamExtensions.ReadAsync<T>(stream, buffer, token);

        public ValueTask ReadAsync(Memory<byte> output, CancellationToken token = default)
            => StreamExtensions.ReadBlockAsync(stream, output, token);

        public ValueTask<string> ReadStringAsync(int length, DecodingContext context, CancellationToken token = default)
            => StreamExtensions.ReadStringAsync(stream, length, context, buffer, token);

        public ValueTask<string> ReadStringAsync(StringLengthEncoding lengthFormat, DecodingContext context, CancellationToken token = default)
            => StreamExtensions.ReadStringAsync(stream, lengthFormat, context, buffer, token);

        ValueTask<byte> IAsyncBinaryReader.ReadByteAsync(StringLengthEncoding lengthFormat, DecodingContext context, NumberStyles style, IFormatProvider? provider, CancellationToken token)
            => StreamExtensions.ReadByteAsync(stream, lengthFormat, context, buffer, style, provider, token);

        async ValueTask<short> IAsyncBinaryReader.ReadInt16Async(bool littleEndian, CancellationToken token)
        {
            var result = await StreamExtensions.ReadAsync<short>(stream, buffer, token).ConfigureAwait(false);
            result.ReverseIfNeeded(littleEndian);
            return result;
        }

        ValueTask<short> IAsyncBinaryReader.ReadInt16Async(StringLengthEncoding lengthFormat, DecodingContext context, NumberStyles style, IFormatProvider? provider, CancellationToken token)
            => StreamExtensions.ReadInt16Async(stream, lengthFormat, context, buffer, style, provider, token);

        async ValueTask<int> IAsyncBinaryReader.ReadInt32Async(bool littleEndian, CancellationToken token)
        {
            var result = await StreamExtensions.ReadAsync<int>(stream, buffer, token).ConfigureAwait(false);
            result.ReverseIfNeeded(littleEndian);
            return result;
        }

        ValueTask<int> IAsyncBinaryReader.ReadInt32Async(StringLengthEncoding lengthFormat, DecodingContext context, NumberStyles style, IFormatProvider? provider, CancellationToken token)
            => StreamExtensions.ReadInt32Async(stream, lengthFormat, context, buffer, style, provider, token);

        async ValueTask<long> IAsyncBinaryReader.ReadInt64Async(bool littleEndian, CancellationToken token)
        {
            var result = await StreamExtensions.ReadAsync<long>(stream, buffer, token).ConfigureAwait(false);
            result.ReverseIfNeeded(littleEndian);
            return result;
        }

        ValueTask<long> IAsyncBinaryReader.ReadInt64Async(StringLengthEncoding lengthFormat, DecodingContext context, NumberStyles style, IFormatProvider? provider, CancellationToken token)
            => StreamExtensions.ReadInt64Async(stream, lengthFormat, context, buffer, style, provider, token);

        ValueTask<decimal> IAsyncBinaryReader.ReadDecimalAsync(StringLengthEncoding lengthFormat, DecodingContext context, NumberStyles style, IFormatProvider? provider, CancellationToken token)
            => StreamExtensions.ReadDecimalAsync(stream, lengthFormat, context, buffer, style, provider, token);

        ValueTask<float> IAsyncBinaryReader.ReadSingleAsync(StringLengthEncoding lengthFormat, DecodingContext context, NumberStyles style, IFormatProvider? provider, CancellationToken token)
            => StreamExtensions.ReadSingleAsync(stream, lengthFormat, context, buffer, style, provider, token);

        ValueTask<double> IAsyncBinaryReader.ReadDoubleAsync(StringLengthEncoding lengthFormat, DecodingContext context, NumberStyles style, IFormatProvider? provider, CancellationToken token)
            => StreamExtensions.ReadDoubleAsync(stream, lengthFormat, context, buffer, style, provider, token);

        ValueTask<Guid> IAsyncBinaryReader.ReadGuidAsync(StringLengthEncoding lengthFormat, DecodingContext context, CancellationToken token)
            => StreamExtensions.ReadGuidAsync(stream, lengthFormat, context, buffer, token);

        ValueTask<Guid> IAsyncBinaryReader.ReadGuidAsync(StringLengthEncoding lengthFormat, DecodingContext context, string format, CancellationToken token)
            => StreamExtensions.ReadGuidAsync(stream, lengthFormat, context, buffer, format, token);

        ValueTask<DateTime> IAsyncBinaryReader.ReadDateTimeAsync(StringLengthEncoding lengthFormat, DecodingContext context, DateTimeStyles style, IFormatProvider? provider, CancellationToken token)
            => StreamExtensions.ReadDateTimeAsync(stream, lengthFormat, context, buffer, style, provider, token);

        ValueTask<DateTime> IAsyncBinaryReader.ReadDateTimeAsync(StringLengthEncoding lengthFormat, DecodingContext context, string[] formats, DateTimeStyles style, IFormatProvider? provider, CancellationToken token)
            => StreamExtensions.ReadDateTimeAsync(stream, lengthFormat, context, buffer, style, provider, token);

        ValueTask<DateTimeOffset> IAsyncBinaryReader.ReadDateTimeOffsetAsync(StringLengthEncoding lengthFormat, DecodingContext context, DateTimeStyles style, IFormatProvider? provider, CancellationToken token)
            => StreamExtensions.ReadDateTimeOffsetAsync(stream, lengthFormat, context, buffer, style, provider, token);

        ValueTask<DateTimeOffset> IAsyncBinaryReader.ReadDateTimeOffsetAsync(StringLengthEncoding lengthFormat, DecodingContext context, string[] formats, DateTimeStyles style, IFormatProvider? provider, CancellationToken token)
            => StreamExtensions.ReadDateTimeOffsetAsync(stream, lengthFormat, context, buffer, style, provider, token);

        ValueTask<TimeSpan> IAsyncBinaryReader.ReadTimeSpanAsync(StringLengthEncoding lengthFormat, DecodingContext context, IFormatProvider? provider, CancellationToken token)
            => StreamExtensions.ReadTimeSpanAsync(stream, lengthFormat, context, buffer, provider, token);

        ValueTask<TimeSpan> IAsyncBinaryReader.ReadTimeSpanAsync(StringLengthEncoding lengthFormat, DecodingContext context, string[] formats, TimeSpanStyles style, IFormatProvider? provider, CancellationToken token)
            => StreamExtensions.ReadTimeSpanAsync(stream, lengthFormat, context, buffer, formats, style, provider, token);

        Task IAsyncBinaryReader.CopyToAsync(Stream output, CancellationToken token)
            => stream.CopyToAsync(output, token);

        Task IAsyncBinaryReader.CopyToAsync(PipeWriter output, CancellationToken token)
            => stream.CopyToAsync(output, token);

        Task IAsyncBinaryReader.CopyToAsync(IBufferWriter<byte> writer, CancellationToken token)
            => stream.CopyToAsync(writer, token: token);

        Task IAsyncBinaryReader.CopyToAsync<TArg>(ReadOnlySpanAction<byte, TArg> reader, TArg arg, CancellationToken token)
            => stream.ReadAsync(reader, arg, buffer, token);

        Task IAsyncBinaryReader.CopyToAsync<TArg>(Func<ReadOnlyMemory<byte>, TArg, CancellationToken, ValueTask> reader, TArg arg, CancellationToken token)
            => stream.ReadAsync(reader, arg, buffer, token);
#endregion

#region Writer
        public ValueTask WriteAsync<T>(T value, CancellationToken token)
            where T : unmanaged
            => stream.WriteAsync(value, buffer, token);

        public ValueTask WriteAsync(ReadOnlyMemory<byte> input, CancellationToken token)
            => stream.WriteAsync(input, token);

        public ValueTask WriteAsync(ReadOnlyMemory<char> chars, EncodingContext context, StringLengthEncoding? lengthFormat, CancellationToken token)
            => stream.WriteStringAsync(chars, context, buffer, lengthFormat, token);

        ValueTask IAsyncBinaryWriter.WriteByteAsync(byte value, StringLengthEncoding lengthFormat, EncodingContext context, string? format, IFormatProvider? provider, CancellationToken token)
            => stream.WriteByteAsync(value, lengthFormat, context, buffer, format, provider, token);

        ValueTask IAsyncBinaryWriter.WriteInt16Async(short value, bool littleEndian, CancellationToken token)
        {
            value.ReverseIfNeeded(littleEndian);
            return WriteAsync(value, token);
        }

        ValueTask IAsyncBinaryWriter.WriteInt16Async(short value, StringLengthEncoding lengthFormat, EncodingContext context, string? format, IFormatProvider? provider, CancellationToken token)
            => stream.WriteInt16Async(value, lengthFormat, context, buffer, format, provider, token);

        ValueTask IAsyncBinaryWriter.WriteInt32Async(int value, bool littleEndian, CancellationToken token)
        {
            value.ReverseIfNeeded(littleEndian);
            return WriteAsync(value, token);
        }

        ValueTask IAsyncBinaryWriter.WriteInt32Async(int value, StringLengthEncoding lengthFormat, EncodingContext context, string? format, IFormatProvider? provider, CancellationToken token)
            => stream.WriteInt32Async(value, lengthFormat, context, buffer, format, provider, token);

        ValueTask IAsyncBinaryWriter.WriteInt64Async(long value, bool littleEndian, CancellationToken token)
        {
            value.ReverseIfNeeded(littleEndian);
            return WriteAsync(value, token);
        }

        ValueTask IAsyncBinaryWriter.WriteInt64Async(long value, StringLengthEncoding lengthFormat, EncodingContext context, string? format, IFormatProvider? provider, CancellationToken token)
            => stream.WriteInt64Async(value, lengthFormat, context, buffer, format, provider, token);

        ValueTask IAsyncBinaryWriter.WriteDecimalAsync(decimal value, StringLengthEncoding lengthFormat, EncodingContext context, string? format, IFormatProvider? provider, CancellationToken token)
            => stream.WriteDecimalAsync(value, lengthFormat, context, buffer, format, provider, token);

        ValueTask IAsyncBinaryWriter.WriteSingleAsync(float value, StringLengthEncoding lengthFormat, EncodingContext context, string? format, IFormatProvider? provider, CancellationToken token)
            => stream.WriteSingleAsync(value, lengthFormat, context, buffer, format, provider, token);

        ValueTask IAsyncBinaryWriter.WriteDoubleAsync(double value, StringLengthEncoding lengthFormat, EncodingContext context, string? format, IFormatProvider? provider, CancellationToken token)
            => stream.WriteDoubleAsync(value, lengthFormat, context, buffer, format, provider, token);

        ValueTask IAsyncBinaryWriter.WriteDateTimeAsync(DateTime value, StringLengthEncoding lengthFormat, EncodingContext context, string? format, IFormatProvider? provider, CancellationToken token)
            => stream.WriteDateTimeAsync(value, lengthFormat, context, buffer, format, provider, token);

        ValueTask IAsyncBinaryWriter.WriteDateTimeOffsetAsync(DateTimeOffset value, StringLengthEncoding lengthFormat, EncodingContext context, string? format, IFormatProvider? provider, CancellationToken token)
            => stream.WriteDateTimeOffsetAsync(value, lengthFormat, context, buffer, format, provider, token);

        ValueTask IAsyncBinaryWriter.WriteTimeSpanAsync(TimeSpan value, StringLengthEncoding lengthFormat, EncodingContext context, string? format, IFormatProvider? provider, CancellationToken token)
            => stream.WriteTimeSpanAsync(value, lengthFormat, context, buffer, format, provider, token);

        Task IAsyncBinaryWriter.CopyFromAsync(Stream input, CancellationToken token)
            => input.CopyToAsync(stream, token);

        Task IAsyncBinaryWriter.CopyFromAsync(PipeReader input, CancellationToken token)
            => input.CopyToAsync(stream, token);

        Task IAsyncBinaryWriter.WriteAsync(ReadOnlySequence<byte> input, CancellationToken token)
            => stream.WriteAsync(input, token).AsTask();

        Task IAsyncBinaryWriter.CopyFromAsync<TArg>(Func<TArg, CancellationToken, ValueTask<ReadOnlyMemory<byte>>> supplier, TArg arg, CancellationToken token)
            => stream.WriteAsync(supplier, arg, token);
#endregion
    }
}