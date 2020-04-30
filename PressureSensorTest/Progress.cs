using System;

namespace PressureSensorTest
{
    public class Progress : IProgress<int>
    {
        public event EventHandler<int> ProgressChanged;

        public void Report(int value)
        {
            ProgressChanged?.Invoke(this, value);
        }
    }
}
