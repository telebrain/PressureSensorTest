using System;

namespace ArchvingTestResult
{
    [Serializable]
    public class SavingToRemoteArchiveException: Exception
    {
        public SavingToRemoteArchiveException(string message): base(message) { }
    }
}
