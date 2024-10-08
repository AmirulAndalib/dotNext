using System.Collections;
using System.ComponentModel;

namespace DotNext.Collections.Generic;

/// <summary>
/// Represents ad-hoc enumerator implemented as value type to avoid enumerator allocation
/// and prevent the compiler to generate <see cref="IDisposable.Dispose()"/> call.
/// </summary>
/// <remarks>
/// The enumerator doesn't implement <see cref="IEnumerator{T}"/> interface implicitly
/// but can be converted to it by using regular type cast.
/// </remarks>
/// <typeparam name="TSelf">The value type that implements an enumerator.</typeparam>
/// <typeparam name="T"></typeparam>
[EditorBrowsable(EditorBrowsableState.Advanced)]
public interface IEnumerator<in TSelf, out T>
    where TSelf : struct, IEnumerator<TSelf, T>
{
    /// <inheritdoc cref="IEnumerator.MoveNext()"/>
    bool MoveNext();

    /// <inheritdoc cref="IEnumerator{T}.Current"/>
    T Current { get; }

    /// <inheritdoc cref="IEnumerator.Reset()"/>
    void Reset() => throw new NotSupportedException();

    /// <summary>
    /// Converts ad-hoc enumerator to a generic enumerator.
    /// </summary>
    /// <param name="enumerator">Ad-hoc enumerator.</param>
    /// <returns>The enumerator over values of type <typeparamref name="T"/>.</returns>
    internal static virtual IEnumerator<T> ToEnumerator(TSelf enumerator)
        => new BoxedEnumerator<TSelf, T>(enumerator);

    /// <summary>
    /// Converts ad-hoc enumerator to a generic enumerator.
    /// </summary>
    /// <param name="enumerator">Ad-hoc enumerator.</param>
    /// <param name="token">The token that can be used to cancel the enumeration.</param>
    /// <returns>The enumerator over values of type <typeparamref name="T"/>.</returns>
    internal static virtual IAsyncEnumerator<T> ToEnumerator(TSelf enumerator, CancellationToken token)
        => new BoxedEnumerator<TSelf, T>(enumerator, token);
}

file sealed class BoxedEnumerator<TEnumerator, T>(TEnumerator enumerator, CancellationToken token = default) : IEnumerator<T>, IAsyncEnumerator<T>
    where TEnumerator : struct, IEnumerator<TEnumerator, T>
{
    public T Current => enumerator.Current;

    object? IEnumerator.Current => enumerator.Current;

    bool IEnumerator.MoveNext() => enumerator.MoveNext();

    ValueTask<bool> IAsyncEnumerator<T>.MoveNextAsync()
        => token.IsCancellationRequested ? ValueTask.FromCanceled<bool>(token) : ValueTask.FromResult(enumerator.MoveNext());

    void IEnumerator.Reset() => enumerator.Reset();

    void IDisposable.Dispose() => enumerator = default;

    ValueTask IAsyncDisposable.DisposeAsync()
    {
        enumerator = default;
        return ValueTask.CompletedTask;
    }
}