using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace MusicPlayerDXMonoGamePort
{
    public class DropShadow : Form
    {
        Form parentForm;

        // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowpos
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_NOMOVE = 0x0002;
        const uint SWP_NOACTIVATE = 0x0010;

        public DropShadow(Form parentForm, bool ManualFocus) : base()
        {
            parentForm.Closed += ParentForm_Closed; //Closes this when parent closes
            parentForm.Move += ParentForm_Move; //Follows movement of parent form
            this.GotFocus += DropShadow_GotFocus;
            this.Shown += DropShadow_Shown;
            if (!ManualFocus)
                parentForm.GotFocus += ParentForm_GotFocus;

            this.ShowInTaskbar = false;
            this.parentForm = parentForm;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
        }

        private void DropShadow_Shown(object sender, EventArgs e)
        {
            UpdateSizeLocation();
        }

        public void UpdateSizeLocation()
        {
            Size = new Size(parentForm.Size.Width, parentForm.Size.Height);
            Location = new Point(parentForm.Location.X, parentForm.Location.Y);
            KeepBehindParent();
        }

        /// <summary>
        /// Re-inserts this window directly below the parent in the Z order.
        /// This is one atomic SetWindowPos instead of showing or activating this window, because
        /// Show()/Activate() raise it above the parent and leave this opaque form covering the player.
        /// </summary>
        public void KeepBehindParent()
        {
            if (parentForm == null || parentForm.IsDisposed || !parentForm.IsHandleCreated || !IsHandleCreated)
                return;

            // hWndInsertAfter = the parent window means "place this window directly below the parent".
            SetWindowPos(Handle, parentForm.Handle, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        }

        private void DropShadow_GotFocus(object sender, EventArgs e)
        {
            // Safety net: this window must never sit above the parent.
            KeepBehindParent();
        }

        private void ParentForm_GotFocus(object sender, EventArgs e)
        {
            KeepBehindParent();
        }

        private void ParentForm_Closed(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ParentForm_Move(object sender, EventArgs e)
        {
            UpdateSizeLocation();
        }

        protected override void Dispose(bool disposing)
        {
            // Without this, a re-created shadow (background mode swap) leaves stale handlers on the
            // parent form that keep resizing a disposed window.
            if (disposing && parentForm != null)
            {
                parentForm.Closed -= ParentForm_Closed;
                parentForm.Move -= ParentForm_Move;
                parentForm.GotFocus -= ParentForm_GotFocus;
            }
            base.Dispose(disposing);
        }

        // This window is decoration only: it must never take activation away from the player.
        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= 0x00020000; // CS_DROPSHADOW
                cp.ExStyle |= 0x08000000;    // WS_EX_NOACTIVATE
                return cp;
            }
        }
    }
}
