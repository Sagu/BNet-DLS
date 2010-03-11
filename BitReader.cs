//Most code taken from Ralek and Kynox's BNet2 client
//DEPRECIATED! Now using Shared.Streams.BitStream
using System;
using System.Text;

namespace BNet
{
    public class BitReader
    {
        private readonly byte[] _buffer;
        private readonly int _numBits;
        private int _readPos;

        public BitReader(byte[] buffer)
        {
            _buffer = buffer;
            _numBits = buffer.Length * 8;
            _readPos = 0;
        }

        public byte[] Buffer
        {
            get { return _buffer; }
        }

        public int NumBits
        {
            get { return _numBits; }
        }

        public int ByteLength
        {
            get { return NumBits / 8; }
        }

        public int ReadPos
        {
            get { return _readPos; }
        }

        private void AlignToNextByte()
        {
            _readPos = (ReadPos + 7) & ~7;
        }

        public byte[] ReadBytes(int count)
        {
            AlignToNextByte();
            var buf = new byte[count];
            System.Buffer.BlockCopy(_buffer, ReadPos >> 3, buf, 0, count);
            _readPos = ReadPos + count * 8;
            return buf;
        }

        public byte[] ReadBuffer(int sizeBitCount)
        {
            int blobSize = ReadInt32(sizeBitCount);
            return ReadBytes(blobSize);
        }

        public bool ReadBoolean()
        {
            return ReadInt32(1) != 0;
        }

        public bool ReadOptional()
        {
            return ReadBoolean();
        }

        public float ReadFloat()
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(ReadInt32(32)), 0);
        }

        public string ReadAsciiString(int bitCount)
        {
            int len = ReadInt32(bitCount);
            return Encoding.ASCII.GetString(ReadBytes(len));
        }

        public string ReadAsciiString(int bitCount, int min)
        {
            int len = ReadInt32(bitCount, min);
            return Encoding.ASCII.GetString(ReadBytes(len));
        }

        public string ReadUTFString(int bitCount)
        {
            int len = ReadInt32(bitCount + 2);
            return Encoding.UTF8.GetString(ReadBytes(len));
        }

        public uint ReadUInt32(int numBits)
        {
            if (numBits <= 0)
                return 0;
            uint ret = 0;
            while (true)
            {
                if (ReadPos >= _numBits)
                {
                    return ret;
                }

                int pos7 = (ReadPos & 7);
                int bitsLeftInByte = 8 - pos7;

                int subNum = bitsLeftInByte;
                if (bitsLeftInByte >= numBits)
                    subNum = numBits;

                var lShift = (byte)(1 << subNum);

                int v10 = _buffer[ReadPos >> 3] >> (ReadPos & 7);
                numBits -= subNum;

                ret |= (uint)(((lShift - 1) & v10) << numBits);
                _readPos = ReadPos + subNum;

                if (numBits == 0)
                    return ret;
            }
        }

        public int ReadInt32(int numBits)
        {
            if (numBits <= 0)
                return 0;
            int ret = 0;
            while (true)
            {
                if (ReadPos >= _numBits)
                {
                    return ret;
                }

                int pos7 = (ReadPos & 7);
                int bitsLeftInByte = 8 - pos7;

                int subNum = bitsLeftInByte;
                if (bitsLeftInByte >= numBits)
                    subNum = numBits;

                var lShift = (byte)(1 << subNum);

                int v10 = _buffer[ReadPos >> 3] >> (ReadPos & 7);
                numBits -= subNum;

                ret |= ((lShift - 1) & v10) << numBits;
                _readPos = ReadPos + subNum;

                if (numBits == 0)
                    return ret;
            }
        }


        public int ReadInt32(int numBits, int min)
        {
            return ReadInt32(numBits) + min;
        }


        public long ReadInt64(int numBits)
        {
            if (numBits <= 0)
                return 0;
            long ret = 0;
            while (true)
            {
                if (ReadPos >= _numBits)
                {
                    return ret;
                }

                int pos7 = (ReadPos & 7);
                int bitsLeftInByte = 8 - pos7;

                int subNum = bitsLeftInByte;
                if (bitsLeftInByte >= numBits)
                    subNum = numBits;

                var lShift = (byte)(1 << subNum);
                long v10 = _buffer[ReadPos >> 3] >> (ReadPos & 7);
                numBits -= subNum;

                ret |= ((lShift - 1) & v10) << numBits;
                _readPos = ReadPos + subNum;

                if (numBits == 0)
                    return ret;
            }
        }
    }
}