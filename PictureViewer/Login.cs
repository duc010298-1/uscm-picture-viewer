using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows.Forms;

namespace PictureViewer
{
    public partial class Login : Form
    {
        private readonly string AUTH_ENDPOINT = "https://phongkhamsan158.ddns.net/uscm-api/api/token/";

        public Login()
        {
            InitializeComponent();
        }

        private async void LoginButton_Click(object sender, EventArgs e)
        {
            DisableAll();
            string username = usernameTextbox.Text.Trim();
            string password = passwordTextbox.Text.Trim();
            try
            {
                var values = new Dictionary<string, string> {
                        { "username", username },
                        { "password", password }
                    };
                var content = new FormUrlEncodedContent(values);
                var response = await Main.httpClient.PostAsync(AUTH_ENDPOINT, content);
                string code = response.StatusCode.ToString();
                var responseString = await response.Content.ReadAsStringAsync();
                if (code != "OK")
                {
                    MessageBox.Show("Tên đăng nhập hoặc mật khẩu không đúng",
                        "Lỗi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    EnableAll();
                }
                else
                {
                    var credential = JObject.Parse(responseString);
                    Main.httpClient.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", credential["access"]));
                    Registry.CurrentUser.OpenSubKey(@"Software\Classes\Applications\PictureViewer.exe\Credential", true)
                        .SetValue("token", credential["access"]);
                    MessageBox.Show("Đăng nhập thành công",
                        "Thông báo",
                        MessageBoxButtons.OK
                    );
                    this.Close();
                }
            }
            catch
            {
                MessageBox.Show("Không thể kết nối đến máy chủ, vui lòng thử lại",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                EnableAll();
            }
        }

        private void DisableAll()
        {
            usernameTextbox.Enabled = false;
            passwordTextbox.Enabled = false;
            loginButton.Enabled = false;
        }

        private void EnableAll()
        {
            usernameTextbox.Enabled = true;
            passwordTextbox.Enabled = true;
            loginButton.Enabled = true;
        }

        private void UsernameTextbox_KeyUp(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Enter) || (e.KeyCode == Keys.Return))
            {
                this.SelectNextControl((Control)sender, true, true, true, true);
            }
        }

        private void PasswordTextbox_KeyUp(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Enter) || (e.KeyCode == Keys.Return))
            {
                LoginButton_Click(sender, e);
            }
        }
    }
}
