using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace FtpConnect
{
    public class FtpService
    {
        public string Login { get; set; }
        public string Password { get; set; }

        public FtpService()
        {
            Login = "";
            Password = "";
        }

        public FtpService(string login, string password)
        {
            Login = login;
            Password = password;
        }

        public string Load(string path)
        {
            string data = "";
            
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(path);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            if (!string.IsNullOrEmpty(Password))
                request.Credentials = new NetworkCredential(Login, Password);
            // request.EnableSsl = true; // если используется ssl

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();

            byte[] buffer = new byte[65535];
            int size = responseStream.Read(buffer, 0, buffer.Length);

            data = Encoding.Default.GetString(buffer, 0, size);
            
            response.Close();

            return data;
        }

        public bool Write(string path, string content)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(path);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            if (!string.IsNullOrEmpty(Password))
                request.Credentials = new NetworkCredential(Login, Password);

            Stream requestStream = request.GetRequestStream();
            byte[] buffer = Encoding.UTF8.GetBytes(content);
            requestStream.Write(buffer, 0, buffer.Length);
            requestStream.Close();

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            string status = response.StatusDescription;
            response.Close();

            return (status.IndexOf("Successfully transferred") >= 0);

        }
    }
}
