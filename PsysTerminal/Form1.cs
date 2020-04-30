using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PressureRack;

namespace PsysTerminal
{
    public partial class Form1 : Form
    {
        PressSystemRack psys = null;

        public Form1()
        {
            InitializeComponent();
            psys = new PressSystemRack("10.4.14.40", 49002, 3);
            psys.UpdPsysVarEvent += UpdPressure;
            psys.ExceptionEvent += Psys_ExceptionEvent;
        }

        private void Psys_ExceptionEvent(object sender, EventArgs e)
        {
            Invoke(new Action(() => ShowErrorMessage(psys.CurrentException.Message)));
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await Task.Run(() => psys.StartPsys());
        }

        private void UpdPressure(object sender, EventArgs e)
        {
            textBox1.Invoke(new Action(() => textBox1.Text = psys.PV.ToString("0.000")));
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            await Task.Run(() => psys.Stop());
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message);
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            await psys.WriteSP((float)numericUpDown1.Value, (int)numericUpDown2.Value);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            psys.ReadPressSystemInfo();
        }
    }
}
