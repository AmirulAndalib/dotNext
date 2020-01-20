using System;
using System.Runtime.InteropServices;

namespace DotNext.Buffers
{
    internal interface IBuffer<T>
        where T : unmanaged
    {
        int Length { get; }
        Span<T> Span { get; }
    }

    [StructLayout(LayoutKind.Auto)]
    internal unsafe readonly struct UnsafeBuffer<T> : IBuffer<T>
        where T : unmanaged
    {
        private readonly T* ptr;
        private readonly int length;

        internal UnsafeBuffer(T* ptr, int length)
        {
            this.ptr = ptr;
            this.length = length;
        }

        int IBuffer<T>.Length => length;

        Span<T> IBuffer<T>.Span => new Span<T>(ptr, length);
    }
    
    [StructLayout(LayoutKind.Auto)]
    internal readonly struct ArrayBuffer<T> : IBuffer<T>, IDisposable
        where T : unmanaged
    {
        private readonly ArrayRental<T> buffer;

        internal ArrayBuffer(int length)
        {
            buffer = new ArrayRental<T>(length);
        }

        int IBuffer<T>.Length => buffer.Length;

        Span<T> IBuffer<T>.Span => buffer.Span;

        public void Dispose() => buffer.Dispose();
    }
}