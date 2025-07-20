using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
namespace OperatorEntryApp
{
    public partial class LoginForm : Form
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["Haier_DB"].ConnectionString;


        public LoginForm()
        {
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeComponent();
            txtPassword.UseSystemPasswordChar = true;
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            int userId;
            string password = txtPassword.Text.Trim();

            if (!int.TryParse(txtUserId.Text.Trim(), out userId))
            {
                lblMessage.Text = "Kullanıcı ID'si bir sayı olmalıdır.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return; //Hatalı girişte metot sonlanmalı dbye gönderilmemeli
            }

            // Şifreyi hashle
            string hashedPassword = ComputeSha256Hash(password);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = "SELECT FullName, Role FROM Users WHERE UserId = @UserId AND Password = @Password";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@Password", hashedPassword); // HASH EDİLMİŞ ŞİFREYİ DBYE GÖNDER

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) // başarılı giriş
                        {
                            string fullName = reader.GetString(0);
                            string role = reader.GetString(1);
                            Logger.Log($"Login successful", userId);
                            MainForm mainForm = new MainForm(this, userId, fullName, role);
                            mainForm.Show();
                            this.Hide();
                        }
                        else // başarısız giriş
                        {
                            Logger.Log("Login failed", userId);

                            txtUserId.Clear();
                            txtPassword.Clear();
                            txtUserId.Focus();

                            lblMessage.Visible = true;
                            lblMessage.Text = "Giriş başarısız. Geçersiz ID veya şifre.";
                            lblMessage.ForeColor = System.Drawing.Color.Red;
                        }
                    }
                }
            }
        }


        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }
        private bool isPasswordVisible = false;
        private void picShowPassword_Click(object sender, EventArgs e)
        {
            isPasswordVisible = !isPasswordVisible;
            txtPassword.UseSystemPasswordChar = !isPasswordVisible;
        }

        private void LoginForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnLogin.PerformClick(); // Butona tıklanmış gibi davranır enter'e basınca
            }
         
        }

        private void txtUserId_Enter(object sender, EventArgs e)
        {
            if (txtUserId.Text == "Kullanıcı ID'sini girin")
            {
                txtUserId.Text = "";
                txtUserId.ForeColor = Color.Black;
            }
        }

        private void ttxtUserId_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUserId.Text))
            {
                txtUserId.Text = "Kullanıcı ID'sini girin";
                txtUserId.ForeColor = Color.Gray;
            }
        }

        private void txtPassword_Enter(object sender, EventArgs e)
        {
            if (txtPassword.Text == "Şifreyi girin")
            {
                txtPassword.Text = "";
                txtPassword.ForeColor = Color.Black;
                txtPassword.UseSystemPasswordChar = true; // gerçek şifre görünümü
            }
        }

        private void txtPassword_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                txtPassword.UseSystemPasswordChar = false; // tekrar placeholder için kapat
                txtPassword.Text = "Şifreyi girin";
                txtPassword.ForeColor = Color.Gray;
            }
        }

        private bool isMousePressed = false; // şifre gösterme için fare basılı mı kontrolü
        private void picShowPassword_MouseDown(object sender, MouseEventArgs e) // Fare basıldığında şifreyi göster
        {
            if (txtPassword.Text != "Şifreyi girin")
            {
                isMousePressed = true;
                txtPassword.UseSystemPasswordChar = false;
            }
        }

        private void picShowPassword_MouseUp(object sender, MouseEventArgs e) // Fare bırakıldığında şifreyi gizle
        {
            isMousePressed = false; 
            if (txtPassword.Text != "Şifreyi girin")
                txtPassword.UseSystemPasswordChar = true;
        }

        private void picShowPassword_MouseMove(object sender, MouseEventArgs e) // Fare hareket ettiğinde şifreyi kontrol et eğer fare basılıysa ve fare PictureBox'ın dışında ise şifreyi gizle
        {
            if (isMousePressed)
            {
                // PictureBox'ın sınırları içinde mi
                Point cursorPos = picShowPasswordIcon.PointToClient(Cursor.Position);
                if (cursorPos.X < 0 || cursorPos.X > picShowPasswordIcon.Width || // pozisyonda sol üst köşe baz alınır (0,0)
                    cursorPos.Y < 0 || cursorPos.Y > picShowPasswordIcon.Height)
                {
                    isMousePressed = false;
                    if (txtPassword.Text != "Şifreyi girin")
                        txtPassword.UseSystemPasswordChar = true;
                }
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit(); // Uygulamadan çık
        }
    }
}
