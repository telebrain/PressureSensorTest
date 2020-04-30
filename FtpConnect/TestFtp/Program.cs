using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FtpConnect;

namespace TestFtp
{
    class Program
    {
        static void Main(string[] args)
        {
            FtpConn ftp = new FtpConn("ftp://127.0.0.1/F1/", "barsick", "zanachka");
            //Console.WriteLine(ftp.Load("Test.txt"));
            Console.WriteLine(ftp.Write("Test5.txt", "Hello, Lita!!!"));
            Console.ReadKey();
        }
    }
}
