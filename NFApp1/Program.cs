using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace NFApp1
{
    public class Program
    {
        public static AddressFamily GetAddressFamily(IPAddress ipAddress)
        {
#if (!MF_FRAMEWORK_VERSION_V4_2 && !MF_FRAMEWORK_VERSION_V4_3 && !NANOFRAMEWORK_V1_0)
            return ipAddress.AddressFamily;
#else
            return (ipAddress.ToString().IndexOf(':') != -1) ?
                AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork;
#endif
        }

        // Set the SSID & Password to your local WiFi network
        const string MYSSID = "";
        const string MYPASSWORD = "";

        public static string SetupAndConnectNetwork()
        {
            NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces();
            if (nis.Length > 0)
            {
                // get the first interface
                NetworkInterface ni = nis[0];

                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {
                    // network interface is Wi-Fi
                    Console.WriteLine("Network connection is: Wi-Fi");

                    Wireless80211Configuration wc = Wireless80211Configuration.GetAllWireless80211Configurations()[ni.SpecificConfigId];
                    if (wc.Ssid != MYSSID && wc.Password != MYPASSWORD)
                    {
                        // have to update Wi-Fi configuration
                        wc.Ssid = MYSSID;
                        wc.Password = MYPASSWORD;
                        wc.SaveConfiguration();
                    }
                    else
                    {   // Wi-Fi configuration matches
                    }
                }
                else
                {
                    // network interface is Ethernet
                    Console.WriteLine("Network connection is: Ethernet");

                    ni.EnableDhcp();
                }

                // wait for DHCP to complete
                return WaitIP();
            }
            else
            {
                throw new NotSupportedException("ERROR: there is no network interface configured.\r\nOpen the 'Edit Network Configuration' in Device Explorer and configure one.");
            }
        }

        static string WaitIP()
        {
            Console.WriteLine("Waiting for IP...");

            while (true)
            {
                NetworkInterface ni = NetworkInterface.GetAllNetworkInterfaces()[0];
                if (ni.IPv4Address != null && ni.IPv4Address.Length > 0)
                {
                    if (ni.IPv4Address[0] != '0')
                    {
                        Console.WriteLine($"We have an IP: {ni.IPv4Address}");
                        return ni.IPv4Address;
                    }
                }

                Thread.Sleep(500);
            }
        }

        public static void Main()
        {
            var localIp = SetupAndConnectNetwork();
            var localEndPoint = new IPEndPoint(IPAddress.Parse(localIp), 3671);
            var endpoint = new IPEndPoint(IPAddress.Parse("192.168.0.225"), 3671);
            var socket = new Socket(GetAddressFamily(endpoint.Address), SocketType.Dgram, ProtocolType.Udp);

            try
            {
                //int i = _socket.SendTo(new byte[] { 0 }, _endpoint);
                socket.Bind(localEndPoint);

            }
            catch (SocketException ex)
            {
                int i = ex.ErrorCode;
            }

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
