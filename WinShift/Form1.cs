using Microsoft.Win32;
using System;
using System.Windows.Forms;

namespace WinShift {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }
        
        private void Form1_Load(object sender, EventArgs e) {
            var gh = new GlobalHotkeys();

            MenuItem exitMenuItem = new MenuItem("Exit", new EventHandler(Exit));

            this.Hide();
            notifyIcon1.ContextMenu = new ContextMenu(
                new MenuItem[] {
                    exitMenuItem
                });
            notifyIcon1.Visible = true;

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true)) {
                key.SetValue("WinShift", "\"" + Application.ExecutablePath + "\"");
            }
        }

        protected override void OnVisibleChanged(EventArgs e) {
            base.OnVisibleChanged(e);
            this.Visible = false;
        }
        
        void Exit(object sender, EventArgs e) {
            notifyIcon1.Visible = false;
            Application.Exit();
        }
    }
}
