﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using System.IO;
using System.Text;
using Microsoft.Win32;
using System.Reflection;
using MethodInvoker = System.Windows.Forms.MethodInvoker;
using Persistence;
using MusicPlayerDXMonoGamePort.HelperClasses;

namespace MusicPlayerDXMonoGamePort
{
    public static class Values
    {
        public static Random RDM = new();

        public static Point WindowSize = new((int)(500 * UiScaling.scaleMult), (int)(300 * UiScaling.scaleMult)); // 500, 300 default; 1360, 768 - 40 fullscreen left
        public static Rectangle WindowRect
        {
            get
            {
                return new Rectangle(0, 0, WindowSize.X, WindowSize.Y);
            }
        }

        // Console Constants
        const int MF_BYCOMMAND = 0x00000000;
        const int SC_CLOSE = 0xF060;
        const int SC_MINIMIZE = 0xF020;
        const int SC_MAXIMIZE = 0xF030;
        const int SC_SIZE = 0xF000;

        // Volumes
        public static float OutputVolume = 0;
        public static float LastOutputVolume = 0;
        public static float OutputVolumeIncrease = 0;
        public static float TargetVolume
        {
            get
            {
                return Config.Data.Volume;
            }
            set
            {
                Config.Data.Volume = value;
            }
        }
        public static float VolumeMultiplier = 1;
        public static float BaseVolume = 0.12f;
        
        public static long Timer = 0;
        public static string CurrentExecutablePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        // String Distances
        public static int LevenshteinDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s))
            {
                if (string.IsNullOrEmpty(t))
                    return 0;
                return t.Length;
            }

            if (string.IsNullOrEmpty(t))
            {
                return s.Length;
            }

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // initialize the top and right of the table to 0, 1, 2, ...
            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 1; j <= m; d[0, j] = j++) ;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    int min1 = d[i - 1, j] + 1;
                    int min2 = d[i, j - 1] + 1;
                    int min3 = d[i - 1, j - 1] + cost;
                    d[i, j] = Math.Min(Math.Min(min1, min2), min3);
                }
            }
            return d[n, m];
        }
        public static double JaroWinklerDistance(string aString1, string aString2)
        {
            int lLen1 = aString1.Length;
            int lLen2 = aString2.Length;
            if (lLen1 == 0)
                return lLen2 == 0 ? 1.0 : 0.0;

            int lSearchRange = Math.Max(0, Math.Max(lLen1, lLen2) / 2 - 1);

            // default initialized to false
            bool[] lMatched1 = new bool[lLen1];
            bool[] lMatched2 = new bool[lLen2];

            int lNumCommon = 0;
            for (int i = 0; i < lLen1; ++i)
            {
                int lStart = Math.Max(0, i - lSearchRange);
                int lEnd = Math.Min(i + lSearchRange + 1, lLen2);
                for (int j = lStart; j < lEnd; ++j)
                {
                    if (lMatched2[j]) continue;
                    if (aString1[i] != aString2[j])
                        continue;
                    lMatched1[i] = true;
                    lMatched2[j] = true;
                    ++lNumCommon;
                    break;
                }
            }
            if (lNumCommon == 0) return 0.0;

            int lNumHalfTransposed = 0;
            int k = 0;
            for (int i = 0; i < lLen1; ++i)
            {
                if (!lMatched1[i]) continue;
                while (!lMatched2[k]) ++k;
                if (aString1[i] != aString2[k])
                    ++lNumHalfTransposed;
                ++k;
            }
            // System.Diagnostics.Debug.WriteLine("numHalfTransposed=" + numHalfTransposed);
            int lNumTransposed = lNumHalfTransposed / 2;

            // System.Diagnostics.Debug.WriteLine("numCommon=" + numCommon + " numTransposed=" + numTransposed);
            double lNumCommonD = lNumCommon;
            double lWeight = (lNumCommonD / lLen1
                             + lNumCommonD / lLen2
                             + (lNumCommon - lNumTransposed) / lNumCommonD) / 3.0;

            if (lWeight <= 0.7) return lWeight;
            int lMax = Math.Min(4, Math.Min(aString1.Length, aString2.Length));
            int lPos = 0;
            while (lPos < lMax && aString1[lPos] == aString2[lPos])
                ++lPos;
            if (lPos == 0) return lWeight;
            return lWeight + 0.1 * lPos * (1.0 - lWeight);

        }
        public static float OwnDistance(string Input, string SongName)
        {
            if (Input == "" || Input == null || SongName == "" || SongName == null)
                return float.MaxValue;

            int Errors = 0;
            int Distances = 0;
            int LastFindingIndex = -5;
            List<float> Scores = new List<float>();
            
            for (int k = 0; k < SongName.Length; k++)
            {
                if (SongName.ElementAt(k) == char.ToUpper(Input.First()) ||
                    SongName.ElementAt(k) == char.ToLower(Input.First()))
                {
                    LastFindingIndex = k - 1;
                    foreach (char c in Input.ToCharArray())
                    {
                        int j = LastFindingIndex;
                        if (j < 0)
                            j = 0;
                        int a = SongName.IndexOf(char.ToLower(c), j);
                        int b = SongName.IndexOf(char.ToUpper(c), j);

                        if (a == -1 && b == -1)
                            Errors++;
                        else
                        {
                            int i;
                            if (b == -1 || a < b && a != -1)
                                i = a;
                            else
                                i = b;

                            if (LastFindingIndex != -5)
                                Distances += i - LastFindingIndex - 1;
                            LastFindingIndex = i;
                        }
                    }
                    Scores.Add(Errors + Distances / 3f);
                    Errors = 0;
                    Distances = 0;
                    LastFindingIndex = -5;
                }
            }

            if (Scores.Count > 0)
                return Scores.Min();
            else
                return 1000;
        } //useless but I kept it for nostalgia
        public static float LevenshteinDistanceWrapper(string Input, string SongName)
        {
            if (Input == null || Input == "" || SongName == "" || SongName == null)
                return float.MaxValue;

            Input = Input.ToLower();
            SongName = SongName.ToLower();

            if (SongName.Length <= Input.Length)
                return LevenshteinDistance(Input, SongName);

            List<float> Distances = new List<float>();

            string[] InSplit = Input.Split(' ');

            if (InSplit.Length == 1)
            {
                string[] split = SongName.Split(' ');
                for (int i = 0; i < split.Length; i++)
                    if (split[i] != "-")
                        Distances.Add(LevenshteinDistance(split[i], Input));
            }
            else
            {
                float count = 0;
                for (int i = 0; i < InSplit.Length; i++)
                {
                    List<float> Distances2 = new List<float>();
                    string[] split = SongName.Split(' ');
                    for (int j = 0; j < split.Length; j++)
                        if (split[j] != "-")
                            Distances2.Add(LevenshteinDistance(split[j], InSplit[i]));
                    count += Distances2.Min();
                }
                Distances.Add(count);
            }

            return Distances.Min();
        }

        // Averages
        public static float GetAverageAmplitude(float[] samples)
        {
            if (samples != null)
            {
                float average = 0;

                for (int i = 0; i < samples.Length; i++)
                    average += Math.Abs(samples[i]);

                average /= samples.Length;

                return average;
            }
            else
                return 0;
        }
        public static float GetAverageChangeRate(float[] samples)
        {
            if (samples != null)
            {
                float average = 0;

                for (int i = 1; i < samples.Length; i++)
                    average += Math.Abs(samples[i] - samples[i - 1]);

                average /= samples.Length;

                return average;
            }
            else
                return 0;
        }
        public static float GetRootMeanSquare(float[] samples)
        {
            if (samples != null)
            {
                float n = 0;

                for (int i = 0; i < samples.Length; i++)
                    n += samples[i] * samples[i];
                
                n = (float)Math.Sqrt(n / samples.Length);

                return n;
            }
            else
                return 0;
        }
        public static float GetRootMeanSquareApproximation(float[] samples)
        {
            if (samples != null)
            {
                float n = 0;

                for (int i = 0; i < samples.Length; i++)
                    n += samples[i] * samples[i];

                n = (float)Approximate.Sqrt(n / samples.Length);

                return n;
            }
            else
                return 0;
        }

        // Math Functions
        public static float AnimationFunction(float x)
        {
            return (float)((Math.Pow(2, -1.5 * (x - 1) * (x - 1)) / 10f + 1) * (-Math.Pow(5, -x) + 1));
        }
        public static double AnimationFunction2(double x)
        {
            x += Math.PI;
            return Math.Pow(1.2, -x / 2) * Math.Sin(x * 1.8);
        }
        public static float Sigmoid(double value)
        {
            return (float)(1.0 / (1.0 + Math.Pow(Math.E, -value)));
        }

        // Simple Geometry
        public static float DistanceFromLineToPoint(Vector2 Line1, Vector2 Line2, Vector2 Point)
        {
            Vector2 HelpingVector = new Vector2(-(Line1.Y - Line2.Y), Line1.X - Line2.X);
            Vector2 Intersection = IntersectionPoint(Line1, Line2, Point, HelpingVector);
            return (Point - Intersection).Length();
        }
        public static Vector2 IntersectionPoint(Vector2 A1, Vector2 A2, Vector2 B1, Vector2 B2)
        {
            // Nutze hier die Cramerschen Regel:
            return new Vector2(((B2.X - B1.X) * (A2.X * A1.Y - A1.X * A2.Y) - (A2.X - A1.X) * (B2.X * B1.Y - B1.X * B2.Y)) / 
                                ((B2.Y - B1.Y) * (A2.X - A1.X) - (A2.Y - A1.Y) * (B2.X - B1.X)),

                                ((A1.Y - A2.Y) * (B2.X * B1.Y - B1.X * B2.Y) - (B1.Y - B2.Y) * (A2.X * A1.Y - A1.X * A2.Y)) / 
                                ((B2.Y - B1.Y) * (A2.X - A1.X) - (A2.Y - A1.Y) * (B2.X - B1.X)));
        }
        
        // Console Methods
        public static void GivConsole()
        {
            AllocConsole();
            IntPtr stdHandle = GetStdHandle(STD_OUTPUT_HANDLE);
            SafeFileHandle safeFileHandle = new SafeFileHandle(stdHandle, true);
            FileStream fileStream = new FileStream(safeFileHandle, FileAccess.Write);
            Encoding encoding = System.Text.Encoding.GetEncoding(MY_CODE_PAGE);
            StreamWriter standardOutput = new StreamWriter(fileStream, encoding);
            standardOutput.AutoFlush = true;
            Console.SetOut(standardOutput);
        }
        public static void HideConsole() { ShowWindow(GetConsoleWindow(), 0); }
        public static void MinimizeConsole() { ShowWindow(GetConsoleWindow(), 2); }
        public static void ShowConsole() { ShowWindow(GetConsoleWindow(), 5); }
        public static void DisableConsoleRezise()
        {
            IntPtr handle = GetConsoleWindow();
            IntPtr sysMenu = GetSystemMenu(handle, false);

            if (handle != IntPtr.Zero)
            {
                DeleteMenu(sysMenu, SC_MAXIMIZE, MF_BYCOMMAND);
                DeleteMenu(sysMenu, SC_SIZE, MF_BYCOMMAND);
            }
        }
        public static bool AttachToConsole()
        {
            const uint ParentProcess = 0xFFFFFFFF;
            if (!AttachConsole(ParentProcess))
                return false;

            return true;
        }

        // Extensions
        public static void EnableBlur(this Form @this)
        {
            var accent = new AccentPolicy();
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;
            var accentStructSize = Marshal.SizeOf(accent);
            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);
            var Data = new WindowCompositionAttributeData();
            Data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            Data.SizeOfData = accentStructSize;
            Data.Data = accentPtr;
            SetWindowCompositionAttribute(@this.Handle, ref Data);
            Marshal.FreeHGlobal(accentPtr);
        }
        public static void RestoreFromMinimzied(this Form form)
        {
            form.WindowState = FormWindowState.Normal;
        }
        public static void InvokeIfRequired(this ISynchronizeInvoke obj, MethodInvoker action)
        {
            if (obj.InvokeRequired)
            {
                var args = new object[0];
                obj.Invoke(action, args);
            }
            else
            {
                action();
            }
        }
        public static bool Contains(this String str, String substring, StringComparison comp)
        {
            return str.IndexOf(substring, comp) >= 0;
        }

        // Other
        public static string AsTime(double seconds)
        {
            int s = (int)(seconds % 60);
            int m = (int)(seconds / 60) % 60;
            int h = (int)(seconds / 60 / 60) % 24;
            int d = (int)(seconds / 60 / 60 / 24) % 365;
            int y = (int)(seconds / 60 / 60 / 24 / 365);

            string sS = s.ToString();
            if (sS.Length == 0)
                sS = sS.Insert(0, "00");
            else if (sS.Length == 1)
                sS = sS.Insert(0, "0");

            string mS = m.ToString();
            if (mS.Length == 0)
                mS = mS.Insert(0, "00");
            else if (mS.Length == 1)
                mS = mS.Insert(0, "0");

            if (h == 0)
                return mS + ":" + sS;
            else if (d == 0)
                return h + ":" + mS + ":" + sS;
            else if (y == 0)
                return d + ":" + h + ":" + mS + ":" + sS;
            else
                return y + ":" + d + ":" + h + ":" + mS + ":" + sS;
        }
        public static Task StartSTATask(Action func)
        {
            var tcs = new TaskCompletionSource<object>();
            var thread = new Thread(() =>
            {
                try
                {
                    func();
                    tcs.SetResult(null);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }
        public static void RegisterUriScheme()
        {
            string UriScheme = "MusicPlayerScheme";
            string FriendlyName = "MusicPlayer";

            string[] subkeys = Registry.CurrentUser.OpenSubKey("SOFTWARE").OpenSubKey("Classes").GetSubKeyNames();
            if (subkeys.Contains(UriScheme))
                return;

            RegistryKey key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\" + UriScheme);

            key.SetValue("", "URL:" + FriendlyName);
            key.SetValue("URL Protocol", "");

            RegistryKey defaultIcon = key.CreateSubKey("DefaultIcon");
            defaultIcon.SetValue("", "MusicPlayer.exe" + ",1");

            RegistryKey commandKey = key.CreateSubKey(@"shell\open\command");
            commandKey.SetValue("", "\"" + CurrentExecutablePath + "\\MusicPlayer.exe" + "\" \"%1\"");
        }
        public static bool IsForegroundFullScreen()
        {
            return IsForegroundFullScreen(null);
        }
        public static bool IsForegroundFullScreen(Screen screen)
        {
            if (screen == null)
                screen = Screen.PrimaryScreen;
            RECT rect = new RECT();
            IntPtr hWnd = (IntPtr)GetForegroundWindow();

            GetWindowRect(new HandleRef(null, hWnd), ref rect);

            if (screen.Bounds.Width == (rect.right - rect.left) && screen.Bounds.Height == (rect.bottom - rect.top))
                return true;
            else
                return false;
        }

        #region Imports
        [DllImport("user32.dll")]
        static internal extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("Kernel32")]
        public static extern void AllocConsole();
        [DllImport("Kernel32")]
        public static extern void FreeConsole();
        [DllImport("kernel32.dll")]
        public static extern bool AttachConsole(uint dwProcessId);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetStdHandle(UInt32 nStdHandle);
        [DllImport("kernel32.dll")]
        public static extern void SetStdHandle(UInt32 nStdHandle, IntPtr handle);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);
        [DllImport("user32.dll")]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("kernel32.dll",
            EntryPoint = "GetStdHandle",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetStdHandle(int nStdHandle);
        public const int STD_OUTPUT_HANDLE = -11;
        public const int MY_CODE_PAGE = 437;
        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(HandleRef hWnd, [In, Out] ref RECT rect);
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
        public delegate void WinEventDelegate(IntPtr hWinEventHook,
            uint eventType, IntPtr hwnd, int idObject,
            int idChild, uint dwEventThread, uint dwmsEventTime);
        public const uint WINEVENT_OUTOFCONTEXT = 0;
        public const uint EVENT_SYSTEM_FOREGROUND = 3;
        [DllImport("user32.dll")]
        public static extern bool UnhookWinEvent(IntPtr hWinEventHook);
        [DllImport("user32.dll")]
        public static extern IntPtr SetWinEventHook(uint eventMin,
            uint eventMax, IntPtr hmodWinEventProc,
            WinEventDelegate lpfnWinEventProc, uint idProcess,
            uint idThread, uint dwFlags);
        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd,
            StringBuilder lpString, int nMaxCount);
        #endregion
    }

    // Approximate Math Functions
    public class Approximate
    {
        public static float Sqrt(float z)
        {
            if (z == 0) return 0;
            FloatIntUnion u;
            u.tmp = 0;
            u.f = z;
            u.tmp -= 1 << 23; /* Subtract 2^m. */
            u.tmp >>= 1; /* Divide by 2. */
            u.tmp += 1 << 29; /* Add ((b + 1) / 2) * 2^m. */
            return u.f;
        }
        public static double Exp(double val)
        {
            long tmp = (long)(1512775 * val + 1072632447);
            return BitConverter.Int64BitsToDouble(tmp << 32);
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct FloatIntUnion
        {
            [FieldOffset(0)]
            public float f;

            [FieldOffset(0)]
            public int tmp;
        }
    }

    #region struct and enum dump
    enum AccentState
    {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_GRADIENT = 1,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_INVALID_STATE = 4
    }
    struct AccentPolicy
    {
        public AccentState AccentState;
    }
    struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }
    enum WindowCompositionAttribute
    {
        WCA_ACCENT_POLICY = 19
    }
    #endregion
}
