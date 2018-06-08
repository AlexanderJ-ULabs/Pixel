#region header
// GateKeeper.Pixel - NetworkBrowser.cs
// 
// Copyright Untethered Labs, Inc.  All rights reserved.
// 
// Created: 2018-06-08 3:43 PM
#endregion

using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Security;

namespace GateKeeper.Pixel
{
    public sealed class NetworkBrowser
    {
        [DllImport("Netapi32", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int NetServerEnum(
            string serverName,
            int dwLevel,
            ref IntPtr pBuf,
            int dwPrefMaxLen,
            out int dwEntriesRead,
            out int dwTotalEntries,
            int dwServerType,
            string domain,
            out int dwResumeHandle
            );

        [DllImport("Netapi32", SetLastError = true)]
        public static extern int NetApiBufferFree(IntPtr pBuf);

        [StructLayout(LayoutKind.Sequential)]
        public struct ServerInfo100
        {
            internal int sv100_platform_id;
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string sv100_name;
        }

        public static ArrayList GetNetworkComputers()
        {
            ArrayList networkComputers = new ArrayList();
            const int MAX_PREFERRED_LENGTH = -1;
            int SV_TYPE_WORKSTATION = 1;
            int SV_TYPE_SERVER = 2;
            IntPtr buffer = IntPtr.Zero;
            IntPtr tmpBuffer = IntPtr.Zero;
            int entriesRead;
            int totalEntries;
            int resHandle;
            int sizeofInfo = Marshal.SizeOf(typeof(ServerInfo100));


            try
            {
                int ret = NetServerEnum(null, 100, ref buffer,
                                        MAX_PREFERRED_LENGTH, out entriesRead, out totalEntries,
                                        SV_TYPE_WORKSTATION | SV_TYPE_SERVER, null, out resHandle);
                if (ret == 0)
                {
                    for (int i = 0; i < totalEntries; i++)
                    {
                        tmpBuffer = new IntPtr((long)buffer + (i * sizeofInfo));

                        ServerInfo100 svrInfo = (ServerInfo100)
                                                   Marshal.PtrToStructure(tmpBuffer,
                                                                          typeof(ServerInfo100));
                        networkComputers.Add(svrInfo.sv100_name);
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                NetApiBufferFree(buffer);
            }
            return networkComputers;
        }
    }
}