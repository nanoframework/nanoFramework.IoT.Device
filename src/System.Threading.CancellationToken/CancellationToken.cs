// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Threading
{
    /// <summary>
    /// Propagates notification that operations should be canceled.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="CancellationToken"/> may be created directly in an unchangeable canceled or non-canceled state
    /// using the CancellationToken's constructors. However, to have a CancellationToken that can change 
    /// from a non-canceled to a canceled state, 
    /// <see cref="System.Threading.CancellationTokenSource">CancellationTokenSource</see> must be used.
    /// CancellationTokenSource exposes the associated CancellationToken that may be canceled by the source through its 
    /// <see cref="System.Threading.CancellationTokenSource.Token">Token</see> property. 
    /// </para>
    /// <para>
    /// Once canceled, a token may not transition to a non-canceled state, and a token whose 
    /// <see cref="CanBeCanceled"/> is false will never change to one that can be canceled.
    /// </para>
    /// <para>
    /// All members of this struct are thread-safe and may be used concurrently from multiple threads.
    /// </para>
    /// </remarks>
    public struct CancellationToken
    {
        // The backing TokenSource.  
        // if null, it implicitly represents the same thing as new CancellationToken(false).
        // When required, it will be instantiated to reflect this.
        private CancellationTokenSource _source;

        /// <summary>
        /// Returns an empty CancellationToken value.
        /// </summary>
        /// <remarks>
        /// The <see cref="CancellationToken"/> value returned by this property will be non-cancelable by default.
        /// </remarks>
        public static CancellationToken None
        {
            get { return default(CancellationToken); }
        }


        /// <summary>
        /// Gets whether cancellation has been requested for this token.
        /// </summary>
        /// <value>Whether cancellation has been requested for this token.</value>
        /// <remarks>
        /// <para>
        /// This property indicates whether cancellation has been requested for this token, 
        /// either through the token initially being construted in a canceled state, or through
        /// calling <see cref="System.Threading.CancellationTokenSource.Cancel()">Cancel</see> 
        /// on the token's associated <see cref="CancellationTokenSource"/>.
        /// </para>
        /// <para>
        /// If this property is true, it only guarantees that cancellation has been requested.  
        /// It does not guarantee that every registered handler
        /// has finished executing, nor that cancellation requests have finished propagating
        /// to all registered handlers.  Additional synchronization may be required,
        /// particularly in situations where related objects are being canceled concurrently.
        /// </para>
        /// </remarks>
        public bool IsCancellationRequested
        {
            get
            {
                return _source != null && _source.IsCancellationRequested;
            }
        }

        /// <summary>
        /// Gets whether this token is capable of being in the canceled state.
        /// </summary>
        /// <remarks>
        /// If CanBeCanceled returns false, it is guaranteed that the token will never transition
        /// into a canceled state, meaning that <see cref="IsCancellationRequested"/> will never
        /// return true.
        /// </remarks>
        public bool CanBeCanceled
        {
            get
            {
                return _source != null && _source.CanBeCanceled;
            }
        }

        /// <summary>
        /// Gets a <see cref="T:System.Threading.WaitHandle"/> that is signaled when the token is canceled.</summary>
        /// <remarks>
        /// Accessing this property causes a <see cref="T:System.Threading.WaitHandle">WaitHandle</see>
        /// to be instantiated.  It is preferable to only use this property when necessary, and to then
        /// dispose the associated <see cref="CancellationTokenSource"/> instance at the earliest opportunity (disposing
        /// the source will dispose of this allocated handle).  The handle should not be closed or disposed directly.
        /// </remarks>
        /// <exception cref="T:System.ObjectDisposedException">The associated <see
        /// cref="T:System.Threading.CancellationTokenSource">CancellationTokenSource</see> has been disposed.</exception>
        public WaitHandle WaitHandle
        {
            get
            {
                if (_source == null)
                {
                    _source = CancellationTokenSource.InternalGetStaticSource(false);
                }

                return _source.WaitHandle;
            }
        }

        /// <summary>
        /// Internal constructor only a CancellationTokenSource should create a CancellationToken
        /// </summary>
        internal CancellationToken(CancellationTokenSource source)
        {
            _source = source;
        }

        /// <summary>
        /// Initializes the <see cref="T:System.Threading.CancellationToken">CancellationToken</see>.
        /// </summary>
        /// <param name="canceled">
        /// The canceled state for the token.
        /// </param>
        /// <remarks>
        /// Tokens created with this constructor will remain in the canceled state specified
        /// by the <paramref name="canceled"/> parameter.  If <paramref name="canceled"/> is false,
        /// both <see cref="CanBeCanceled"/> and <see cref="IsCancellationRequested"/> will be false.
        /// If <paramref name="canceled"/> is true,
        /// both <see cref="CanBeCanceled"/> and <see cref="IsCancellationRequested"/> will be true. 
        /// </remarks>
        public CancellationToken(bool canceled) :
            this()
        {
            if (canceled)
                _source = CancellationTokenSource.InternalGetStaticSource(canceled);
        }

        /// <summary>
        /// Registers a delegate that will be called when this <see cref="T:System.Threading.CancellationToken">CancellationToken</see> is canceled.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If this token is already in the canceled state, the
        /// delegate will be run immediately and synchronously. Any exception the delegate generates will be
        /// propagated out of this method call.
        /// </para>
        /// <para>
        /// The current <see cref="System.Threading.ExecutionContext">ExecutionContext</see>, if one exists, will be captured
        /// along with the delegate and will be used when executing it.
        /// </para>
        /// </remarks>
        /// <param name="callback">The delegate to be executed when the <see cref="T:System.Threading.CancellationToken">CancellationToken</see> is canceled.</param>
        /// <returns>The <see cref="T:System.Threading.CancellationTokenRegistration"/> instance that can 
        /// be used to deregister the callback.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="callback"/> is null.</exception>
        public CancellationTokenRegistration Register(Action callback)
        {
            if (callback == null)
                throw new ArgumentNullException();

            if (CanBeCanceled == false)
            {
                return new CancellationTokenRegistration(); // nothing to do for tokens than can never reach the canceled state. Give them a dummy registration.
            }

            // Register the callback with the source.
            return _source.InternalRegister(callback);
        }

        /// <summary>
        /// Determines whether the current <see cref="T:System.Threading.CancellationToken">CancellationToken</see> instance is equal to the 
        /// specified token.
        /// </summary>
        /// <param name="other">The other <see cref="T:System.Threading.CancellationToken">CancellationToken</see> to which to compare this
        /// instance.</param>
        /// <returns>True if the instances are equal; otherwise, false. Two tokens are equal if they are associated
        /// with the same <see cref="T:System.Threading.CancellationTokenSource">CancellationTokenSource</see> or if they were both constructed 
        /// from public CancellationToken constructors and their <see cref="IsCancellationRequested"/> values are equal.</returns>
        public bool Equals(CancellationToken other)
        {
            //if both sources are null, then both tokens represent the Empty token.
            if (_source == null && other._source == null)
            {
                return true;
            }

            // one is null but other has inflated the default source
            // these are only equal if the inflated one is the staticSource(false)
            if (_source == null)
            {
                return other._source == CancellationTokenSource.InternalGetStaticSource(false);
            }

            if (other._source == null)
            {
                return _source == CancellationTokenSource.InternalGetStaticSource(false);
            }

            // general case, we check if the sources are identical

            return _source == other._source;
        }

        /// <summary>
        /// Determines whether the current <see cref="T:System.Threading.CancellationToken">CancellationToken</see> instance is equal to the 
        /// specified <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="other">The other object to which to compare this instance.</param>
        /// <returns>True if <paramref name="other"/> is a <see cref="T:System.Threading.CancellationToken">CancellationToken</see>
        /// and if the two instances are equal; otherwise, false. Two tokens are equal if they are associated
        /// with the same <see cref="T:System.Threading.CancellationTokenSource">CancellationTokenSource</see> or if they were both constructed 
        /// from public CancellationToken constructors and their <see cref="IsCancellationRequested"/> values are equal.</returns>
        /// <exception cref="T:System.ObjectDisposedException">An associated <see
        /// cref="T:System.Threading.CancellationTokenSource">CancellationTokenSource</see> has been disposed.</exception>
        public override bool Equals(Object other)
        {
            if (other is CancellationToken)
            {
                return Equals((CancellationToken)other);
            }

            return false;
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="T:System.Threading.CancellationToken">CancellationToken</see>.
        /// </summary>
        /// <returns>A hash code for the current <see cref="T:System.Threading.CancellationToken">CancellationToken</see> instance.</returns>
        public override Int32 GetHashCode()
        {
            if (_source == null)
            {
                // link to the common source so that we have a source to interrogate.
                return CancellationTokenSource.InternalGetStaticSource(false).GetHashCode();
            }

            return _source.GetHashCode();
        }

        /// <summary>
        /// Throws a <see cref="T:System.OperationCanceledException">OperationCanceledException</see> if
        /// this token has had cancellation requested.
        /// </summary>
        /// <remarks>
        /// This method provides functionality equivalent to:
        /// <code>
        /// if (token.IsCancellationRequested) 
        ///    throw new OperationCanceledException(token);
        /// </code>
        /// </remarks>
        /// <exception cref="System.OperationCanceledException">The token has had cancellation requested.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The associated <see
        /// cref="T:System.Threading.CancellationTokenSource">CancellationTokenSource</see> has been disposed.</exception>
        public void ThrowIfCancellationRequested()
        {
            if (IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }

        }
    }
}