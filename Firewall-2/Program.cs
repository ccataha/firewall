using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NdisApiDotNet;
using NdisApiDotNetPacketDotNet.Extensions;
using PacketDotNet;


namespace Firewall
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            var filter = NdisApi.Open();  // check driver available
            if (!filter.IsValid)
                throw new ApplicationException("Драйвер не найден");
            Console.WriteLine($"Версия сетевого драйвера: {filter.GetVersion()}");
            // Создать и установить событие для адаптеров       
            var waitHandlesCollection = new List<ManualResetEvent>();
            // Creating network adapter list
            var tcpAdapters = new List<NetworkAdapter>();
            foreach (var networkAdapter in filter.GetNetworkAdapters())
                if (networkAdapter.IsValid) 
                {
                    var success = filter.SetAdapterMode(networkAdapter,
                    NdisApiDotNet.Native.NdisApi.MSTCP_FLAGS.MSTCP_FLAG_TUNNEL |
                    NdisApiDotNet.Native.NdisApi.MSTCP_FLAGS.MSTCP_FLAG_LOOPBACK_FILTER |
                    NdisApiDotNet.Native.NdisApi.MSTCP_FLAGS.MSTCP_FLAG_LOOPBACK_BLOCK);
                    var manualResetEvent = new ManualResetEvent(false);
                    success &= filter.SetPacketEvent(networkAdapter,
                    manualResetEvent.SafeWaitHandle);
                    if (success)
                    {
                        Console.WriteLine($"Добавлен адаптер: {networkAdapter.FriendlyName}");
                        // Добавление адаптеров в список
                        waitHandlesCollection.Add(manualResetEvent);
                        tcpAdapters.Add(networkAdapter);
                    }
                }
            var waitHandlesManualResetEvents =
            waitHandlesCollection.Cast<ManualResetEvent>().ToArray();
            var waitHandles = waitHandlesCollection.Cast<WaitHandle>().ToArray();
            Console.Write("Press ENTER to start");
            ConsoleKeyInfo keyInfo = Console.ReadKey();
            if (keyInfo.Key == ConsoleKey.Enter)
            {
                var t1 = Task.Factory.StartNew(() => Threadripper.PassThruThread(filter, waitHandles, tcpAdapters.ToArray(), waitHandlesManualResetEvents));
                Task.WaitAll(t1);
                Console.Read();

            }
        }
        
    }

}
