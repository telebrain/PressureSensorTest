using System;
using System.Collections.Generic;
using MS_Access_DB;

namespace DbCalibration
{
    class InpBufferTbl: MS_Access_DB.BaseTable
    {
        const string NameTable = "Входной_буфер";
        public InpBufferTbl(DbCore db): base(NameTable, db)
        {     
        }

        public InpProduct ExtractFirstItem()
        {
            InpProduct firstItem = this.GetFirstItem();
            if (firstItem == null) return null;
            this.DeleteRow("Номер", AddQuotes(firstItem.SN));
            return firstItem;
        }

        private InpProduct GetFirstItem()
        {
            List<InpProduct> table = this.GetTable();
            if (table == null) return null;
            if (table.Count == 0) return null;
            InpProduct firstItem = table[0];
            foreach (InpProduct item in table)
            {
                if (item.Date < firstItem.Date) firstItem = item;
            }
            return firstItem;
        }

        private InpProduct ReadInpBufferItem(DateTime key)
        {
            string sDate = base.DateTimeToAccessFormat(key);
            string[] row = base.ReadRow("Дата", sDate);
            if (row == null) return null;
            return RowToClass(row);
        }
        private List<InpProduct> GetTable()
        {
            List<string[]> strTable = ReadTable();
            int size = strTable.Count;
            if (size == 0) return null;
            List<InpProduct> Table = new List<InpProduct>();
            foreach (string[] item in strTable)
            {
                Table.Add(RowToClass(item));
            }
            return Table;
        }

        private InpProduct RowToClass(string[] row)
        {
            InpProduct inpBuffer = new InpProduct();
            inpBuffer.Date = base.DateTimeFromAccess(row[0]);
            inpBuffer.SN = row[1];
            inpBuffer.Device = row[2];
            inpBuffer.Name = row[3];
            return inpBuffer;
        }
    }
}