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
using System.Security;
using Microsoft.Win32.SafeHandles;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;

namespace Alachisoft.NCache.Common.Util
{
#if NETCORE
    [SuppressUnmanagedCodeSecurity, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
    internal sealed class SafeLocalMemHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeLocalMemHandle() : base(true) { }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        internal SafeLocalMemHandle(IntPtr existingHandle, bool ownsHandle) : base(ownsHandle)
        {
            SetHandle(existingHandle);
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool ConvertStringSecurityDescriptorToSecurityDescriptor(string stringSecurityDescriptor,
            int stringSDRevision, out SafeLocalMemHandle pSecurityDescriptor, IntPtr securityDescriptorSize);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll")]
        private static extern IntPtr LocalFree(IntPtr hMem);

        protected override bool ReleaseHandle()
        {
            return (LocalFree(handle) == IntPtr.Zero);
        }
    }
#endif
}
