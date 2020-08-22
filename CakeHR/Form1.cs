using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using System.Net;
using System.IO;
using System.Activities;
using RestSharp;
using Newtonsoft.Json;
using System.Diagnostics;

namespace CakeHR
{
    public partial class Form1 : Form
    {
        //For Widget
        [DllImport("User32.dll")]
        static extern IntPtr FindWindow(String lpClassName, String lpWindowName);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("user32.dll")]
        static extern int SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        //

        private static bool logOff = false;

        // For Drag and Corners
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

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

        public Form1()
        {
            InitializeComponent();
        }



        private void populateFormData() {
            dynamic employeesOut = CakeAPI.getEmployeesOutofOffice();
            if (employeesOut == null) {
                MessageBox.Show("Please make sure you have an active internet connection!", "SRB CakeHR Widget");
                signOut();
                return;
            }
            for (int i = 0; i < employeesOut.data.Count; i++)
            {
                addEmployee(CakeAPI.getEmployeeProfileUrl(int.Parse(JsonConvert.SerializeObject(employeesOut.data[i].employee_id))),
                    "On leave",
                    JsonConvert.SerializeObject(employeesOut.data[i].policy.name).Replace("\"", ""), "Faraz");
            }
            List<List<String>> leavingEmployees = CakeAPI.getEmployeesLeavingSoon();
            for (int i = 0; i < leavingEmployees.Count; i++) {
                List<String> employee = leavingEmployees[i];
                if (int.Parse(employee[1]) > 0)
                    addEmployee(CakeAPI.getEmployeeProfileUrl(int.Parse(employee[0])),
                        "Leaving in " + employee[1] + " day(s)",
                        employee[2], "Faraz");
            }
            //runCmd("");

            /*List<List<string>> notifications = CakeAPI.fetchNotifications();
            if (notifications == null) {
                signOut();
                return;
            }
            for (int i = 0; i < notifications.Count; i++) {
                addNotification(notifications[i][0], notifications[i][1]);
            }*/
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            makeWidget();
        }


        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void materialLabel3_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //addEmployee("https://s3-eu-west-1.amazonaws.com/cakehr/profile_pictures/100h_32d3785d2596eaf52c0d30edec689030?1549278518", "Hello World!", "Hello!");
        }

        private void addEmployee(string imageURL, string headline, string subHeader, string name) {
            System.Windows.Forms.Panel panel3 = new System.Windows.Forms.Panel();
            panel3.Width = panel2.Width;
            panel3.Height = panel2.Height;
            foreach (Control c in panel2.Controls)
            {
                Control c2 = new Control();
                if (c.GetType() == typeof(System.Windows.Forms.TextBox))
                    c2 = new System.Windows.Forms.TextBox();
                if (c.GetType() == typeof(System.Windows.Forms.Label))
                {
                    c2 = new Label();
                    c2.MaximumSize = c.MaximumSize;
                }
                if (c.GetType() == typeof(CheckBox))
                    c2 = new CheckBox();
                if (c.GetType() == typeof(DataGridView))
                    c2 = new DataGridView();
                if (c.GetType() == typeof(PictureBox))
                {
                    c2 = new PictureBox();
                    ((PictureBox)c2).Image = urlToImg(imageURL);
                    ((PictureBox)c2).SizeMode = ((PictureBox)c).SizeMode;
                }
                c2.Location = c.Location;
                c2.Size = c.Size;
                if (c.Text == "headline")
                {
                    c2.Text = headline;
                }
                else if (c.Text == "name") {
                    c2.Text = name;
                }
                else
                {
                    c2.Text = subHeader;
                    if (headline.Length > 20)
                    {
                        Point labelLocation = c2.Location;
                    }
                }
                c2.Font = c.Font;
                c2.ForeColor = c.ForeColor;
                panel3.Controls.Add(c2);
            }

            panel3.BackColor = panel2.BackColor;
            flowLayoutPanel1.Controls.Add(panel3);

            adjustComponentPositions();
        }

        private void addNotification(string headline, string text)
        {
            Panel panel3 = new Panel();
            panel3.Width = panel4.Width;
            panel3.Height = panel4.Height;
            foreach (Control c in panel4.Controls)
            {
                Control c2 = new Control();
                if (c.GetType() == typeof(RichTextBox))
                {
                    c2 = new RichTextBox();
                    c2.MaximumSize = c.MaximumSize;
                    ((RichTextBox)c2).Multiline = true;
                    c2.MinimumSize = c.MinimumSize;
                    ((RichTextBox)c2).ReadOnly = true;
                    ((RichTextBox)c2).BorderStyle = BorderStyle.None;
                }
                c2.Location = c.Location;
                c2.Size = c.Size;
                if (c.GetType() == typeof(Label))
                {
                    c2 = new Label();
                    c2.MaximumSize = c.MaximumSize;
                }
                if (c.Text == "heading")
                {
                    c2.Text = headline;
                }
                else
                {
                    c2.Text = text;
                }
                c2.Size = c.Size;
                c2.Font = c.Font;
                c2.ForeColor = c.ForeColor;
                panel3.Controls.Add(c2);
            }

            flowLayoutPanel2.Controls.Add(panel3);

            adjustComponentPositions();
        }

        private System.Drawing.Image urlToImg(string url) {
            WebClient wc = new WebClient();
            byte[] bytes = wc.DownloadData(url);
            MemoryStream ms = new MemoryStream(bytes);
            System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
            return img;
        }

        private void adjustComponentPositions() {
            int newY = flowLayoutPanel1.Location.Y + flowLayoutPanel1.Size.Height + 5;
            panel3.Location = new Point(panel3.Location.X, newY);
            newY = newY + panel3.Height + 2;
            flowLayoutPanel2.Location = new Point(flowLayoutPanel2.Location.X, newY);
            newY = newY + flowLayoutPanel2.Size.Height;
            this.Size = new Size(this.Size.Width, newY + 5);


            this.FormBorderStyle = FormBorderStyle.None;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 15, 15));
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void flowLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void makeWidget()
        {
            if (!logOff)
            {
                int width = Screen.FromControl(this).Bounds.Width;
                this.Location = new Point((width - this.Width) - 10, 10);
                this.StartPosition = FormStartPosition.Manual;
                sendToDesktop();
            }
        }

        private void sendToDesktop()
        {
            IntPtr pWnd = FindWindow("Progman", null);
            pWnd = FindWindowEx(pWnd, IntPtr.Zero, "SHELLDLL_DefVIew", null);
            pWnd = FindWindowEx(pWnd, IntPtr.Zero, "SysListView32", null);
            IntPtr tWnd = this.Handle;
            SetParent(tWnd, pWnd);
        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            signOut();
        }

        private bool checkLogin() {
            /*if (Properties.Settings.Default.userName == "" || Properties.Settings.Default.password == "" || logOff) {
                signOut();
                return false;
            }*/
            return true;
        }

        private void signOut() {
            logOff = true;
            Properties.Settings.Default.userName = "";
            Properties.Settings.Default.password = "";
            Properties.Settings.Default.Save();
            //new LoginForm().Show();
            LoginForm.currentObject.Show();
            this.Hide();
            this.Close();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (checkLogin())
            {
                populateFormData();
                //this.FormBorderStyle = FormBorderStyle.None;
                //Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 15, 15));

                //addNotification("Hello", "Hello Worldsahdasigdgcuyew\nwuheriuweruiewuirh\nohewrihweorhwe\nojfoihweoihrwfeshdjfuewhtdsfj\nsdhfuoehowihtewo!");
                //addNotification("Hello", "Hello World!");
            }
            adjustComponentPositions();
        }
    }
}
