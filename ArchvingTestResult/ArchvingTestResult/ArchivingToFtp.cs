using System;
using System.IO;
using System.Text;
using FtpConnect;

namespace ArchvingTestResult
{
    public class ArchivingToFtp: Archiving
    {
        readonly string login;
        readonly string password;
        FtpService ftp;

        public ArchivingToFtp(string remotePath, string localFolder, int maxArchivingIntervalInHours, string login = "", string password = ""):
            base(maxArchivingIntervalInHours)
        {
            RemotePath = remotePath;
            LocalFolder = localFolder;
            this.login = login;
            this.password = password;
            ftp = new FtpService(login, password);
        }

        public override void AddResultToArchive(string fileName, string data)
        {
            try
            {
                ftp.Write(RemotePath + fileName, data);
            }
            catch
            {
                throw new SavingToRemoteArchiveException("Не удалось сохранить файл результатов");
            }
        }
      
    }
}
