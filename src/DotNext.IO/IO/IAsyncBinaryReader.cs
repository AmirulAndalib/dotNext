using System;
using System.Buffers;
using System.Globalization;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace DotNext.IO
{
    using static Buffers.BufferReader;
    using DecodingContext = Text.DecodingContext;

    /// <summary>
    /// Providers a uniform way to decode the data
    /// from various sources such as streams, pipes, unmanaged memory etc.
    /// </summary>
    /// <seealso cref="IAsyncBinaryWriter"/>
    public interface IAsyncBinaryReader
    {
        /// <summary>
        /// Represents empty reader.
        /// </summary>
        public static IAsyncBinaryReader Empty { get; } = new EmptyBinaryReader();

        /// <summary>
        /// Decodes the value of blittable type.
        /// </summary>
        /// <param name="token">The token that can be used to cancel the operation.</param>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <returns>The decoded value.</returns>
        /// <exception cref="OperationCanceledException">The operation has been canceled.</exception>
        /// <exception cref="EndOfStreamException">The underlying source doesn't contain necessary amount of bytes to decode the value.</exception>
        ValueTask<T> ReadAsync<T>(CancellationToken token = default)
            where T : unmanaged;

        /// <summary>
        /// Decodes 64-bit signed integer using the specified endianness.
        /// </summary>
        /// <param name="littleEndian"><see langword="true"/> if value is stored in the underlying binary stream as little-endian; otherwise, use big-endian.</param>
        /// <param name="token">The token that can be used to cancel the operation.</param>
        /// <returns>The decoded value.</returns>
        /// <exception cref="OperationCanceledException">The operation has been canceled.</exception>
        /// <exception cref="EndOfStreamException">The underlying source doesn't contain necessary amount of bytes to decode the value.</exception>
        async ValueTask<long> ReadInt64Async(bool littleEndian, CancellationToken token = default)
        {
            var result = await ReadAsync<long>(token).ConfigureAwait(false);
            result.ReverseIfNeeded(littleEndian);
            return result;
        }

        /// <summary>
        /// Parses 64-bit signed integer from its string representation encoded in the underlying stream.
        /// </summary>
        /// <param name="lengthFormat">The format of the string length encoded in the stream.</param>
        /// <param name="context">The decoding context containing string characters encoding.</param>
        /// <param name="style">A bitwise combination of the enumeration values that indicates the style elements.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="token">The token that can be used to cancel the operation.</param>
        /// <returns>The parsed value.</returns>
        /// <exception cref="FormatException">The number is in incorrect format.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled.</exception>
        /// <exception cref="EndOfStreamException">The underlying source doesn't contain necessary amount of bytes to decode the value.</exception>
        async ValueTask<long> ReadInt64Async(StringLengthEncoding lengthFormat, DecodingContext context, NumberStyles style = NumberStyles.Integer, IFormatProvider? provider = null, CancellationToken token = default)
            => long.Parse(await ReadStringAsync(lengthFormat, context, token).ConfigureAwait(false), style, provider);

        /// <summary>
        /// Decodes 32-bit signed integer using the specified endianness.
        /// </summary>
        /// <param name="littleEndian"><see langword="true"/> if value is stored in the underlying binary stream as little-endian; otherwise, use big-endian.</param>
        /// <param name="token">The token that can be used to cancel the operation.</param>
        /// <returns>The decoded value.</returns>
        /// <exception cref="OperationCanceledException">The operation has been canceled.</exception>
        /// <exception cref="EndOfStreamException">The underlying source doesn't contain necessary amount of bytes to decode the value.</exception>
        async ValueTask<int> ReadInt32Async(bool littleEndian, CancellationToken token = default)
        {
            var result = await ReadAsync<int>(token).ConfigureAwait(false);
            result.ReverseIfNeeded(littleEndian);
            return result;
        }

        /// <summary>
        /// Parses 32-bit signed integer from its string representation encoded in the underlying stream.
        /// </summary>
        /// <param name="lengthFormat">The format of the string length encoded in the stream.</param>
        /// <param name="context">The decoding context containing string characters encoding.</param>
        /// <param name="style">A bitwise combination of the enumeration values that indicates the style elements.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="token">The token that can be used to cancel the operation.</param>
        /// <returns>The parsed value.</returns>
        /// <exception cref="FormatException">The number is in incorrect format.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled.</exception>
        /// <exception cref="EndOfStreamException">The underlying source doesn't contain necessary amount of bytes to decode the value.</exception>
        async ValueTask<int> ReadInt32Async(StringLengthEncoding lengthFormat, DecodingContext context, NumberStyles style = NumberStyles.Integer, IFormatProvider? provider = null, CancellationToken token = default)
            => int.Parse(await ReadStringAsync(lengthFormat, context, token).ConfigureAwait(false), style, provider);

        /// <summary>
        /// Decodes 16-bit signed integer using the specified endianness.
        /// </summary>
        /// <param name="littleEndian"><see langword="true"/> if value is stored in the underlying binary stream as little-endian; otherwise, use big-endian.</param>
        /// <param name="token">The token that can be used to cancel the operation.</param>
        /// <returns>The decoded value.</returns>
        /// <exception cref="OperationCanceledException">The operation has been canceled.</exception>
        /// <exception cref="EndOfStreamException">The underlying source doesn't contain necessary amount of bytes to decode the value.</exception>
        async ValueTask<short> ReadInt16Async(bool littleEndian, CancellationToken token = default)
        {
            var result = await ReadAsync<short>(token).ConfigureAwait(false);
            result.ReverseIfNeeded(littleEndian);
            return result;
        }

        /// <summary>
        /// Parses 16-bit signed integer from its string representation encoded in the underlying stream.
        /// </summary>
        /// <param name="lengthFormat">The format of the string length encoded in the stream.</param>
        /// <param name="context">The decoding context containing string characters encoding.</param>
        /// <param name="style">A bitwise combination of the enumeration values that indicates the style elements.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="token">The token that can be used to cancel the operation.</param>
        /// <returns>The parsed value.</returns>
        /// <exception cref="FormatException">The number is in incorrect format.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled.</exception>
        /// <exception cref="EndOfStreamException">The underlying source doesn't contain necessary amount of bytes to decode the value.</exception>
        async ValueTask<short> ReadInt16Async(StringLengthEncoding lengthFormat, DecodingContext context, NumberStyles style = NumberStyles.Integer, IFormatProvider? provider = null, CancellationToken token = default)
            => short.Parse(await ReadStringAsync(lengthFormat, context, token).ConfigureAwait(false), style, provider);

        /// <summary>
        /// Parses single-precision floating-point number from its string representation encoded in the underlying stream.
        /// </summary>
        /// <param name="lengthFormat">The format of the string length encoded in the stream.</param>
        /// <param name="context">The decoding context containing string characters encoding.</param>
        /// <param name="style">A bitwise combination of the enumeration values that indicates the style elements.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="token">The token that can be used to cancel the operation.</param>
        /// <returns>The parsed value.</returns>
        /// <exception cref="FormatException">The number is in incorrect format.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled.</exception>
        /// <exception cref="EndOfStreamException">The underlying source doesn't contain necessary amount of bytes to decode the value.</exception>
        async ValueTask<float> ReadSingleAsync(StringLengthEncoding lengthFormat, DecodingContext context, NumberStyles style = NumberStyles.AllowThousands | NumberStyles.Float, IFormatProvider? provider = null, CancellationToken token = default)
            => float.Parse(await ReadStringAsync(lengthFormat, context, token).ConfigureAwait(false), style, provider);

        /// <summary>
        /// Parses double-precision floating-point number from its string representation encoded in the underlying stream.
        /// </summary>
        /// <param name="lengthFormat">The format of the string length encoded in the stream.</param>
        /// <param name="context">The decoding context containing string characters encoding.</param>
        /// <param name="style">A bitwise combination of the enumeration values that indicates the style elements.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="token">The token that can be used to cancel the operation.</param>
        /// <returns>The parsed value.</returns>
        /// <exception cref="FormatException">The number is in incorrect format.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled.</exception>
        /// <exception cref="EndOfStreamException">The underlying source doesn't contain necessary amount of bytes to decode the value.</exception>
        async ValueTask<double> ReadDoubleAsync(StringLengthEncoding lengthFormat, DecodingContext context, NumberStyles style = NumberStyles.AllowThousands | NumberStyles.Float, IFormatProvider? provider = null, CancellationToken token = default)
            => double.Parse(await ReadStringAsync(lengthFormat, context, token).ConfigureAwait(false), style, provider);

        /// <summary>
        /// Parses 8-bit unsigned integer from its string representation encoded in the underlying stream.
        /// </summary>
        /// <param name="lengthFormat">The format of the string length encoded in the stream.</param>
        /// <param name="context">The decoding context containing string characters encoding.</param>
        /// <param name="style">A bitwise combination of the enumeration values that indicates the style elements.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="token">The token that can be used to cancel the operation.</param>
        /// <returns>The parsed value.</returns>
        /// <exception cref="FormatException">The number is in incorrect format.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled.</exception>
        /// <exception cref="EndOfStreamException">The underlying source doesn't contain necessary amount of bytes to decode the value.</exception>
        async ValueTask<byte> ReadByteAsync(StringLengthEncoding lengthFormat, DecodingContext context, NumberStyles style = NumberStyles.Integer, IFormatProvider? provider = null, CancellationToken token = default)
            => byte.Parse(await ReadStringAsync(lengthFormat, context, token).ConfigureAwait(false), style, provider);

        /// <summary>
        /// Parses <see cref="decimal"/> from its string representation encoded in the underlying stream.
        /// </summary>
        /// <param name="lengthFormat">The format of the string length encoded in the stream.</param>
        /// <param name="context">The decoding context containing string characters encoding.</param>
        /// <param name="style">A bitwise combination of the enumeration values that indicates the style elements.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="token">The token that can be used to cancel the operation.</param>
        /// <returns>The parsed value.</returns>
        /// <exception cref="FormatException">The number is in incorrect format.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled.</exception>
        /// <exception cref="EndOfStreamException">The underlying source doesn't contain necessary amount of bytes to decode the value.</exception>
        async ValueTask<decimal> ReadDecimalAsync(StringLengthEncoding lengthFormat, DecodingContext context, NumberStyles style = NumberStyles.Number, IFormatProvider? provider = null, CancellationToken token = default)
            => decimal.Parse(await ReadStringAsync(lengthFormat, context, token).ConfigureAwait(false), style, provider);

        /// <summary>
        /// Parses <see cref="DateTime"/> from its string representation encoded in the underlying stream.
        /// </summary>
        /// <param name="lengthFormat">The format of the string length encoded in the stream.</param>
        /// <param name="context">The decoding context containing string characters encoding.</param>
        /// <param name="style">A bitwise combination of the enumeration values that indicates the style elements.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="token">The token that can be used to cancel the operation.</param>
        /// <returns>The parsed value.</returns>
        /// <exception cref="FormatException">The date/time is in incorrect format.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled.</exception>
        /// <exception cref="EndOfStreamException">The underlying source doesn't contain necessary amount of bytes to decode the value.</exception>
        async ValueTask<DateTime> ReadDateTimeAsync(StringLengthEncoding lengthFormat, DecodingContext context, DateTimeStyles style = DateTimeStyles.None, IFormatProvider? provider = null, CancellationToken token = default)
            => DateTime.Parse(await ReadStringAsync(lengthFormat, context, token).ConfigureAwait(false), provider, style);

        /// <summary>
        /// Parses <see cref="DateTime"/> from its string representation encoded in the underlying stream.
        /// </summary>
        /// <param name="lengthFormat">The format of the string length encoded in the stream.</param>
        /// <param name="context">The decoding context containing string characters encoding.</param>
        /// <param name="formats">An array of allowable formats.</param>
        /// <param name="style">A bitwise combination of the enumeration values that indicates the style elements.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="token">The token that can be used to cancel the operation.</param>
        /// <returns>The parsed value.</returns>
        /// <exception cref="FormatException">The date/time is in incorrect format.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled.</exception>
        /// <exception cref="EndOfStreamException">The underlying source doesn't contain necessary amount of bytes to decode the value.</exception>
        async ValueTask<DateTime> ReadDateTimeAsync(StringLengthEncoding lengthFormat, DecodingContext context, string[] formats, DateTimeStyles style = DateTimeStyles.None, IFormatProvider? provider = null, CancellationToken token = default)
            => DateTime.ParseExact(await ReadStringAsync(lengthFormat, context, token).ConfigureAwait(false), formats, provider, style);

        /// <summary>
        /// Parses <see cref="DateTimeOffset"/> from its string representation encoded in the underlying stream.
        /// </summary>
        /// <param name="lengthFormat">The format of the string length encoded in the stream.</param>
        /// <param name="context">The decoding context containing string characters encoding.</param>
        /// <param name="style">A bitwise combination of the enumeration values that indicates the style elements.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="token">The token that can be used to cancel the operation.</param>
        /// <returns>The parsed value.</returns>
        /// <exception cref="FormatException">The date/time is in incorrect format.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled.</exception>
        /// <exception cref="EndOfStreamException">The underlying source doesn't contain necessary amount of bytes to decode the value.</exception>
        async ValueTask<DateTimeOffset> ReadDateTimeOffsetAsync(StringLengthEncoding lengthFormat, DecodingContext context, DateTimeStyles style = DateTimeStyles.None, IFormatProvider? provider = null, CancellationToken token = default)
            => DateTimeOffset.Parse(await ReadStringAsync(lengthFormat, context, token).ConfigureAwait(false), provider, style);

        /// <summary>
        /// Parses <see cref="DateTimeOffset"/> from its string representation encoded in the underlying stream.
        /// </summary>
        /// <param name="lengthFormat">The format of the string length encoded in the stream.</param>
        /// <param name="context">The decoding context containing string characters encoding.</param>
        /// <param name="formats">An array of allowable formats.</param>
        /// <param name="style">A bitwise combination of the enumeration values that indicates the style elements.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="token">The token that can be used to cancel the operation.</param>
        /// <returns>The parsed value.</returns>
        /// <exception cref="FormatException">The date/time is in incorrect format.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled.</exception>
        /// <exception cref="EndOfStreamException">The underlying source doesn't contain necessary amount of bytes to decode the value.</exception>
        async ValueTask<DateTimeOffset> ReadDateTimeOffsetAsync(StringLengthEncoding lengthFormat, DecodingContext context, string[] formats, DateTimeStyles style = DateTimeStyles.None, IFormatProvider? provider = null, CancellationToken token = default)
            => DateTimeOffset.ParseExact(await ReadStringAsync(lengthFormat, context, token).ConfigureAwait(false), formats, provider, style);

        /// <summary>
        /// Parses <see cref="Guid"/> from its string representation encoded in the underlying stream.
        /// </summary>
        /// <param name="lengthFormat">The format of the string length encoded in the stream.</param>
        /// <param name="context">The decoding context containing string characters encoding.</param>
        /// <param name="token">The token that can be used to cancel the operation.</param>
        /// <returns>The parsed value.</returns>
        /// <exception cref="FormatException">GUID is in incorrect format.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled.</exception>
        /// <exception cref="EndOfStreamException">The underlying source doesn't contain necessary amount of bytes to decode the value.</exception>
        async ValueTask<Guid> ReadGuidAsync(StringLengthEncoding lengthFormat, DecodingContext context, CancellationToken token = default)
            => Guid.Parse(await ReadStringAsync(lengthFormat, context, token).ConfigureAwait(false));

        /// <summary>
        /// Parses <see cref="Guid"/> from its string representation encoded in the underlying stream.
        /// </summary>
        /// <param name="lengthFormat">The format of the string length encoded in the stream.</param>
        /// <param name="context">The decoding context containing string characters encoding.</param>
        /// <param name="format">The expected format of GUID value.</param>
        /// <param name="token">The token that can be used to cancel the operation.</param>
        /// <returns>The parsed value.</returns>
        /// <exception cref="FormatException">GUID is in incorrect format.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled.</exception>
        /// <exception cref="EndOfStreamException">The underlying source doesn't contain necessary amount of bytes to decode the value.</exception>
        async ValueTask<Guid> ReadGuidAsync(StringLengthEncoding lengthFormat, DecodingContext context, string format, CancellationToken token = default)
            => Guid.ParseExact(await ReadStringAsync(lengthFormat, context, token).ConfigureAwait(false), format);

        /// <summary>
        /// Reads the block of bytes.
        /// </summary>
        /// <param name="output">The block of memory to fill.</param>
        /// <param name="token">The token that can be used to cancel the operation.</param>
        /// <returns>The task representing state of asynchronous execution.</returns>
        /// <exception cref="OperationCanceledException">The operation has been canceled.</exception>
        /// <exception cref="EndOfStreamException">The underlying source doesn't contain necessary amount of bytes to decode the value.</exception>
        ValueTask ReadAsync(Memory<byte> output, CancellationToken token = default);

        /// <summary>
        /// Decodes the string.
        /// </summary>
        /// <param name="length">The length of the encoded string, in bytes.</param>
        /// <param name="context">The decoding context containing string characters encoding.</param>
        /// <param name="token">The token that can be used to cancel the operation.</param>
        /// <returns>The decoded string.</returns>
        /// <exception cref="OperationCanceledException">The operation has been canceled.</exception>
        /// <exception cref="EndOfStreamException">The underlying source doesn't contain necessary amount of bytes to decode the value.</exception>
        ValueTask<string> ReadStringAsync(int length, DecodingContext context, CancellationToken token = default);

        /// <summary>
        /// Decodes the string.
        /// </summary>
        /// <param name="lengthFormat">The format of the string length encoded in the stream.</param>
        /// <param name="context">The decoding context containing string characters encoding.</param>
        /// <param name="token">The token that can be used to cancel the operation.</param>
        /// <returns>The decoded string.</returns>
        /// <exception cref="OperationCanceledException">The operation has been canceled.</exception>
        /// <exception cref="EndOfStreamException">The underlying source doesn't contain necessary amount of bytes to decode the value.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="lengthFormat"/> is invalid.</exception>
        ValueTask<string> ReadStringAsync(StringLengthEncoding lengthFormat, DecodingContext context, CancellationToken token = default);

        /// <summary>
        /// Copies the content to the specified stream.
        /// </summary>
        /// <param name="output">The output stream receiving object content.</param>
        /// <param name="token">The token that can be used to cancel asynchronous operation.</param>
        /// <exception cref="OperationCanceledException">The operation has been canceled.</exception>
        Task CopyToAsync(Stream output, CancellationToken token = default);

        /// <summary>
        /// Copies the content to the specified pipe writer.
        /// </summary>
        /// <param name="output">The writer.</param>
        /// <param name="token">The token that can be used to cancel operation.</param>
        /// <returns>The task representing asynchronous execution of this method.</returns>
        /// <exception cref="OperationCanceledException">The operation has been canceled.</exception>
        Task CopyToAsync(PipeWriter output, CancellationToken token = default);

        /// <summary>
        /// Copies the content to the specified buffer.
        /// </summary>
        /// <param name="writer">The buffer writer.</param>
        /// <param name="token">The token that can be used to cancel operation.</param>
        /// <returns>The task representing asynchronous execution of this method.</returns>
        /// <exception cref="OperationCanceledException">The operation has been canceled.</exception>
        async Task CopyToAsync(IBufferWriter<byte> writer, CancellationToken token = default)
        {
            using var stream = writer.AsStream();
            await CopyToAsync(stream, token).ConfigureAwait(false);
            await stream.FlushAsync(token).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates default implementation of binary reader for the stream.
        /// </summary>
        /// <remarks>
        /// It is recommended to use extension methods from <see cref="StreamExtensions"/> class
        /// for decoding data from the stream. This method is intended for situation
        /// when you need an object implementing <see cref="IAsyncBinaryReader"/> interface.
        /// </remarks>
        /// <param name="input">The stream to be wrapped into the reader.</param>
        /// <param name="buffer">The buffer used for decoding data from the stream.</param>
        /// <returns>The stream reader.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is empty.</exception>
        public static IAsyncBinaryReader Create(Stream input, Memory<byte> buffer) => new AsyncStreamBinaryReader(input, buffer);

        /// <summary>
        /// Creates default implementation of binary reader over sequence of bytes.
        /// </summary>
        /// <param name="sequence">The sequence of bytes.</param>
        /// <returns>The binary reader for the sequence of bytes.</returns>
        public static SequenceBinaryReader Create(ReadOnlySequence<byte> sequence) => new SequenceBinaryReader(sequence);

        /// <summary>
        /// Creates default implementation of binary reader over contiguous memory block.
        /// </summary>
        /// <param name="memory">The block of memory.</param>
        /// <returns>The binary reader for the memory block.</returns>
        public static SequenceBinaryReader Create(ReadOnlyMemory<byte> memory) => new SequenceBinaryReader(memory);

        /// <summary>
        /// Creates default implementation of binary reader for the specifed pipe reader.
        /// </summary>
        /// <remarks>
        /// It is recommended to use extension methods from <see cref="Pipelines.PipeExtensions"/> class
        /// for decoding data from the stream. This method is intended for situation
        /// when you need an object implementing <see cref="IAsyncBinaryReader"/> interface.
        /// </remarks>
        /// <param name="reader">The pipe reader.</param>
        /// <returns>The binary reader.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> is <see langword="null"/>.</exception>
        public static IAsyncBinaryReader Create(PipeReader reader) => new Pipelines.PipeBinaryReader(reader);
    }
}