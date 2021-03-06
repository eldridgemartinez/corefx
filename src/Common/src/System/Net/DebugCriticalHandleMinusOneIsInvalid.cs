// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Win32.SafeHandles;

using System.Diagnostics.CodeAnalysis;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Net
{
#if DEBUG
    //
    // This is a helper class for debugging GC-ed handles that we define.
    // As a general rule normal code path should always destroy handles explicitly
    //
    internal abstract class DebugCriticalHandleMinusOneIsInvalid : CriticalHandleMinusOneIsInvalid
    {
        private string _trace;

        protected DebugCriticalHandleMinusOneIsInvalid() : base()
        {
            Trace();
        }

        private void Trace()
        {
            _trace = "WARNING! GC-ed  >>" + this.GetType().FullName + "<< (should be excplicitly closed) \r\n";
            if (GlobalLog.IsEnabled)
            {
                GlobalLog.Print("Creating SafeHandle, type = " + this.GetType().FullName);
            }
#if TRACE_VERBOSE
            string stacktrace = Environment.StackTrace;
            _trace += stacktrace;
#endif //TRACE_VERBOSE
        }

        ~DebugCriticalHandleMinusOneIsInvalid()
        {
            GlobalLog.SetThreadSource(ThreadKinds.Finalization);
            if (GlobalLog.IsEnabled)
            {
                GlobalLog.Print(_trace);
            }
        }
    }
#endif // DEBUG
}
