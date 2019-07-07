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
            FirstThird, SecondThird, ThirdThird
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
        }
        
        private void Register(Keys key, Location direction) {
            int modifier = (int)KeyModifier.WinKey + (int)KeyModifier.Alt + (int)KeyModifier.Control;
            int freeId = registered.Count;
            RegisterHotKey(this.Handle, freeId, modifier, key.GetHashCode());
            registered.Add(freeId);
            keyToLocation.Add(key, direction);
        }

        private void MoveWindow(Location location) {
            // TODO shouldnt be hardcoded
            Screen second = Screen.AllScreens[1];
            IntPtr handle = GetForegroundWindow();
            
            int x = second.WorkingArea.X;
            int y = second.WorkingArea.Y;
            int width = second.WorkingArea.Width;
            int height = second.WorkingArea.Height / 2;

            // TODO refactor
            if (location == Location.Down) {
                y += height;
            } else if (location == Location.Center) {
                // Doesnt really Center it sets position same height as my left landscape display
                y = 0;
                height = 1080;
            } else if (location != Location.Top) {
                height = (height * 2) / 3; // First Third
                if (location == Location.SecondThird) {
                    y += height;
                } else if (location == Location.ThirdThird) { 
                    y += height * 2;
                }
            }
            
            // win10 seems to have invisible margin/border around windows
            MoveWindow(handle, x, y, width, height, true);
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

        [DllImport("user32.dll")]
        static extern int DwmGetWindowAttribute(
            IntPtr hWnd,
            int dwAttribute,
            out Rectangle pvAttribute,
            int cbAttribute);
        #endregion
    }
}
