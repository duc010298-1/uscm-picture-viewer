using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
        private readonly string TEMP_PATH_FILE = Path.Combine(Path.GetTempPath(), "temp.png");
        private readonly Stack<Bitmap> stackImage = new Stack<Bitmap>();
        private bool isImageLoaded = false;

        private bool manualCrop = false;
        private int cropX;
        private int cropY;
        private int cropWidth;
        private int cropHeight;
        private Pen cropPen;

        private readonly Font drawFont = new Font("Segoe UI", 21);
        private readonly SolidBrush blueBrushFillText = new SolidBrush(Color.FromArgb(3, 3, 3));
        private readonly Pen penBorderText = new Pen(Color.FromArgb(83, 83, 83), 2);
        private readonly Brush brushText = new SolidBrush(Color.FromArgb(45, 210, 51));

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
            textBoxNote.Enabled = false;
            buttonPaste.Enabled = false;
            buttonAddNote.Enabled = false;
        }

        public Main(string fileStr)
        {
            InitializeComponent();
            FileInfo file = new FileInfo(fileStr);
            string extension = file.Extension;
            if (file.Exists && (extension.Equals(".png") || extension.Equals(".jpg") || extension.Equals(".bmp")))
            {
                isImageLoaded = true;
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox.Load(fileStr);
                buttonPrint.Enabled = true;
                buttonOpenPhotoshop.Enabled = true;
                buttonCropNormal.Enabled = true;
                buttonCropFace.Enabled = true;
                buttonUndo.Enabled = false;
                buttonCropManual.Enabled = false;
                buttonSelectArea.Enabled = true;
                textBoxNote.Enabled = true;
                buttonPaste.Enabled = true;
                buttonAddNote.Enabled = true;
            }
        }


        private void OpenItemMenu_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                isImageLoaded = true;
                stackImage.Clear();
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox.Load(openFileDialog.FileName);
                buttonPrint.Enabled = true;
                buttonOpenPhotoshop.Enabled = true;
                buttonCropNormal.Enabled = true;
                buttonCropFace.Enabled = true;
                buttonUndo.Enabled = false;
                buttonSelectArea.Enabled = true;
                buttonCropManual.Enabled = false;
                textBoxNote.Enabled = true;
                buttonPaste.Enabled = true;
                buttonAddNote.Enabled = true;
                textBoxNote.Text = "";
            }
        }

        private void ButtonPrint_Click(object sender, EventArgs e)
        {
            Print();
        }

        private void PrintItemMenu_Click(object sender, EventArgs e)
        {
            Print();
        }

        private void ExitItemMenu_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void CreateTempFile()
        {
            Bitmap target = pictureBox.Image as Bitmap;
            target.Save(TEMP_PATH_FILE, ImageFormat.Png);
        }

        private void Print()
        {
            var p = new Process();
            if (!isImageLoaded)
            {
                MessageBox.Show("Không có ảnh để in");
                return;
            }
            CreateTempFile();
            p.StartInfo.FileName = TEMP_PATH_FILE;
            p.StartInfo.Verb = "Print";
            p.Start();
        }

        private void ButtonCropNormal_Click(object sender, EventArgs e)
        {
            Bitmap currentImage = pictureBox.Image as Bitmap;
            stackImage.Push(currentImage);
            Rectangle cropRect = new Rectangle(500, 105, 1005, 710);
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);
            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(currentImage, new Rectangle(0, 0, target.Width, target.Height), cropRect, GraphicsUnit.Pixel);
            }
            pictureBox.Image = target;
            buttonUndo.Enabled = true;
            buttonCropNormal.Enabled = false;
            buttonCropFace.Enabled = false;
        }

        private void ButtonCropFace_Click(object sender, EventArgs e)
        {
            Bitmap currentImage = pictureBox.Image as Bitmap;
            stackImage.Push(currentImage);
            Rectangle cropRect = new Rectangle(530, 65, 850, 850);
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);
            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(currentImage, new Rectangle(0, 0, target.Width, target.Height), cropRect, GraphicsUnit.Pixel);
            }
            pictureBox.Image = target;
            buttonCropNormal.Enabled = false;
            buttonCropFace.Enabled = false;
            buttonUndo.Enabled = true;
        }

        private void TextBoxNote_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string text = textBoxNote.Text.Trim();
                if (!String.IsNullOrEmpty(text))
                {
                    AddNote(text);
                }
            }
        }

        private void ButtonPaste_Click(object sender, EventArgs e)
        {
            Paste();
        }

        private void Paste()
        {
            if (Clipboard.ContainsText(TextDataFormat.Text) && isImageLoaded)
            {
                string clipboardText = Clipboard.GetText(TextDataFormat.UnicodeText);
                textBoxNote.Text = clipboardText;
                AddNote(clipboardText);
            }
        }

        private void ButtonAddNote_Click(object sender, EventArgs e)
        {
            string text = textBoxNote.Text.Trim();
            if (String.IsNullOrEmpty(text))
            {
                MessageBox.Show("Không có ghi chú để thêm");
                return;
            }
            AddNote(text);
        }

        private void AddNote(string text)
        {
            Bitmap currentImage = pictureBox.Image as Bitmap;
            stackImage.Push(currentImage);
            Bitmap target = (Bitmap)currentImage.Clone();
            using (Graphics g = Graphics.FromImage(target))
            {
                SizeF stringSize = g.MeasureString(text, drawFont);
                int rectWidth = (int)stringSize.Width + 6;
                Rectangle textRect = new Rectangle(5, 8, rectWidth, 40);
                g.FillRectangle(blueBrushFillText, textRect);
                g.DrawRectangle(penBorderText, textRect);
                g.DrawString(text, drawFont, brushText, new RectangleF(8, 8, rectWidth, 40));
            }
            pictureBox.Image = target;
            buttonUndo.Enabled = true;
        }

        private void ButtonUndo_Click(object sender, EventArgs e)
        {
            pictureBox.Image = stackImage.Pop();
            if (stackImage.Count == 0)
            {
                buttonCropNormal.Enabled = true;
                buttonCropFace.Enabled = true;
                buttonUndo.Enabled = false;
            }
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

            Bitmap currentImage = pictureBox.Image as Bitmap;
            stackImage.Push(currentImage);

            Size clientSize = pictureBox.Size;
            float ratioWidth = ((float)currentImage.Width) / ((float)clientSize.Width);
            float ratioHeight = ((float)currentImage.Height) / ((float)clientSize.Height);
            cropX = (int)(Math.Round(cropX * ratioWidth));
            cropY = (int)(Math.Round(cropY * ratioHeight));
            cropWidth = (int)(Math.Round(cropWidth * ratioWidth));
            cropHeight = (int)(Math.Round(cropHeight * ratioHeight));

            Rectangle cropRect = new Rectangle(cropX, cropY, cropWidth, cropHeight);
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);
            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(currentImage, new Rectangle(0, 0, target.Width, target.Height), cropRect, GraphicsUnit.Pixel);
            }
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
            if (!isImageLoaded)
            {
                MessageBox.Show("Không có ảnh để mở");
                return;
            }

            var p = new Process();
            p.StartInfo.FileName = photoshopLocation;
            CreateTempFile();
            p.StartInfo.Arguments = TEMP_PATH_FILE;
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
            if (File.Exists(TEMP_PATH_FILE))
            {
                File.Delete(TEMP_PATH_FILE);
            }
        }
    }
}
