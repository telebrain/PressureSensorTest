using System;
using System.Collections.Generic;
using System.Data.OleDb;
using log4net;

namespace ProductDatabase2
{
    public class BaseTable
    {
        readonly static ILog log = LogManager.GetLogger(typeof(BaseTable));

        readonly DbCore db = null;
        public string TableName { get; }
        OleDbCommand com = null;
        OleDbDataReader reader = null;

        public BaseTable(string tableName, DbCore db)
        {
            TableName = tableName;
            this.db = db;
        }

        protected List<string[]> ReadTable()
        {
            string sql = $"SELECT * FROM {TableName};";
            try
            {

                List<string[]> values = new List<string[]>();
                using (com = new OleDbCommand(sql, db.DbConnection))
                using (reader = com.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string[] row = new string[reader.VisibleFieldCount];
                        for (int i = 0; i < reader.VisibleFieldCount; i++)
                        {
                            row[i] = reader[i].ToString();
                        }
                        values.Add(row);
                    }
                }
                return values;
            }
            catch (Exception ex)
            {
                log.Error($"Ошибка чтения базы: {ex.Message}, sql: {sql}");
                throw;
            }
        }

        protected string[] ReadRow(string keyheader, string key)
        {
            string sql = "SELECT * FROM " + TableName + " WHERE " + keyheader + "=" + key + ";";
            string[] row = null;
            try
            {
                using (com = new OleDbCommand(sql, db.DbConnection))
                using (reader = com.ExecuteReader())
                {
                    row = new string[reader.VisibleFieldCount];
                    if (!reader.Read()) return null;
                    for (int i = 0; i < reader.VisibleFieldCount; i++)
                    {
                        row[i] = reader[i].ToString();
                    }
                    if (row.Length == 0) return null;
                    return row;
                }
            }
            catch (Exception ex)
            {
                log.Error($"Ошибка чтения базы: {ex.Message}, sql: {sql}");
                throw;
            }
        }

        protected List<string[]> ReadRows(string keyheader, string key)
        {
            string sql = "SELECT * FROM " + TableName + " WHERE " + keyheader + "=" + key + ";";
            try
            {
                List<string[]> rows = null;
                using (com = new OleDbCommand(sql, db.DbConnection))
                using (reader = com.ExecuteReader())
                {
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
                    if (rows.Count == 0) return null;
                    return rows;
                }
            }
            catch (Exception ex)
            {
                log.Error($"Ошибка чтения базы: {ex.Message}, sql: {sql}");
                throw;
            }
        }
        protected string ReadField(string header, string keyheader, string key)
        {
            string sql = "SELECT " + header + " FROM " + TableName + " WHERE " + keyheader + "=" + key + ";";
            try
            {
                using (com = new OleDbCommand(sql, db.DbConnection))
                using (reader = com.ExecuteReader())
                {
                    bool res = reader.Read();
                    string value = reader[0].ToString();
                    return value;
                }
            }
            catch (Exception ex)
            {
                log.Error($"Ошибка чтения базы: {ex.Message}, sql: {sql}");
                throw;
            }
        }

        //public DateTime ReadField(string header, string keyheader, string key)
        //{
        //    string dt = ReadField()
        //}
        protected List<Attachment> ReadAttachments(string header, string keyheader, string key)
        {

            string sql = "SELECT " + header + ".FileData, " + header + ".FileName FROM " + TableName + " WHERE " + keyheader + "=" + key + ";";
            try
            {
                using (com = new OleDbCommand(sql, db.DbConnection))
                using (reader = com.ExecuteReader())
                {
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
            }
            catch (Exception ex)
            {
                log.Error($"Ошибка чтения базы: {ex.Message}, sql: {sql}");
                throw;
            }
        }

        private void ExecuteNonQuery(string sql)
        {
            try
            {
                using (com = new OleDbCommand(sql, db.DbConnection))
                {
                    com.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                log.Error($"Ошибка записи в базу: {ex.Message}, sql: {sql}");
            }
        }



        protected void NewRow(string keyHeader, string key)
        {
            string sql = "INSERT INTO " + TableName + " (" + keyHeader + ") VALUES(" + key + ");";
            ExecuteNonQuery(sql);

        }
        protected void UpdField(string header, string keyHeader, string key, string value)
        {
            string sql = "UPDATE " + TableName + " SET " + header + " = " + value + " WHERE " + keyHeader + " = " + key + ";";
            ExecuteNonQuery(sql);
        }
        protected void UpdField(string header, string keyHeader, string key, DateTime value)
        {
            UpdField(header, keyHeader, key, this.DateTimeToAccessFormat(value));
        }
        protected void WriteRow(string[] values)
        {
            string felds = values[0];
            for (int i = 1; i < values.Length; i++)
            {
                felds = felds + "," + values[i];
            }
            string sql = "INSERT INTO " + TableName + " VALUES(" + felds + ");";
            ExecuteNonQuery(sql);
        }

        protected void AddAttachment(string header, string keyHeader, string key, Attachment attachment)
        {
            string sql = "INSERT INTO " + TableName + " (" + header + ".FileName, " + header +
                ".FileData) VALUES(\"" + attachment.Name + "\", @file) WHERE " + keyHeader + "=" + key;
            try
            {
                using (com = new OleDbCommand(sql, db.DbConnection))
                {
                    com.Parameters.AddWithValue("@file", attachment.AttachContent);
                    com.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                log.Error($"Ошибка записи в базу: {ex.Message}, sql: {sql}");
            }


        }

        protected void DeleteRow(string keyheader, string key)
        {
            string sql = "DELETE FROM " + TableName + " WHERE " + keyheader + "=" + key + ";";
            ExecuteNonQuery(sql);
        }

        protected void DeleteAll()
        {
            string sql = "DELETE FROM " + TableName + ";";
            ExecuteNonQuery(sql);
        }

        protected string DateTimeToStr(DateTime dt)
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
                DateTime value = DateTime.ParseExact(dt, "dd.MM.yyyy H:mm:ss",
                                         System.Globalization.CultureInfo.InvariantCulture);
                return value;

            }
            catch
            {
                return DateTime.Now;

            }
        }
        protected string AddQuotes(string parametr)
        {
            return ("\"" + parametr + "\"");
        }
    }
}

