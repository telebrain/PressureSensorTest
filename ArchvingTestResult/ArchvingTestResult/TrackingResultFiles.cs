using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.IO;

namespace ArchvingTestResult
{
    public class TrackingResultFiles: IDisposable
    {
        readonly Archiving archiving;
        Timer timer;
        readonly double interval;

        public event EventHandler SuccessfulSaveEvent;

        public TrackingResultFiles(Archiving archiving, int trackinIntervalInMinutes)
        {
            this.archiving = archiving;
            interval = Convert.ToDouble(trackinIntervalInMinutes * 60000);
        }

        public void StartTracking()
        {
            // Через определенное время будет происходить проверка возможности копирования в удаленную папку из локальной
            timer = new Timer(interval);
            timer.Elapsed += ((obj, e) => TryArchiving());
            TryArchiving();
            timer.Start();
        }

        public void StopTracking()
        {
            timer.Stop();
            Dispose();
        }

        private void TryArchiving()
        {
            string[] files = archiving.GetFilesFromLocalFolder();
            if (files == null || files.Length == 0) // Если папка пуста, выходим
                return;
            try
            {
                foreach (string filePath in files)
                {
                    lock (archiving.SyncRoot)
                    {
                        string fileName = Path.GetFileName(filePath);
                        archiving.AddFromLocalFolder(fileName, filePath);                       
                    }
                }
                SuccessfulSaveEvent?.Invoke(this, new EventArgs());
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        public void Dispose()
        {
            timer.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
