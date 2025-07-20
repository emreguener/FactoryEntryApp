using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace OperatorEntryApp
{
    public partial class RegisterForm : Form
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["Haier_DB"].ConnectionString;

        private MainForm _mainForm;  // back için

        public RegisterForm(MainForm mainForm)
        {
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeComponent();
            _mainForm = mainForm;
        }


        private void RegisterForm_Load(object sender, EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = true; // Şifre alanında yıldız karakter kullanımı
            cmbRole.Items.Add("engineer");
            cmbRole.Items.Add("operator");
            cmbRole.Items.Add("supervisor");
            cmbRole.SelectedIndex = 1; // default operator
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string fullName = txtFullName.Text.Trim();
            string password = txtPassword.Text.Trim();
            string role = cmbRole.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(role))
            {
                lblStatus.Text = "Lütfen tüm alanları doldurun.";
                lblStatus.ForeColor = System.Drawing.Color.Red;
                return;
            }

            if (password.Length < 6)
            {
                lblStatus.Visible = true;
                lblStatus.Text = "Şifre en az 6 karakter olmalıdır.";
                lblStatus.ForeColor = System.Drawing.Color.Red;
                return;
            }

            string hashedPassword = ComputeSha256Hash(password);

            try
            {
                int newUserId;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "INSERT INTO Users (FullName, Password, Role) VALUES (@fullName, @password, @role); SELECT SCOPE_IDENTITY();";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@fullName", fullName);
                        cmd.Parameters.AddWithValue("@password", hashedPassword);
                        cmd.Parameters.AddWithValue("@role", role);

                        object result = cmd.ExecuteScalar();
                        newUserId = Convert.ToInt32(result);
                    }
                }

                lblStatus.Visible = true;
                lblStatus.Text = $"Kullanıcı başarıyla kaydedildi. Yeni ID: {newUserId}";
                lblStatus.ForeColor = System.Drawing.Color.Green;

                txtFullName.Clear();
                txtPassword.Clear();
                cmbRole.SelectedIndex = 1;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Hata: " + ex.Message;
                lblStatus.ForeColor = System.Drawing.Color.Red;
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

        private void btnBack_Click(object sender, EventArgs e)
        {
            _mainForm.Show();  // Ana formu tekrar göster
            this.Close();      // Kayıt formunu kapat
        }
    }
}
