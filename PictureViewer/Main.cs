using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PictureViewer
{
    public partial class Main : Form
    {
        private readonly string TEMP_PATH_FILE = Path.Combine(Path.GetTempPath(), "temp.png");
        private readonly string UPLOAD_IMAGE_URL = "http://localhost:8000/common/new-image/";
        public static readonly HttpClient httpClient = new HttpClient();
        private readonly Stack<Bitmap> stackImage = new Stack<Bitmap>();
        private string currentImageDir;
        private bool isImageLoaded = false;

        private bool manualCrop = false;
        private int cropX;
        private int cropY;
        private int cropWidth;
        private int cropHeight;
        private Pen cropPen;

        private readonly Font drawFont = new Font("Segoe UI", 24);
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
            SetHeader();
        }

        public Main(string fileStr)
        {
            InitializeComponent();
            currentImageDir = fileStr;
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
            SetHeader();
        }

        private void SetHeader()
        {
            var subKey = Registry.CurrentUser.OpenSubKey(@"Software\Classes\Applications\PictureViewer.exe\Credential", true);
            if (subKey == null)
            {
                return;
            }
            string token = (string)subKey.GetValue("token");
            Main.httpClient.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", token));
        }


        private void OpenItemMenu_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                currentImageDir = openFileDialog.FileName;
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
                    AddNote();
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
                string text = Clipboard.GetText(TextDataFormat.UnicodeText);
                text = ProcessTextCopy(text);
                textBoxNote.Text = text;
                AddNote();
            }
        }

        private string ProcessTextCopy(string text)
        {
            string regex = @"^.+ \| pks[0-9]{10}$";
            if (Regex.IsMatch(text, regex))
            {
                SendRequestUpload(text.Substring(text.Length - 13, 13));
            }
            return text.Substring(0, text.Length - 16).Trim();
        }

        private async void SendRequestUpload(string code)
        {
            using (var formData = new MultipartFormDataContent())
            {
                formData.Add(new StringContent(code), "code");
                formData.Add(new ByteArrayContent(File.ReadAllBytes(currentImageDir)), "file", Path.GetFileName(currentImageDir));
                await Main.httpClient.PostAsync(UPLOAD_IMAGE_URL, formData);
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
            AddNote();
        }

        private void AddNote()
        {
            String name = textBoxNote.Text;
            String date = DateTime.Now.ToString("dd/MM/yy");
            Bitmap currentImage = pictureBox.Image as Bitmap;
            stackImage.Push(currentImage);
            Bitmap target = (Bitmap)currentImage.Clone();
            using (Graphics g = Graphics.FromImage(target))
            {
                // draw name
                SizeF nameSize = g.MeasureString(name, drawFont);
                int rectNameWidth = (int)nameSize.Width + 6;
                Rectangle nameRect = new Rectangle(5, 8, rectNameWidth, 48);
                g.FillRectangle(blueBrushFillText, nameRect);
                g.DrawRectangle(penBorderText, nameRect);
                g.DrawString(name, drawFont, brushText, new RectangleF(8, 8, rectNameWidth, 48));

                // draw date
                SizeF dateSize = g.MeasureString(date, drawFont);
                int rectDateWidth = (int)dateSize.Width + 6;
                int xPointDate = currentImage.Width - rectDateWidth - 5;
                Rectangle dateRect = new Rectangle(xPointDate, 8, rectDateWidth, 48);
                g.FillRectangle(blueBrushFillText, dateRect);
                g.DrawRectangle(penBorderText, dateRect);
                g.DrawString(date, drawFont, brushText, new RectangleF(xPointDate + 3, 8, rectDateWidth, 48));
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

        private void LoginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Login loginForm = new Login();
            loginForm.ShowDialog();
        }
    }
}
