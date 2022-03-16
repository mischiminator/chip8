using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CH8
{
    internal class Program
    {
        public static Main main;
        private static byte[] gfx_buffer = new byte[2048];

        [STAThread]
        static void Main(string[] args)
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Chip8 chip8 = new Chip8();
            main = new Main(ref chip8);
            for (int i = 0; i < gfx_buffer.Length; i++)
            {
                gfx_buffer[i] = 0;
            }

            chip8.init();
            chip8.loadRom(@"D:\Users\BKU\MischaFuerst\Downloads\test_opcode.ch8");
            //chip8.loadRom(@"D:\Users\BKU\MischaFuerst\Downloads\tetris.c8");

            while (chip8.unknown_opcode == 0)
            {
                chip8.emulateCycle();
                chip8.debug();
                if (chip8.drawFlag)
                {
                    chip8.drawFlag = false;
                }
                //Thread.Sleep(17);
            }

            //Console.WriteLine(ByteArrayToString(chip8.memory));

            Application.Run();
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}
