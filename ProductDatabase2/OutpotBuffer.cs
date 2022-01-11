using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductDatabase2
{
    public class OutBuffer : BaseTable
    {
        public OutBuffer(DbCore db, string name) : base(name, db) { }

        const string SnHeader = "Номер";

        public void AddProduct(Product product)
        {
            string[] row = ReadRow(SnHeader, AddQuotes(product.SN));
            if (row != null)
                DeleteRow(SnHeader, AddQuotes(product.SN));
            string[] newRow = new string[] { AddQuotes(DateTimeToStr(product.DateCl)), AddQuotes(product.SN), Convert.ToString(product.Error) };
            base.WriteRow(newRow);
        }

        public void ClearTable() => DeleteAll();
    }
}
