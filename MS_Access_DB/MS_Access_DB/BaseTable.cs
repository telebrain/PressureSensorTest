using System;
using System.Collections.Generic;
using System.Data.OleDb;

namespace MS_Access_DB
{
    public class BaseTable
    {

        DbCore db = null;
        string nameTable = "";
        OleDbCommand com = null;
        OleDbDataReader reader = null;

        public BaseTable (string name, DbCore db)
        {
            nameTable = name;
            this.db = db;
        }

        public List<string[]> ReadTable()
        {
            string sql = "SELECT * FROM " + nameTable + ";";
            List<string[]> values = new List<string[]>();
            com = new OleDbCommand(sql, db.DbConnection);
            reader = com.ExecuteReader();
            while (reader.Read())
            {
                string[] row = new string[reader.VisibleFieldCount];
                for (int i = 0; i < reader.VisibleFieldCount; i++)
                {
                    row[i] = reader[i].ToString();
                }
                values.Add(row);
            }
            reader.Close();
            com.Dispose();
            return values;
        }

        public string[] ReadRow(string keyheader, string key)
        {
            string sql = "SELECT * FROM " + nameTable + " WHERE " + keyheader + "=" + key + ";";
            string[] row = null;
            com = new OleDbCommand(sql, db.DbConnection);
            reader = com.ExecuteReader();
            row = new string[reader.VisibleFieldCount];
            if (!reader.Read()) return null;
            for (int i = 0; i < reader.VisibleFieldCount; i++)
            {
                row[i] = reader[i].ToString();
            }
            reader.Close();
            com.Dispose();
            if (row.Length == 0) return null;
            return row;
        }

        public List<string[]> ReadRows(string keyheader, string key)
        {
            string sql = "SELECT * FROM " + nameTable + " WHERE " + keyheader + "=" + key + ";";
            List<string[]> rows = null;
            com = new OleDbCommand(sql, db.DbConnection);
            reader = com.ExecuteReader();
            rows = new List<string[]>();
            while (reader.Read())
            {
                string[] row = new string[reader.VisibleFieldCount];
                for (int i = 0; i < reader.VisibleFieldCount; i++)
                {
                    row[i] = reader[i].ToString();
                }
                rows.Add(row);
            }
            reader.Close();
            com.Dispose();
            if (rows.Count == 0) return null;
            return rows;
        }
        public string ReadField(string header, string keyheader, string key)
        {
            string sql = "SELECT " + header + " FROM " + nameTable + " WHERE " + keyheader + "=" + key + ";";
            com = new OleDbCommand(sql, db.DbConnection);
            reader = com.ExecuteReader();
            bool res = reader.Read();
            string value = reader[0].ToString();
            reader.Close();
            com.Dispose();
            return value;
        }

        //public DateTime ReadField(string header, string keyheader, string key)
        //{
        //    string dt = ReadField()
        //}
        public List<Attachment> ReadAttachments(string header, string keyheader, string key)
        {
            
            string sql = "SELECT " + header + ".FileData, " + header + ".FileName FROM " + nameTable + " WHERE " + keyheader + "=" + key + ";";
            com = new OleDbCommand(sql, db.DbConnection);
            reader = com.ExecuteReader();
            List<Attachment> attachments = new List<Attachment>();
            byte[] data = null;
            while (reader.Read())
            {

                try
                {
                    data = (byte[])reader[0];
                }
                catch
                {
                    break;
                }
                string name = (string)reader[1];
                Attachment attachment = new Attachment(name, data);
                attachments.Add(attachment);
                
            }
            if (attachments.Count == 0) attachments = null;
            return attachments;
        }
        public void NewRow(string keyHeader, string key)
        {
            string sql = "INSERT INTO " + nameTable + " (" + keyHeader + ") VALUES(" + key + ");";
            com = new OleDbCommand(sql, db.DbConnection);
            com.ExecuteNonQuery();
            com.Dispose();
                
        }
        public void UpdField(string header, string keyHeader, string key, string value)
        {
            string sql = "UPDATE " + nameTable + " SET " + header + " = " + value + " WHERE " + keyHeader + " = " + key + ";";
            com = new OleDbCommand(sql, db.DbConnection);
            com.ExecuteNonQuery();
            com.Dispose();
        }
        public void UpdField(string header, string keyHeader, string key, DateTime value)
        {
            UpdField(header, keyHeader, key, this.DateTimeToAccessFormat(value));
        }
        public void WriteRow(string nameTable, string[] values)
        {
            string felds = values[0];
            for (int i = 1; i < values.Length; i++)
            {
                felds = felds + "," + values[i];
            }
            string sql = "INSERT INTO " + nameTable + " VALUES(" + felds + ");";
            com = new OleDbCommand(sql, db.DbConnection);
            com.ExecuteNonQuery();
            com.Dispose();
        }

        public void AddAttachment(string header, string keyHeader, string key, Attachment attachment)
        {
            string sql = "INSERT INTO " + nameTable + " (" + header + ".FileName, " + header + 
                ".FileData) VALUES(\"" + attachment.Name + "\", @file) WHERE " + keyHeader + "=" + key;
            
            com = new OleDbCommand(sql, db.DbConnection);
            com.Parameters.AddWithValue("@file", attachment.AttachContent);
            com.ExecuteNonQuery();
            com.Dispose();
        }

        public void DeleteRow(string keyheader, string key)
        {
            string sql = "DELETE FROM " + nameTable + " WHERE " + keyheader + "=" + key + ";";
            com = new OleDbCommand(sql, db.DbConnection);
            com.ExecuteNonQuery();
            com.Dispose();
        }

        public void DeleteAll()
        {
            string sql = "DELETE FROM " + nameTable + ";";
            com = new OleDbCommand(sql, db.DbConnection);
            com.ExecuteNonQuery();
            com.Dispose();
        }

        protected string dateTimeToStr(DateTime dt)
        {
            string formatStr = "dd.MM.yyyy H:mm:ss";
            return dt.ToString(formatStr);
        }

        protected string DateTimeToAccessFormat(DateTime dt)
        {
            DateTime dtStart = new DateTime(1899, 12, 30);
            TimeSpan d = dt.Subtract(dtStart);
            string dtAccess = Convert.ToString(d.Days + ((Convert.ToDouble(dt.Second) / 86400) + (Convert.ToDouble(dt.Minute) / 1440) + (Convert.ToDouble(dt.Hour) / 24)));
            dtAccess = dtAccess.Replace(",", ".");
            return dtAccess;
        }
       

        protected DateTime DateTimeFromAccess(string dt)
        {

            try
            {
                DateTime value = DateTime.ParseExact(dt, "dd.MM.yyyy HH:mm:ss",
                                         System.Globalization.CultureInfo.InvariantCulture);
                return value;

            }
            catch
            {
                try
                {
                    DateTime value = DateTime.ParseExact(dt, "dd.MM.yyyy H:mm:ss",
                                          System.Globalization.CultureInfo.InvariantCulture);
                    return value;
                }
                catch
                {
                    return DateTime.Now;
                }

            }
        }
        protected string AddQuotes(string parametr)
        {
            return ("\"" + parametr + "\"");
        }
    }
}
