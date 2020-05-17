﻿using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;

namespace DotNext.IO
{
    /// <summary>
    /// Represents transfer object for value of blittable type.
    /// </summary>
    /// <typeparam name="T">The type of encapsulated value.</typeparam>
    public class BinaryTransferObject<T> : IDataTransferObject, IConvertible<T>
        where T : unmanaged
    {
        /// <summary>
        /// Gets or sets a value of blittable type encapsulated by this object.
        /// </summary>
        public T Content
        {
            get;
            set;
        }

        /// <inheritdoc/>
        T IConvertible<T>.Convert() => Content;

        /// <inheritdoc/>
        bool IDataTransferObject.IsReusable => true;

        private static unsafe int Length => sizeof(T);

        /// <inheritdoc/>
        long? IDataTransferObject.Length => Length;

        /// <inheritdoc/>
        ValueTask IDataTransferObject.WriteToAsync<TWriter>(TWriter writer, CancellationToken token)
            => writer.WriteAsync(Content, token);
    }

    /// <summary>
    /// Represents binary object.
    /// </summary>
    public class BinaryTransferObject : IDataTransferObject, IConvertible<ReadOnlySequence<byte>>
    {
        /// <summary>
        /// Initializes a new binary DTO.
        /// </summary>
        /// <param name="content">The content of the object.</param>
        public BinaryTransferObject(ReadOnlySequence<byte> content) => Content = content;

        /// <summary>
        /// Initializes a new binary object.
        /// </summary>
        /// <param name="content">The content of the object.</param>
        public BinaryTransferObject(ReadOnlyMemory<byte> content)
            : this(new ReadOnlySequence<byte>(content))
        {
        }

        /// <summary>
        /// Gets stream representing content.
        /// </summary>
        public ReadOnlySequence<byte> Content { get; }

        /// <inheritdoc/>
        ReadOnlySequence<byte> IConvertible<ReadOnlySequence<byte>>.Convert() => Content;

        /// <inheritdoc/>
        bool IDataTransferObject.IsReusable => true;

        /// <inheritdoc/>
        long? IDataTransferObject.Length => Content.Length;

        /// <inheritdoc/>
        async ValueTask IDataTransferObject.WriteToAsync<TWriter>(TWriter writer, CancellationToken token)
        {
            foreach (var segment in Content)
                await writer.WriteAsync(segment, token).ConfigureAwait(false);
        }
    }
}
