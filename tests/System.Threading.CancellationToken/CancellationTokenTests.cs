// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.TestFramework;
using System;
using System.Diagnostics;
using System.Threading;

namespace System.Threading
{
    [TestClass]
    public class CancellationTokenTests
    {
        static bool isCancelledFunction = false;
        static bool isCancelledAction = false;

        [TestMethod]
        public void SimmpleTokenCancellationTest()
        {
            // Arrange
            CancellationTokenSource cs = new ();
            CancellationToken token = cs.Token;
            // Act
            new Thread(() =>
            {
                Thread.Sleep(100);
                cs.Cancel();
            }).Start();
            TestCancealltionToken(token);
            // Assert
            Assert.True(isCancelledFunction);
        }

        private void TestCancealltionToken(CancellationToken csToken)
        {
            while(!csToken.IsCancellationRequested)
            {
                Thread.Sleep(1);
            }

            isCancelledFunction = true;
        }

        private void ActionToNotify()
        {
            isCancelledAction = true;
        }

        [TestMethod]
        public void SimmpleTokenCancellationThreadsTest()
        {
            // Arrange
            CancellationTokenSource cs = new();
            CancellationToken token = cs.Token;
            isCancelledFunction = false;
            // Act
            var toCancelThread = new Thread(() =>
             {
                 TestCancealltionToken(token);
             });
            toCancelThread.Start();
            new Thread(() =>
            {
                Thread.Sleep(100);
                cs.Cancel();
            }).Start();
            // We have to wait for the 2 other threads to finish
            Thread.Sleep(200);
            // Assert
            // Clean in case
            toCancelThread?.Abort();
            Assert.True(isCancelledFunction);

        }

        [TestMethod]
        public void CallbackRegistrationTests()
        {
            // Arrange
            isCancelledFunction = false;
            CancellationTokenSource cs = new();
            CancellationToken token = cs.Token;
            CancellationTokenRegistration ctr = token.Register(ActionToNotify);
            // Act
            cs.Cancel();
            Assert.True(isCancelledAction);
        }


        [TestMethod]
        public void CallbackRegistrationAndThreadsTests()
        {
            // Arrange
            isCancelledFunction = false;
            isCancelledAction = false;
            CancellationTokenSource cs = new();
            CancellationToken token = cs.Token;
            CancellationTokenRegistration ctr = token.Register(ActionToNotify);
            // Act
            var toCancelThread = new Thread(() =>
            {
                TestCancealltionToken(token);
            });
            toCancelThread.Start();

            cs.Cancel();
            cs.WaitForCallbackToComplete(ActionToNotify);
            // We wait a bit to have the thread getting the cancellation as well
            Thread.Sleep(10);
            // Assert
            // Clean in case
            toCancelThread?.Abort();

            Assert.True(isCancelledFunction);
            Assert.True(isCancelledAction);
        }

        [TestMethod]
        public void TimerCancellationTest()
        {
            CancellationTokenSource cs = new(200);
            CancellationToken token = cs.Token;
            Assert.False(token.IsCancellationRequested);
            Thread.Sleep(210);
            Assert.True(token.IsCancellationRequested);
        }

        [TestMethod]
        public void TimerCancellationTokenTest()
        {
            CancellationTokenSource cs = new();
            cs.CancelAfter(200);
            Assert.False(cs.IsCancellationRequested);
            Thread.Sleep(210);
            Assert.True(cs.IsCancellationRequested);
        }
    }
}
