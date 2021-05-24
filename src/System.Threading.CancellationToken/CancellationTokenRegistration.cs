// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Threading
{
    /// <summary>
    /// Represents a callback delegate that has been registered with a <see cref="T:System.Threading.CancellationToken">CancellationToken</see>.
    /// </summary>
    /// <remarks>
    /// To unregister a callback, dispose the corresponding Registration instance.
    /// </remarks>
    public struct CancellationTokenRegistration : IDisposable
    {
        private readonly Action _callback;
        private readonly CancellationTokenSource _source;

        internal CancellationTokenRegistration(
            Action callbackInfo, CancellationTokenSource registrationInfo)
        {
            _callback = callbackInfo;
            _source = registrationInfo;
        }

        /// <summary>
        /// Attempts to deregister the item. If it's already being run, this may fail.
        /// Entails a full memory fence.
        /// </summary>
        /// <returns>True if the callback was found and deregistered, false otherwise.</returns>
        internal bool TryDeregister()
        {
            // Try to remove the callback info from the array.
            // It is possible the callback info is missing (removed for run, or removed by someone else)
            // It is also possible there is info in the array but it doesn't match our current registration's callback info.  
            try
            {
                _source.Unregister(_callback);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Disposes of the registration and unregisters the target callback from the associated 
        /// <see cref="T:System.Threading.CancellationToken">CancellationToken</see>.
        /// If the target callback is currently executing this method will wait until it completes, except
        /// in the degenerate cases where a callback method deregisters itself.
        /// </summary>
        public void Dispose()
        {
            // Remove the entry from the array.
            // This call includes a full memory fence which prevents potential reorderings of the reads below
            bool deregisterOccured = TryDeregister();

            // We guarantee that we will not return if the callback is being executed (assuming we are not currently called by the callback itself)
            // We achieve this by the following rules:
            //    1. if we are called in the context of an executing callback, no need to wait (determined by tracking callback-executor threadID)
            //       - if the currently executing callback is this CTR, then waiting would deadlock. (We choose to return rather than deadlock)
            //       - if not, then this CTR cannot be the one executing, hence no need to wait
            //
            //    2. if deregistration failed, and we are on a different thread, then the callback may be running under control of cts.Cancel()
            //       => poll until cts.ExecutingCallback is not the one we are trying to deregister.

            var callbackInfo = _callback;
            if (callbackInfo != null)
            {
                var tokenSource = _source;
                if (tokenSource.IsCancellationRequested && //running callbacks has commenced.
                    !tokenSource.IsCancellationCompleted && //running callbacks hasn't finished
                    !deregisterOccured)//deregistration failed (ie the callback is missing from the list)                    
                {
                    // Callback execution is in progress, the executing thread is different to us and has taken the callback for execution
                    // so observe and wait until this target callback is no longer the executing callback.
                    tokenSource.WaitForCallbackToComplete(_callback);
                }
            }
        }

        /// <summary>
        /// Determines whether two <see
        /// cref="T:System.Threading.CancellationTokenRegistration">CancellationTokenRegistration</see>
        /// instances are equal.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True if the instances are equal; otherwise, false.</returns>
        public static bool operator ==(CancellationTokenRegistration left, CancellationTokenRegistration right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="T:System.Threading.CancellationTokenRegistration">CancellationTokenRegistration</see> instances are not equal.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True if the instances are not equal; otherwise, false.</returns>
        public static bool operator !=(CancellationTokenRegistration left, CancellationTokenRegistration right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Determines whether the current <see cref="T:System.Threading.CancellationTokenRegistration">CancellationTokenRegistration</see> instance is equal to the 
        /// specified <see cref="T:System.Object"/>.
        /// </summary> 
        /// <param name="obj">The other object to which to compare this instance.</param>
        /// <returns>True, if both this and <paramref name="obj"/> are equal. False, otherwise.
        /// Two <see cref="T:System.Threading.CancellationTokenRegistration">CancellationTokenRegistration</see> instances are equal if
        /// they both refer to the output of a single call to the same Register method of a 
        /// <see cref="T:System.Threading.CancellationToken">CancellationToken</see>. 
        /// </returns>
        public override bool Equals(object obj)
        {
            return ((obj is CancellationTokenRegistration) && Equals((CancellationTokenRegistration)obj));
        }

        /// <summary>
        /// Determines whether the current <see cref="T:System.Threading.CancellationToken">CancellationToken</see> instance is equal to the 
        /// specified <see cref="T:System.Object"/>.
        /// </summary> 
        /// <param name="other">The other <see cref="T:System.Threading.CancellationTokenRegistration">CancellationTokenRegistration</see> to which to compare this instance.</param>
        /// <returns>True, if both this and <paramref name="other"/> are equal. False, otherwise.
        /// Two <see cref="T:System.Threading.CancellationTokenRegistration">CancellationTokenRegistration</see> instances are equal if
        /// they both refer to the output of a single call to the same Register method of a 
        /// <see cref="T:System.Threading.CancellationToken">CancellationToken</see>. 
        /// </returns>
        public bool Equals(CancellationTokenRegistration other)
        {
            return _callback == other._callback &&
                   _source == other._source;
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="T:System.Threading.CancellationTokenRegistration">CancellationTokenRegistration.</see>.
        /// </summary>
        /// <returns>A hash code for the current <see cref="T:System.Threading.CancellationTokenRegistration">CancellationTokenRegistration</see> instance.</returns>
        public override int GetHashCode()
        {
            if (_source != null)
                return _source.GetHashCode() ^ _callback.GetHashCode();

            return _callback.GetHashCode();
        }
    }
}