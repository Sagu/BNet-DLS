//Most code taken from Patrick Sapinski's http://github.com/kow/Starcraft-2-Battle.Net-Wrapper repo

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace BNet
{
    public class Crypto
    {
        public static byte[] RC4DecryptionKey = { 0xF4, 0x66, 0x31, 0x59, 0xFC, 0x83, 0x6E, 0x31, 0x31, 0x02, 0x51, 0xD5, 0x44, 0x31, 0x67, 0x98 };
        public static byte[] RC4EncryptionKey = { 0x22, 0xBE, 0xE5, 0xCF, 0xBB, 0x07, 0x64, 0xD9, 0x00, 0x45, 0x1B, 0xD0, 0x24, 0xB8, 0xD5, 0x45 };
        private static readonly HMACSHA1 s_decryptClientDataHMAC = new HMACSHA1(RC4DecryptionKey);
        private static readonly HMACSHA1 s_encryptServerDataHMAC = new HMACSHA1(RC4EncryptionKey);
        private readonly ARC4 InData;
        private readonly ARC4 OutData;

        public Crypto(byte[] sessionKey)
        {
            byte[] encryptHash = s_encryptServerDataHMAC.ComputeHash(sessionKey);
            byte[] decryptHash = s_decryptClientDataHMAC.ComputeHash(sessionKey);

            // Used by the client to decrypt packets sent by the server
            InData = new ARC4(encryptHash); // CLIENT-SIDE
            // Used by the server to decrypt packets sent by the client
            var decryptClientData = new ARC4(decryptHash); // SERVER-SIDE
            // Used by the server to encrypt packets sent to the client
            var encryptServerData = new ARC4(encryptHash); // SERVER-SIDE
            // Used by the client to encrypt packets sent to the server
            OutData = new ARC4(decryptHash); // CLIENT-SIDE

            // Use the 2 encryption objects to generate a common starting point
            var syncBuffer = new byte[1024];
            encryptServerData.Process(syncBuffer, 0, syncBuffer.Length);
            InData.Process(syncBuffer, 0, syncBuffer.Length);

            // Use the 2 decryption objects to generate a common starting point
            syncBuffer = new byte[1024];
            OutData.Process(syncBuffer, 0, syncBuffer.Length);
            decryptClientData.Process(syncBuffer, 0, syncBuffer.Length);
        }

        public void Decrypt(byte[] data, int start, int count)
        {
            InData.Process(data, start, count);
        }

        public void Encrypt(byte[] data, int start, int count)
        {
            OutData.Process(data, start, count);
        }
    }

    public class PacketKeyGenerator
    {
        static readonly byte[] SeedKey = { 0x38, 0xA7, 0x83, 0x15, 0xF8, 0x92, 0x25, 0x30, 0x71, 0x98, 0x67, 0xB1, 0x8C, 0x4, 0xE2, 0xAA };

        public static byte[] GenerateKey(byte[] sessionKey)
        {
            byte[] firstBuffer = new byte[64];
            byte[] secondBuffer = new byte[64];

            memset(firstBuffer, 0x36);
            memset(secondBuffer, 0x5C);

            for (int i = 0; i < SeedKey.Length; i++)
            {
                firstBuffer[i] = (byte)(SeedKey[i] ^ firstBuffer[i]);
                secondBuffer[i] = (byte)(SeedKey[i] ^ secondBuffer[i]);
            }

            Sha1Hash sha = new Sha1Hash();

            sha.Update(firstBuffer);
            sha.Update(sessionKey);

            byte[] tempDigest = sha.Final();

            sha = new Sha1Hash();

            sha.Update(secondBuffer);
            sha.Update(tempDigest);

            byte[] finalKey = sha.Final();

            return finalKey;
        }

        private static void memset(byte[] buffer, byte value)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = value;
            }
        }
    }
    public class WoWCrypt
    {
        private bool mInitialised = false;

        // Encryption state
        private byte mEncPrev;
        public int mEncIndex;
        // Decryption state
        public byte mDecPrev;
        public int mDecIndex;

        public byte[] mKey;

        private static object LockObject;

        public void Init(byte[] Key)
        {
            mKey = PacketKeyGenerator.GenerateKey(Key);
            mInitialised = true;
        }
        public byte GetDecPrev()
        {
            return mDecPrev;
        }
        public void SetDecPrev(byte SetTo)
        {
            mDecPrev = SetTo;
        }
        public int GetDecIndex()
        {
            return mDecIndex;
        }
        public void SetDecIndex(int SetTo)
        {
            mDecIndex = SetTo;
        }
        public void Decrypt(byte[] Data, int Length)
        {
            if (mInitialised == false) return;

            lock (LockObject)
            {
                for (int i = 0; i < Length; ++i)
                {
                    byte x = (byte)((Data[i] - mDecPrev) ^ mKey[mDecIndex]);
                    ++mDecIndex;
                    mDecIndex %= mKey.Length;
                    mDecPrev = Data[i];
                    Data[i] = x;
                }
            }
        }

        public void Encrypt(byte[] Data, int Length)
        {
            if (mInitialised == false) return;

            lock (LockObject)
            {
                for (int i = 0; i < Length; ++i)
                {
                    byte x = (byte)((Data[i] ^ mKey[mEncIndex]) + mEncPrev);
                    ++mEncIndex;
                    mEncIndex %= mKey.Length;
                    mEncPrev = x;
                    Data[i] = x;
                }
            }
        }
    }


    /*
     * Copyright ¸ 2005 Kele (fooleau@gmail.com)
     * This library is free software; you can redistribute it and/or 
     * modify it under the terms of the GNU Lesser General Public 
     * License version 2.1 as published by the Free Software Foundation
     * (the "LGPL").
     * This software is distributed on an "AS IS" basis, WITHOUT WARRANTY
     * OF ANY KIND, either express or implied.
     */

    /// <summary>
    /// A wrapper for .NET's SHA1 class
    /// This is designed to be compatable with my initial implementation
    /// </summary>
    public class Sha1Hash
    {
        private static readonly byte[] ZeroArray = new byte[0];
        private readonly SHA1 mSha;

        public Sha1Hash()
        {
            mSha = SHA1.Create();
        }

        public void Update(byte[] Data)
        {
            mSha.TransformBlock(Data, 0, Data.Length, Data, 0);
        }

        public void Update(string s)
        {
            Update(Encoding.Default.GetBytes(s));
        }

        public void Update(Int32 data)
        {
            Update(BitConverter.GetBytes(data));
        }

        public void Update(UInt32 data)
        {
            Update(BitConverter.GetBytes(data));
        }

        public void Update(UInt64 data)
        {
            Update(BitConverter.GetBytes(data));
        }

        public void Update(Int64 data)
        {
            Update(BitConverter.GetBytes(data));
        }

        public byte[] Final()
        {
            mSha.TransformFinalBlock(ZeroArray, 0, 0);
            return mSha.Hash;
        }

        public byte[] Final(byte[] Data)
        {
            mSha.TransformFinalBlock(Data, 0, Data.Length);
            return mSha.Hash;
        }
    }
}
