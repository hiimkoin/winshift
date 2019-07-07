using System;
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

        enum Direction {
            Top, Down,
            FirstThird, SecondThird, ThirdThird
        }

        public GlobalHotkeys() {
            // TODO refactor
            int modifier = (int)KeyModifier.WinKey + (int)KeyModifier.Alt;
            RegisterHotKey(this.Handle, 0, modifier, Keys.Up.GetHashCode());
            RegisterHotKey(this.Handle, 1, modifier, Keys.Down.GetHashCode());
            int extModifier = (int)KeyModifier.WinKey + (int)KeyModifier.Alt + (int)KeyModifier.Control;
            RegisterHotKey(this.Handle, 2, extModifier, Keys.D8.GetHashCode());
            RegisterHotKey(this.Handle, 3, extModifier, Keys.D9.GetHashCode());
            RegisterHotKey(this.Handle, 4, extModifier, Keys.D0.GetHashCode());
        }
        
        private void MoveWindow(Direction direction) {
            // TODO shouldnt be hardcoded
            Screen second = Screen.AllScreens[1];
            IntPtr handle = GetForegroundWindow();
            
            int x = second.WorkingArea.X;
            int y = second.WorkingArea.Y;
            int width = second.WorkingArea.Width;
            int height = second.WorkingArea.Height / 2;

            // TODO refactor
            if (direction == Direction.Down) {
                y += height;
            } else if (direction == Direction.FirstThird) {
                height = (height * 2) / 3;
            } else if (direction == Direction.SecondThird) {
                height = (height * 2) / 3;
                y += height;
            } else if (direction == Direction.ThirdThird) {
                height = (height * 2) / 3;
                y += height * 2;
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
                
                var idToDirection = new SortedDictionary<int, Direction>();
                idToDirection.Add(0, Direction.Top);
                idToDirection.Add(1, Direction.Down);
                idToDirection.Add(2, Direction.FirstThird);
                idToDirection.Add(3, Direction.SecondThird);
                idToDirection.Add(4, Direction.ThirdThird);
                
                MoveWindow(idToDirection[id]);
            }
        }

        ~GlobalHotkeys() {
            // TODO refactor
            UnregisterHotKey(this.Handle, 0);
            UnregisterHotKey(this.Handle, 1);
            UnregisterHotKey(this.Handle, 2);
            UnregisterHotKey(this.Handle, 3);
            UnregisterHotKey(this.Handle, 4);
        }

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
    }
}
