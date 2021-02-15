using System.Collections.Generic;
namespace Firewall
{
    using NdisApiDotNet;
    using NdisApiDotNetPacketDotNet.Extensions;
    using PacketDotNet;
    using System;
    using System.Net;
    using System.Threading;

    public class Threadripper
    {
        public static void PassThruThread(NdisApi filter, WaitHandle[] waitHandles, IReadOnlyList<NetworkAdapter> networkAdapters, IReadOnlyList<ManualResetEvent> waitHandlesManualResetEvents)
        {
            int port=0;
            string ip="";
            var ndisApiHelper = new NdisApiHelper();
            var ethRequest = ndisApiHelper.CreateEthRequest();
            Console.WriteLine("\nChoose the mode");
            //Console.WriteLine("\n1. Show all connections");
            Console.WriteLine("\n1. Close connections by source port" +
                              "\n2. Close connections by source IP"+
                              "\n3. Close connections by destination port"+
                              "\n4. Close connections by destination IP"+
                              "\n5. Close all connections");
            var mode = int.Parse(Console.ReadLine());            
            if (mode == 1 | mode == 3) 
            { 
            Console.WriteLine("\nEnter a port:");
            port = int.Parse(Console.ReadLine());
            }
            if (mode == 2 | mode == 4)
            {
            Console.WriteLine("\nEnter an IP:");
            ip = (Console.ReadLine());
            }

            while (true)
            {
                var handle = WaitHandle.WaitAny(waitHandles);
                ethRequest.AdapterHandle = networkAdapters[handle].Handle;               
                while (filter.ReadPacket(ref ethRequest))
                {
                    var ethPacket = ethRequest.Packet.GetEthernetPacket(ndisApiHelper);
                    if (ethPacket.PayloadPacket is IPv4Packet iPv4Packet)
                    {
                        if (iPv4Packet.PayloadPacket is TcpPacket tcpPacket)
                        {
                            
                            if (mode == 1 && tcpPacket.SourcePort == port)
                            {
                                Console.WriteLine($"{iPv4Packet.SourceAddress}:{tcpPacket.SourcePort} -> { iPv4Packet.DestinationAddress}:{ tcpPacket.DestinationPort}.");
                                continue;

                            }
                            if (mode == 3 && tcpPacket.DestinationPort == port)
                            {
                                Console.WriteLine($"{iPv4Packet.SourceAddress}:{tcpPacket.SourcePort} -> { iPv4Packet.DestinationAddress}:{ tcpPacket.DestinationPort}.");
                                continue;

                            }
                            if (mode == 2 && iPv4Packet.SourceAddress == IPAddress.Parse(ip))
                            {
                                Console.WriteLine($"{iPv4Packet.SourceAddress}:{tcpPacket.SourcePort} -> { iPv4Packet.DestinationAddress}:{ tcpPacket.DestinationPort}.");
                                continue;
                            }

                            if (mode == 4 && iPv4Packet.DestinationAddress == IPAddress.Parse(ip))
                            {
                                Console.WriteLine($"{iPv4Packet.SourceAddress}:{tcpPacket.SourcePort} -> { iPv4Packet.DestinationAddress}:{ tcpPacket.DestinationPort}.");
                                continue;
                            }
                            if (mode == 5 )
                            {
                                Console.WriteLine($"{iPv4Packet.SourceAddress}:{tcpPacket.SourcePort} -> { iPv4Packet.DestinationAddress}:{ tcpPacket.DestinationPort}.");
                                continue;
                            }

                        }
                    }
                    filter.SendPacket(ref ethRequest);
                }
                waitHandlesManualResetEvents[handle].Reset();
            }
        }
    }

}
