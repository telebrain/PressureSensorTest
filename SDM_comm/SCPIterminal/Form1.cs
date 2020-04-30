using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SDM_comm;

namespace SCPIterminal
{
    public partial class Form1 : Form
    {
        Transport transport = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            transport = new Transport("10.4.14.103", 5024);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            transport.Dispose();
            transport = null;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            transport.OnlySend(textBox1.Text);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox2.Text = transport.OnlyReceive();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            textBox2.Text = transport.Exch(textBox1.Text);
        }
    }
}
