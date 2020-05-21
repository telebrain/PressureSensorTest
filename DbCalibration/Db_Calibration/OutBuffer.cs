using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MS_Access_DB;

namespace StandDb
{
    public class OutBuffer: BaseTable
    {
        public OutBuffer(DbCore db): base ("Выходной_буфер", db) { }

        public void AddProduct(DateTime dtClosed, string SN, int error)
        {
            string[] row = ReadRow("Номер", AddQuotes(SN));
            if (row != null) DeleteRow("Номер", AddQuotes(SN));
            NewRow("Номер", AddQuotes(SN));
            string dateField = AddQuotes(dateTimeToStr(dtClosed));
            UpdField("Дата", "Номер", AddQuotes(SN), dateField);
            UpdField("Ошибка", "Номер", AddQuotes(SN), Convert.ToString(error));
        }

        
    }
}
