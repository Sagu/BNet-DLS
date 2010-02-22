//Most code taken from Patrick Sapinski's http://github.com/kow/Starcraft-2-Battle.Net-Wrapper repo
using System;
using System.Text;

/**
 * RC4 Class
 * @package RC4.NET
 */
namespace BNet
{
    public class ARC4
    {
        private readonly byte[] state;
        private byte x, y;

        public ARC4(byte[] key)
        {
            state = new byte[256];
            x = y = 0;
            KeySetup(key);
        }

        public int Process(byte[] buffer, int start, int count)
        {
            return InternalTransformBlock(buffer, start, count, buffer, start);
        }

        private void KeySetup(byte[] key)
        {
            byte index1 = 0;
            byte index2 = 0;

            for (int counter = 0; counter < 256; counter++)
            {
                state[counter] = (byte)counter;
            }
            x = 0;
            y = 0;
            for (int counter = 0; counter < 256; counter++)
            {
                index2 = (byte)(key[index1] + state[counter] + index2);
                // swap byte
                byte tmp = state[counter];
                state[counter] = state[index2];
                state[index2] = tmp;
                index1 = (byte)((index1 + 1) % key.Length);
            }
        }

        private int InternalTransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            for (int counter = 0; counter < inputCount; counter++)
            {
                x = (byte)(x + 1);
                y = (byte)(state[x] + y);
                // swap byte
                byte tmp = state[x];
                state[x] = state[y];
                state[y] = tmp;

                byte xorIndex = (byte)(state[x] + state[y]);
                outputBuffer[outputOffset + counter] = (byte)(inputBuffer[inputOffset + counter] ^ state[xorIndex]);
            }
            return inputCount;
        }
    }
    public class RC4
    { /* inherits Page for ASP.NET */

        public static void RC4test(ref byte[] bytes, byte[] key)
        {
            Byte[] s = new byte[256];
            Byte[] k = new byte[256];
            Byte temp;
            int i, j;

            for (i = 0; i < 256; i++)
            {
                s[i] = (byte)i;
                k[i] = key[i % key.GetLength(0)];
            }

            j = 0;
            for (i = 0; i < 256; i++)
            {
                j = (j + s[i] + k[i]) % 256;
                temp = s[i];
                s[i] = s[j];
                s[j] = temp;
            }

            i = j = 0;
            for (int x = 0; x < bytes.GetLength(0); x++)
            {
                i = (i + 1) % 256;
                j = (j + s[i]) % 256;
                temp = s[i];
                s[i] = s[j];
                s[j] = temp;
                int t = (s[i] + s[j]) % 256;
                bytes[x] ^= s[t];
            }
        }
        /**
         * Get ASCII Integer Code
         *
         * @param char ch Character to get ASCII value for
         * @access private
         * @return int
         */
        private static int ord(char ch)
        {
            return (int)(Encoding.GetEncoding(1252).GetBytes(ch + "")[0]);
        }
        /**
         * Get character representation of ASCII Code
         *
         * @param int i ASCII code
         * @access private
         * @return char
         */
        private static char chr(int i)
        {
            byte[] bytes = new byte[1];
            bytes[0] = (byte)i;
            return Encoding.GetEncoding(1252).GetString(bytes)[0];
        }
        /**
         * Convert Hex to Binary (hex2bin)
         *
         * @param string packtype Rudimentary in this implementation
         * @param string datastring Hex to be packed into Binary
         * @access private
         * @return string
         */
        private static string pack(string packtype, string datastring)
        {
            int i, j, datalength, packsize;
            byte[] bytes;
            char[] hex;
            string tmp;

            datalength = datastring.Length;
            packsize = (datalength / 2) + (datalength % 2);
            bytes = new byte[packsize];
            hex = new char[2];

            for (i = j = 0; i < datalength; i += 2)
            {
                hex[0] = datastring[i];
                if (datalength - i == 1)
                    hex[1] = '0';
                else
                    hex[1] = datastring[i + 1];
                tmp = new string(hex, 0, 2);
                try { bytes[j++] = byte.Parse(tmp, System.Globalization.NumberStyles.HexNumber); }
                catch { } /* grin */
            }
            return Encoding.GetEncoding(1252).GetString(bytes);
        }
        /**
         * Convert Binary to Hex (bin2hex)
         *
         * @param string bindata Binary data
         * @access public
         * @return string
         */
        public static string bin2hex(string bindata)
        {
            int i;
            byte[] bytes = Encoding.GetEncoding(1252).GetBytes(bindata);
            string hexString = "";
            for (i = 0; i < bytes.Length; i++)
            {
                hexString += bytes[i].ToString("x2");
            }
            return hexString;
        }
        /**
         * The symmetric encryption function
         *
         * @param string pwd Key to encrypt with (can be binary of hex)
         * @param string data Content to be encrypted
         * @param bool ispwdHex Key passed is in hexadecimal or not
         * @access public
         * @return string
         */
        public static string Encrypt(string pwd, string data, bool ispwdHex)
        {
            int a, i, j, k, tmp, pwd_length, data_length;
            int[] key, box;
            byte[] cipher;
            //string cipher;

            if (ispwdHex)
                pwd = pack("H*", pwd); // valid input, please!
            pwd_length = pwd.Length;
            data_length = data.Length;
            key = new int[256];
            box = new int[256];
            cipher = new byte[data.Length];
            //cipher = "";

            for (i = 0; i < 256; i++)
            {
                key[i] = ord(pwd[i % pwd_length]);
                box[i] = i;
            }
            for (j = i = 0; i < 256; i++)
            {
                j = (j + box[i] + key[i]) % 256;
                tmp = box[i];
                box[i] = box[j];
                box[j] = tmp;
            }
            for (a = j = i = 0; i < data_length; i++)
            {
                a = (a + 1) % 256;
                j = (j + box[a]) % 256;
                tmp = box[a];
                box[a] = box[j];
                box[j] = tmp;
                k = box[((box[a] + box[j]) % 256)];
                cipher[i] = (byte)(ord(data[i]) ^ k);
                //cipher += chr(ord(data[i]) ^ k);
            }
            return Encoding.GetEncoding(1252).GetString(cipher);
            //return cipher;
        }
        /**
         * Decryption, recall encryption
         *
         * @param string pwd Key to decrypt with (can be binary of hex)
         * @param string data Content to be decrypted
         * @param bool ispwdHex Key passed is in hexadecimal or not
         * @access public
         * @return string
         */
        public static string Decrypt(string pwd, string data, bool ispwdHex)
        {
            return Encrypt(pwd, data, ispwdHex);
        }
    }
}