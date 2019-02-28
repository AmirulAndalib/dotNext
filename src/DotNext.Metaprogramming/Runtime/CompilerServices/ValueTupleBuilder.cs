﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DotNext.Runtime.CompilerServices
{
    /// <summary>
    /// Represents value tuple builder with arbitrary number of tuple
    /// items.
    /// </summary>
    /// <seealso cref="ValueTuple"/>
    /// <seealso href="https://docs.microsoft.com/en-us/dotnet/csharp/tuples">Tuples</seealso>
    public sealed class ValueTupleBuilder: Disposable, IEnumerable<Type>
    {
        private readonly IList<Type> items = new List<Type>(7);//no more than 7 items because max number of generic arguments of tuple type
        private ValueTupleBuilder Rest;

        /// <summary>
        /// Number of elements in the tuple.
        /// </summary>
        public int Count => items.Count + (Rest is null ? 0 : Rest.Count);

        /// <summary>
        /// Constructs value tuple.
        /// </summary>
        /// <returns>Value tuple.</returns>
        public Type Build()
        {
            switch (Count)
            {
                case 0:
                    return typeof(ValueTuple);
                case 1:
                    return typeof(ValueTuple<>).MakeGenericType(items[0]);
                case 2:
                    return typeof(ValueTuple<,>).MakeGenericType(items[0], items[1]);
                case 3:
                    return typeof(ValueTuple<,,>).MakeGenericType(items[0], items[1], items[2]);
                case 4:
                    return typeof(ValueTuple<,,,>).MakeGenericType(items[0], items[1], items[2], items[3]);
                case 5:
                    return typeof(ValueTuple<,,,,>).MakeGenericType(items[0], items[1], items[2], items[3], items[4]);
                case 6:
                    return typeof(ValueTuple<,,,,,>).MakeGenericType(items[0], items[1], items[2], items[3], items[4], items[5]);
                case 7:
                    return typeof(ValueTuple<,,,,,,>).MakeGenericType(items[0], items[1], items[2], items[3], items[4], items[5], items[6]);
                default:
                    return typeof(ValueTuple<,,,,,,,>).MakeGenericType(items[0], items[1], items[2], items[3], items[4], items[5], items[6], Rest.Build());
            }
        }

        private void Build(Expression instance, Span<MemberExpression> output)
        {
            for (var i = 0; i < items.Count; i++)
                output[i] = Expression.Field(instance, "Item" + (i + 1));
            if (!(Rest is null))
            {
                instance = Expression.Field(instance, "Rest");
                Build(instance, output.Slice(8));
            }
        }

        /// <summary>
        /// Constructs expression tree based on value tuple type.
        /// </summary>
        /// <typeparam name="E">Type of expression tree.</typeparam>
        /// <param name="expressionFactory">A function accepting value tuple type and returning expression tree.</param>
        /// <param name="expression">Constructed expression.</param>
        /// <returns>Sorted array of value tuple type components.</returns>
        public MemberExpression[] Build<E>(Func<Type, E> expressionFactory, out E expression)
            where E : Expression
        {
            expression = expressionFactory(Build());
            var fieldAccessExpression = new MemberExpression[Count];
            Build(expression, fieldAccessExpression.AsSpan());
            return fieldAccessExpression;
        }

        /// <summary>
        /// Adds a new component into tuple.
        /// </summary>
        /// <param name="itemType">The type of the tuple component.</param>
        public void Add(Type itemType)
        {
            if (Count < 7)
                items.Add(itemType);
            else if (Rest is null)
                Rest = new ValueTupleBuilder() { itemType };
            else
                Rest.Add(itemType);
        }

        /// <summary>
        /// Adds a new component into tuple.
        /// </summary>
        /// <typeparam name="T">The type of the tuple component.</typeparam>
        public void Add<T>() => Add(typeof(T));

        /// <summary>
        /// Returns an enumerator over all tuple components.
        /// </summary>
        /// <returns>An enumerator over all tuple components.</returns>
        public IEnumerator<Type> GetEnumerator()
            => (Rest is null ? items : Enumerable.Concat(items, Rest)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Releases all managed resources associated with this builder.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> if called from <see cref="Disposable.Dispose()"/>; otherwise, <see langword="false"/>.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                items.Clear();
                Rest?.Dispose(disposing);
            }
        }
    }
}
