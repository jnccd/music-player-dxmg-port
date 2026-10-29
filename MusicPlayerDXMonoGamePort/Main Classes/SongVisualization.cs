using Persistence;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MusicPlayerSyncInterface.Database;
using MusicPlayerDXMonoGamePort.Persistence.Database;
using MusicPlayerDXMonoGamePort.Main_Classes;

namespace MusicPlayerDXMonoGamePort
{
    public class SongVisualization
    {
        //NAudio
        //public const int bufferLength = 8192;
        //public const int bufferLength = 16384;
        //public const int bufferLength = 32768;
        public const int bufferLength = 65536;
        //public const int bufferLength = 131072; 
        //public const int bufferLength = 262144;
        public static GigaFloatList EntireSongWaveBuffer;
        public static byte[] buffer = new byte[bufferLength];
        public static float[] WaveBuffer = new float[bufferLength / 4];
        public static float[] FFToutput;
        public static float[] RawFFToutput;
        public static Complex[] tempbuffer = null;
        static int TempBufferLengthLog2;
        public static bool SongBufferThreadWasAborted = false;
        public static Exception LastSongBufferThreadException = new Exception();
        public static float[] hBlurWeights, vBlurWeights;

        public static class HammingWindowValues
        {
            public static float GetHammingWindow(int i)
            {
                if (i >= 0 && i < HammingWindow.Length)
                    return HammingWindow[i];
                else
                    return 0;
            }
            public static void CreateIfNotFilled(int Length)
            {
                if (HammingWindow == null || HammingWindow.Length != Length)
                {
                    HammingWindow = new float[Length];
                    HammingWindowValues.Length = Length;

                    for (int i = 0; i < HammingWindow.Length; i++)
                        HammingWindow[i] = (float)FastFourierTransform.HammingWindow(i, Length);
                }
            }

            private static float[] HammingWindow;
            public static int Length;
        }

        // Visualization
        public static void UpdateWaveBuffer()
        {
            //buffer = new byte[bufferLength];
            //WaveBuffer = new float[bufferLength / 4];

            if (SongManager.Channel32 != null && SongManager.Channel32Reader != null && SongManager.Channel32Reader.CanRead)
            {
                SongManager.Channel32Reader.Position = SongManager.Channel32.Position;

                try
                {
                    int Read = SongManager.Channel32Reader.Read(buffer, 0, bufferLength);
                }
                catch { Debug.WriteLine("AHAHHAHAHAHA.... ich kann nicht lesen"); }

                // Converting the byte buffer in readable data
                for (int i = 0; i < bufferLength / 4; i++)
                    WaveBuffer[i] = BitConverter.ToSingle(buffer, i * 4);
            }
        }
        public static void UpdateFFTbuffer()
        {
            lock (SongManager.Channel32)
            {
                //CurrentDebugTime = Stopwatch.GetTimestamp();
                if (tempbuffer == null)
                {
                    tempbuffer = new Complex[WaveBuffer.Length];
                    TempBufferLengthLog2 = (int)Math.Log(tempbuffer.Length, 2.0);
                }
                //Debug.WriteLine("UpdateFFTbuffer 1 " + (Stopwatch.GetTimestamp() - CurrentDebugTime));

                //CurrentDebugTime = Stopwatch.GetTimestamp();
                HammingWindowValues.CreateIfNotFilled(tempbuffer.Length);
                for (int i = 0; i < tempbuffer.Length; i++)
                {
                    tempbuffer[i].X = WaveBuffer[i] * HammingWindowValues.GetHammingWindow(i);
                    tempbuffer[i].Y = 0;
                }
                //Debug.WriteLine("UpdateFFTbuffer 2 " + (Stopwatch.GetTimestamp() - CurrentDebugTime));

                //CurrentDebugTime = Stopwatch.GetTimestamp();
                FastFourierTransform.FFT(true, TempBufferLengthLog2, tempbuffer);
                //Debug.WriteLine("UpdateFFTbuffer 3 " + (Stopwatch.GetTimestamp() - CurrentDebugTime));

                //CurrentDebugTime = Stopwatch.GetTimestamp();
                FFToutput = new float[tempbuffer.Length / 2 - 1];
                RawFFToutput = new float[tempbuffer.Length / 2 - 1];
                for (int i = 0; i < FFToutput.Length; i++)
                {
                    RawFFToutput[i] = Approximate.Sqrt((tempbuffer[i].X * tempbuffer[i].X) + (tempbuffer[i].Y * tempbuffer[i].Y)) * 7;
                    FFToutput[i] = (RawFFToutput[i] * Approximate.Sqrt(i + 1));
                }
                //Debug.WriteLine("UpdateFFTbuffer 4 " + (Stopwatch.GetTimestamp() - CurrentDebugTime));
            }
        }
        public static void UpdateEntireSongBuffers()
        {
            Thread.CurrentThread.Name = "Song Buffer Loading Thread";
            Program.game.SongBufferLoaderThread = Thread.CurrentThread;
            try
            {
                lock (SongManager.Channel32ReaderThreaded)
                {
                    byte[] buffer = new byte[16384];

                    EntireSongWaveBuffer = null;

                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    SongManager.Channel32ReaderThreaded.Position = 0;
                    // Hand the song length over so the buffer is sized once up front (it used to
                    // allocate and immediately drop a 256 MB array per song, see GigaFloatList).
                    EntireSongWaveBuffer = new GigaFloatList(SongManager.Channel32ReaderThreaded.Length / 4);

                    while (SongManager.Channel32ReaderThreaded.Position < SongManager.Channel32ReaderThreaded.Length)
                    {
                        if (SongManager.AbortAbort)
                            break;

                        int read = SongManager.Channel32ReaderThreaded.Read(buffer, 0, 16384);

                        if (SongManager.AbortAbort)
                            break;

                        for (int i = 0; i < read / 4; i++)
                        {
                            EntireSongWaveBuffer.Add(BitConverter.ToSingle(buffer, i * 4));

                            if (SongManager.AbortAbort)
                                break;
                        }

                        bool noVolumeData = SongManager.DoesCurrentSongaveNoVolumeData();

                        while (SongManager.Channel32 != null &&
                            SongManager.Channel32.Position < SongManager.Channel32ReaderThreaded.Position - Config.Data.WavePreload * SongManager.Channel32ReaderThreaded.Length / 100f &&
                            !noVolumeData)
                        {
                            if (SongManager.AbortAbort)
                                break;
                            Thread.Sleep(20);
                        }
                    }

                    SongBufferThreadWasAborted = SongManager.AbortAbort;

                    if (SongManager.Channel32ReaderThreaded.Position >= SongManager.Channel32ReaderThreaded.Length && SongManager.DoesCurrentSongaveNoVolumeData())
                        UpdateCurrentSongVolume();

                    Debug.WriteLine("SongBuffer Length: " + EntireSongWaveBuffer.Count + " Memory: " + GC.GetTotalMemory(true));
                    Debug.WriteLine("Memory per SongBuffer Length: " + (GC.GetTotalMemory(true) / (double)EntireSongWaveBuffer.Count));
                    SongManager.AbortAbort = false;
                }
            }
            catch (Exception e)
            {
                LastSongBufferThreadException = e;
                bool knownSongName = false;
                try { string s = SongManager.currentlyPlayingSongName; knownSongName = true; } catch { }

                Debug.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                if (knownSongName)
                    Debug.WriteLine("Couldn't load " + SongManager.currentlyPlayingSongPath);
                Debug.WriteLine("SongBuffer Length: " + EntireSongWaveBuffer.Count + " Memory: " + GC.GetTotalMemory(true));
                Debug.WriteLine("Memory per SongBuffer Length: " + (GC.GetTotalMemory(true) / (double)EntireSongWaveBuffer.Count));
                Debug.WriteLine("Exception: " + e);
                Debug.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");

                if (EntireSongWaveBuffer.Count > 70000000 && SongManager.DoesCurrentSongaveNoVolumeData())
                    UpdateCurrentSongVolume();
            }
        }
        public static void UpdateWaveBufferWithEntireSongWB()
        {
            bool worked = false;
            try
            {
                lock (EntireSongWaveBuffer)
                {
                    if (SongManager.Channel32 != null && SongManager.Channel32.CanRead && EntireSongWaveBuffer.Count > SongManager.Channel32.Position / 4 && SongManager.Channel32.Position > bufferLength)
                    {
                        // Same window as before, but copied into the existing buffer instead of
                        // building a temporary List<float> + array every frame (~128 KB of garbage
                        // per video frame, for the whole run).
                        if (WaveBuffer == null || WaveBuffer.Length != bufferLength / 4)
                            WaveBuffer = new float[bufferLength / 4];
                        worked = EntireSongWaveBuffer.CopyRange((int)((SongManager.Channel32.Position - bufferLength / 2) / 4), WaveBuffer, 0, bufferLength / 4);
                    }
                    else
                    {
                        if (WaveBuffer == null || WaveBuffer.Length != bufferLength / 4)
                            WaveBuffer = new float[bufferLength / 4];
                        for (int i = 0; i < bufferLength / 4; i++)
                            WaveBuffer[i] = 0;
                    }
                }
            }
            catch { }

            if (!worked)
                UpdateWaveBuffer();
        }
        public static float GetAverageHeight(float[] array, int from, int to)
        {
            float temp = 0;

            if (from < 0)
                from = 0;

            if (to > array.Length)
                to = array.Length;

            for (int i = from; i < to; i++)
                temp += array[i];

            return temp / array.Length;
        }
        public static float GetMaxHeight(float[] array, int from, int to)
        {
            if (from < 0)
                from = 0;

            if (to > array.Length)
                to = array.Length;

            if (from >= to)
                to = from + 1;

            float max = 0;
            for (int i = from; i < to; i++)
                if (array[i] > max)
                    max = array[i];

            return max;
        }
        public static void UpdateCurrentSongVolume()
        {
            EntireSongWaveBuffer.updateCount();

            float n = 0;

            for (int i = 0; i < EntireSongWaveBuffer.Count; i++)
                n += EntireSongWaveBuffer.Get(i) * EntireSongWaveBuffer.Get(i);
            n /= EntireSongWaveBuffer.Count;

            float sn = Approximate.Sqrt(n);

            float mult = Values.BaseVolume / sn;
            if (mult < 999)
                Program.game.ShowSecondRowMessage("Applied Volume multiplier of: " + Math.Round(mult, 2), 1);
            else
                Program.game.ShowSecondRowMessage("Applied Volume multiplier of: very large", 1);

            using var songDbContext = new SongDbContext();
            var upvotedSong = songDbContext.UpvotedSongs.FirstOrDefault(x => x.Name == SongManager.currentlyPlayingSongName);
            Values.VolumeMultiplier = mult;
            upvotedSong.Volume = sn;
            songDbContext.SaveChanges();
            SyncManager.UpdateVolume(upvotedSong, sn);

            Debug.WriteLine("---------------------------------------------------------------------------------------------------------");
            Debug.WriteLine("RMS Volume for " + SongManager.currentlyPlayingSongName + " = " + sn);
            Debug.WriteLine("Volume multiplier for " + SongManager.currentlyPlayingSongName + " = " + mult);
            Debug.WriteLine("---------------------------------------------------------------------------------------------------------");
        }
        /// <summary>
        /// Reads the desktop wallpaper again after the wallpaper, the theme, the DPI or the system colours
        /// changed (SystemEvents.UserPreferenceChanged). Startup does not come through here any more -
        /// Assets.Load loads the wallpaper once itself.
        ///
        /// The image is decoded on a background thread, then swapped in on the UI thread, where the
        /// previous texture can be disposed: the frame loop draws Assets.bg, so a background thread must
        /// never dispose a texture the current frame may still be using. The old code replaced the
        /// texture without disposing it, which leaked one whole wallpaper per event (31.9 MiB for a 4K
        /// image) - permanently, because MonoGame textures have no finalizer.
        /// </summary>
        public static void RefreshBGtex(GraphicsDevice GD)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    RegistryKey UserWallpaper = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", false);
                    if (Convert.ToInt32(UserWallpaper.GetValue("WallpaperStyle")) != 2)
                    {
                        MessageBox.Show("The background won't work if the Desktop WallpaperStyle isn't set to stretch! \nDer Hintergrund wird nicht funktionieren, wenn der Dektop WallpaperStyle nicht auf Dehnen gesetzt wurde!");
                    }

                    Texture2D newBackground = LoadWallpaperTexture(GD, UserWallpaper.GetValue("WallPaper").ToString());

                    Program.game.gameWindowForm.InvokeIfRequired(() =>
                    {
                        Texture2D previousBackground = Assets.bg;
                        Assets.bg = newBackground;
                        previousBackground?.Dispose();
                        Program.game.ForceBackgroundRedraw();
                    });
                }
                catch (Exception e)
                {
                    Console.WriteLine("Couldn't reload the wallpaper: " + e.Message);
                }
            });
        }

        /// <summary>
        /// Loads the desktop wallpaper into a GPU texture, clamped to the largest screen size (see
        /// <see cref="LoadClampedWallpaperTexture"/>). Falls back to the plain importer when System.Drawing
        /// cannot decode the file (some WebP/HEIF wallpapers), so an exotic wallpaper still loads - it just
        /// keeps its full resolution. Throws when the wallpaper is unreadable (callers decide whether that
        /// is fatal).
        /// </summary>
        public static Texture2D LoadWallpaperTexture(GraphicsDevice GD, string wallpaperPath)
        {
            try
            {
                return LoadClampedWallpaperTexture(GD, wallpaperPath);
            }
            catch (Exception e)
            {
                Console.WriteLine("Couldn't clamp the wallpaper (" + e.Message + "), loading it unscaled.");
                using FileStream stream = new FileStream(wallpaperPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return Texture2D.FromStream(GD, stream);
            }
        }

        /// <summary>
        /// Loads the desktop wallpaper, clamped to the largest screen size. The wallpaper is only ever drawn
        /// scaled to a screen (or, blurred, into a window sized render target), so the pixels a 4K wallpaper
        /// has above the biggest screen are pure VRAM cost: 3840x2176 is 31.9 MiB as a Color texture, the
        /// same image clamped to a 2560x1440 screen is 14.8 MiB. Never upscales - a smaller wallpaper is
        /// uploaded as it is.
        /// </summary>
        static Texture2D LoadClampedWallpaperTexture(GraphicsDevice GD, string wallpaperPath)
        {
            int maxWidth = 0;
            int maxHeight = 0;
            foreach (Screen screen in Screen.AllScreens)
            {
                if (screen.Bounds.Width > maxWidth)
                    maxWidth = screen.Bounds.Width;
                if (screen.Bounds.Height > maxHeight)
                    maxHeight = screen.Bounds.Height;
            }

            // Read-only and shared: reading the wallpaper never needs write access, and files another
            // program has open (or on a read-only share) must still load.
            using FileStream stream = new FileStream(wallpaperPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using System.Drawing.Image source = System.Drawing.Image.FromStream(stream);

            int width = source.Width;
            int height = source.Height;
            if (maxWidth > 0 && maxHeight > 0 && (width > maxWidth || height > maxHeight))
            {
                // Preserve the aspect ratio, shrink only as far as the largest screen requires.
                float scale = Math.Min(maxWidth / (float)width, maxHeight / (float)height);
                width = Math.Max(1, (int)Math.Round(width * scale));
                height = Math.Max(1, (int)Math.Round(height * scale));
            }

            Color[] pixels;
            using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap))
                {
                    graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    graphics.DrawImage(source, new System.Drawing.Rectangle(0, 0, width, height));
                }

                pixels = new Color[width * height];
                System.Drawing.Imaging.BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, width, height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                try
                {
                    byte[] bytes = new byte[Math.Abs(data.Stride) * height];
                    System.Runtime.InteropServices.Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
                    for (int y = 0; y < height; y++)
                    {
                        int row = data.Stride >= 0 ? y * data.Stride : (height - 1 - y) * -data.Stride;
                        for (int x = 0; x < width; x++)
                        {
                            int i = row + x * 4; // the bitmap is BGRA
                            pixels[y * width + x] = Color.FromNonPremultiplied(bytes[i + 2], bytes[i + 1], bytes[i], bytes[i + 3]);
                        }
                    }
                }
                finally
                {
                    bitmap.UnlockBits(data);
                }
            }

            Texture2D texture = new Texture2D(GD, width, height);
            texture.SetData(pixels);
            return texture;
        }


        // Draw Methods
        public static void DrawLine(Vector2 End1, Vector2 End2, int Thickness, Color Col, SpriteBatch SB)
        {
            Vector2 Line = End1 - End2;
            SB.Draw(Assets.White, End1, null, Col, -(float)Math.Atan2(Line.X, Line.Y) - (float)Math.PI / 2, new Vector2(0, 0.5f), new Vector2(Line.Length(), Thickness), SpriteEffects.None, 0f);
        }
        public static void DrawCircle(Vector2 Pos, float Radius, Color Col, SpriteBatch SB)
        {
            if (Radius < 0)
                Radius *= -1;

            for (int i = -(int)Radius; i < (int)Radius; i++)
            {
                int HalfHeight = (int)Approximate.Sqrt(Radius * Radius - i * i);
                SB.Draw(Assets.White, new Rectangle((int)Pos.X + i, (int)Pos.Y - HalfHeight, 1, HalfHeight * 2), Col);
            }
        }
        public static void DrawCircle(Vector2 Pos, float Radius, float HeightMultiplikator, Color Col, SpriteBatch SB)
        {
            if (Radius < 0)
                Radius *= -1;

            for (int i = -(int)Radius; i < (int)Radius; i++)
            {
                int HalfHeight = (int)Math.Sqrt(Radius * Radius - i * i);
                SB.Draw(Assets.White, new Rectangle((int)Pos.X + i, (int)Pos.Y, 1, (int)(HalfHeight * HeightMultiplikator)), Col);
            }

            for (int i = -(int)Radius; i < (int)Radius; i++)
            {
                int HalfHeight = (int)Math.Sqrt(Radius * Radius - i * i);
                SB.Draw(Assets.White, new Rectangle((int)Pos.X + i + 1, (int)Pos.Y, -1, (int)(-HalfHeight * HeightMultiplikator)), Col);
            }
        }
        public static void DrawRoundedRectangle(Rectangle Rect, float PercentageOfRounding, Color Col, SpriteBatch SB)
        {
            float Rounding = PercentageOfRounding / 100;
            Rectangle RHorz = new Rectangle(Rect.X, (int)(Rect.Y + Rect.Height * (Rounding / 2)), Rect.Width, (int)(Rect.Height * (1 - Rounding)));
            Rectangle RVert = new Rectangle((int)(Rect.X + Rect.Width * (Rounding / 2)), Rect.Y, (int)(Rect.Width * (1 - Rounding)), (int)(Rect.Height * 0.999f));

            int RadiusHorz = (int)(Rect.Width * (Rounding / 2));
            int RadiusVert = (int)(Rect.Height * (Rounding / 2));

            if (RadiusHorz != 0)
            {
                // Top-Left
                DrawCircle(new Vector2(Rect.X + RadiusHorz, Rect.Y + RadiusVert), RadiusHorz, RadiusVert / (float)RadiusHorz, Col, SB);

                // Top-Right
                DrawCircle(new Vector2(Rect.X + Rect.Width - RadiusHorz - 1, Rect.Y + RadiusVert), RadiusHorz, RadiusVert / (float)RadiusHorz, Col, SB);

                // Bottom-Left
                DrawCircle(new Vector2(Rect.X + RadiusHorz, Rect.Y + RadiusVert + (int)(Rect.Height * (1 - Rounding))), RadiusHorz, RadiusVert / (float)RadiusHorz, Col, SB);

                // Bottom-Right
                DrawCircle(new Vector2(Rect.X + Rect.Width - RadiusHorz - 1, Rect.Y + RadiusVert + (int)(Rect.Height * (1 - Rounding))), RadiusHorz, RadiusVert / (float)RadiusHorz, Col, SB);
            }

            SB.Draw(Assets.White, RHorz, Col);
            SB.Draw(Assets.White, RVert, Col);
        }
    }
}
