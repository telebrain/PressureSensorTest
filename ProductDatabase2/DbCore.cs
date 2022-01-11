using System.Data.OleDb;
using System;
using log4net;

namespace ProductDatabase2
{
    public class DbCore
    {
        readonly static ILog log = LogManager.GetLogger(typeof(DbCore));

        public OleDbConnection DbConnection { get; private set; }

        public string BaseName { get; }

        string path;
        string password = "";

        public DbCore(string baseName, string path, string password = "")
        {
            BaseName = baseName;
            this.path = path;
            this.password = password;
        }

        public DbCore() { }

        public void Connect(string path, string password = "")
        {
            this.path = path;
            this.password = password;
            Connect();
        }

        public void Connect()
        {
            string strConnect = "";
            if (string.IsNullOrEmpty(password))
                strConnect = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={path};Persist Security Info=False";
            else
                strConnect = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={path};Jet OLEDB:Database Password={password};";
            TryConnect(strConnect);

        }

        public void Disconnect()
        {
            DbConnection.Close();
            DbConnection = null;
        }


        void TryConnect(string strConnect)
        {
            try
            {
                DbConnection = new OleDbConnection(strConnect);
                DbConnection.Open();
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                throw;
            }
        }
    }
}
