// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

namespace System.Threading
{
    /// <summary>
    /// Signals to a <see cref="System.Threading.CancellationToken"/> that it should be canceled.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="T:System.Threading.CancellationTokenSource"/> is used to instantiate a <see
    /// cref="T:System.Threading.CancellationToken"/>
    /// (via the source's <see cref="System.Threading.CancellationTokenSource.Token">Token</see> property)
    /// that can be handed to operations that wish to be notified of cancellation or that can be used to
    /// register asynchronous operations for cancellation. That token may have cancellation requested by
    /// calling to the source's <see cref="System.Threading.CancellationTokenSource.Cancel()">Cancel</see>
    /// method.
    /// </para>
    /// <para>
    /// All members of this class, except <see cref="Dispose">Dispose</see>, are thread-safe and may be used
    /// concurrently from multiple threads.
    /// </para>
    /// </remarks>
    public class CancellationTokenSource : IDisposable
    {
        //static sources that can be used as the backing source for 'fixed' CancellationTokens that never change state.
        private static readonly CancellationTokenSource _staticSourceSet = new CancellationTokenSource(true);
        private static readonly CancellationTokenSource _staticSourceNotCancelable = new CancellationTokenSource(false);
        //lazily initialized if required.
        private ManualResetEvent _kernelEvent;
        private ArrayList _registeredCallbacksLists;
        private CancellationTokenRegistration[] _linkingRegistrations;
        private Action _executingCallback;
        // legal values for state
        private const int CannotBeCancelled = 0;
        private const int NotCancelled = 1;
        private const int Notifying = 2;
        private const int NotifyingCompleted = 3; 

        private int _state;
        private bool _disposed;

        // Timer for cancellation after specific amount of time
        private static readonly TimerCallback _timerCallback = new TimerCallback(TimerCallbackLogic);
        /// The ID of the thread currently executing the main body of CTS.Cancel()
        /// this helps us to know if a call to ctr.Dispose() is running 'within' a cancellation callback.
        /// This is updated as we move between the main thread calling cts.Cancel() and any syncContexts that are used to 
        /// actually run the callbacks.
        private int _threadIDExecutingCallbacks = -1;
        // provided for CancelAfter and timer-related constructors
        private Timer _timer;

        /// <summary>
        /// Gets whether cancellation has been requested for this <see
        /// cref="System.Threading.CancellationTokenSource">CancellationTokenSource</see>.
        /// </summary>
        /// <value>Whether cancellation has been requested for this <see
        /// cref="System.Threading.CancellationTokenSource">CancellationTokenSource</see>.</value>
        /// <remarks>
        /// <para>
        /// This property indicates whether cancellation has been requested for this token source, such as
        /// due to a call to its
        /// <see cref="System.Threading.CancellationTokenSource.Cancel()">Cancel</see> method.
        /// </para>
        /// <para>
        /// If this property returns true, it only guarantees that cancellation has been requested. It does not
        /// guarantee that every handler registered with the corresponding token has finished executing, nor
        /// that cancellation requests have finished propagating to all registered handlers. Additional
        /// synchronization may be required, particularly in situations where related objects are being
        /// canceled concurrently.
        /// </para>
        /// </remarks>
        public bool IsCancellationRequested
        {
            get { return _state >= Notifying; }
        }

        /// <summary>
        /// A simple helper to determine whether cancellation has finished.
        /// </summary>
        internal bool IsCancellationCompleted
        {
            get { return _state == NotifyingCompleted; }
        }

        /// <summary>
        /// A simple helper to determine whether disposal has occured.
        /// </summary>
        internal bool IsDisposed
        {
            get { return _disposed; }
        }

        /// <summary>
        /// The ID of the thread that is running callbacks.
        /// </summary>
        internal int ThreadIDExecutingCallbacks
        {
            set { _threadIDExecutingCallbacks = value; }
            get { return _threadIDExecutingCallbacks; }
        }

        /// <summary>
        /// Gets the <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// associated with this <see cref="CancellationTokenSource"/>.
        /// </summary>
        /// <value>The <see cref="System.Threading.CancellationToken">CancellationToken</see>
        /// associated with this <see cref="CancellationTokenSource"/>.</value>
        /// <exception cref="T:System.ObjectDisposedException">The token source has been
        /// disposed.</exception>
        public CancellationToken Token
        {
            get
            {
                ThrowIfDisposed();
                return new CancellationToken(this);
            }
        }

        internal bool CanBeCanceled
        {
            get { return _state != CannotBeCancelled; }
        }

        internal WaitHandle WaitHandle
        {
            get
            {
                ThrowIfDisposed();

                // fast path if already allocated.
                if (_kernelEvent != null)
                    return _kernelEvent;

                _kernelEvent = new ManualResetEvent(false);

                // There is a ---- between checking IsCancellationRequested and setting the event.
                // However, at this point, the kernel object definitely exists and the cases are:
                //   1. if IsCancellationRequested = true, then we will call Set()
                //   2. if IsCancellationRequested = false, then NotifyCancellation will see that the event exists, and will call Set().
                if (IsCancellationRequested)
                    _kernelEvent.Set();

                return _kernelEvent;
            }
        }

        /// <summary>
        /// Throws an exception if the source has been disposed.
        /// </summary>
        internal void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException();
            }
        }

        /// <summary>
        /// Constructs a <see cref="T:System.Threading.CancellationTokenSource"/> that will be canceled after a specified time span.
        /// </summary>
        /// <param name="millisecondsDelay">The time span to wait before canceling this <see cref="T:System.Threading.CancellationTokenSource"/></param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The exception that is thrown when <paramref name="millisecondsDelay"/> is less than -1.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The countdown for the millisecondsDelay starts during the call to the constructor.  When the millisecondsDelay expires, 
        /// the constructed <see cref="T:System.Threading.CancellationTokenSource"/> is canceled (if it has
        /// not been canceled already).
        /// </para>
        /// <para>
        /// Subsequent calls to CancelAfter will reset the millisecondsDelay for the constructed 
        /// <see cref="T:System.Threading.CancellationTokenSource"/>, if it has not been
        /// canceled already.
        /// </para>
        /// </remarks>
        public CancellationTokenSource(Int32 millisecondsDelay)
        {
            if (millisecondsDelay < -1)
            {
                throw new ArgumentOutOfRangeException();
            }

            InitializeWithTimer(millisecondsDelay);
        }

        /// <summary>
        /// Constructs a <see cref="T:System.Threading.CancellationTokenSource"/> that will be canceled after a specified time span.
        /// </summary>
        /// <param name="delay">The time span to wait before canceling this <see cref="T:System.Threading.CancellationTokenSource"/></param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The exception that is thrown when <paramref name="delay"/> is less than -1 or greater than Int32.MaxValue.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The countdown for the delay starts during the call to the constructor.  When the delay expires, 
        /// the constructed <see cref="T:System.Threading.CancellationTokenSource"/> is canceled, if it has
        /// not been canceled already.
        /// </para>
        /// <para>
        /// Subsequent calls to CancelAfter will reset the delay for the constructed 
        /// <see cref="T:System.Threading.CancellationTokenSource"/>, if it has not been
        /// canceled already.
        /// </para>
        /// </remarks>
        public CancellationTokenSource(TimeSpan delay)
        {
            long totalMilliseconds = (long)delay.TotalMilliseconds;
            if (totalMilliseconds < -1 || totalMilliseconds > Int32.MaxValue)
            {
                throw new ArgumentOutOfRangeException();
            }

            InitializeWithTimer((int)totalMilliseconds);
        }

        /// <summary>
        /// Initializes the <see cref="T:System.Threading.CancellationTokenSource"/>.
        /// </summary>
        public CancellationTokenSource()
        {
            _state = NotCancelled;
        }

        // ** Private constructors for static sources.
        // set=false ==> cannot be canceled.
        // set=true  ==> is canceled. 
        private CancellationTokenSource(bool set)
        {
            _state = set ? NotifyingCompleted : CannotBeCancelled;
        }

        // Common initialization logic when constructing a CTS with a delay parameter
        private void InitializeWithTimer(Int32 millisecondsDelay)
        {
            _state = NotCancelled;
            _timer = new Timer(_timerCallback, this, millisecondsDelay, -1);
        }

        /// <summary>
        /// Communicates a request for cancellation.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The associated <see cref="T:System.Threading.CancellationToken" /> will be
        /// notified of the cancellation and will transition to a state where 
        /// <see cref="System.Threading.CancellationToken.IsCancellationRequested">IsCancellationRequested</see> returns true. 
        /// Any callbacks or cancelable operations
        /// registered with the <see cref="T:System.Threading.CancellationToken"/>  will be executed.
        /// </para>
        /// <para>
        /// Cancelable operations and callbacks registered with the token should not throw exceptions.
        /// However, this overload of Cancel will aggregate any exceptions thrown into a <see cref="System.AggregateException"/>,
        /// such that one callback throwing an exception will not prevent other registered callbacks from being executed.
        /// </para>
        /// <para>
        /// The <see cref="T:System.Threading.ExecutionContext"/> that was captured when each callback was registered
        /// will be reestablished when the callback is invoked.
        /// </para>
        /// </remarks>
        /// <exception cref="T:System.AggregateException">An aggregate exception containing all the exceptions thrown
        /// by the registered callbacks on the associated <see cref="T:System.Threading.CancellationToken"/>.</exception>
        /// <exception cref="T:System.ObjectDisposedException">This <see
        /// cref="T:System.Threading.CancellationTokenSource"/> has been disposed.</exception> 
        public void Cancel()
        {
            Cancel(false);
        }

        /// <summary>
        /// Communicates a request for cancellation.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The associated <see cref="T:System.Threading.CancellationToken" /> will be
        /// notified of the cancellation and will transition to a state where 
        /// <see cref="System.Threading.CancellationToken.IsCancellationRequested">IsCancellationRequested</see> returns true. 
        /// Any callbacks or cancelable operations
        /// registered with the <see cref="T:System.Threading.CancellationToken"/>  will be executed.
        /// </para>
        /// <para>
        /// Cancelable operations and callbacks registered with the token should not throw exceptions. 
        /// If <paramref name="throwOnFirstException"/> is true, an exception will immediately propagate out of the
        /// call to Cancel, preventing the remaining callbacks and cancelable operations from being processed.
        /// If <paramref name="throwOnFirstException"/> is false, this overload will aggregate any 
        /// exceptions thrown into a <see cref="System.AggregateException"/>,
        /// such that one callback throwing an exception will not prevent other registered callbacks from being executed.
        /// </para>
        /// <para>
        /// The <see cref="T:System.Threading.ExecutionContext"/> that was captured when each callback was registered
        /// will be reestablished when the callback is invoked.
        /// </para>
        /// </remarks>
        /// <param name="throwOnFirstException">Specifies whether exceptions should immediately propagate.</param>
        /// <exception cref="T:System.AggregateException">An aggregate exception containing all the exceptions thrown
        /// by the registered callbacks on the associated <see cref="T:System.Threading.CancellationToken"/>.</exception>
        /// <exception cref="T:System.ObjectDisposedException">This <see
        /// cref="T:System.Threading.CancellationTokenSource"/> has been disposed.</exception> 
        public void Cancel(bool throwOnFirstException)
        {
            ThrowIfDisposed();
            NotifyCancellation(throwOnFirstException);
        }

        /// <summary>
        /// Schedules a Cancel operation on this <see cref="T:System.Threading.CancellationTokenSource"/>.
        /// </summary>
        /// <param name="delay">The time span to wait before canceling this <see
        /// cref="T:System.Threading.CancellationTokenSource"/>.
        /// </param>
        /// <exception cref="T:System.ObjectDisposedException">The exception thrown when this <see
        /// cref="T:System.Threading.CancellationTokenSource"/> has been disposed.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The exception thrown when <paramref name="delay"/> is less than -1 or 
        /// greater than Int32.MaxValue.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The countdown for the delay starts during this call.  When the delay expires, 
        /// this <see cref="T:System.Threading.CancellationTokenSource"/> is canceled, if it has
        /// not been canceled already.
        /// </para>
        /// <para>
        /// Subsequent calls to CancelAfter will reset the delay for this  
        /// <see cref="T:System.Threading.CancellationTokenSource"/>, if it has not been
        /// canceled already.
        /// </para>
        /// </remarks>
        public void CancelAfter(TimeSpan delay)
        {
            long totalMilliseconds = (long)delay.TotalMilliseconds;
            if (totalMilliseconds < -1 || totalMilliseconds > Int32.MaxValue)
            {
                throw new ArgumentOutOfRangeException();
            }

            CancelAfter((int)totalMilliseconds);
        }

        /// <summary>
        /// Schedules a Cancel operation on this <see cref="T:System.Threading.CancellationTokenSource"/>.
        /// </summary>
        /// <param name="millisecondsDelay">The time span to wait before canceling this <see
        /// cref="T:System.Threading.CancellationTokenSource"/>.
        /// </param>
        /// <exception cref="T:System.ObjectDisposedException">The exception thrown when this <see
        /// cref="T:System.Threading.CancellationTokenSource"/> has been disposed.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The exception thrown when <paramref name="millisecondsDelay"/> is less than -1.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The countdown for the millisecondsDelay starts during this call.  When the millisecondsDelay expires, 
        /// this <see cref="T:System.Threading.CancellationTokenSource"/> is canceled, if it has
        /// not been canceled already.
        /// </para>
        /// <para>
        /// Subsequent calls to CancelAfter will reset the millisecondsDelay for this  
        /// <see cref="T:System.Threading.CancellationTokenSource"/>, if it has not been
        /// canceled already.
        /// </para>
        /// </remarks>
        public void CancelAfter(Int32 millisecondsDelay)
        {
            ThrowIfDisposed();

            if (millisecondsDelay < -1)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (IsCancellationRequested) return;

            // There is a race condition here as a Cancel could occur between the check of
            // IsCancellationRequested and the creation of the timer.  This is benign; in the 
            // worst case, a timer will be created that has no effect when it expires.

            // Also, if Dispose() is called right here (after ThrowIfDisposed(), before timer
            // creation), it would result in a leaked Timer object (at least until the timer
            // expired and Disposed itself).  But this would be considered bad behavior, as
            // Dispose() is not thread-safe and should not be called concurrently with CancelAfter().

            if (_timer == null)
            {
                // Lazily initialize the timer in a thread-safe fashion.
                // Initially set to "never go off" because we don't want to take a
                // chance on a timer "losing" the initialization ---- and then
                // cancelling the token before it (the timer) can be disposed.
                _timer = new Timer(_timerCallback, this, -1, -1);
            }

            // It is possible that m_timer has already been disposed, so we must do
            // the following in a try/catch block.
            try
            {
                _timer.Change(millisecondsDelay, -1);
            }
            catch (ObjectDisposedException)
            {
                // Just eat the exception.  There is no other way to tell that
                // the timer has been disposed, and even if there were, there
                // would not be a good way to deal with the observe/dispose
                // race condition.
            }
        }

        // Common logic for a timer delegate
        private static void TimerCallbackLogic(object obj)
        {
            CancellationTokenSource cts = (CancellationTokenSource)obj;

            // Cancel the source; handle a race condition with cts.Dispose()
            if (!cts.IsDisposed)
            {
                // There is a small window for a race condition where a cts.Dispose can sneak
                // in right here.  I'll wrap the cts.Cancel() in a try/catch to proof us
                // against this ----.
                try
                {
                    cts.Cancel(); // will take care of disposing of m_timer
                }
                catch (ObjectDisposedException)
                {
                    // If the ODE was not due to the target cts being disposed, then propagate the ODE.
                    if (!cts.IsDisposed) throw;
                }
            }
        }

        /// <summary>
        /// Releases the resources used by this <see cref="T:System.Threading.CancellationTokenSource" />.
        /// </summary>
        /// <remarks>
        /// This method is not thread-safe for any other concurrent calls.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.Threading.CancellationTokenSource" /> class and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            // There is nothing to do if disposing=false because the CancellationTokenSource holds no unmanaged resources.

            if (disposing)
            {
                //NOTE: We specifically tolerate that a callback can be deregistered
                //      after the CTS has been disposed and/or concurrently with cts.Dispose().
                //      This is safe without locks because the reg.Dispose() only
                //      mutates a sparseArrayFragment and then reads from properties of the CTS that are not
                //      invalidated by cts.Dispose().
                //     
                //      We also tolerate that a callback can be registered after the CTS has been
                //      disposed.  This is safe without locks because InternalRegister is tolerant
                //      of m_registeredCallbacksLists becoming null during its execution.  However,
                //      we run the acceptable risk of m_registeredCallbacksLists getting reinitialized
                //      to non-null if there is a ---- between Dispose and Register, in which case this
                //      instance may unnecessarily hold onto a registered callback.  But that's no worse
                //      than if Dispose wasn't safe to use concurrently, as Dispose would never be called,
                //      and thus no handlers would be dropped.

                if (_disposed)
                    return;

                if (_timer != null) _timer.Dispose();

                var linkingRegistrations = _linkingRegistrations;
                if (linkingRegistrations != null)
                {
                    _linkingRegistrations = null; // free for GC once we're done enumerating
                    for (int i = 0; i < linkingRegistrations.Length; i++)
                    {
                        linkingRegistrations[i].Dispose();
                    }
                }

                // registered callbacks are now either complete or will never run, due to guarantees made by ctr.Dispose()
                // so we can now perform main disposal work without risk of linking callbacks trying to use this CTS.

                _registeredCallbacksLists = null; // free for GC.

                if (_kernelEvent != null)
                {
                    //m_kernelEvent.Close(); // the critical cleanup to release an OS handle
                    _kernelEvent = null; // free for GC.
                }

                _disposed = true;
            }
        }

        internal static CancellationTokenSource InternalGetStaticSource(bool set)
        {
            return set ? _staticSourceSet : _staticSourceNotCancelable;
        }

        private void NotifyCancellation(bool throwOnFirstException)
        {
            // fast-path test to check if Notify has been called previously
            if (IsCancellationRequested)
                return;

            // If we're the first to signal cancellation, do the main extra work.
            if (Interlocked.CompareExchange(ref _state, Notifying, NotCancelled) == NotCancelled)
            {
                // Dispose of the timer, if any
                Timer timer = _timer;
                if (timer != null) timer.Dispose();

                //record the threadID being used for running the callbacks.
                ThreadIDExecutingCallbacks = Thread.CurrentThread.ManagedThreadId;

                //If the kernel event is null at this point, it will be set during lazy construction.
                if (_kernelEvent != null)
                    _kernelEvent.Set(); // update the MRE value.

                // - late enlisters to the Canceled event will have their callbacks called immediately in the Register() methods.
                // - Callbacks are not called inside a lock.
                // - After transition, no more delegates will be added to the 
                // - list of handlers, and hence it can be consumed and cleared at leisure by ExecuteCallbackHandlers.
                ExecuteCallbackHandlers(throwOnFirstException);
            }
        }

        internal CancellationTokenRegistration InternalRegister(Action callback)
        {
            if (!IsCancellationRequested)
            {
                if (_disposed)
                {
                    return new CancellationTokenRegistration();
                }

                _registeredCallbacksLists = _registeredCallbacksLists ?? new ArrayList();
                _registeredCallbacksLists.Add(callback);
                CancellationTokenRegistration registration = new CancellationTokenRegistration(callback, this);
                if (!IsCancellationRequested)
                {
                    return registration;
                }

                bool deregisterOccurred = registration.TryDeregister();

                if (!deregisterOccurred)
                {
                    // The thread that is running Cancel() snagged our callback for execution.
                    // So we don't need to run it, but we do return the registration so that 
                    // ctr.Dispose() will wait for callback completion.
                    return registration;
                }
            }

            callback?.Invoke();
            return new CancellationTokenRegistration();
        }

        internal void Unregister(Action callback)
        {
            if (!IsCancellationRequested)
            {
                if (_registeredCallbacksLists != null)
                {
                    _registeredCallbacksLists.Remove(callback);

                }
            }
        }

        private void ExecuteCallbackHandlers(bool throwOnFirstException)
        {
            // Design decision: call the delegates in LIFO order so that callbacks fire 'deepest first'.
            // This is intended to help with nesting scenarios so that child enlisters cancel before their parents.
            bool exception = false;

            // If there are no callbacks to run, we can safely exit.  Any ----s to lazy initialize it
            // will see IsCancellationRequested and will then run the callback themselves.
            if (_registeredCallbacksLists == null)
            {
                Interlocked.Exchange(ref _state, NotifyingCompleted);
                return;
            }

            try
            {
                ArrayList toExecute = new ArrayList();
                // Copy the current one in here
                foreach(var callback in _registeredCallbacksLists)
                {
                    toExecute.Add(callback);
                }

                for (int index = toExecute.Count-1; index >= 0; index--)
                {
                    _executingCallback = (Action)toExecute[index];
                    try
                    {
                        Unregister(_executingCallback);
                        _executingCallback.Invoke();
                    }
                    catch
                    {
                        exception = true;
                    }
                }
            }
            finally
            {
                _state = NotifyingCompleted;
                _executingCallback = null;
            }

            if (exception)
            {
                throw new Exception();
            }
        }

        // Wait for a single callback to complete (or, more specifically, to not be running).
        // It is ok to call this method if the callback has already finished.
        // Calling this method before the target callback has been selected for execution would be an error.
        internal void WaitForCallbackToComplete(Action callbackInfo)
        {
            SpinWait sw = new SpinWait();
            while (_executingCallback == callbackInfo)
            {
                sw.SpinOnce();  //spin as we assume callback execution is fast and that this situation is rare.
            }
        }
    }
}