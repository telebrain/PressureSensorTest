using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ArchvingTestResult
{
    public class LocalFilePropertys: IComparable
    {
        public string FilePath { get; private set; }
        public string FileName { get; private set; }
        public DateTime CreationTime { get; private set; }

        public LocalFilePropertys(string filePath)
        {
            FilePath = filePath;
            Path.GetFileName(filePath);
            CreationTime = File.GetCreationTime(filePath);
        }

        public int CompareTo(object obj)
        {
            var comparableObj = obj as LocalFilePropertys;
            if (CreationTime > comparableObj.CreationTime)
                return 1;
            if (CreationTime < comparableObj.CreationTime)
                return -1;
            return 0;
        }
    }
}
