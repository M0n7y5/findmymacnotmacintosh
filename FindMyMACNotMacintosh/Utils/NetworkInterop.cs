using System;
using System.Globalization;
using System.Net;
using System.Runtime.InteropServices;

namespace FindMyMACNotMacintosh.Utils
{
    public static class NetworkInterop
    {
        public static bool TryGetMACAdrByIp(IPAddress ip, out string mac)
        {
            uint uintAddress = BitConverter.ToUInt32(ip.GetAddressBytes(), 0);
            byte[] macAddr = new byte[6];
            int macAddrLen = macAddr.Length;

            int retValue = NativeMethods.SendARP(uintAddress, 0, macAddr, ref macAddrLen);
            if (retValue != 0 || macAddrLen == 0)
            {
                mac = "";
                return false;
            }

            string[] res = new string[macAddrLen];
            for (int i = 0; i < macAddrLen; i++)
                res[i] = macAddr[i].ToString("X2", CultureInfo.InvariantCulture);

            mac = string.Join(":", res);

            return true;
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
