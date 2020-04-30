using System.Data.OleDb;

namespace MS_Access_DB
{
    public class DbCore
    {
        string _path = "";
        OleDbConnection conn = null;
        

        public DbCore(string path)
        {
            _path = path;
        }

        public OleDbConnection DbConnection 
        {
            get { return conn; } 
        }

        public void Connect()
        {
            string strConnect = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + _path + ";Persist Security Info=False";
            TryConnect(strConnect);
            
        }

        public void Disconnect()
        {
            conn.Close();
            conn = null;
        }


        void TryConnect(string strConnect)
        {
            conn = new OleDbConnection(strConnect);
            conn.Open();
        }
    }
}
