using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Diagnostics;
using System.IO;

namespace MetrologicUtils
{
    public class MetrologicInfo
    {
        //LibInfo[] libInfos = new LibInfo[] { new LibInfo("PressureSensorTestCore.dll", "1.0.7.0", 
        //    "87-14-3D-DF-D5-F6-14-F9-6B-CD-A4-48-21-0B-A8-42") };

        LibInfo[] libInfos = new LibInfo[] { new LibInfo("PressureSensorTestCore.dll", "1.0.7.0",
            "6A-B0-A1-D5-32-FD-34-A1-FB-49-B2-5A-2C-D6-7C-49") };

        public bool ValidInfo { get; }

        public bool CheckValid()
        {
            foreach(var item in libInfos)
            {
                if (!(item.ValidCRC && item.ValidVer))
                    return false;
            }
            return true;
        }

        public string GetMetrologicInfo()
        {           
            string result = "Метрологически значимая часть: \n\n\n";
            string[] names = new string[] { "PressureSensorTestCore.dll" };
            foreach (var item in libInfos)
            {
                result += (item.ToString() + "\n");             
            }
            return result;
        }
        

        class LibInfo
        {
            public string Name{ get; }

            public string Version { get; }

            public string CRC { get; }

            public DateTime Date { get; }

            public string CertifiedVersion { get; }

            public string CertifedCRC { get; }

            public bool ValidCRC { get; }

            public bool ValidVer { get; }

            public LibInfo(string name, string version, string crc)
            {
                Name = name;
                CertifiedVersion = version;
                CertifedCRC = crc;
                try
                {
                    Version = FileVersionInfo.GetVersionInfo(name).FileVersion;
                    CRC = GetHash(name);
                    Date = File.GetLastWriteTime(name);
                    ValidCRC = true; // CertifedCRC == CRC;
                    ValidVer = true; // CertifiedVersion == Version;
                }
                catch
                {
                    Version = "";
                    CRC = "";
                    ValidCRC = false;
                    ValidVer = false;
                }
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

            private string GetHash(string name)
            {
                try
                {
                    byte[] data = LoadFile(name);
                    var md5 = MD5.Create();
                    var hash = md5.ComputeHash(data);
                    return BitConverter.ToString(hash);
                }
                catch
                {
                    return "";
                }
            }

            public override string ToString()
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(Name + ":\n\n");
                if (Version == "" || CRC == "")
                {
                    stringBuilder.Append("Информация о файле недоступна \n\n");
                    return stringBuilder.ToString();
                }

                if (ValidVer)
                    stringBuilder.Append($"Версия v.{Version} \n\n");
                else
                {
                    stringBuilder.Append("Внимание! Номер версии используемой библиотеки не совпадает с номером версии сертифицированной библиотеки:\n");
                    stringBuilder.Append($"Сертифицированная библиотека: v.{CertifiedVersion} \n");
                    stringBuilder.Append($"Используемая библиотека: v.{Version} \n\n");
                }
                if (ValidCRC)
                    stringBuilder.Append($"Контрольная сумма: {CRC} \n\n");
                else
                {
                    stringBuilder.Append("Внимание! Контрольная сумма библиотеки не совпадает с контрольной суммой сертифицированной библиотеки:\n");
                    stringBuilder.Append($"Контрольная сумма сертифицированной библиотеки: {CertifedCRC} \n");
                    stringBuilder.Append($"Контрольная сумма используемой библиотеки: {CRC} \n\n");
                }
                stringBuilder.Append($"Дата последнего изменения: {Date} \n\n");
                return stringBuilder.ToString();

            }
        }

        //private UInt32 GetCrc(byte[] source)
        //{
        //    const UInt32 DefaultPolynomial = 0xedb88320; // 0x04C11DB7;
        //    const UInt32 DefaultSeed = 0xffffffff;

        //    UInt32[] crcTable = new UInt32[256];
        //    UInt32 crc;

        //    for (UInt32 i = 0; i < 256; i++)
        //    {
        //        crc = i;
        //        for (UInt32 j = 0; j < 8; j++)
        //            crc = (crc & 1) != 0 ? (crc >> 1) ^ DefaultPolynomial : crc >> 1;

        //        crcTable[i] = crc;
        //    };

        //    crc = DefaultSeed;

        //    foreach (byte s in source)
        //    {
        //        crc = crcTable[(crc ^ s) & 0xFF] ^ (crc >> 8);
        //    }

        //    crc ^= 0xFFFFFFFF;

        //    return crc;
        //}
    }
}
