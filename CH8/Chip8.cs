using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CH8
{
    public class Chip8
    {


        SolidBrush white = new SolidBrush(Color.White);
        SolidBrush black = new SolidBrush(Color.Black);

        internal ushort unknown_opcode = 0;
        internal bool drawFlag = false;
        internal Dictionary<Keys, byte> keymap = new Dictionary<Keys, byte>
        {
            { Keys.D1, 1},   { Keys.D2, 2}, { Keys.D3, 3},   { Keys.D4, 0xC},
            { Keys.Q,  4},   { Keys.W,  5}, { Keys.E,  6},   { Keys.R,  0xD},
            { Keys.A,  7},   { Keys.S,  8}, { Keys.D,  9},   { Keys.F,  0xE},
            { Keys.Y,  0xA}, { Keys.X,  0}, { Keys.C,  0xB}, { Keys.V,  0xF}
        };

        ushort opcode;
        ushort SP;
        ushort I;
        ushort PC;
        ushort[] stack = new ushort[16];

        byte delay_timer;
        byte sound_timer;
        internal byte[] memory = new byte[4096];
        byte[] V = new byte[16];
        internal byte[] gfx = new byte[64 * 32];
        internal byte[] key = new byte[16];

        ushort[] chip8_fontset = new ushort[]
{
  0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
  0x20, 0x60, 0x20, 0x20, 0x70, // 1
  0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
  0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
  0x90, 0x90, 0xF0, 0x10, 0x10, // 4
  0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
  0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
  0xF0, 0x10, 0x20, 0x40, 0x40, // 7
  0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
  0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
  0xF0, 0x90, 0xF0, 0x90, 0x90, // A
  0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
  0xF0, 0x80, 0x80, 0x80, 0xF0, // C
  0xE0, 0x90, 0x90, 0x90, 0xE0, // D
  0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
  0xF0, 0x80, 0xF0, 0x80, 0x80  // F
};

        internal void debug()
        {
            Console.Clear();
            Console.WriteLine("PC: {0:X2}", PC);
            Console.WriteLine("I:  {0:X2}", I);
            Console.WriteLine("SP: {0:X2}", SP);

            Console.WriteLine("OP Code: {0:X} Masked: {1:X}", opcode, (opcode & 0xF000));

            Console.WriteLine("V:");
            for (int i = 0; i < 16; i++)
            {
                Console.WriteLine(" " + (i + 1) + ": {0:X2}", V[i]);
            }

            if (unknown_opcode != 0)
            {
                Console.WriteLine("Unknown OPCODE: {0:X}", unknown_opcode);
            }
        }

        internal void init()
        {
            opcode = 0;
            SP = 0;
            I = 0;
            PC = 0x200;

            for (int i = 0; i < memory.Length; i++)
            {
                memory[i] = 0;
            }

            for (int i = 0; i < V.Length; i++)
            {
                V[i] = 0;
            }

            for (int i = 0; i < gfx.Length; i++)
            {
                gfx[i] = 0;
            }

            for (int i = 0; i < stack.Length; i++)
            {
                stack[i] = 0;
            }

            for (int i = 0; i < 80; i++)
            {
                memory[i] = (byte)(chip8_fontset[i]);
            }

            delay_timer = 0;
            sound_timer = 0;
        }

        internal void loadRom(string path)
        {
            int i = 0;
            foreach (byte b in File.ReadAllBytes(path))
            {
                memory[0x200 + i++] = b;
            }
        }


        public void Draw(Graphics g)
        {
            int pWidth = 10;
            for (int y = 0; y < 32; ++y)
                for (int x = 0; x < 64; ++x)
                    g.FillRectangle(gfx[(y * 64) + x] == 1 ? white : black, x * pWidth, y * pWidth, pWidth, pWidth);
        }

        internal void emulateCycle()
        {
            // Fetch opcode
            opcode = (ushort)(memory[PC] << 8 | memory[PC + 1]);

            // Decode opcode

            int offset = 0;

            ushort vx = (ushort)((opcode & 0x0F00) >> 8);
            ushort vy = (ushort)((opcode & 0x00F0) >> 4);
            ushort kk = (ushort)(opcode & 0x00FF);

            switch (opcode & 0xF000)
            {
                // Some opcodes //

                case 0x0000:
                    switch (opcode & 0x000F)
                    {
                        case 0x0000: // 0x00E0: Clears the screen
                            for (int i = 0; i < 2048; ++i)
                                gfx[i] = 0x0;
                            drawFlag = true;
                            break;

                        case 0x000E: // 0x00EE: Returns from subroutine
                            SP--;
                            PC = stack[SP];
                            break;

                        default:
                            unknown_opcode = opcode;
                            break;
                    }
                    break;

                case 0x1000: // Jump to Location
                    PC = (ushort)(opcode & 0x0FFF);
                    offset = -2;
                    break;

                case 0x2000: // Jump to Subroutine
                    stack[SP] = PC;
                    SP++;
                    PC = (ushort)(opcode & 0x0FFF);
                    offset = -2;
                    break;

                case 0x3000: // Skip next Instruction if Vx = kk
                    if (V[vx] == kk) offset = 2;
                    break;

                case 0x4000: // Skip next Instruction if Vx != kk
                    if (V[vx] != kk) offset = 2;
                    break;

                case 0x5000: // Skip next Instruction if Vx == Vy
                    if (V[vx] == V[vy]) offset = 2;
                    break;

                case 0x6000: // Set Vx to kk
                    V[vx] = (byte)kk;
                    break;

                case 0x7000: // Add kk to Vx
                    V[vx] += (byte)kk;
                    break;

                case 0x9000: // Skip next Instruction if Vx != Vy
                    if (V[vx] != V[vy]) offset = 2;
                    break;

                case 0xA000: // ANNN: Sets I to the address NNN
                    I = (ushort)(opcode & 0x0FFF);
                    break;

                case 0xD000:
                    {
                        ushort x = V[(opcode & 0x0F00) >> 8];
                        ushort y = V[(opcode & 0x00F0) >> 4];
                        ushort height = (ushort)(opcode & 0x000F);
                        ushort pixel;

                        V[0xF] = 0;
                        for (int yline = 0; yline < height; yline++)
                        {
                            pixel = memory[I + yline];
                            for (int xline = 0; xline < 8; xline++)
                            {
                                if ((pixel & (0x80 >> xline)) != 0)
                                {
                                    if (gfx[(x + xline + ((y + yline) * 64))] == 1)
                                        V[0xF] = 1;
                                    gfx[x + xline + ((y + yline) * 64)] ^= 1;
                                }
                            }
                        }

                        drawFlag = true;
                        break;
                    }


                // More opcodes //

                default:
                    unknown_opcode = opcode;
                    break;
            }

            // Update timers
            if (delay_timer > 0)
                --delay_timer;

            if (sound_timer > 0)
            {
                if (sound_timer == 1)
                    Console.WriteLine("BEEP!\n");
                --sound_timer;
            }
            PC += (ushort)(2 + offset);
        }
    }
}
