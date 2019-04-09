﻿using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace DotNext.Threading
{
    using Generic;

    /// <summary>
	/// Various atomic operations for <see cref="double"/> data type
	/// accessible as extension methods.
	/// </summary>
	/// <remarks>
	/// Methods exposed by this class provide volatile read/write
	/// of the field even if it is not declared as volatile field.
	/// </remarks>
	/// <seealso cref="Interlocked"/>
    public static class AtomicDouble
    {
        private sealed class CASProvider : Constant<CAS<double>>
        {
            public CASProvider()
                : base(CompareAndSet)
            {
            }
        }

        /// <summary>
        /// Reads the value of the specified field. On systems that require it, inserts a
        /// memory barrier that prevents the processor from reordering memory operations
        /// as follows: If a read or write appears after this method in the code, the processor
        /// cannot move it before this method.
        /// </summary>
        /// <param name="value">The field to read.</param>
        /// <returns>
        /// The value that was read. This value is the latest written by any processor in
        /// the computer, regardless of the number of processors or the state of processor
        /// cache.
        /// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double VolatileGet(ref this double value) => Volatile.Read(ref value);

        /// <summary>
        /// Writes the specified value to the specified field. On systems that require it,
        /// inserts a memory barrier that prevents the processor from reordering memory operations
        /// as follows: If a read or write appears before this method in the code, the processor
        /// cannot move it after this method.
        /// </summary>
        /// <param name="value">The field where the value is written.</param>
        /// <param name="newValue">
        /// The value to write. The value is written immediately so that it is visible to
        /// all processors in the computer.
        /// </param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void VolatileSet(ref this double value, double newValue)
            => Volatile.Write(ref value, newValue);

        /// <summary>
		/// Atomically increments by one referenced value.
		/// </summary>
		/// <param name="value">Reference to a value to be modified.</param>
		/// <returns>Incremented value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double IncrementAndGet(ref this double value) => UpdateAndGet(ref value, x => x + 1D);

        /// <summary>
		/// Atomically decrements by one the current value.
		/// </summary>
		/// <param name="value">Reference to a value to be modified.</param>
		/// <returns>Decremented value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double DecrementAndGet(ref this double value) => UpdateAndGet(ref value, x => x - 1D);

        /// <summary>
		/// Adds two 64-bit floating-point numbers and replaces referenced storage with the sum, 
		/// as an atomic operation.
		/// </summary>
		/// <param name="value">Reference to a value to be modified.</param>
		/// <param name="operand">The value to be added to the currently stored integer.</param>
		/// <returns>Result of sum operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Add(ref this double value, double operand) => AccumulateAndGet(ref value, operand, (x, y) => x + y);

        /// <summary>
        /// Atomically sets referenced value to the given updated value if the current value == the expected value.
        /// </summary>
        /// <param name="value">Reference to a value to be modified.</param>
        /// <param name="expected">The expected value.</param>
        /// <param name="update">The new value.</param>
        /// <returns><see langword="true"/> if successful. <see langword="false"/> return indicates that the actual value was not equal to the expected value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CompareAndSet(ref this double value, double expected, double update)
            => Interlocked.CompareExchange(ref value, update, expected) == expected;

        /// <summary>
		/// Modifies referenced value atomically.
		/// </summary>
		/// <param name="value">Reference to a value to be modified.</param>
		/// <param name="update">A new value to be stored into managed pointer.</param>
		/// <returns>Original value before modification.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double GetAndSet(ref this double value, double update)
            => Interlocked.Exchange(ref value, update);

        /// <summary>
        /// Modifies value atomically.
        /// </summary>
        /// <param name="value">Reference to a value to be modified.</param>
        /// <param name="update">A new value to be stored into managed pointer.</param>
        /// <returns>A new value passed as argument.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double SetAndGet(ref this double value, double update)
        {
            VolatileSet(ref value, update);
            return update;
        }

        /// <summary>
		/// Atomically updates the current value with the results of applying the given function 
		/// to the current and given values, returning the updated value.
		/// </summary>
		/// <remarks>
		/// The function is applied with the current value as its first argument, and the given update as the second argument.
		/// </remarks>
		/// <param name="value">Reference to a value to be modified.</param>
		/// <param name="x">Accumulator operand.</param>
		/// <param name="accumulator">A side-effect-free function of two arguments</param>
		/// <returns>The updated value.</returns>
		public static double AccumulateAndGet(ref this double value, double x, Func<double, double, double> accumulator)
            => Atomic<double, CASProvider>.Accumulute(ref value, x, accumulator).NewValue;

        /// <summary>
        /// Atomically updates the current value with the results of applying the given function 
        /// to the current and given values, returning the original value.
        /// </summary>
        /// <remarks>
        /// The function is applied with the current value as its first argument, and the given update as the second argument.
        /// </remarks>
        /// <param name="value">Reference to a value to be modified.</param>
        /// <param name="x">Accumulator operand.</param>
        /// <param name="accumulator">A side-effect-free function of two arguments</param>
        /// <returns>The original value.</returns>
        public static double GetAndAccumulate(ref this double value, double x, Func<double, double, double> accumulator)
            => Atomic<double, CASProvider>.Accumulute(ref value, x, accumulator).OldValue;

        /// <summary>
		/// Atomically updates the stored value with the results 
		/// of applying the given function, returning the updated value.
		/// </summary>
		/// <param name="value">Reference to a value to be modified.</param>
		/// <param name="updater">A side-effect-free function</param>
		/// <returns>The updated value.</returns>
		public static double UpdateAndGet(ref this double value, Func<double, double> updater)
            => Atomic<double, CASProvider>.Update(ref value, updater).NewValue;

        /// <summary>
        /// Atomically updates the stored value with the results 
        /// of applying the given function, returning the original value.
        /// </summary>
        /// <param name="value">Reference to a value to be modified.</param>
        /// <param name="updater">A side-effect-free function</param>
        /// <returns>The original value.</returns>
        public static double GetAndUpdate(ref this double value, Func<double, double> updater)
            => Atomic<double, CASProvider>.Update(ref value, updater).OldValue;

        /// <summary>
        /// Performs volatile read of the array element.
        /// </summary>
        /// <param name="array">The array to read from.</param>
        /// <param name="index">The array element index.</param>
        /// <returns>The array element.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double VolatileGet(this double[] array, long index)
            => VolatileGet(ref array[index]);

        /// <summary>
        /// Performs volatile write to the array element.
        /// </summary>
        /// <param name="array">The array to write into.</param>
        /// <param name="index">The array element index.</param>
        /// <param name="value">The new value of the array element.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void VolatileSet(this double[] array, long index, double value)
            => VolatileSet(ref array[index], value);

        /// <summary>
		/// Atomically increments the array element by one.
		/// </summary>
		/// <param name="array">The array to write into.</param>
        /// <param name="index">The index of the element to increment atomically.</param>
		/// <returns>Incremented array element.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double IncrementAndGet(this double[] array, long index)
            => IncrementAndGet(ref array[index]);

        /// <summary>
		/// Atomically decrements the array element by one.
		/// </summary>
		/// <param name="array">The array to write into.</param>
        /// <param name="index">The index of the array element to decrement atomically.</param>
		/// <returns>Decremented array element.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double DecrementAndGet(this double[] array, long index)
            => DecrementAndGet(ref array[index]);

        /// <summary>
		/// Atomically sets array element to the given updated value if the array element == the expected value.
		/// </summary>
		/// <param name="array">The array to be modified.</param>
        /// <param name="index">The index of the array element to be modified.</param>
		/// <param name="comparand">The expected value.</param>
		/// <param name="update">The new value.</param>
		/// <returns>The original value of the array element.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long CompareExchange(this long[] array, long index, long update, long comparand)
            => Interlocked.CompareExchange(ref array[index], update, comparand);

        /// <summary>
		/// Atomically sets array element to the given updated value if the array element == the expected value.
		/// </summary>
		/// <param name="array">The array to be modified.</param>
        /// <param name="index">The index of the array element to be modified.</param>
		/// <param name="expected">The expected value.</param>
		/// <param name="update">The new value.</param>
		/// <returns><see langword="true"/> if successful. <see langword="false"/> return indicates that the actual value was not equal to the expected value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CompareAndSet(this double[] array, long index, double expected, double update)
            => CompareAndSet(ref array[index], expected, update);

        /// <summary>
		/// Adds two 64-bit integers and replaces array element with the sum, 
		/// as an atomic operation.
		/// </summary>
		/// <param name="array">The array to be modified.</param>
        /// <param name="index">The index of the array element to be modified.</param>
		/// <param name="operand">The value to be added to the currently stored integer.</param>
		/// <returns>Result of sum operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Add(this double[] array, long index, double operand)
            => Add(ref array[index], operand);

        /// <summary>
		/// Modifies the array element atomically.
		/// </summary>
		/// <param name="array">The array to be modified.</param>
        /// <param name="index">The index of array element to be modified.</param>
		/// <param name="update">A new value to be stored as array element.</param>
		/// <returns>Original array element before modification.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double GetAndSet(this double[] array, long index, double update)
            => GetAndSet(ref array[index], update);

        /// <summary>
		/// Modifies the array element atomically.
		/// </summary>
		/// <param name="array">The array to be modified.</param>
        /// <param name="index">The index of array element to be modified.</param>
		/// <param name="update">A new value to be stored as array element.</param>
		/// <returns>The array element after modification.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double SetAndGet(this double[] array, long index, double update)
        {
            VolatileSet(array, index, update);
            return update;
        }

        /// <summary>
		/// Atomically updates the array element with the results of applying the given function 
		/// to the array element and given values, returning the updated value.
		/// </summary>
		/// <remarks>
		/// The function is applied with the array element as its first argument, and the given update as the second argument.
		/// </remarks>
		/// <param name="array">The array to be modified.</param>
        /// <param name="index">The index of the array element to be modified.</param>
		/// <param name="x">Accumulator operand.</param>
		/// <param name="accumulator">A side-effect-free function of two arguments.</param>
		/// <returns>The updated value.</returns>
		public static double AccumulateAndGet(this double[] array, long index, double x, Func<double, double, double> accumulator)
            => AccumulateAndGet(ref array[index], x, accumulator);

        /// <summary>
		/// Atomically updates the array element with the results of applying the given function 
		/// to the array element and given values, returning the original value.
		/// </summary>
		/// <remarks>
		/// The function is applied with the array element as its first argument, and the given update as the second argument.
		/// </remarks>
		/// <param name="array">The array to be modified.</param>
        /// <param name="index">The index of the array element to be modified.</param>
		/// <param name="x">Accumulator operand.</param>
		/// <param name="accumulator">A side-effect-free function of two arguments.</param>
		/// <returns>The original value of the array element.</returns>
		public static double GetAndAccumulate(this double[] array, long index, double x, Func<double, double, double> accumulator)
            => GetAndAccumulate(ref array[index], x, accumulator);

        /// <summary>
		/// Atomically updates the array element with the results 
		/// of applying the given function, returning the updated value.
		/// </summary>
		/// <param name="array">The array to be modified.</param>
        /// <param name="index">The index of the array element to be modified.</param>
		/// <param name="updater">A side-effect-free function</param>
		/// <returns>The updated value.</returns>
		public static double UpdateAndGet(this double[] array, long index, Func<double, double> updater)
            => UpdateAndGet(ref array[index], updater);

        /// <summary>
		/// Atomically updates the array element with the results 
		/// of applying the given function, returning the original value.
		/// </summary>
		/// <param name="array">The array to be modified.</param>
        /// <param name="index">The index of the array element to be modified.</param>
		/// <param name="updater">A side-effect-free function</param>
		/// <returns>The original value of the array element.</returns>
		public static double GetAndUpdate(this double[] array, long index, Func<double, double> updater)
            => GetAndUpdate(ref array[index], updater);
    }
}
