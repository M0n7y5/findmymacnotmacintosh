using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace FindMyMACNotMacintosh.Utils
{
    public class IPCalc
    {
        public static uint ToInt(IPAddress addr)
        {
            return BitConverter.ToUInt32(addr.GetAddressBytes(), 0);
        }

        public static IPAddress ToAddr(uint address)
        {
            return new IPAddress(address);
        }

        public static IPAddress GetNetworkAddress(IPAddress netAddress, IPAddress netmask)
        {

            uint ip = ToInt(netAddress);
            uint mask = ToInt(netmask);
            uint firstIp = (ip & mask);

            return ToAddr(firstIp);
        }

        public static IPAddress GetBroadCastAddress(IPAddress netAddress, IPAddress netmask)
        {
            uint ip = ToInt(netAddress);
            uint mask = ToInt(netmask);
            uint lastIp = (ip & mask) + ~mask;

            return ToAddr(lastIp);
        }

        public static IPAddress GetLastHostAddress(IPAddress netAddress, IPAddress netmask)
        {
            uint ip = ToInt(netAddress);
            uint mask = ToInt(netmask);
            uint lastIp = (ip & mask) + ~mask;

            lastIp -= 16777216;

            return ToAddr(lastIp);
        }

        public static IPAddress GetFirstHostAddress(IPAddress netAddress, IPAddress netmask)
        {
            uint ip = ToInt(netAddress);
            uint mask = ToInt(netmask);
            uint firstIp = (ip & mask);

            firstIp += 16777216;

            IPAddress firstHost = ToAddr(firstIp);
            return firstHost;
        }

        public static List<IPAddress> GetListIpInNetwork(IPAddress netAddress, uint netMaskLenght)
        {

            List<IPAddress> ipInNetwork = new List<IPAddress>();

            BitArray netmaskBits = new BitArray(8);
            IPAddress netmaskIP = getNetmask(netMaskLenght);

            IPAddress first = GetFirstHostAddress(netAddress, netmaskIP);
            IPAddress last = GetLastHostAddress(netAddress, netmaskIP);

            string[] stringStart = first.ToString().Split('.');
            string[] stringEnd = last.ToString().Split('.');

            int a = int.Parse(stringStart[0]);
            int b = int.Parse(stringStart[1]);
            int c = int.Parse(stringStart[2]);
            int d = int.Parse(stringStart[3]);

            int start = BitConverter.ToInt32(new byte[] { (byte)d, (byte)c, (byte)b, (byte)a }, 0);

            a = int.Parse(stringEnd[0]);
            b = int.Parse(stringEnd[1]);
            c = int.Parse(stringEnd[2]);
            d = int.Parse(stringEnd[3]);

            int end = BitConverter.ToInt32(new byte[] { (byte)d, (byte)c, (byte)b, (byte)a }, 0);

            for (int i = start; i <= end; i++)
            {
                byte[] bytes = BitConverter.GetBytes(i);
                ipInNetwork.Add(new IPAddress(new[] { bytes[3], bytes[2], bytes[1], bytes[0] }));
            }

            return ipInNetwork;
        }

        private static IPAddress getNetmask(uint netmaskLenght)
        {

            if (netmaskLenght > 31)
            {
                throw new ArgumentException("Netmask length must be 1 - 31");
            }
            string netmaskString = "";

            switch (netmaskLenght)
            {
                case 1:
                    netmaskString = "128.0.0.0";
                    break;
                case 2:
                    netmaskString = "192.0.0.0";
                    break;
                case 3:
                    netmaskString = "224.0.0.0";
                    break;
                case 4:
                    netmaskString = "240.0.0.0";
                    break;
                case 5:
                    netmaskString = "248.0.0.0";
                    break;
                case 6:
                    netmaskString = "252.0.0.0";
                    break;
                case 7:
                    netmaskString = "254.0.0.0";
                    break;
                case 8:
                    netmaskString = "255.0.0.0";
                    break;
                case 9:
                    netmaskString = "255.128.0.0";
                    break;
                case 10:
                    netmaskString = "255.192.0.0";
                    break;
                case 11:
                    netmaskString = "255.224.0.0";
                    break;
                case 12:
                    netmaskString = "255.240.0.0";
                    break;
                case 13:
                    netmaskString = "255.248.0.0";
                    break;
                case 14:
                    netmaskString = "255.252.0.0";
                    break;
                case 15:
                    netmaskString = "255.254.0.0";
                    break;
                case 16:
                    netmaskString = "255.255.0.0";
                    break;
                case 17:
                    netmaskString = "255.255.128.0";
                    break;
                case 18:
                    netmaskString = "255.255.192.0";
                    break;
                case 19:
                    netmaskString = "255.255.224.0";
                    break;
                case 20:
                    netmaskString = "255.255.240.0";
                    break;
                case 21:
                    netmaskString = "255.255.248.0";
                    break;
                case 22:
                    netmaskString = "255.255.252.0";
                    break;
                case 23:
                    netmaskString = "255.255.254.0";
                    break;
                case 24:
                    netmaskString = "255.255.255.0";
                    break;
                case 25:
                    netmaskString = "255.255.255.128";
                    break;
                case 26:
                    netmaskString = "255.255.255.192";
                    break;
                case 27:
                    netmaskString = "255.255.255.224";
                    break;
                case 28:
                    netmaskString = "255.255.255.240";
                    break;
                case 29:
                    netmaskString = "255.255.255.248";
                    break;
                case 30:
                    netmaskString = "255.255.255.252";
                    break;
                case 31:
                    netmaskString = "255.255.255.254";
                    break;
                default:
                    netmaskString = "255.255.255.0";
                    break;
            }

            IPAddress netmask = IPAddress.Parse(netmaskString);
            return netmask;
        }

    }
}
