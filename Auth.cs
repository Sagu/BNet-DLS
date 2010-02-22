using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BNet
{
    public static class AppPackets
    {
        #region In enum

        public enum In
        {
            AuthComplete = 0,
            ProofRequest = 2
        }

        #endregion

        #region Out enum

        public enum Out
        {
            InformationRequest = 0,
            ProofResponse = 2
        }

        #endregion

        public static void HandlePacket(In packetId, BitReader bitReader)
        {
            switch (packetId)
            {
                case In.ProofRequest:
                    GenerateProofRequest(bitReader);
                    break;
                case In.AuthComplete:
                    GenerateAuthComplete(bitReader);
                    break;
                default:
                    DeepLogic.Core.ErrorHandler.Log("BNet","debug",String.Format("App: Unhandled packet, {0}", (int)packetId));
                    break;
            }
        }

        //Copy of HandleProofRequest, sort of a starting point...
        public static void GenerateProofRequest(BitReader bitReader)
        {
            DeepLogic.Core.ErrorHandler.Log("BNet","debug","GenerateProofRequest");
            /*
            int numModules = bitReader.ReadInt32(3);
            var moduleInputs = new List<ModuleInput>();

            DeepLogic.Core.ErrorHandler.Log("BNet","debug",String.Format("Modules ({0}):", numModules));
            for (int i = 0; i < numModules; i++)
            {
                var cacheHandle = ReadModuleInfo(bitReader);
                int blobSize = bitReader.ReadInt32(10);
                byte[] moduleBlob = bitReader.ReadBytes(blobSize);

                DeepLogic.Core.ErrorHandler.Log("BNet","debug",String.Format("ModuleID: {0}", cacheHandle.ModuleID.ToHexString()));
                DeepLogic.Core.ErrorHandler.Log("BNet","debug",String.Format("Auth: {0}", cacheHandle.AuthString));
                DeepLogic.Core.ErrorHandler.Log("BNet","debug",String.Format("Locale: {0}", cacheHandle.Locale));
                DeepLogic.Core.ErrorHandler.Log("BNet","debug",String.Format("Blob:\n{0}", moduleBlob.ToHexString()));

                var moduleInput = new ModuleInput { ModuleCacheHandle = cacheHandle, ModuleData = moduleBlob };
                moduleInputs.Add(moduleInput);
            }

            byte[] inBuf = moduleInputs[0].ModuleData;
            var outBuf = new byte[1024];
            int outSize;
            AuthInterface.RequestPassword(inBuf, inBuf.Length, outBuf, out outSize);


            var bitWriter = new BitWriter(1024);
            bitWriter.WriteHeader((int)Out.ProofResponse, (int)Channels.App);
            bitWriter.WriteBits(1, 3); // module count
            bitWriter.WriteBuffer(outBuf, outSize, 10); // [14] Blob, {Type: UInt16, Bits: 10, Min: 0, Max: 1023} - Battlenet::Client::Authentication::ModuleData

            var myBuf = new byte[bitWriter.Length - 1];
            Array.Copy(bitWriter.Buffer, 0, myBuf, 0, bitWriter.Length - 1);
            //DeepLogic.Core.ErrorHandler.Log("BNet","debug","Sending: \n{0}\n", myBuf.ToHexDump());

            Global.Connection.Write(myBuf);
             */
        }

        //Copy of HandleAuthComplete, not really a starting point...
        public static void GenerateAuthComplete(BitReader bitReader)
        {
            DeepLogic.Core.ErrorHandler.Log("BNet","debug","GenerateAuthComplete");
            /*
            bool isFailure = bitReader.ReadBoolean(); // choice of 0,1
            DeepLogic.Core.ErrorHandler.Log("BNet","debug","Choice: {0}", isFailure == false ? "Success" : "Failure");
            if (isFailure == false) // Battlenet::Client::Authentication::Complete::Result::Success
            {
                int numModules = bitReader.ReadInt32(3); // [1A] Array, {Type: UInt8, Bits: 3, Min: 0, Max: 4}, NextTypeId: 0x15 - Battlenet::Client::Authentication::ModuleRequest

                for (int i = 0; i < numModules; i++)
                {
                    //byte[] authStr = bitReader.ReadBytes(4);
                    //DeepLogic.Core.ErrorHandler.Log("BNet","debug","Auth: {0}", Encoding.ASCII.GetString(authStr));
                    //bitReader.ReadBytes(2); // Unk
                    //DeepLogic.Core.ErrorHandler.Log("BNet","debug","Locale: {0}", Encoding.ASCII.GetString(bitReader.ReadBytes(2)));
                    //byte[] moduleId = bitReader.ReadBytes(32);// - Battlenet::Cache::Handle
                    //DeepLogic.Core.ErrorHandler.Log("BNet","debug","Module Id: {0}", moduleId.ToHexString());
                    ModuleCacheHandle moduleCacheHandle = ReadModuleInfo(bitReader);
                    DeepLogic.Core.ErrorHandler.Log("BNet", "debug", String.Format("Auth: {0}", moduleCacheHandle.AuthString));
                    DeepLogic.Core.ErrorHandler.Log("BNet", "debug", String.Format("Locale: {0}", moduleCacheHandle.Locale));
                    DeepLogic.Core.ErrorHandler.Log("BNet", "debug", String.Format("ModuleId: {0}", moduleCacheHandle.ModuleID.ToHexString()));


                    //int blobSize = bitReader.ReadInt32(10);
                    //byte[] blob = bitReader.ReadBytes(blobSize);
                    byte[] blob = bitReader.ReadBuffer(10);
                    //DeepLogic.Core.ErrorHandler.Log("BNet","debug","Blob Size: {0}, blobId: {1}", blobSize, blob[0]);
                    DeepLogic.Core.ErrorHandler.Log("BNet", "debug", String.Format("Blob Size: {0}, blobId: {1}", blob.Length, blob[0]));

                    if (i == 0)
                    {
                        var sessionKey = new byte[64];
                        //AuthInterface.RequestSessionKey(blob, blobSize, sessionKey);
                        AuthInterface.RequestSessionKey(blob, blob.Length, sessionKey);

                        Global.SessionKey = sessionKey;
                        DeepLogic.Core.ErrorHandler.Log("BNet", "debug", String.Format("Session Key: \n{0}", sessionKey.ToHexDump()));
                    }
                }

                //int pingTimeout = Convert.ToInt32(bitReader.ReadInt32(32) + 2147483648); // 2147483648 is the "Min"
                int pingTimeout = bitReader.ReadInt32(32, int.MinValue);// 2147483648
                DeepLogic.Core.ErrorHandler.Log("BNet", "debug", String.Format("Ping Timeout: {0}", pingTimeout));


                if (bitReader.ReadOptional()) // optional segment
                {
                    // Choice of 0, 1
                    bool choice = bitReader.ReadBoolean();
                    if (choice) // choice 1
                    {
                        // Battlenet::Regulator::LeakyBucketParams - Struct, Components: { 1 - m_threshold, 1 - m_rate}
                        // m_regulatorRules?
                        int threshold = bitReader.ReadInt32(32);
                        int rate = bitReader.ReadInt32(32);
                        DeepLogic.Core.ErrorHandler.Log("BNet", "debug", "Regulator Rules:");
                        DeepLogic.Core.ErrorHandler.Log("BNet", "debug", String.Format("\tThreshold: {0}", threshold));
                        DeepLogic.Core.ErrorHandler.Log("BNet", "debug", String.Format("\tRate: {0}", rate));
                    }
                    else // choice 0
                    {

                    }
                }

                //int strLen = bitReader.ReadInt32(7);
                //string accountHolder = Encoding.ASCII.GetString(bitReader.ReadBytes(strLen));
                var accountHolder = bitReader.ReadAsciiString(7);
                DeepLogic.Core.ErrorHandler.Log("BNet", "debug", String.Format("Account Holder: {0}", accountHolder));

                int region = bitReader.ReadInt32(8);
                // Possibly region, US = 1, EU = 2
                DeepLogic.Core.ErrorHandler.Log("BNet", "debug", String.Format("Region?: {0}", region));

                //int strLen = bitReader.ReadInt32(5) + 1; // + 1 for the "Min" value
                //int strLen = bitReader.ReadInt32(5, 1); // 1 is the min for [67] AsciiString, {Type: UInt8, Bits: 5, Min: 1, Max: 32}
                //string accountName = Encoding.ASCII.GetString(bitReader.ReadBytes(strLen));
                var accountName = bitReader.ReadAsciiString(5, 1);
                DeepLogic.Core.ErrorHandler.Log("BNet", "debug", String.Format("Account Name: {0}", accountName));

                Global.Username = accountName;

                long subLong = bitReader.ReadInt64(64);
                int subInt4 = bitReader.ReadInt32(32);

                DeepLogic.Core.ErrorHandler.Log("BNet", "debug", String.Format("SubInts: {0:x} {1:x}", subLong, subInt4));
                WoWPackets.SendListSubscribeRequest();
            }
            else
            {
                if (bitReader.ReadOptional())
                {
                    // 40 byte Blob - Battlenet::Cache::Handle
                    // Choice
                    // 0 - Null
                    // 1 - Int16, Int32
                    // 2 - Null
                    //
                    ModuleCacheHandle moduleCacheHandle = ReadModuleInfo(bitReader);
                    DeepLogic.Core.ErrorHandler.Log("BNet", "debug", "Didn't handle optional argument");
                }
                switch (bitReader.ReadInt32(2)) // Choice of 0,1,2
                {
                    case 0:
                    case 2:
                        break;
                    case 1:
                        {
                            var errorCode = bitReader.ReadInt32(16);
                            //var uint_1 = bitReader.ReadInt32(32) + 2147483648; // min
                            var uint_1 = bitReader.ReadInt32(32, int.MinValue);
                            DeepLogic.Core.ErrorHandler.Log("BNet", "debug", String.Format("AuthComplete Error: {0} {1}", errorCode, uint_1));
                            break;
                        }
                }
            }*/
        }

        static ModuleCacheHandle ReadModuleInfo(BitReader bitReader)
        {
            var moduleInfo = new ModuleCacheHandle(bitReader.ReadBytes(40));
            return moduleInfo;
            //byte[] authStr = bitReader.ReadBytes(4);
            //DeepLogic.Core.ErrorHandler.Log("BNet","debug","Auth: {0}", Encoding.ASCII.GetString(authStr));
            //bitReader.ReadBytes(2); // Unk
            //DeepLogic.Core.ErrorHandler.Log("BNet","debug","Locale: {0}", Encoding.ASCII.GetString(bitReader.ReadBytes(2)));

            //byte[] moduleId = bitReader.ReadBytes(32);// - Battlenet::Cache::Handle
            //DeepLogic.Core.ErrorHandler.Log("BNet","debug","Module Id: {0}", moduleId.ToHexString());
        }

/*        public static void SendInformationRequest()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            DeepLogic.Core.ErrorHandler.Log("BNet","debug","SendInformationRequest");
            Console.ResetColor();

            var bitWriter = new BitWriter(1024);
            bitWriter.WriteHeader((int)Out.InformationRequest, 0);

            bitWriter.WriteFourCC("WoW");
            bitWriter.WriteFourCC("Win");
            bitWriter.WriteFourCC("enUS");

            var components = new List<Component>
                                 {
                                     new Component("WoW", "Win", 10505),
                                     new Component("WoW", "base", 10468),
                                     new Component("WoW", "enUS", 10505),
                                     new Component("Tool", "Win", 1374),
                                     new Component("Bnet", "Win", 13590)
                                 };

            bitWriter.WriteBits(components.Count, 6);
            foreach (Component component in components)
            {
                bitWriter.WriteFourCC(component.Program);
                bitWriter.WriteFourCC(component.Platform);
                bitWriter.WriteInt32(component.Version);
            }

            bitWriter.WriteBits(1, 1);

            const string accountName = "email@wut.com";

            bitWriter.WriteBits(accountName.Length - 3, 9);
            bitWriter.WriteBytes(Encoding.ASCII.GetBytes(accountName));

            var myBuf = new byte[bitWriter.Length];
            Array.Copy(bitWriter.Buffer, 0, myBuf, 0, bitWriter.Length);

            DeepLogic.Core.ErrorHandler.Log("BNet","debug",String.Format("Sending: \n{0}\n", myBuf.ToHexDump()));
            Global.Connection.Write(myBuf);
        }
 */

        #region Nested type: Component

        private struct Component
        {
            public readonly string Platform;
            public readonly string Program;
            public readonly int Version;

            public Component(string program, string platform, int version)
            {
                Program = program;
                Version = version;
                Platform = platform;
            }
        }

        #endregion

        #region Nested type: ModuleCacheHandle


        public class ModuleInput
        {
            public ModuleCacheHandle ModuleCacheHandle;
            public byte[] ModuleData;
        }

        public class ModuleCacheHandle
        {
            public string AuthString;
            public short UnkShort;
            public string Locale;
            public byte[] ModuleID;

            public ModuleCacheHandle(byte[] buf)
            {
                AuthString = Encoding.ASCII.GetString(buf, 0, 4);
                UnkShort = BitConverter.ToInt16(buf, 4);
                Locale = Encoding.ASCII.GetString(buf, 6, 2);
                ModuleID = new byte[32];
                Array.Copy(buf, 8, ModuleID, 0, 32);
            }
        }

        #endregion
    }
}