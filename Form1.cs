using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading;
using System.IO;
using Microsoft.Win32;

namespace Slide
{
    public partial class Form1 : Form
    {
        List<string> filename = new List<string> { };
        Thread Slide;
        int curr_x, curr_y;
        bool isWndMove = false;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            disableToolStripMenuItem_Click(sender,e);
            Form.CheckForIllegalCrossThreadCalls = false;
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            this.FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.Lime;
            TransparencyKey = Color.Lime;

            this.Width = Properties.Settings.Default.FormW;
            this.Height = Properties.Settings.Default.FormH;

            //RegistryKey Reg = Registry.CurrentUser.OpenSubKey("Software", true);
            /*if (!Reg.GetSubKeyNames().Contains("Slide"))
                Reg.CreateSubKey("Slide");*/
            //Registry.SetValue("HKEY_CURRENT_USER\\Software\\Slide", "path", path.SelectedPath, RegistryValueKind.String);
            //Convert.ToString(Registry.GetValue("HKEY_CURRENT_USER\\Software\\Slide", "path", ""));
            //Reg.Close();

            FolderBrowserDialog path = new FolderBrowserDialog();
            if (!Directory.Exists(Properties.Settings.Default.path))
            {
                while (true)
                {
                    if (path.ShowDialog() == DialogResult.OK)
                    {
                        Properties.Settings.Default.path = path.SelectedPath;
                        Properties.Settings.Default.Save();

                        if (filename.Count() == 0)
                        {
                            filename.Clear();
                            serchfile(path.SelectedPath);
                        }

                        if (filename.Count() != 0)
                        {
                            SlideTh(path.SelectedPath);
                            break;
                        }
                    }
                    else this.Close();
                }
            }
            else
            {
                while (true)
                {
                    filename.Clear();
                    serchfile(Properties.Settings.Default.path);
                    if (filename.Count() == 0)
                        if (path.ShowDialog() == DialogResult.OK)
                        {
                            Properties.Settings.Default.path = path.SelectedPath;
                            Properties.Settings.Default.Save();
                        }
                        else break;

                    if (filename.Count() != 0)
                    {
                        SlideTh(Properties.Settings.Default.path);
                        break;
                    }
                }
            }
        }

        private void serchfile(string path)
        {
            foreach (string file in Directory.GetDirectories(path))
                serchfile(file);
            foreach (string file in Directory.GetFiles(path))
                if (file.ToLower().IndexOf(".jpg") != -1)
                    filename.Add(file);
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.curr_x = e.X;
                this.curr_y = e.Y;
                this.isWndMove = true;
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.isWndMove)
                this.Location = new Point(this.Left + e.X - this.curr_x, this.Top + e.Y - this.curr_y);
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            this.isWndMove = false;
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (Slide != null && Slide.IsAlive)
                Slide.Abort();
            this.Close();
        }

        private void enableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            enableToolStripMenuItem.Checked = true;
            disableToolStripMenuItem.Checked = false;
            if (!disableToolStripMenuItem.Checked && enableToolStripMenuItem.Checked)
                this.FormBorderStyle = FormBorderStyle.Sizable;
        }

        private void disableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            enableToolStripMenuItem.Checked = false;
            disableToolStripMenuItem.Checked = true;
            if (disableToolStripMenuItem.Checked && !enableToolStripMenuItem.Checked)
                this.FormBorderStyle = FormBorderStyle.None;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Slide != null && Slide.IsAlive)
                Slide.Abort();
        }

        private void folderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog path = new FolderBrowserDialog();
            while (true)
                if (path.ShowDialog() == DialogResult.OK)
                {
                    if (Slide != null && Slide.IsAlive)
                        Slide.Abort();
                    Properties.Settings.Default.path = path.SelectedPath;
                    Properties.Settings.Default.Save();

                    if (filename.Count() == 0)
                    {
                        filename.Clear();
                        serchfile(path.SelectedPath);
                    }

                    if (filename.Count() != 0)
                    {
                        SlideTh(path.SelectedPath);
                        break;
                    }
                }
                else break;
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            Properties.Settings.Default.FormW = this.Width;
            Properties.Settings.Default.FormH = this.Height;
            Properties.Settings.Default.Save();
        }

        private void SlideTh(string path)
        {
            Slide = new Thread(() =>
            {
                while (true)
                {
                    if (filename.Count() == 0)
                        serchfile(path);
                    else
                    {
                        int rNumber = new Random().Next(0, filename.Count());
                        pictureBox1.Image = Image.FromFile(filename[rNumber]);
                        filename.RemoveAt(rNumber);
                        Thread.Sleep(5000);
                        pictureBox1.Image.Dispose();
                    }
                }
            });
            Slide.Start();
        }
    }
}
