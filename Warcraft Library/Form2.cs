using System;
using System.Drawing;
using System.Windows.Forms;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Warcraft_Library
{
    public partial class Form2 : Form
    {
        private Label lblTitle;
        private Label lblUser;
        private Label lblPass;
        private Label lblConfirm;
        private TextBox txtUser;
        private TextBox txtPass;
        private TextBox txtConfirm;
        private Button btnSignUp;

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            CreateSignUpUI();
            ApplyTheme();
        }

        private void CreateSignUpUI()
        {
            lblTitle = new Label
            {
                Text = "🛡️ CREATE ACCOUNT 🛡️",
                Font = new Font("Georgia", 28, FontStyle.Bold),
                ForeColor = Color.Gold,
                Dock = DockStyle.Top,
                Height = 100,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblTitle);

            lblUser = new Label
            {
                Text = "Username:",
                ForeColor = Color.Goldenrod,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(150, 150),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblUser);

            txtUser = new TextBox
            {
                Location = new Point(300, 150),
                Width = 250,
                Height = 35,
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                BackColor = Color.Black,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(txtUser);

            lblPass = new Label
            {
                Text = "Password:",
                ForeColor = Color.Goldenrod,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(150, 210),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblPass);

            txtPass = new TextBox
            {
                Location = new Point(300, 210),
                Width = 250,
                Height = 35,
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                BackColor = Color.Black,
                ForeColor = Color.White,
                UseSystemPasswordChar = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(txtPass);

            lblConfirm = new Label
            {
                Text = "Confirm Password:",
                ForeColor = Color.Goldenrod,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(100, 270),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblConfirm);

            txtConfirm = new TextBox
            {
                Location = new Point(300, 270),
                Width = 250,
                Height = 35,
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                BackColor = Color.Black,
                ForeColor = Color.White,
                UseSystemPasswordChar = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(txtConfirm);

            btnSignUp = new Button
            {
                Text = "SIGN UP",
                Location = new Point(300, 330),
                Width = 250,
                Height = 50,
                Font = new Font("Georgia", 14, FontStyle.Bold),
                BackColor = Color.DarkGoldenrod,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSignUp.FlatAppearance.BorderSize = 3;
            btnSignUp.FlatAppearance.BorderColor = Color.Gold;
            btnSignUp.Click += BtnSignUp_Click;
            btnSignUp.MouseEnter += (s, e) => btnSignUp.BackColor = Color.Gold;
            btnSignUp.MouseLeave += (s, e) => btnSignUp.BackColor = Color.DarkGoldenrod;
            this.Controls.Add(btnSignUp);
        }

        private void ApplyTheme()
        {
            this.Text = "Warcraft Sign-Up Portal";
            this.BackColor = Color.FromArgb(15, 15, 15);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.ClientSize = new Size(800, 500);
        }

        private async void BtnSignUp_Click(object sender, EventArgs e)
        {
            string username = txtUser.Text.Trim();
            string password = txtPass.Text.Trim();
            string confirm = txtConfirm.Text.Trim();

            if (username == "" || password == "" || confirm == "")
            {
                MessageBox.Show("All fields are required.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password != confirm)
            {
                MessageBox.Show("Passwords do not match.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("Warcraft_LibraryDB");
            var collection = database.GetCollection<BsonDocument>("Users");

            var existingUser = await collection.Find(Builders<BsonDocument>.Filter.Eq("Username", username)).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                MessageBox.Show("Username already exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var newUser = new BsonDocument
            {
                { "Username", username },
                { "Password", password }
            };
            await collection.InsertOneAsync(newUser);

            MessageBox.Show("Account created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close(); 
        }
    }
}
