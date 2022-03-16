using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace CH8
{
    public partial class Main : Form
    {
        private Chip8 c;

        public Main(ref Chip8 c8)
        {
            c = c8;
            InitializeComponent();

            displayBox.Paint += DisplayBox_Paint;

            this.FormClosing += Main_FormClosing;
            this.KeyDown += Main_KeyDown;
            this.KeyUp += Main_KeyUp;
            this.Show();

        }

        private void DisplayBox_Paint(object sender, PaintEventArgs e)
        {
            c.Draw(e.Graphics);
        }

        private void Main_KeyUp(object sender, KeyEventArgs e)
        {
            if (c.keymap.ContainsKey(e.KeyCode))
            {
                c.key[c.keymap[e.KeyCode]] = 0;
            }
        }

        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            if (c.keymap.ContainsKey(e.KeyCode))
            {
                c.key[c.keymap[e.KeyCode]] = 1;
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

    }
}
