using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;

namespace FindMyMACNotMacintosh.Utils
{
    public static class NetworkInterop
    {
        public static string GetMACAdrByIp(IPAddress ip)
        {
            uint uintAddress = BitConverter.ToUInt32(ip.GetAddressBytes(), 0);
            byte[] macAddr = new byte[6];
            int macAddrLen = macAddr.Length;

            int retValue = NativeMethods.SendARP(uintAddress, 0, macAddr, ref macAddrLen);
            if (retValue != 0)
            {
                throw new Win32Exception(retValue, "SendARP failed.");
            }

            string[] mac = new string[macAddrLen];
            for (int i = 0; i < macAddrLen; i++)
                mac[i] = macAddr[i].ToString("x2");

            return string.Join(":", mac);
        }
    }

    internal static class NativeMethods
    {
        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        public static extern int SendARP(uint DestIP,
                                         uint SrcIP,
                                         byte[] pMacAddr,
                                         ref int PhyAddrLen);


    }
}
