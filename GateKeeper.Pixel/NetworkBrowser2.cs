#region header
// GateKeeper.Pixel - NetworkBrowser2.cs
// 
// Copyright Untethered Labs, Inc.  All rights reserved.
// 
// Created: 2018-06-08 4:18 PM
#endregion

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;

namespace GateKeeper.Pixel
{
    internal static partial class NativeMethods
    {
        const int MAX_PREFERRED_LENGTH = -1;

        [Flags]
        public enum ServerTypes : uint
        {
            Workstation = 0x00000001,
            Server = 0x00000002,
            SqlServer = 0x00000004,
            DomainCtrl= 0x00000008,
            BackupDomainCtrl= 0x00000010,
            TimeSource= 0x00000020,
            AppleFilingProtocol = 0x00000040,
            Novell= 0x00000080,
            DomainMember = 0x00000100,
            PrintQueueServer = 0x00000200,
            DialinServer = 0x00000400,
            XenixServer = 0x00000800,
            UnixServer = 0x00000800,
            NT = 0x00001000,
            WindowsForWorkgroups = 0x00002000,
            MicrosoftFileAndPrintServer= 0x00004000,
            NTServer = 0x00008000,
            BrowserService = 0x00010000,
            BackupBrowserService= 0x00020000,
            MasterBrowserService= 0x00040000,
            DomainMaster = 0x00080000,
            OSF1Server = 0x00100000,
            VMSServer = 0x00200000,
            Windows = 0x00400000, 
            DFS = 0x00800000, 
            NTCluster = 0x01000000, 
            TerminalServer= 0x02000000, 
            VirtualNTCluster = 0x04000000, 
            DCE = 0x10000000, 
            AlternateTransport = 0x20000000, 
            LocalListOnly = 0x40000000, 
            PrimaryDomain = 0x80000000,
            All = 0xFFFFFFFF
        };

        public enum ServerPlatform
        {
            DOS = 300,
            OS2 = 400,
            NT = 500,
            OSF = 600,
            VMS = 700
        }

        [DllImport("Netapi32", CharSet = CharSet.Auto, SetLastError = true), SuppressUnmanagedCodeSecurityAttribute]
        private static extern int NetServerEnum(
            [MarshalAs(UnmanagedType.LPWStr)] string servernane, // must be null
            int level,
            out IntPtr bufptr,
            int prefmaxlen,
            out int entriesread,
            out int totalentries,
            ServerTypes servertype,
            [MarshalAs(UnmanagedType.LPWStr)] string domain, // null for login domain
            IntPtr resume_handle // Must be IntPtr.Zero
            );

        [DllImport("Netapi32", SetLastError = true), SuppressUnmanagedCodeSecurityAttribute]
        private static extern int NetApiBufferFree(IntPtr pBuf);

        [StructLayout(LayoutKind.Sequential)]
        private struct SERVER_INFO_100
        {
            public ServerPlatform sv100_platform_id;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            public string sv100_name;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NetworkComputerInfo // SERVER_INFO_101
        {
            ServerPlatform sv101_platform_id;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            string sv101_name;
            int sv101_version_major;
            int sv101_version_minor;
            ServerTypes sv101_type;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            string sv101_comment;

            public ServerPlatform Platform { get { return sv101_platform_id; } }
            public string Name { get { return sv101_name; } }
            public string Comment { get { return sv101_comment; } }
            public ServerTypes ServerTypes { get { return sv101_type; } }
            public Version Version { get { return new Version(sv101_version_major, sv101_version_minor); } }
        };

        public static IEnumerable<string> GetNetworkComputerNames(ServerTypes serverTypes = ServerTypes.Workstation | ServerTypes.Server, string domain = null)
        {            
            IntPtr bufptr = IntPtr.Zero;
            try
            {
                int entriesRead, totalEntries;
                IntPtr resumeHandle = IntPtr.Zero;

                int ret = NetServerEnum(null, 100, out bufptr, MAX_PREFERRED_LENGTH, out entriesRead, out totalEntries, serverTypes, domain, resumeHandle);
                if (ret == 0)
                    return Array.ConvertAll<SERVER_INFO_100, string>(InteropUtil.ToArray<SERVER_INFO_100>(bufptr, entriesRead), si => si.sv100_name);
                throw new System.ComponentModel.Win32Exception(ret);
            }
            finally
            {
                NetApiBufferFree(bufptr);
            }
        }

        public static IEnumerable<NetworkComputerInfo> GetNetworkComputerInfo(ServerTypes serverTypes = ServerTypes.Workstation | ServerTypes.Server, string domain = null)
        {
            IntPtr bufptr = IntPtr.Zero;
            try
            {
                int entriesRead, totalEntries;
                IntPtr resumeHandle = IntPtr.Zero;

                int ret = NetServerEnum(null, 101, out bufptr, MAX_PREFERRED_LENGTH, out entriesRead, out totalEntries, serverTypes, domain, resumeHandle);
                if (ret == 0)
                    return InteropUtil.ToArray<NetworkComputerInfo>(bufptr, entriesRead);
                throw new System.ComponentModel.Win32Exception(ret);
            }
            finally
            {
                NetApiBufferFree(bufptr);
            }
        }
    }
}

namespace GateKeeper.Pixel
{
    internal static class InteropUtil
    {
        public static T ToStructure<T>(IntPtr ptr)
        {
            return (T)Marshal.PtrToStructure(ptr, typeof(T));
        }

        /// <summary>
        /// Converts an <see cref="IntPtr"/> that points to a C-style array into a CLI array.
        /// </summary>
        /// <typeparam name="T">Type of native structure used by the C-style array.</typeparam>
        /// <param name="ptr">The <see cref="IntPtr"/> pointing to the native array.</param>
        /// <param name="count">The number of items in the native array.</param>
        /// <returns>An array of type <typeparamref name="T"/> containing the elements of the native array.</returns>
        public static T[] ToArray<T>(IntPtr ptr, int count)
        {
            IntPtr tempPtr;
            T[] ret = new T[count];
            int stSize = Marshal.SizeOf(typeof(T));
            for (int i = 0; i < count; i++)
            {
                tempPtr = new IntPtr(ptr.ToInt64() + (i * stSize));
                ret[i] = ToStructure<T>(tempPtr);
            }
            return ret;
        }
    }
}