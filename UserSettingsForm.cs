using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace OperatorEntryApp
{
    public partial class UserSettingsForm : Form
    {
        private int _userId;
        private string _fullName;
        private string _role;
        private string connectionString = ConfigurationManager.ConnectionStrings["Haier_DB"].ConnectionString;
        MainForm _mainForm;

        public UserSettingsForm(MainForm mainForm, int userId, string fullName, string role)
        {
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeComponent();
            _mainForm = mainForm;
            _userId = userId;
            _fullName = fullName;
            _role = role;
            this.KeyPreview = true; // Form klavye olaylarını alacak
            this.KeyDown += UserSettingsForm_KeyDown;
        }

        private void UserSettingsForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) // Enter’a basıldığında buton click’leri ya da form gönderimi gibi hiçbir şey gerçekleşmez.
            {
                e.Handled = true;
                e.SuppressKeyPress = true; // Enter bastığında hiçbir şey yapma
            }
        }

        private void UserSettingsForm_Load(object sender, EventArgs e)
        {
            txtBoxCurrentPassword.UseSystemPasswordChar = true;
            txtBoxNewPassword.UseSystemPasswordChar = true;
            txtBoxUserNewPassword.UseSystemPasswordChar = true;

            if (_role.ToLower() == "operator")
            {
                cmbUserSelection.Visible = false;
                txtBoxUserNewPassword.Visible = false;
                btnUserSave.Visible = false;
            }
            else
            {
                LoadUsersToComboBox();
            }
        }

        private void LoadUsersToComboBox()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT UserId, FullName, Role FROM Users";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string name = reader.GetString(1);
                            string role = reader.GetString(2);
                            cmbUserSelection.Items.Add($"{name} (Id: {id}, Rol: {role})");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kullanıcılar yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            _mainForm.Show();
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string currentPassword = txtBoxCurrentPassword.Text.Trim();
            string newPassword = txtBoxNewPassword.Text.Trim();

            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword))
            {
                MessageBox.Show("Lütfen tüm alanları doldurunuz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newPassword.Length < 6)
            {
                MessageBox.Show("Yeni şifre en az 6 karakter olmalıdır.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string hashedCurrentPassword = ComputeSha256Hash(currentPassword);
            string hashedNewPassword = ComputeSha256Hash(newPassword);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE UserId = @userId AND Password = @currentPassword";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@userId", _userId);
                        checkCmd.Parameters.AddWithValue("@currentPassword", hashedCurrentPassword);
                        int count = (int)checkCmd.ExecuteScalar();
                        if (count == 0)
                        {
                            MessageBox.Show("Mevcut şifre hatalı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            txtBoxCurrentPassword.Clear();
                            txtBoxCurrentPassword.Focus();
                            return;
                        }
                    }

                    string updateQuery = "UPDATE Users SET Password = @newPassword WHERE UserId = @userId";
                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                    {
                        updateCmd.Parameters.AddWithValue("@newPassword", hashedNewPassword);
                        updateCmd.Parameters.AddWithValue("@userId", _userId);

                        int rowsAffected = updateCmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Şifre başarıyla güncellendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.Close();
                            _mainForm.Show();
                        }
                        else
                        {
                            MessageBox.Show("Şifre güncellenemedi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUserSave_Click(object sender, EventArgs e)
        {
            if (cmbUserSelection.SelectedItem == null || string.IsNullOrWhiteSpace(txtBoxUserNewPassword.Text))
            {
                MessageBox.Show("Lütfen kullanıcı seçin ve yeni şifreyi girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string newPassword = txtBoxUserNewPassword.Text.Trim();
            if (newPassword.Length < 6)
            {
                MessageBox.Show("Yeni şifre en az 6 karakter olmalıdır.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedText = cmbUserSelection.SelectedItem.ToString();
            int start = selectedText.IndexOf("Id: ") + 4;
            int end = selectedText.IndexOf(", Rol:");
            string idPart = selectedText.Substring(start, end - start);
            int selectedUserId = int.Parse(idPart);

            string hashedPassword = ComputeSha256Hash(newPassword);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string updateQuery = "UPDATE Users SET Password = @password WHERE UserId = @userId";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@password", hashedPassword);
                        cmd.Parameters.AddWithValue("@userId", selectedUserId);
                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                            MessageBox.Show("Kullanıcının şifresi başarıyla güncellendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        else
                            MessageBox.Show("Şifre güncellenemedi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }
    }
}
