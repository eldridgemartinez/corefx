// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;

using Xunit;

namespace System.Net.Sockets.Tests
{
    public class ConnectExTest
    {
        private static void OnConnectAsyncCompleted(object sender, SocketAsyncEventArgs args)
        {
            ManualResetEvent complete = (ManualResetEvent)args.UserToken;
            complete.Set();
        }

        [Fact]
        public void Success()
        {
            int port;
            SocketTestServer server = SocketTestServer.SocketTestServerFactory(IPAddress.Loopback, out port);
            SocketTestServer server6 = SocketTestServer.SocketTestServerFactory(new IPEndPoint(IPAddress.IPv6Loopback, port));

            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.RemoteEndPoint = new IPEndPoint(IPAddress.Loopback, port);
            args.Completed += OnConnectAsyncCompleted;

            ManualResetEvent complete = new ManualResetEvent(false);
            args.UserToken = complete;

            Assert.True(sock.ConnectAsync(args));

            Assert.True(complete.WaitOne(Configuration.PassingTestTimeout), "IPv4: Timed out while waiting for connection");

            Assert.True(args.SocketError == SocketError.Success);

            sock.Dispose();

            sock = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            args.RemoteEndPoint = new IPEndPoint(IPAddress.IPv6Loopback, port);
            complete.Reset();

            Assert.True(sock.ConnectAsync(args));

            Assert.True(complete.WaitOne(Configuration.PassingTestTimeout), "IPv6: Timed out while waiting for connection");

            Assert.True(args.SocketError == SocketError.Success);

            sock.Dispose();

            server.Dispose();
            server6.Dispose();
        }

        #region GC Finalizer test
        // This test assumes sequential execution of tests and that it is going to be executed after other tests
        // that used Sockets. 
        [Fact]
        public void TestFinalizers()
        {
            // Making several passes through the FReachable list.
            for (int i = 0; i < 3; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
        #endregion 
    }
}
