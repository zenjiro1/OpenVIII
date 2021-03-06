﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;

namespace FF8
{
    internal static class Module_overture_debug
    {
        private static OvertureInternalModule internalModule = OvertureInternalModule._4Squaresoft;
        private static ArchiveWorker aw;
        private const string names = "name";
        private const string loops = "loop";

        private static Texture2D splashTex = null;
        private static Texture2D white=null;

        enum OvertureInternalModule
        {
            _0InitSound,
            _1WaitBeforeFirst,
            _2PlaySequence,
            _3SequenceFinishedPlayMainMenu,
            _4Squaresoft
        }

        private static double internalTimer;
        private static bool bNames = true; //by default we are starting with names
        private static int splashIndex, splashName = 1, splashLoop = 1;

        private static bool bFadingIn = true; //by default first should fade in, wait, then fire fading out and wait for finish, then loop
        private static bool bWaitingSplash, bFadingOut;
        private static float fSplashWait, Fade;

        internal static void Update()
        {
            if (Input.Button(Buttons.Okay) || Input.Button(Buttons.Cancel) || Input.Button(Keys.Space))
            {
                Input.ResetInputLimit();
                init_debugger_Audio.StopAudio();
                Memory.module = Memory.MODULE_MAINMENU_DEBUG;

                if (splashTex != null && !splashTex.IsDisposed)
                    splashTex.Dispose();
                if (white != null && !white.IsDisposed)
                    white.Dispose();
            }
            switch (internalModule)
            {
                case OvertureInternalModule._0InitSound:
                    InitSound();
                    break;
                case OvertureInternalModule._1WaitBeforeFirst:
                    Memory.SuppressDraw = true;
                    WaitForFirst();
                    break;
                case OvertureInternalModule._2PlaySequence:
                    SplashUpdate(ref splashIndex);
                    break;
            }
        }


        public static void ResetModule()
        {
            internalModule = 0;
            internalTimer = 0.0f;
            bFadingIn = true;
            bWaitingSplash = false;
            fSplashWait = 0.0f;
            bFadingOut = false;
            Fade = 0;
            Memory.spriteBatch.GraphicsDevice.Clear(Color.Black);
            Memory.module = Memory.MODULE_OVERTURE_DEBUG;
            internalModule = OvertureInternalModule._4Squaresoft;
            Module_movie_test.ReturnState = Memory.MODULE_OVERTURE_DEBUG;
            aw = null; // was getting exception when running the overture again as the aw target changed.
        }
        private static void WaitForFirst()
        {
            if (internalTimer > 6.0f)
            {
                internalModule++;
                Console.WriteLine("MODULE_OVERTURE: DEBUG MODULE 2");
            }
            internalTimer += Memory.gameTime.ElapsedGameTime.Milliseconds / 1000.0d;

        }
        private static void InitSound()
        {
            Memory.MusicIndex = 79;//79; //Overture
            init_debugger_Audio.PlayMusic();
            Memory.MusicIndex = ushort.MaxValue; // reset pos after playing overture; will loop back to start if push next


            if (white != null && !white.IsDisposed)
                white.Dispose();
            white = new Texture2D(Memory.graphics.GraphicsDevice, 4, 4, false, SurfaceFormat.Color);
            byte[] whiteBuffer = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                whiteBuffer[i] = 255;
            }

            internalModule++;
        }
        internal static void Draw()
        {
            switch (internalModule)
            {
                case OvertureInternalModule._0InitSound:
                case OvertureInternalModule._1WaitBeforeFirst:
                    Memory.graphics.GraphicsDevice.Clear(Color.Black);
                    break;
                case OvertureInternalModule._2PlaySequence:
                    Memory.graphics.GraphicsDevice.Clear(Color.Black);
                    DrawSplash();
                    break; //actually this is our entry point for draw;
                case OvertureInternalModule._3SequenceFinishedPlayMainMenu:
                    DrawLogo(); //after this ends, jump into main menu module
                    break;
                case OvertureInternalModule._4Squaresoft:
                    internalModule = OvertureInternalModule._0InitSound;
                    Module_movie_test.Index = 103;//103;
                    Module_movie_test.ReturnState = Memory.MODULE_OVERTURE_DEBUG;
                    Memory.module = Memory.MODULE_MOVIETEST;
                    break;
            }
        }

        private static void DrawLogo()
        {
            //fade to white
            if (!bWaitingSplash)
            {
                Memory.graphics.GraphicsDevice.Clear(Color.White);
            }
            else
            {
                Memory.graphics.GraphicsDevice.Clear(Color.Black);
            }

            Memory.SpriteBatchStartAlpha();
            Memory.spriteBatch.Draw(splashTex, new Microsoft.Xna.Framework.Rectangle(0, 0, Memory.graphics.GraphicsDevice.Viewport.Width, Memory.graphics.GraphicsDevice.Viewport.Height),
                new Microsoft.Xna.Framework.Rectangle(0, 0, splashTex.Width, splashTex.Height)
                , Color.White * Fade);
            if (bFadingIn)
            {
                Fade += Memory.gameTime.ElapsedGameTime.Milliseconds / 5000.0f;
            }

            if (bFadingOut)
            {
                Fade -= Memory.gameTime.ElapsedGameTime.Milliseconds / 2000.0f;
            }

            if (Fade < 0.0f)
            {
                bFadingIn = true;
                ReadSplash(true);
                bFadingOut = false;
            }
            if (bFadingIn && Fade > 1.0f && !bWaitingSplash)
            {
                internalTimer += Memory.gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
            }

            if (internalTimer > 5.0f)
            {
                bWaitingSplash = true;
                bFadingOut = true;
            }
            Memory.SpriteBatchEnd();
            if (bWaitingSplash && Fade < 0.0f)
            {
                init_debugger_Audio.StopAudio();
                Memory.module = Memory.MODULE_MAINMENU_DEBUG;
                if (splashTex != null && !splashTex.IsDisposed)
                    splashTex.Dispose();
                if (white != null && !white.IsDisposed)
                    white.Dispose();

            }

        }

        private static void DrawSplash()
        {
            if (splashTex == null)
            {
                return;
            }

            Memory.SpriteBatchStartAlpha();
            Memory.spriteBatch.Draw(splashTex, new Microsoft.Xna.Framework.Rectangle(0, 0, Memory.graphics.GraphicsDevice.Viewport.Width, Memory.graphics.GraphicsDevice.Viewport.Height),
                new Microsoft.Xna.Framework.Rectangle(0, 0, splashTex.Width, splashTex.Height)
                , Color.White * Fade);
            Memory.SpriteBatchEnd();
        }

        internal static void SplashUpdate(ref int _splashIndex)
        {
            if (aw == null)
            {
                aw = new ArchiveWorker(Memory.Archives.A_MAIN);
            }

            if (splashTex == null)
            {
                ReadSplash();
            }

            if (bFadingIn)
            {
                Fade += Memory.gameTime.ElapsedGameTime.Milliseconds / 1000.0f * 2f;
                if (Fade > 1.0f)
                {
                    Fade = 1.0f;
                    bFadingIn = false;
                    bWaitingSplash = true;
                }
            }
            if (bFadingOut)
            {
                if (splashLoop + 1 >= 0x0F && splashName >= 0x0F)
                {
                    bFadingIn = false;
                    bFadingOut = true;
                    bWaitingSplash = false;
                    internalTimer = 0.0f;
                    Fade = 1.0f;
                    internalModule++;
                    return;
                }
                Fade -= Memory.gameTime.ElapsedGameTime.Milliseconds / 1000.0f * 2f;
                if (Fade < 0.0f)
                {
                    bFadingIn = true;
                    bFadingOut = false;
                    Fade = 0.0f;
                    _splashIndex++;
                    if (bNames)
                    {
                        splashName++;
                    }
                    else
                    {
                        splashLoop++;
                    }

                    if (_splashIndex > 1)
                    {
                        bNames = !bNames;
                        _splashIndex = 0;
                    }

                    ReadSplash();
                }
            }
            if (bWaitingSplash)
            {
                if (bNames)
                {
                    if (fSplashWait > 4.8f)
                    {
                        bWaitingSplash = false;
                        bFadingOut = true;
                        fSplashWait = 0.0f;
                    }
                }
                else
                {
                    if (fSplashWait > 6.5f)
                    {
                        bWaitingSplash = false;
                        bFadingOut = true;
                        fSplashWait = 0.0f;
                    }
                }
                Memory.SuppressDraw = true;
                fSplashWait += Memory.gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
            }
            //loop 01-14 + name01-14;
        }


        //Splash is 640x400 16BPP typical TIM with palette of ggg bbbbb a rrrrr gg
        internal static void ReadSplash(bool bLogo = false)
        {
            if (!bLogo)
            {
                if (splashName > 0x0f)
                {
                    return;
                }

                string[] lof = aw.GetListOfFiles();
                string fileName = bNames
                    ? lof.First(x => x.ToLower().Contains($"{names}{splashName.ToString("D2")}"))
                    : lof.First(x => x.ToLower().Contains($"{loops}{splashLoop.ToString("D2")}"));
                byte[] buffer = ArchiveWorker.GetBinaryFile(Memory.Archives.A_MAIN, fileName);
                uint uncompSize = BitConverter.ToUInt32(buffer, 0);
                buffer = LZSS.DecompressAll(buffer, (uint)buffer.Length);

                if (splashTex != null && !splashTex.IsDisposed)
                    splashTex.Dispose();
                splashTex = new Texture2D(Memory.graphics.GraphicsDevice, 640, 400, false, SurfaceFormat.Color);
                byte[] rgbBuffer = new byte[splashTex.Width * splashTex.Height * 4];
                int innerBufferIndex = 0;
                for (int i = 0; i < rgbBuffer.Length; i += 4)
                {
                    if (innerBufferIndex + 1 >= buffer.Length)
                    {
                        break;
                    }

                    ushort pixel = (ushort)((buffer[innerBufferIndex + 1] << 8) | buffer[innerBufferIndex]);
                    byte red = (byte)((pixel) & 0x1F);
                    byte green = (byte)((pixel >> 5) & 0x1F);
                    byte blue = (byte)((pixel >> 10) & 0x1F);
                    red = (byte)MathHelper.Clamp((red * 8), 0, 255);
                    green = (byte)MathHelper.Clamp((green * 8), 0, 255);
                    blue = (byte)MathHelper.Clamp((blue * 8), 0, 255);
                    rgbBuffer[i] = red;
                    rgbBuffer[i + 1] = green;
                    rgbBuffer[i + 2] = blue;
                    rgbBuffer[i + 3] = 255;//(byte)(((pixel >> 7) & 0x1) == 1 ? 255 : 0);
                    innerBufferIndex += 2;
                }
                splashTex.SetData(rgbBuffer);
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            else
            {
                string[] lof = aw.GetListOfFiles();
                string fileName = lof.First(x => x.ToLower().Contains($"ff8.lzs"));

                byte[] buffer = ArchiveWorker.GetBinaryFile(Memory.Archives.A_MAIN, fileName);
                uint uncompSize = BitConverter.ToUInt32(buffer, 0);
                buffer = LZSS.DecompressAll(buffer, (uint)buffer.Length);
                if (splashTex != null && !splashTex.IsDisposed)
                    splashTex.Dispose();
                splashTex = new Texture2D(Memory.graphics.GraphicsDevice, 640, 400, false, SurfaceFormat.Color);
                byte[] rgbBuffer = new byte[splashTex.Width * splashTex.Height * 4];
                int innerBufferIndex = 0;
                for (int i = 0; i < rgbBuffer.Length; i += 4)
                {
                    if (innerBufferIndex + 1 >= buffer.Length)
                    {
                        break;
                    }

                    ushort pixel = (ushort)((buffer[innerBufferIndex + 1] << 8) | buffer[innerBufferIndex]);
                    byte red = (byte)((pixel) & 0x1F);
                    byte green = (byte)((pixel >> 5) & 0x1F);
                    byte blue = (byte)((pixel >> 10) & 0x1F);
                    red = (byte)MathHelper.Clamp((red * 8), 0, 255);
                    green = (byte)MathHelper.Clamp((green * 8), 0, 255);
                    blue = (byte)MathHelper.Clamp((blue * 8), 0, 255);
                    rgbBuffer[i] = red;
                    rgbBuffer[i + 1] = green;
                    rgbBuffer[i + 2] = blue;
                    rgbBuffer[i + 3] = 255;//(byte)(((pixel >> 7) & 0x1) == 1 ? 255 : 0);
                    innerBufferIndex += 2;
                }
                splashTex.SetData(rgbBuffer);
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }
}