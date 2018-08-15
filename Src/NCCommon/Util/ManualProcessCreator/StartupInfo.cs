// Copyright (c) 2018 Alachisoft
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License

using System;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace Alachisoft.NCache.Common.Util
{
#if NETCORE
    [StructLayout(LayoutKind.Sequential)]
    internal class StartupInfo
    {
        public int cb;
        public IntPtr lpReserved = IntPtr.Zero;
        public IntPtr lpDesktop = IntPtr.Zero;
        public IntPtr lpTitle = IntPtr.Zero;
        public int dwX;
        public int dwY;
        public int dwXSize;
        public int dwYSize;
        public int dwXCountChars;
        public int dwYCountChars;
        public int dwFillAttribute;
        public int dwFlags;
        public short wShowWindow;
        public short cbReserved2;
        public IntPtr lpReserved2 = IntPtr.Zero;
        public SafeFileHandle hStdInput = new SafeFileHandle(IntPtr.Zero, false);
        public SafeFileHandle hStdOutput = new SafeFileHandle(IntPtr.Zero, false);
        public SafeFileHandle hStdError = new SafeFileHandle(IntPtr.Zero, false);

        public StartupInfo()
        {
            dwY = -1;
            cb = Marshal.SizeOf(this);
        }

        public void Dispose()
        {
            // close the handles created for child process
            if (hStdInput != null && !hStdInput.IsInvalid)
            {
                hStdInput.Close();
                hStdInput = null;
            }

            if (hStdOutput != null && !hStdOutput.IsInvalid)
            {
                hStdOutput.Close();
                hStdOutput = null;
            }

            if (hStdError == null || hStdError.IsInvalid) return;

            hStdError.Close();
            hStdError = null;
        }
    }
#endif
}
