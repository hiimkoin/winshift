using System;
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
            Top, Down, None
        }

        public GlobalHotkeys() {
            int modifier = (int)KeyModifier.WinKey + (int)KeyModifier.Alt;
            RegisterHotKey(this.Handle, 0, modifier, Keys.Up.GetHashCode());
            RegisterHotKey(this.Handle, 1, modifier, Keys.Down.GetHashCode());
        }
        
        private void MoveWindow(Direction direction) {
            // TODO shouldnt be hardcoded
            Screen second = Screen.AllScreens[1];
            IntPtr handle = GetForegroundWindow();
            
            int x = second.WorkingArea.X;
            int y = second.WorkingArea.Y;
            int width = second.WorkingArea.Width;
            int height = second.WorkingArea.Height / 2;

            if (direction == Direction.Down) {
                y += height;
            }

            Console.WriteLine("x: " + x);
            Console.WriteLine("y: " + y);
            Console.WriteLine("width: " + width);
            Console.WriteLine("height: " + height);

            MoveWindow(handle, x, y, width, height, true);
        }

        protected override void WndProc(ref Message m) {
            base.WndProc(ref m);
            if (m.Msg == 0x0312) {
                Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                KeyModifier modifier = (KeyModifier)((int)m.LParam & 0xFFFF);
                int id = m.WParam.ToInt32();

                var direction = Direction.None;
                switch (id) {
                    case 0:
                        direction = Direction.Top;
                        break;
                    case 1:
                        direction = Direction.Down;
                        break;
                    default:
                        return;
                }
                MoveWindow(direction);
            }
        }

        ~GlobalHotkeys() {
            UnregisterHotKey(this.Handle, 0);
            UnregisterHotKey(this.Handle, 1);
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
