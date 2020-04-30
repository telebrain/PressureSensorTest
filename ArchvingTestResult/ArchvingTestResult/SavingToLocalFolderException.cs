using System;


namespace ArchvingTestResult
{
    [Serializable]
    public class SavingToLocalFolderException: Exception
    {
        public SavingToLocalFolderException(string message): base(message) { }
    }
}
