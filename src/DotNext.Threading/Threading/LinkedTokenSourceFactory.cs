using Debug = System.Diagnostics.Debug;

namespace DotNext.Threading;

/// <summary>
/// Represents helper methods for working with linked cancellation tokens.
/// </summary>
public static class LinkedTokenSourceFactory
{
    /// <summary>
    /// Links two cancellation tokens.
    /// </summary>
    /// <param name="first">The first cancellation token. Can be modified by this method.</param>
    /// <param name="second">The second cancellation token.</param>
    /// <returns>The linked token source; or <see langword="null"/> if <paramref name="first"/> or <paramref name="second"/> is not cancelable.</returns>
    public static LinkedCancellationTokenSource? LinkTo(this ref CancellationToken first, CancellationToken second)
    {
        var result = default(LinkedCancellationTokenSource);

        if (first == second)
        {
            // nothing to do, just return from this method
        }
        else if (!first.CanBeCanceled || second.IsCancellationRequested)
        {
            first = second;
        }
        else if (second.CanBeCanceled && !first.IsCancellationRequested)
        {
            result = new Linked2CancellationTokenSource(in first, in second);
            first = result.Token;
        }

        return result;
    }

    /// <summary>
    /// Links cancellation token with the timeout.
    /// </summary>
    /// <param name="token">The first cancellation token. Can be modified by this method.</param>
    /// <param name="timeout">The timeout to link.</param>
    /// <returns>The linked token source; or <see langword="null"/> if <paramref name="token"/> is canceled or <paramref name="timeout"/> is <see cref="System.Threading.Timeout.InfiniteTimeSpan"/>.</returns>
    public static CancellationTokenSource? LinkTo(this ref CancellationToken token, TimeSpan timeout)
    {
        CancellationTokenSource? result;

        if (token.IsCancellationRequested || timeout < default(TimeSpan))
        {
            result = null;
        }
        else
        {
            result = token.CanBeCanceled ? CancellationTokenSource.CreateLinkedTokenSource(token) : new CancellationTokenSource();
            result.CancelAfter(timeout);
            token = result.Token;
        }

        return result;
    }

    /// <summary>
    /// Links two cancellation tokens with the given timeout.
    /// </summary>
    /// <param name="first">The first cancellation token. Can be modified by this method.</param>
    /// <param name="timeout">The timeout to link.</param>
    /// <param name="second">The second cancellation token.</param>
    /// <returns>The linked token source; or <see langword="null"/> if not needed.</returns>
    public static CancellationTokenSource? LinkTo(this ref CancellationToken first, TimeSpan timeout, CancellationToken second)
    {
        CancellationTokenSource? result;

        if (first == second)
        {
            result = LinkTo(ref first, timeout);
        }
        else if (timeout < TimeSpan.Zero || !second.CanBeCanceled || first.IsCancellationRequested || second.IsCancellationRequested)
        {
            result = LinkTo(ref first, second);
        }
        else if (!first.CanBeCanceled)
        {
            result = LinkTo(ref second, timeout);
            first = second;
        }
        else
        {
            result = null;
        }

        return result;
    }

    /// <summary>
    /// Determines whether the operation was canceled by the specified source.
    /// </summary>
    /// <param name="source">The linked token source.</param>
    /// <param name="e">The exception to analyze.</param>
    /// <param name="token">The token to check</param>
    /// <returns><see langword="true"/> indicates that the cancellation caused by <paramref name="source"/> and <see cref="LinkedCancellationTokenSource.CancellationOrigin"/> is <paramref name="token"/> ;or by <paramref name="token"/>.</returns>
    public static bool CausedBy(this OperationCanceledException e, LinkedCancellationTokenSource? source, CancellationToken token)
        => source is null ? e.CancellationToken == token : (e.CancellationToken == source.Token && source.CancellationOrigin == token);

    private sealed class Linked2CancellationTokenSource : LinkedCancellationTokenSource
    {
        private readonly CancellationTokenRegistration registration1, registration2;

        internal Linked2CancellationTokenSource(in CancellationToken token1, in CancellationToken token2)
        {
            Debug.Assert(token1.CanBeCanceled);
            Debug.Assert(token2.CanBeCanceled);

            registration1 = token1.UnsafeRegister(CancellationCallback, this);
            registration2 = token2.UnsafeRegister(CancellationCallback, this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                registration1.Dispose();
                registration2.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}