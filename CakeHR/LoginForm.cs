using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CakeHR
{
    public partial class LoginForm : Form
    {
        public static String username;
        public static String password;
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        public static LoginForm currentObject;

        //For Widget
        [DllImport("User32.dll")]
        static extern IntPtr FindWindow(String lpClassName, String lpWindowName);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("user32.dll")]
        static extern int SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        //For Drag
        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,     // x-coordinate of upper-left corner
            int nTopRect,      // y-coordinate of upper-left corner
            int nRightRect,    // x-coordinate of lower-right corner
            int nBottomRect,   // y-coordinate of lower-right corner
            int nWidthEllipse, // width of ellipse
            int nHeightEllipse // height of ellipse
        );

        //For TextBox PlaceHolder
        private const int EM_SETCUEBANNER = 0x1501;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        public LoginForm()
        {
            InitializeComponent();
            currentObject = this;
            materialSingleLineTextField1.ForeColor = Color.FromArgb(210, 209, 210);
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 5, 5));

            // The path to the key where Windows looks for startup applications
            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey(
                                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            //Path to launch shortcut
            string startPath = Environment.GetFolderPath(Environment.SpecialFolder.Programs)
                               + @"\CakeHR\CakeHR.appref-ms";
            rkApp.SetValue("CakeHR", startPath);

            //SendMessage(textBox1.Handle, EM_SETCUEBANNER, 0, "Email");
            //SendMessage(textBox2.Handle, EM_SETCUEBANNER, 0, "Password");
        }

        private bool checkLogin()
        {
            if(Properties.Settings.Default.userName!="" && Properties.Settings.Default.password != ""){
                List<List<string>> notifications = CakeAPI.fetchNotifications();
                if (notifications != null)
                {
                    new Form1().Show();
                    this.Hide();
                    return true;
                }
            }
            return false;
            
        }

        private void LoginForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void materialRaisedButton1_Click(object sender, EventArgs e)
        {
            
        }

        private void makeWidget()
        {
            int width = Screen.FromControl(this).Bounds.Width;
            this.Location = new Point((width - this.Width) - 10, 10);
            this.StartPosition = FormStartPosition.Manual;
            //sendToDesktop();
        }

        private void sendToDesktop()
        {
            IntPtr pWnd = FindWindow("Progman", null);
            pWnd = FindWindowEx(pWnd, IntPtr.Zero, "SHELLDLL_DefVIew", null);
            pWnd = FindWindowEx(pWnd, IntPtr.Zero, "SysListView32", null);
            IntPtr tWnd = this.Handle;
            SetParent(tWnd, pWnd);
        }

        private void LoginForm_Shown(object sender, EventArgs e)
        {
            checkLogin();
            makeWidget();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel1_Click(object sender, EventArgs e)
        {
            username = materialSingleLineTextField1.Text;
            password = materialSingleLineTextField2.Text;
            Properties.Settings.Default.userName = username;
            Properties.Settings.Default.password = password;
            Properties.Settings.Default.Save();
            if (username == "" || password == "")
            {
                MessageBox.Show("Please enter username and password!", "SRB CakeHR Widget");
            }
            else
            {
                List<List<string>> notifications = CakeAPI.fetchNotifications();
                if (notifications != null)
                {
                    this.Hide();
                    new Form1().Show();
                }
                else
                {
                    MessageBox.Show(this, "Please ensure that the username/password you have selected is correct.", "SRB CakeHR Widget");
                    materialSingleLineTextField1.Text = "";
                    materialSingleLineTextField2.Text = "";
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void materialSingleLineTextField2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                panel1_Click(sender, e);
            }
        }
    }
}
