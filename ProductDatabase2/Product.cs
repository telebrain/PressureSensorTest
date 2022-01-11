using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductDatabase2
{
    public class Product
    {
        public DateTime Date { get; set; }
        public DateTime DateCl { get; set; }
        public string SN { get; set; }
        public string Box { get; set; }
        public string Name { get; set; }
        public int Error { get; set; } = -1;
        public byte[] Log { get; set; }
        public string LogFileExtension { get; set; } = ".json";

        public static string TimeToString(DateTime dateTime)
        {
            return dateTime.ToString("dd.MM.yyyy hh.mm.ss");
        }

        public static int CompareDate(Product product_A, Product product_B)
        {
            if (product_A.Date < product_B.Date)
                return 1;
            else if (product_A.Date > product_B.Date)
                return -1;
            else
                return 0;
        }
    }
}
