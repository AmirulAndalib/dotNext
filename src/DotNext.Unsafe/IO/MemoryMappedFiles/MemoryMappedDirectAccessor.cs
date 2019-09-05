﻿using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace DotNext.IO.MemoryMappedFiles
{
    using Runtime.InteropServices;

    /// <summary>
    /// Provides direct access to the memory-mapped file content through pointer
    /// to its virtual memory.
    /// </summary>
    public unsafe readonly struct MemoryMappedDirectAccessor : IUnmanagedMemory
    {
        private readonly MemoryMappedViewAccessor accessor;
        private readonly byte* ptr;

        internal MemoryMappedDirectAccessor(MemoryMappedViewAccessor accessor)
        {
            this.accessor = accessor;
            ptr = default;
            accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
        }

        /// <summary>
        /// Converts the segment of the memory-mapped file
        /// </summary>
        /// <remarks>
        /// The caller is responsible for disposing of the returned stream.
        /// </remarks>
        /// <value>The stream representing virtual memory of the memory-mapped file.</value>
        public Stream AsStream()
        {
            if(accessor is null)
                return Stream.Null;
            FileAccess access;
            switch(accessor.CanRead.ToInt32() + accessor.CanWrite.ToInt32() << 1)
            {
                default:
                    access = default;
                    break;
                case 1:
                    access = FileAccess.Read;
                    break;
                case 2:
                    access = FileAccess.Write;
                    break;
                case 3:
                    access = FileAccess.ReadWrite;
                    break;
            }
            return Pointer.AsStream(Size, access);
        }

        /// <summary>
        /// Gets a value indicating that this object doesn't represent the memory-mapped file segment.
        /// </summary>
        public bool IsEmpty => accessor is null;

        /// <summary>
        /// Gets the number of bytes by which the starting position of this segment is offset from the beginning of the memory-mapped file.
        /// </summary>
        public long Offset => accessor?.PointerOffset ?? 0L;

        /// <summary>
        /// Gets pointer to the virtual memory of the mapped file.
        /// </summary>
        /// <value>The pointer to the memory-mapped file.</value>
        public Pointer<byte> Pointer => accessor is null ? default : new Pointer<byte>(ptr + accessor.PointerOffset);

        /// <summary>
        /// Gets length of the mapped segment.
        /// </summary>
        public long Size => accessor?.Capacity ?? 0L;

        /// <summary>
        /// Represents memory-mapped file segment in the form of <see cref="Span{T}"/>.
        /// </summary>
        /// <value><see cref="Span{T}"/> representing virtual memory of the mapped file segment.</value>
        public Span<byte> Bytes => Pointer.ToSpan(checked((int)accessor.Capacity));

        /// <summary>
        /// Sets all bits of allocated memory to zero.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The underlying unmanaged memory is released.</exception>
        public void Clear()
        {
            if(ptr == Memory.NullPtr)
                throw new ObjectDisposedException(GetType().Name);
            Memory.ClearBits(new IntPtr(ptr), Size);
        }

        /// <summary>
        /// Clears all buffers for this view and causes any buffered data to be written to the underlying file.
        /// </summary>
        public void Flush() => accessor?.Flush();

        /// <summary>
        /// Releases virtual memory associated with the mapped file segment.
        /// </summary>
        public void Dispose()
        {
            accessor.SafeMemoryMappedViewHandle.ReleasePointer();
            accessor.Dispose();
        }
    }
}
