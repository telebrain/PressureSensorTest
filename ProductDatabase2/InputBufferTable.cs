using System;
using System.Collections.Generic;
using log4net;

namespace ProductDatabase2
{
    class InputBufferTable : BaseTable
    {
        readonly string dbName;

        public InputBufferTable(DbCore db, string name) : base(name, db)
        {
            dbName = db.BaseName;
        }

        const string SnHeader = "Номер";
        const string BoxHeader = "Оснастка";

        // Если checkBufferState true, проверяется, пустой ли буфер
        public void AddProduct(Product product, bool checkBufferState = true)
        {
            if (checkBufferState)
            {
                var dataFromTable = base.ReadTable();
                if (dataFromTable.Count != 0)
                    throw new AddToInputBufferException(dbName, TableName);
            }
            string[] values = new string[] { base.DateTimeToStr(product.Date), product.SN, product.Box, product.Name };
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = AddQuotes(values[i]);
            }
            try
            {
                base.WriteRow(values);
            }
            catch(Exception ex)
            {
                throw new WriteToInputBufferException(ex, dbName, TableName, product.SN);
            }
        }

        public Product GetProduct()
        {
            List<Product> table = GetTable();
            if (table == null)
                return null;
            if (table.Count == 0)
                return null;
            if (table.Count > 1)
                throw new InputBufferStateException(dbName, TableName);

            return table[0];
        }

        public Product GetProductBySn(string sn)
        {
            try
            {
                return ReadInpBufferItemBySn(sn);
            }
            catch(Exception ex)
            {
                throw new ReadProductFromInputBufferException(ex, dbName, TableName, sn);
            }
        }

        public Product GetProductByBox(string box)
        {
            try
            {
                return ReadInpBufferItemByBox(box);
            }
            catch (Exception ex)
            {
                throw new ReadProductFromInputBufferException(ex, dbName, TableName, box);
            }
        }

        public Product CutProduct(string SN)
        {
            try
            {
                var product = ReadInpBufferItemBySn(SN);
                DeleteProduct(SN);
                return product;
            }
            catch(Exception ex)
            {
                throw new CutProductFromInputBufferException(ex, dbName, base.TableName, SN);
            }
        }

        public void DeleteProductByBox(string box)
        {
            var product = ReadInpBufferItemByBox(box);
            if (product != null)
                DeleteProduct(product.SN);
        }

        public void ClearBuffer()
        {
            DeleteAll();
        }

        private Product ReadInpBufferItemBySn(string sn)
        {
            string[] row = null;
            row = ReadRow(SnHeader, AddQuotes(sn));
            if (row == null) return null;
            return ParseRow(row);
        }

        private Product ReadInpBufferItemByBox(string box)
        {
            var rows = ReadRows(BoxHeader, AddQuotes(box));
            if (rows == null || rows.Count == 09)
                return null;
            List<Product> products = new List<Product>();
            foreach(var row in rows)
                products.Add(ParseRow(row));
            products.Sort(Product.CompareDate);
            return products[0];        
        }


        private List<Product> GetTable()
        {
            List<string[]> strTable = ReadTable();
            int size = strTable.Count;
            if (size == 0) return null;
            List<Product> Table = new List<Product>();
            foreach (string[] item in strTable)
            {
                Table.Add(ParseRow(item));
            }
            return Table;
        }

        private void DeleteProduct(string sn)
        {
            DeleteRow(SnHeader, AddQuotes(sn));
        }

        private Product ParseRow(string[] row)
        {
            var inpBufferRow = new Product();
            inpBufferRow.Date = DateTimeFromAccess(row[0]);
            inpBufferRow.SN = row[1];
            inpBufferRow.Box = row[2];
            inpBufferRow.Name = row[3];
            return inpBufferRow;
        }
    }
}


