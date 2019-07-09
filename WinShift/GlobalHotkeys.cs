using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WinShift {
    partial class GlobalHotkeys : Form {

        enum KeyModifier {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            WinKey = 8
        }

        enum Location {
            Top, Down, Center, 
            FirstThird, SecondThird, ThirdThird,
            FirstQuater, SecondQuarter, ThirdQuarter, FourthQuarter,
            FirstSecondThird, SecondSecondThird,
        }

        ArrayList registered = new ArrayList();
        SortedDictionary<Keys, Location> keyToLocation = new SortedDictionary<Keys, Location>();

        public GlobalHotkeys() {
            Register(Keys.Up, Location.Top);
            Register(Keys.Down, Location.Down);

            Register(Keys.D8, Location.FirstThird);
            Register(Keys.D9, Location.SecondThird);
            Register(Keys.D0, Location.ThirdThird);

            Register(Keys.M, Location.Center);

            Register(Keys.D1, Location.FirstQuater);
            Register(Keys.D2, Location.SecondQuarter);
            Register(Keys.D3, Location.ThirdQuarter);
            Register(Keys.D4, Location.FourthQuarter);

            Register(Keys.OemOpenBrackets, Location.FirstSecondThird);
            Register(Keys.OemCloseBrackets, Location.SecondSecondThird);
        }
        
        private void Register(Keys key, Location location) {
            int modifier = (int)KeyModifier.WinKey + (int)KeyModifier.Alt + (int)KeyModifier.Control;
            int freeId = registered.Count;
            RegisterHotKey(this.Handle, freeId, modifier, key.GetHashCode());
            registered.Add(freeId);
            keyToLocation.Add(key, location);
        }
        
        private void MoveWindow(Location location) {
            // TODO shouldnt be hardcoded
            Screen second = Screen.AllScreens[1];
            IntPtr handle = GetForegroundWindow();

            // restore original size if window is max or minimized
            ShowWindow(handle, CmdShow.SW_RESTORE);

            int x = second.WorkingArea.X;
            int y = second.WorkingArea.Y;
            int width = second.WorkingArea.Width;
            int height = second.WorkingArea.Height;
            
            // TODO refactor
            switch (location) {
                case Location.Top:
                    height /= 2;
                    break;
                case Location.Down:
                    height /= 2;
                    y += height;
                    break;
                case Location.Center:
                    // Doesnt really Center it sets position same height as my left landscape display
                    y = 0;
                    height = 1080;
                    break;
                case Location.FirstThird:
                    height /= 3;
                    break;
                case Location.SecondThird:
                    height /= 3;
                    y += height;
                    break;
                case Location.ThirdThird:
                    height /= 3;
                    y += height * 2;
                    break;
                case Location.FirstQuater:
                    height /= 4;
                    break;
                case Location.SecondQuarter:
                    height /= 4;
                    y += height;
                    break;
                case Location.ThirdQuarter:
                    height /= 4;
                    y += height * 2;
                    break;
                case Location.FourthQuarter:
                    height /= 4;
                    y += height * 3;
                    break;
                case Location.FirstSecondThird:
                    height /= 3;
                    break;
                case Location.SecondSecondThird:
                    y += height / 3;
                    height = (height * 2) / 3;
                    break;
            }
            
            // win10 seems to have invisible margin/border around windows
            RECT border = calculateBorders(handle);

            x -= border.Left;
            y -= border.Top;
            width += border.Left + border.Right;
            height += border.Top + border.Bottom;
            
            MoveWindow(handle, x, y, width, height, true);
        }

        private RECT calculateBorders(IntPtr handle) {
            RECT rect, frame;
            GetWindowRect(handle, out rect);
            DwmGetWindowAttribute(handle,
                DwmWindowAttribute.DWMWA_EXTENDED_FRAME_BOUNDS, out frame, Marshal.SizeOf<RECT>());

            RECT border;
            border.Left = Math.Abs(frame.Left - rect.Left);
            border.Top = Math.Abs(frame.Top - rect.Top);
            border.Right = Math.Abs(frame.Right - rect.Right);
            border.Bottom = Math.Abs(frame.Bottom - rect.Bottom);

            return border;
        }

        protected override void WndProc(ref Message m) {
            base.WndProc(ref m);
            if (m.Msg == 0x0312) {
                Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                KeyModifier modifier = (KeyModifier)((int)m.LParam & 0xFFFF);
                int id = m.WParam.ToInt32();
                
                MoveWindow(keyToLocation[key]);
            }
        }

        ~GlobalHotkeys() {
            foreach(int id in registered) {
                UnregisterHotKey(this.Handle, id);
            }
        }

        #region winapi
        [DllImport("user32.dll")]
        static extern bool RegisterHotKey(
            IntPtr hWnd,
            int id,
            int fsModifiers,
            int vk);

        [DllImport("user32.dll")]
        static extern bool UnregisterHotKey(
            IntPtr hWnd,
            int id);

        [DllImport("user32.dll")]
        static extern bool MoveWindow(
            IntPtr hWnd,
            int X,
            int Y,
            int nWidth,
            int nHeight,
            bool bRepaint);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern bool GetWindowRect(
            IntPtr hwnd,
            out Rectangle rect);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [DllImport("dwmapi.dll")]
        static extern int DwmGetWindowAttribute(
            IntPtr hWnd,
            DwmWindowAttribute dwAttribute,
            out RECT pvAttribute,
            int cbAttribute);
        
        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct RECT {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
            public Rectangle ToRectangle() {
                return Rectangle.FromLTRB(Left, Top, Right, Bottom);
            }
        }

        [Flags]
        public enum DwmWindowAttribute : uint {
            DWMWA_NCRENDERING_ENABLED = 1,
            DWMWA_NCRENDERING_POLICY,
            DWMWA_TRANSITIONS_FORCEDISABLED,
            DWMWA_ALLOW_NCPAINT,
            DWMWA_CAPTION_BUTTON_BOUNDS,
            DWMWA_NONCLIENT_RTL_LAYOUT,
            DWMWA_FORCE_ICONIC_REPRESENTATION,
            DWMWA_FLIP3D_POLICY,
            DWMWA_EXTENDED_FRAME_BOUNDS,
            DWMWA_HAS_ICONIC_BITMAP,
            DWMWA_DISALLOW_PEEK,
            DWMWA_EXCLUDED_FROM_PEEK,
            DWMWA_CLOAK,
            DWMWA_CLOAKED,
            DWMWA_FREEZE_REPRESENTATION,
            DWMWA_LAST
        }

        [DllImport("user32.dll")]
        static extern bool ShowWindow(
            IntPtr hWnd,
            CmdShow nCmdShow);

        [Flags]
        public enum CmdShow : int {
            SW_HIDE,
            SW_SHOWNORMAL,
            SW_SHOWMINIMIZED,
            SW_SHOWMAXIMIZED,
            SW_SHOWNOACTIVATE,
            SW_SHOW,
            SW_MINIMIZE,
            SW_SHOWMINNOACTIVE,
            SW_SHOWNA,
            SW_RESTORE,
            SW_SHOWDEFAULT,
            SW_FORCEMINIMIZE
        }
        #endregion
    }
}
