using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PictureViewer
{
    public partial class Main : Form
    {
        private Bitmap sourceImg = null;
        private String pathTemp = null;
        private String currPath = null;

        bool manualCrop = false;
        int cropX;
        int cropY;
        int cropWidth;
        int cropHeight;
        public Pen cropPen = new Pen(Color.DarkGray, 1);
        public DashStyle cropDashStyle = DashStyle.DashDot;

        public Main()
        {
            InitializeComponent();
            buttonPrint.Enabled = false;
            buttonCropNormal.Enabled = false;
            buttonCropFace.Enabled = false;
            buttonUndo.Enabled = false;
            buttonCropManual.Enabled = false;
            buttonSelectArea.Enabled = false;
        }

        public Main(String fileStr)
        {
            InitializeComponent();
            FileInfo file = new FileInfo(fileStr);
            string extension = file.Extension;
            if (file.Exists && (extension.Equals(".png") || extension.Equals(".jpg") || extension.Equals(".bmp")))
            {
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                currPath = fileStr;
                pictureBox.Load(fileStr);
                pathTemp = Path.GetDirectoryName(fileStr) + "\\temp.png";
                buttonPrint.Enabled = true;
                buttonCropNormal.Enabled = true;
                buttonCropFace.Enabled = true;
                buttonUndo.Enabled = false;
                buttonCropManual.Enabled = false;
                buttonSelectArea.Enabled = true;
            }
        }

        private void buttonPrint_Click(object sender, EventArgs e)
        {
            print();
        }

        private void openItemMenu_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                currPath = openFileDialog.FileName;
                pictureBox.Load(currPath);
                pathTemp = Path.GetDirectoryName(currPath) + "\\temp.png";
                buttonPrint.Enabled = true;
                buttonCropNormal.Enabled = true;
                buttonCropFace.Enabled = true;
                buttonUndo.Enabled = false;
                buttonSelectArea.Enabled = true;
                buttonCropManual.Enabled = false;
            }
        }

        private void prinyItemMenu_Click(object sender, EventArgs e)
        {
            print();
        }

        private void exitItemMenu_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void print()
        {
            var p = new Process();
            if (currPath == null || pathTemp == null)
            {
                MessageBox.Show("Không có ảnh để in");
                return;
            }
            if (buttonUndo.Enabled)
            {
                p.StartInfo.FileName = pathTemp;
            }
            else
            {
                p.StartInfo.FileName = currPath;
            }
            p.StartInfo.Verb = "Print";
            p.Start();
        }

        private void buttonCropNormal_Click(object sender, EventArgs e)
        {
            sourceImg = pictureBox.Image as Bitmap;
            Rectangle cropRect = new Rectangle(500, 105, 1005, 676);
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);
            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(sourceImg, new Rectangle(0, 0, target.Width, target.Height), cropRect, GraphicsUnit.Pixel);
            }
            target.Save(pathTemp, ImageFormat.Png);
            pictureBox.Image = target;
            buttonUndo.Enabled = true;
            buttonCropNormal.Enabled = false;
            buttonCropFace.Enabled = false;
        }

        private void buttonCropFace_Click(object sender, EventArgs e)
        {
            sourceImg = pictureBox.Image as Bitmap;
            Rectangle cropRect = new Rectangle(530, 65, 850, 850);
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);
            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(sourceImg, new Rectangle(0, 0, target.Width, target.Height), cropRect, GraphicsUnit.Pixel);
            }
            target.Save(pathTemp, ImageFormat.Png);
            pictureBox.Image = target;
            buttonCropNormal.Enabled = false;
            buttonCropFace.Enabled = false;
            buttonUndo.Enabled = true;
        }

        private void buttonUndo_Click(object sender, EventArgs e)
        {
            pictureBox.Image = sourceImg;
            buttonCropNormal.Enabled = true;
            buttonCropFace.Enabled = true;
            buttonUndo.Enabled = false;
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (File.Exists(pathTemp))
            {
                File.Delete(pathTemp);
            }
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (manualCrop)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    Cursor = Cursors.Cross;
                    cropX = e.X;
                    cropY = e.Y;
                    cropPen = new Pen(Color.DarkGray, 1);
                    cropPen.DashStyle = DashStyle.DashDotDot;
                }
                pictureBox.Refresh();
            }
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
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

        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (manualCrop)
            {
                pictureBox.Refresh();
                Cursor = Cursors.Default;
                cropPen = new Pen(Color.Gray, 1);
                cropPen.DashStyle = DashStyle.DashDotDot;
                cropWidth = e.X - cropX;
                cropHeight = e.Y - cropY;
                pictureBox.CreateGraphics().DrawRectangle(cropPen, cropX, cropY, cropWidth, cropHeight);
                buttonCropManual.Enabled = true;
            }
        }

        private void buttonSelectArea_Click(object sender, EventArgs e)
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
                buttonSelectArea.BackColor = default(Color);
            }
            buttonCropManual.Enabled = false;
        }

        private void buttonCropManual_Click(object sender, EventArgs e)
        {
            manualCrop = false;
            buttonSelectArea.BackColor = default(Color);
            pictureBox.Refresh();
            buttonCropManual.Enabled = false;

            sourceImg = pictureBox.Image as Bitmap;

            Size clientSize = pictureBox.ClientSize;
            float ratio = ((float)sourceImg.Width) / ((float)clientSize.Width);
            cropX = (int)(cropX * ratio);
            cropY = (int)(cropY * ratio);
            cropWidth = (int)(cropWidth * ratio);
            cropHeight = (int)(cropHeight * ratio);

            Rectangle cropRect = new Rectangle(cropX, cropY, cropWidth, cropHeight);
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);
            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(sourceImg, new Rectangle(0, 0, target.Width, target.Height), cropRect, GraphicsUnit.Pixel);
            }
            target.Save(pathTemp, ImageFormat.Png);
            pictureBox.Image = target;
            buttonCropNormal.Enabled = false;
            buttonCropFace.Enabled = false;
            buttonUndo.Enabled = true;
        }
    }
}
