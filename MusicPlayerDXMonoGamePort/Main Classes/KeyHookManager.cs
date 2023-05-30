using RawInput_dll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicPlayerDXMonoGamePort
{
    public static class KeyHookManager
    {
        //public static KeyboardHook keyHook = null;
        private static RawInput _rawinput = null;

        public static void CreateGlobalKeyHooks(IntPtr windowHandle)
        {
            if (_rawinput == null)
            {
                _rawinput = new RawInput(windowHandle, false);
                _rawinput.AddMessageFilter();
                _rawinput.KeyPressed += OnRawKeyPressed;
            }
        }
        public static void KeyHook_KeyDown(Keys key, bool Shift, bool Ctrl, bool Alt)
        {
            if (Values.Timer > SongManager.SongChangedTickTime + 30)
            {
                var k = key;

                // Key Events
                if (k == Keys.MediaPlayPause)
                    SongManager.PlayPause();

                if (k == Keys.MediaNextTrack)
                    SongManager.GetNextSong(false, true);

                if (k == Keys.MediaPreviousTrack)
                    SongManager.GetPreviousSong();

                if (k == Keys.MediaStop)
                    SongManager.IsCurrentSongUpvoted = !SongManager.IsCurrentSongUpvoted;
            }
        }
        private static void OnRawKeyPressed(object sender, RawInputEventArg e)
        {
            //Debug.WriteLine($"Event! {(Keys)e.KeyPressEvent.VKey}");
            if (e.KeyPressEvent.KeyPressState == "MAKE")
                KeyHook_KeyDown((Keys)e.KeyPressEvent.VKey, false, false, false);
        }
        public static void DisposeGlobalKeyHooks()
        {
            _rawinput.KeyPressed -= OnRawKeyPressed;
        }
    }
}
