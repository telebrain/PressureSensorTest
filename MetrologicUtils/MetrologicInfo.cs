using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace MetrologicUtils
{
    public class MetrologicInfo
    {
        public string GetMetrologicInfo()
        {
            string result = "Метрологически значимая часть: \n\n";
            string[] names = new string[] { "PressureSensorTestCore.dll", "SDM_comm.dll", "PressSystems.dll", "PressureRack.dll", "MetrologicUtils.dll" };
            foreach (string name in names)
            {
                result += (name + ":\n");
                try
                {
                    result += ($"Версия v.{FileVersionInfo.GetVersionInfo(name).FileVersion} \n");
                    result += $"Контрольная сумма: {GetHash(name)} \n\n";
                }
                catch
                {
                    result += "Информация недоступна\n\n";
                }
            }
            return result;
        }

        private string GetHash(string name)
        {
            string hash = "";

            try
            {
                byte[] data = LoadFile(name);
                hash += GetCrc(data).ToString("x");
            }
            catch
            {
                return "нет информации";
            }
            return hash;
        }

        private byte[] LoadFile(string name)
        {
            byte[] data = null;
            using (FileStream fs = new FileStream(name, FileMode.Open, FileAccess.Read))
            {
                data = new byte[fs.Length];
                int nBytesToRead = (int)fs.Length;
                int nBytesRead = 0;
                while (nBytesToRead > 0)
                {
                    int n = fs.Read(data, nBytesRead, nBytesToRead);
                    if (n == 0)
                        break;
                    nBytesRead += n;
                    nBytesToRead -= n;
                }
            }
            return data;
        }

        private UInt32 GetCrc(byte[] source)
        {
            const UInt32 DefaultPolynomial = 0xedb88320; // 0x04C11DB7;
            const UInt32 DefaultSeed = 0xffffffff;

            UInt32[] crcTable = new UInt32[256];
            UInt32 crc;

            for (UInt32 i = 0; i < 256; i++)
            {
                crc = i;
                for (UInt32 j = 0; j < 8; j++)
                    crc = (crc & 1) != 0 ? (crc >> 1) ^ DefaultPolynomial : crc >> 1;

                crcTable[i] = crc;
            };

            crc = DefaultSeed;

            foreach (byte s in source)
            {
                crc = crcTable[(crc ^ s) & 0xFF] ^ (crc >> 8);
            }

            crc ^= 0xFFFFFFFF;

            return crc;
        }
    }
}
