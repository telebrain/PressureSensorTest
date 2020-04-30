using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MS_Access_DB;

namespace DbCalibration
{
    public class WorkTable : BaseTable
    {
        const string NameTable = "[В работе]";
        public WorkTable(DbCore db) : base(NameTable, db)
        {
        }

        public void WriteRow(InpProduct row)
        {           
            string[] strRow = this.ReadRow("[Серийный номер]", AddQuotes(row.SN));
            if (strRow != null)
            {
                DeleteRow("[Серийный номер]", AddQuotes(row.SN));
            }
            else
            {
                strRow = new string[4];
            }

            strRow[0] = DateTimeToAccessFormat(row.Date);
            strRow[1] = AddQuotes(row.SN);
            strRow[2] = AddQuotes(row.Device);
            strRow[3] = AddQuotes(row.Name);
            base.WriteRow(NameTable, strRow);
        }

        public InpProduct ExtractRow(string SN)
        {
            InpProduct product = GetRow(SN);
            DeleteRow("[Серийный номер]", base.AddQuotes(SN));
            return product;
        }

        public InpProduct GetRow(string SN)
        {
            string[] strRow = this.ReadRow("[Серийный номер]", AddQuotes(SN));
            if (strRow == null) return null;
            InpProduct product = new InpProduct
            {
                Date = base.DateTimeFromAccess(strRow[0]),
                SN = strRow[1],
                Device = strRow[2],
                Name = strRow[3]
            };
            return product;
        }

        public void ClearTable() => base.DeleteAll();
    }
}
