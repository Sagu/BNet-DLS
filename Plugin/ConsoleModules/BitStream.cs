using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Shared = DeepLogic.Shared;

namespace BNet.Plugin.ConsoleModules
{
    public class BitStreamExpConsole : DeepLogic.Core.ExpConsole.ConsoleModule
    {
        public Queue<byte[]> PacketQueue = new Queue<byte[]>();
        public BitStreamExpConsole()
        {
            //Add an alias to the console
            base.Aliases.Add("bs");
        }

        public override string ConsoleModuleName
        {
            get
            {
                return "BitStream";
            }
        }

        public override void ProcessCommand(List<string> args)
        {
            switch (args[0].ToLower())
            {
                case "analyzer":
                    {
                        if (args.Count == 2)
                            switch (args[1].ToLower())
                            {
                                case "on":
                                    NetHandler.AuxServer.ReceivedDataHandler -= InspectStream;
                                    NetHandler.AuxServer.ReceivedDataHandler += InspectStream;
                                    Console.WriteLine("New packets will show up in the BitStream Analyzer window.  Use \"Analyze Off\" to disable.\n");
                                    break;
                                case "off":
                                    NetHandler.AuxServer.ReceivedDataHandler -= InspectStream;
                                    Console.WriteLine("Stream inspector removed from the socket server.\n");
                                    break;
                                default:
                                    Console.WriteLine("-ExpCon: " + args[1] + ": invalid parameter(s)");
                                    break;
                            }
                        else
                            Console.WriteLine("Analyze: Toggle BitStream Analyzer for received packets.\n  Paramters: {On, Off}\n");
                        break;
                    }
                case "injectpacket":
                    {
                        if (args.Count > 1)
                        {
                            try
                            {
                                byte[] packet = DeepLogic.Core.HexUtils.GetBytes(String.Join(" ",args.Where(a => args.IndexOf(a) > 0)));
                                new Thread(delegate() { InjectPacket(packet); }).Start();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("-ExpCon: " + args[1] + ": invalid parameter(s) - " + e.Message);
                            }
                        }
                        else
                        {
                            new Thread(delegate() { InjectPacket(); }).Start();
                        }
                        break;
                    }
                case "queuepacket":
                    {
                        if (args.Count > 1)
                        {
                            try
                            {
                                byte[] packet = DeepLogic.Core.HexUtils.GetBytes(String.Join(" ", args.Where(a => args.IndexOf(a) > 0)));
                                new Thread(delegate() { QueuePacket(packet); }).Start();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("-ExpCon: " + args[1] + ": invalid parameter(s) - " + e.Message);
                            }
                        }
                        else
                        {
                            new Thread(delegate() { QueuePacket(); }).Start();
                        }
                        break;
                    }
                default:
                    {
                        Console.WriteLine("-ExpCon: " + args[0] + ": command not found");
                        break;
                    }
            }
        }

        public override void GetDescription()
        {
            Console.WriteLine("BitStream Plugin: Analyze/Inject packet bitstreams\n");
            Console.WriteLine("  Commands:");
            Console.WriteLine("    Analyzer {On, Off} - Spawns a bitstream analyzer window for received packets (Warning: Controlled traffic is advised)\n");
            Console.WriteLine("    InjectPacket {[HEX DATA]} - Broadcasts a packet to all connected clients");
            Console.WriteLine("                                If no HEX data is present, a bitstream writer window will be shown\n");
            Console.WriteLine("    QueuePacket {[HEX DATA]} - Suspends Packet Processing and queues a packet response for the next client that sends data");
            Console.WriteLine("                               Supports enqueuing of more than one packet. The packet at the front of the queue will be sent");
            Console.WriteLine("                               If no HEX data is present, a bitstream writer window will be shown");
            Console.WriteLine();
        }

        //Methods

        public void InspectStream(byte[] Data, DeepLogic.Object_Wrappers.NetworkClient User)
        {
            Shared.Streams.BitStream bs = new Shared.Streams.BitStream();
            foreach (byte b in Data)
                bs.WriteByte(b);
            Application.Run(new BitStreamEditor.BitStreamAnalyzerForm(bs));
        }

        public void InjectPacket(byte[] Packet = null)
        {
            Shared.Streams.BitStream bs = new Shared.Streams.BitStream();
            if (Packet == null)
            {
                BitStreamEditor.BitStreamAnalyzerForm _bseditor = new BitStreamEditor.BitStreamAnalyzerForm(bs);
                Application.Run(_bseditor);
                while (_bseditor.Visible)
                    Thread.Sleep(100);
            }
            else
            {
                foreach (byte b in Packet)
                    bs.WriteByte(b);
            }
            NetHandler.AuxServer.Broadcast(bs.ToByteArray());
        }

        public void QueuePacket(byte[] Packet = null)
        {
            Shared.Streams.BitStream bs = new Shared.Streams.BitStream();
            if (Packet == null)
            {
                BitStreamEditor.BitStreamAnalyzerForm _bseditor = new BitStreamEditor.BitStreamAnalyzerForm(bs);
                Application.Run(_bseditor);
                while (_bseditor.Visible)
                    Thread.Sleep(100);
            }
            else
            {
                foreach (byte b in Packet)
                    bs.WriteByte(b);
            }
            NetHandler.AuxServer.ReceivedDataHandler -= NetHandler.ProcessPacket;
            NetHandler.AuxServer.ReceivedDataHandler += QueuePacket_Callback;
            PacketQueue.Enqueue(bs.ToByteArray());
        }

#error AuxServer Bug? 6am, no sleep yet, resuming tomorrow
        public void QueuePacket_Callback(byte[] Data, DeepLogic.Object_Wrappers.NetworkClient User)
        {
            //Handle potential rogue async callback
            if (PacketQueue.Count == 0)
                return;
            byte[] buffer = PacketQueue.Dequeue();
            User.NetworkStream.Write(buffer, 0, buffer.Length);
            if (PacketQueue.Count == 0)
            {
                NetHandler.AuxServer.ReceivedDataHandler -= QueuePacket_Callback;
                NetHandler.AuxServer.ReceivedDataHandler += NetHandler.ProcessPacket;
            }
        }
    }
}
