using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        internal ushort unknown_opcode = 0;
        internal bool drawFlag = false;

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
            /*
            Debug.WriteLine("PC: {0:X2}", PC);
            Debug.WriteLine("I:  {0:X2}", I);
            Debug.WriteLine("SP: {0:X2}", SP);

            Debug.WriteLine("OP Code: {0:X} Masked: {1:X}", opcode, (opcode & 0xF000));

            Debug.WriteLine("V:");
            for (int i = 0; i < 16; i++)
            {
                Debug.WriteLine(" " + (i + 1) + ": {0:X2}", V[i]);
            }
            */
            Console.WriteLine("Key");
            for (int i = 0; i < 16; i++)
            {
                Console.WriteLine(" " + (i + 1) + ": {0:X2}", key[i]);
            }

            if (unknown_opcode != 0)
            {
                Debug.WriteLine("Unknown OPCODE: {0:X}", unknown_opcode);
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
                V[i] = key[i] = 0;
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
                memory[i] = (byte)chip8_fontset[i];
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

        internal void emulateCycle()
        {
            // Fetch opcode
            opcode = (ushort)(memory[PC] << 8 | memory[PC + 1]);

            //Debug.WriteLine("OP Code: {0:X} Masked: {1:X}", opcode, (opcode & 0xF000));

            // Decode opcode

            int offset = 0;

            ushort vx = (ushort)((opcode & 0x0F00) >> 8);
            ushort vy = (ushort)((opcode & 0x00F0) >> 4);
            ushort vz = (ushort)((opcode & 0x000F));
            ushort kk = (ushort)(opcode & 0x00FF);

            switch (opcode & 0xF000)
            {
                // Some opcodes //

                case 0x0000:
                    switch (vz)
                    {
                        case 0x0000: // Clears the screen
                            for (int i = 0; i < 2048; ++i)
                                gfx[i] = 0x0;
                            drawFlag = true;
                            break;

                        case 0x000E: // Returns from subroutine
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

                case 0x8000:
                    switch (vz)
                    {
                        case 0x0000:
                            V[vx] = V[vy];
                            break;

                        case 0x0001:
                            V[vx] |= V[vy];
                            break;

                        case 0x0002:
                            V[vx] &= V[vy];
                            break;

                        case 0x0003:
                            V[vx] ^= V[vy];
                            break;

                        case 0x0004:
                            if (V[vy] > (0xFF - V[vx]))
                                V[0xF] = 1;
                            else
                                V[0xF] = 0;
                            V[vx] += V[vy];
                            break;

                        case 0x0005:
                            if (V[vy] > V[vx])
                                V[0xF] = 1;
                            else
                                V[0xF] = 0;
                            V[vx] -= V[vy];
                            break;

                        case 0x0006:
                            V[0xF] = (byte)(V[vx] & 0x1);
                            V[vx] >>= 1;
                            break;

                        case 0x0007: // Sets VX to VY minus VX. VF is set to 0 when there's a borrow, and 1 when there isn't
                            if (V[vx] > V[vy])  // VY-VX
                                V[0xF] = 0; // there is a borrow
                            else
                                V[0xF] = 1;
                            V[vx] = (byte)(V[vy] - V[vx]);
                            break;

                        case 0x000E:
                            V[0xF] = (byte)(V[vx] >> 7);
                            V[vx] <<= 1;
                            break;

                        default:
                            unknown_opcode = opcode;
                            break;
                    }
                    break;

                case 0x9000: // Skip next Instruction if Vx != Vy
                    if (V[vx] != V[vy]) offset = 2;
                    break;

                case 0xA000: // ANNN: Sets I to the address NNN
                    I = (ushort)(opcode & 0x0FFF);
                    break;

                case 0xB000: // BNNN: Jumps to the address NNN plus V0
                    PC = (ushort)((opcode & 0x0FFF) + V[0]);
                    offset = -2;
                    break;

                case 0xC000:
                    V[vx] = (byte)((new Random().Next()% 0xFF) & kk);
                    break;

                case 0xD000:
                    {
                        ushort x = V[vx];
                        ushort y = V[vy];
                        ushort height = vz;
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
                                    {
                                        V[0xF] = 1;
                                    }
                                    gfx[x + xline + ((y + yline) * 64)] ^= 1;
                                }
                            }
                        }


                        drawFlag = true;
                        break;
                    }

                case 0xE000:
                    switch (kk)
                    {
                        case 0x009E:
                            if (key[vx] == 1)
                            {
                                offset = 2;
                            }
                            break;

                        case 0x00A1:
                            if (key[vx] == 0)
                            {
                                offset = 2;
                            }
                            break;

                        default:
                            unknown_opcode = opcode;
                            break;
                    }
                    break;

                case 0xF000:
                    switch (opcode & 0x00ff)
                    {
                        case 0x0007:
                            V[vx] = delay_timer;
                            break;

                        case 0x000A:
                            bool keyPress = false;

                            for (int i = 0; i < 16; i++)
                            {
                                if (key[i] != 0)
                                {
                                    V[vx] = (byte)i;
                                    keyPress = true;
                                }
                            }

                            // If we didn't received a keypress, skip this cycle and try again.
                            if (!keyPress)
                            {
                                offset = -2;
                                return;
                            }
                            break;

                        case 0x0015:
                            delay_timer = (byte)vx;
                            break;

                        case 0x0018:
                            sound_timer = (byte)vx;
                            break;

                        case 0x001E:
                            if (I + V[vx] > 0xFFF)  
                                V[0xF] = 1;
                            else
                                V[0xF] = 0;
                            I += V[vx];
                            break;

                        case 0x0029:
                            I = (ushort)(V[vx] * 0x5);
                            break;

                        case 0x0033: // Stores the Binary-coded decimal representation of VX at the addresses I, I plus 1, and I plus 2
                            memory[I] = (byte)(V[vx] / 100);
                            memory[I + 1] = (byte)((V[vx] / 10) % 10);
                            memory[I + 2] = (byte)(V[vx] % 10);
                            break;

                        case 0x0055:
                            for (int i = 0; i <= vx; i++)
                            {
                                memory[I + i] = V[i];
                            }
                            I += (ushort)(vx + 1);
                            break;

                        case 0x0065:
                            for (int i = 0; i <= vx; i++)
                            {
                                V[i] = memory[I + i];
                            }
                            I += (ushort)(vx + 1);
                            break;

                        default:
                            unknown_opcode = opcode;
                            break;
                    }
                    break;

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
