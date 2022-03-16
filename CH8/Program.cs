using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SDL2;

namespace CH8
{
    internal class Program
    {
        static int pwidth = 10;
        static IntPtr window;
        static IntPtr renderer;

        internal static Dictionary<SDL.SDL_Keycode, byte> keymap = new Dictionary<SDL.SDL_Keycode, byte>
        {
            { SDL.SDL_Keycode.SDLK_1, 1},
            { SDL.SDL_Keycode.SDLK_2, 2},
            { SDL.SDL_Keycode.SDLK_3, 3},
            { SDL.SDL_Keycode.SDLK_4, 0xC},

            { SDL.SDL_Keycode.SDLK_q,  4},
            { SDL.SDL_Keycode.SDLK_w,  5},
            { SDL.SDL_Keycode.SDLK_e,  6},
            { SDL.SDL_Keycode.SDLK_r,  0xD},

            { SDL.SDL_Keycode.SDLK_a,  7},
            { SDL.SDL_Keycode.SDLK_s,  8},
            { SDL.SDL_Keycode.SDLK_d,  9},
            { SDL.SDL_Keycode.SDLK_f,  0xE},

            { SDL.SDL_Keycode.SDLK_y,  0xA},
            { SDL.SDL_Keycode.SDLK_x,  0},
            { SDL.SDL_Keycode.SDLK_c,  0xB},
            { SDL.SDL_Keycode.SDLK_v,  0xF}
        };


        static void Main(string[] args)
        {

            SDL.SDL_Init(SDL.SDL_INIT_VIDEO);
            window = SDL.SDL_CreateWindow(
                "Chip 8",
                SDL.SDL_WINDOWPOS_CENTERED,
                SDL.SDL_WINDOWPOS_CENTERED,
                640,
                320,
                SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);

            renderer = SDL.SDL_CreateRenderer(
                window,
                -1,
                SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);

            Color_White();
            SDL.SDL_RenderClear(renderer);

            SDL.SDL_Rect[,] Display = new SDL.SDL_Rect[64, 32];

            Color_Black();

            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 32; y++)
                {
                    Display[x, y].x = x * pwidth;
                    Display[x, y].y = y * pwidth;
                    Display[x, y].h = pwidth;
                    Display[x, y].w = pwidth;

                    SDL.SDL_RenderFillRect(renderer, ref Display[x, y]);
                }
            }
            SDL.SDL_RenderPresent(renderer);

            SDL.SDL_Event e;
            bool quit = false;

            Chip8 chip8 = new Chip8();

            chip8.init();
            //chip8.loadRom(Environment.CurrentDirectory + "\\chip8-test-rom.c8");
            //chip8.loadRom(Environment.CurrentDirectory + "\\test_opcode.c8");
            //chip8.loadRom(Environment.CurrentDirectory + "\\tetris.c8");
            //chip8.loadRom(Environment.CurrentDirectory + "\\pong2.c8");
            //chip8.loadRom(Environment.CurrentDirectory + "\\invaders.c8");

            while (!quit)
            {
                chip8.emulateCycle();
                //chip8.debug();
                if(chip8.unknown_opcode != 0)
                {
                    Debug.WriteLine("Unknown OPCODE: {0:X}", chip8.unknown_opcode);
                    quit = true;
                }
                if (chip8.drawFlag)
                {
                    for (int x = 0; x < 64; x++)
                    {
                        for (int y = 0; y < 32; y++)
                        {
                            if (chip8.gfx[(y * 64) + x] == 1)
                                Color_White();
                            else
                                Color_Black();

                            SDL.SDL_RenderFillRect(renderer, ref Display[x, y]);
                        }
                    }
                    SDL.SDL_RenderPresent(renderer);
                    chip8.drawFlag = false;
                }

                while (SDL.SDL_PollEvent(out e) != 0)
                {
                    switch (e.type)
                    {
                        case SDL.SDL_EventType.SDL_QUIT:
                            quit = true;
                            break;

                        case SDL.SDL_EventType.SDL_KEYDOWN:
                            if (keymap.ContainsKey(e.key.keysym.sym))
                            {
                                chip8.key[keymap[e.key.keysym.sym]] = 1;
                            }
                            break;
                        case SDL.SDL_EventType.SDL_KEYUP:
                            if (keymap.ContainsKey(e.key.keysym.sym))
                            {
                                chip8.key[keymap[e.key.keysym.sym]] = 0;
                            }
                            break;
                    }
                }

                Thread.Sleep(1);
            }
            SDL.SDL_DestroyWindow(window);
            SDL.SDL_Quit();
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static void Color_White()
        {
            SDL.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);
        }

        public static void Color_Black()
        {

            SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
        }
    }
}
