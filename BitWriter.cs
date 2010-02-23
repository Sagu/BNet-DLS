//Most code taken from Patrick Sapinski's http://github.com/kow/Starcraft-2-Battle.Net-Wrapper repo
//DEPRECIATED! Now using Shared.Streams.BitStream
using System;
using System.Text;

namespace BNet
{
    public class BitWriter
    {
        private readonly byte[] _buffer;
        private int _numBits;

        public BitWriter(int capacity)
        {
            _buffer = new byte[capacity];
        }

        public byte[] Buffer
        {
            get { return _buffer; }
        }

        public int NumBits
        {
            get { return _numBits; }
        }

        public int Length
        {
            get
            {
                int pos7 = (WritePos & 7);
                int bitsLeft = 8 - pos7;

                if (bitsLeft == 0)
                    return NumBits / 8;
                return (_numBits + bitsLeft) / 8;
            }
        }

        public int WritePos { get; set; }

        private static int Shift<T>(T left, int right)
        {
            if (typeof(T).Name == "Int32")
                return Convert.ToInt32(left) >> right;

            if (typeof(T).Name == "UInt32")
                return (int)(Convert.ToUInt32(left) >> right);

            return 0;
        }

        private void AlignToNextByte()
        {
            WritePos = (WritePos + 7) & ~7;
        }

        public void WriteBytes(byte[] bytes)
        {
            AlignToNextByte();

            foreach (var b in bytes)
            {
                WriteBits((int)b, 8);
            }
        }

        private void WriteBytes(byte[] bytes, int length)
        {
            var buf = new byte[length];
            Array.Copy(bytes, 0, buf, 0, length);
            WriteBytes(buf);
        }

        public void WriteBuffer(byte[] bytes, int byteLength, int sizeBitCount)
        {
            WriteBits(byteLength, sizeBitCount);
            WriteBytes(bytes, byteLength);
        }

        public void WriteInt32(int intVal)
        {
            WriteBits(intVal, 32);
        }

        public void WriteBits<T>(T valz, int numBits)
        {
            if (numBits > 0)
            {
                _numBits += numBits;
                while (true)
                {
                    int pos7 = (WritePos & 7);
                    int unk = 8 - pos7;
                    var lShift = (char)(1 << unk);
                    int subNum;

                    if (unk < numBits)
                        subNum = unk;
                    else
                    {
                        lShift = (char)(1 << numBits);
                        subNum = numBits;
                    }

                    numBits -= subNum;

                    var firstHalf = (char)(~((lShift - 1) << pos7));

                    var shifted = (char)Shift(valz, numBits);
                    var secondHalf = (char)(((lShift - 1) & shifted) << pos7);

                    Buffer[WritePos >> 3] = (byte)(Buffer[WritePos >> 3] & firstHalf | secondHalf);

                    WritePos += subNum;

                    if (numBits == 0)
                        return;
                }
            }
        }

        public void WriteFourCC(string fourCC)
        {
            byte[] arr = Encoding.ASCII.GetBytes(fourCC);
            Array.Reverse(arr);

            if (arr.Length < 4)
            {
                byte[] backup = arr;
                arr = new byte[arr.Length + 1];
                backup.CopyTo(arr, 0);
            }

            WriteInt32(BitConverter.ToInt32(arr, 0));
        }

        public void WriteHeader(int packetId, int channelId)
        {
            WriteBits(packetId, 6);
            WriteBits(1, 1);
            WriteBits(channelId, 4);
        }

        public override string ToString()
        {
            string str = "";
            int numBytes = _numBits / 8;

            if (_numBits % 8 > 0)
                numBytes++;

            for (int i = 0; i < numBytes; i++)
            {
                if (i > 0 && i % 16 == 0)
                    str += Environment.NewLine;

                str += string.Format("{0:x2} ", Buffer[i]);
            }
            return str;
        }
    }
}