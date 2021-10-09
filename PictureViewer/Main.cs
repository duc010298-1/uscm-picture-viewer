using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace PictureViewer
{
    public partial class Main : Form
    {
        private Bitmap sourceImg = null;
        private string pathCrop = null;
        private string currPath = null;

        private bool manualCrop = false;
        private int cropX;
        private int cropY;
        private int cropWidth;
        private int cropHeight;
        private Pen cropPen = new Pen(Color.DarkGray, 1);

        public Main()
        {
            InitializeComponent();
            buttonPrint.Enabled = false;
            buttonCropNormal.Enabled = false;
            buttonCropFace.Enabled = false;
            buttonUndo.Enabled = false;
            buttonCropManual.Enabled = false;
            buttonSelectArea.Enabled = false;
            buttonOpenPhotoshop.Enabled = false;
        }

        public Main(string fileStr)
        {
            InitializeComponent();
            FileInfo file = new FileInfo(fileStr);
            string extension = file.Extension;
            if (file.Exists && (extension.Equals(".png") || extension.Equals(".jpg") || extension.Equals(".bmp")))
            {
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                currPath = fileStr;
                pictureBox.Load(fileStr);
                pathCrop = Path.GetDirectoryName(currPath) + "\\temp.png";
                buttonPrint.Enabled = true;
                buttonOpenPhotoshop.Enabled = true;
                buttonCropNormal.Enabled = true;
                buttonCropFace.Enabled = true;
                buttonUndo.Enabled = false;
                buttonCropManual.Enabled = false;
                buttonSelectArea.Enabled = true;
            }
        }

        private void ButtonPrint_Click(object sender, EventArgs e)
        {
            Print();
        }

        private void OpenItemMenu_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                currPath = openFileDialog.FileName;
                pictureBox.Load(currPath);
                pathCrop = Path.GetDirectoryName(currPath) + "\\temp.png";
                buttonPrint.Enabled = true;
                buttonOpenPhotoshop.Enabled = true;
                buttonCropNormal.Enabled = true;
                buttonCropFace.Enabled = true;
                buttonUndo.Enabled = false;
                buttonSelectArea.Enabled = true;
                buttonCropManual.Enabled = false;
            }
        }

        private void PrinyItemMenu_Click(object sender, EventArgs e)
        {
            Print();
        }

        private void ExitItemMenu_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Print()
        {
            var p = new Process();
            if (currPath == null || pathCrop == null)
            {
                MessageBox.Show("Không có ảnh để in");
                return;
            }
            if (buttonUndo.Enabled)
            {
                p.StartInfo.FileName = pathCrop;
            }
            else
            {
                p.StartInfo.FileName = currPath;
            }
            p.StartInfo.Verb = "Print";
            p.Start();
        }

        private void ButtonCropNormal_Click(object sender, EventArgs e)
        {
            sourceImg = pictureBox.Image as Bitmap;
            Rectangle cropRect = new Rectangle(500, 105, 1005, 710);
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);
            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(sourceImg, new Rectangle(0, 0, target.Width, target.Height), cropRect, GraphicsUnit.Pixel);
            }
            target.Save(pathCrop, ImageFormat.Png);
            pictureBox.Image = target;
            buttonUndo.Enabled = true;
            buttonCropNormal.Enabled = false;
            buttonCropFace.Enabled = false;
        }

        private void ButtonCropFace_Click(object sender, EventArgs e)
        {
            sourceImg = pictureBox.Image as Bitmap;
            Rectangle cropRect = new Rectangle(530, 65, 850, 850);
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);
            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(sourceImg, new Rectangle(0, 0, target.Width, target.Height), cropRect, GraphicsUnit.Pixel);
            }
            target.Save(pathCrop, ImageFormat.Png);
            pictureBox.Image = target;
            buttonCropNormal.Enabled = false;
            buttonCropFace.Enabled = false;
            buttonUndo.Enabled = true;
        }

        private void ButtonUndo_Click(object sender, EventArgs e)
        {
            pictureBox.Image = sourceImg;
            buttonCropNormal.Enabled = true;
            buttonCropFace.Enabled = true;
            buttonUndo.Enabled = false;
        }

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (manualCrop)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    Cursor = Cursors.Cross;
                    cropX = e.X;
                    cropY = e.Y;
                    cropPen = new Pen(Color.DarkGray, 1)
                    {
                        DashStyle = DashStyle.DashDotDot
                    };
                }
                pictureBox.Refresh();
            }
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (manualCrop)
            {
                if (pictureBox.Image == null)
                {
                    return;
                }
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    pictureBox.Refresh();
                    cropWidth = e.X - cropX;
                    cropHeight = e.Y - cropY;
                    pictureBox.CreateGraphics().DrawRectangle(cropPen, cropX, cropY, cropWidth, cropHeight);
                }
            }
        }

        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (manualCrop)
            {
                pictureBox.Refresh();
                Cursor = Cursors.Default;
                cropPen = new Pen(Color.Gray, 1)
                {
                    DashStyle = DashStyle.DashDotDot
                };
                cropWidth = e.X - cropX;
                cropHeight = e.Y - cropY;
                pictureBox.CreateGraphics().DrawRectangle(cropPen, cropX, cropY, cropWidth, cropHeight);
                buttonCropManual.Enabled = true;
            }
        }

        private void ButtonSelectArea_Click(object sender, EventArgs e)
        {
            pictureBox.Refresh();
            manualCrop = !manualCrop;
            buttonCropManual.Enabled = !buttonCropManual.Enabled;
            if (manualCrop)
            {
                buttonSelectArea.BackColor = Color.DarkGray;
            }
            else
            {
                buttonSelectArea.BackColor = default;
            }
            buttonCropManual.Enabled = false;
        }

        private void ButtonCropManual_Click(object sender, EventArgs e)
        {
            manualCrop = false;
            buttonSelectArea.BackColor = default;
            pictureBox.Refresh();
            buttonCropManual.Enabled = false;

            sourceImg = pictureBox.Image as Bitmap;

            Size clientSize = pictureBox.Size;
            float ratioWidth = ((float)sourceImg.Width) / ((float)clientSize.Width);
            float ratioHeight = ((float)sourceImg.Height) / ((float)clientSize.Height);
            cropX = (int)(Math.Round(cropX * ratioWidth));
            cropY = (int)(Math.Round(cropY * ratioHeight));
            cropWidth = (int)(Math.Round(cropWidth * ratioWidth));
            cropHeight = (int)(Math.Round(cropHeight * ratioHeight));

            Rectangle cropRect = new Rectangle(cropX, cropY, cropWidth, cropHeight);
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);
            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(sourceImg, new Rectangle(0, 0, target.Width, target.Height), cropRect, GraphicsUnit.Pixel);
            }
            target.Save(pathCrop, ImageFormat.Png);
            pictureBox.Image = target;
            buttonCropNormal.Enabled = false;
            buttonCropFace.Enabled = false;
            buttonUndo.Enabled = true;
        }

        private void ButtonOpenPhotoshop_Click(object sender, EventArgs e)
        {
            var subKey = Registry.CurrentUser.OpenSubKey(@"Software\Classes\Applications\PictureViewer.exe\PhotoshopLocation", true);
            if (subKey == null)
            {
                MessageBox.Show("Bạn chưa thiết lập vị trí của photoshop",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }
            string photoshopLocation = (string)subKey.GetValue("path");
            if (!File.Exists(photoshopLocation))
            {
                MessageBox.Show("Đường dẫn photoshop không tồn tại, vui lòng đặt lại",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }
            if (currPath == null || pathCrop == null)
            {
                MessageBox.Show("Không có ảnh để mở");
                return;
            }

            var p = new Process();
            p.StartInfo.FileName = photoshopLocation;
            if (buttonUndo.Enabled)
            {
                p.StartInfo.Arguments = pathCrop;
            }
            else
            {
                p.StartInfo.Arguments = currPath;
            }
            p.Start();
        }

        private void SetPhotoshopLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Registry.CurrentUser.OpenSubKey(@"Software\Classes\Applications\PictureViewer.exe\PhotoshopLocation", true)
                    .SetValue("path", openFileDialog.FileName);
                MessageBox.Show("Thiết lập vị trí của photoshop thành công",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (File.Exists(pathCrop))
            {
                File.Delete(pathCrop);
            }
        }
    }
}
