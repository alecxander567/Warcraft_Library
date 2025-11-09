using System;
using System.Drawing;
using System.Windows.Forms;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Warcraft_Library
{
    public partial class Form1 : Form
    {
        private Label lblTitle;
        private PictureBox picLogo; 
        private Label lblUser;
        private Label lblPass;
        private TextBox txtUser;
        private TextBox txtPass;
        private Button btnLogin;
        private LinkLabel linkSignup;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CreateLoginUI();
            ApplyWarcraftTheme();
        }

        private void CreateLoginUI()
        {
            lblTitle = new Label
            {
                Text = "⚔️ WARCRAFT PORTAL ⚔️",
                Font = new Font("Georgia", 36, FontStyle.Bold),
                ForeColor = Color.Gold,
                Dock = DockStyle.Top,
                Height = 120,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblTitle);

            picLogo = new PictureBox
            {
                Image = Image.FromFile(@"C:\Users\Lenovo\Pictures\WoW_icon.svg.png"), 
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(400, 120),
                Size = new Size(300, 120),
                BackColor = Color.Transparent
            };
            this.Controls.Add(picLogo);

            lblUser = new Label
            {
                Text = "Username:",
                ForeColor = Color.Goldenrod,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(270, 280), 
                AutoSize = true,
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblUser);

            lblPass = new Label
            {
                Text = "Password:",
                ForeColor = Color.Goldenrod,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(280, 360),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblPass);

            txtUser = new TextBox
            {
                Location = new Point(400, 280), 
                Width = 300,
                Height = 40,
                Font = new Font("Segoe UI", 14, FontStyle.Regular),
                BackColor = Color.Black,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(txtUser);

            txtPass = new TextBox
            {
                Location = new Point(400, 360), 
                Width = 300,
                Height = 40,
                Font = new Font("Segoe UI", 14, FontStyle.Regular),
                BackColor = Color.Black,
                ForeColor = Color.White,
                UseSystemPasswordChar = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(txtPass);

            btnLogin = new Button
            {
                Text = "ENTER THE REALM",
                Location = new Point(400, 440), 
                Width = 300,
                Height = 60,
                Font = new Font("Georgia", 14, FontStyle.Bold),
                BackColor = Color.DarkGoldenrod,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 3;
            btnLogin.FlatAppearance.BorderColor = Color.Gold;
            btnLogin.Click += BtnLogin_Click;
            btnLogin.MouseEnter += (s, e) => btnLogin.BackColor = Color.Gold;
            btnLogin.MouseLeave += (s, e) => btnLogin.BackColor = Color.DarkGoldenrod;
            this.Controls.Add(btnLogin);

            linkSignup = new LinkLabel
            {
                Text = "New adventurer? Create an account",
                Font = new Font("Segoe UI", 12, FontStyle.Underline),
                LinkColor = Color.Gold,
                ActiveLinkColor = Color.Orange,
                VisitedLinkColor = Color.Goldenrod,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(430, 520), 
                Cursor = Cursors.Hand
            };
            linkSignup.Click += LinkSignup_Click;
            this.Controls.Add(linkSignup);
        }

        private void ApplyWarcraftTheme()
        {
            this.Text = "Warcraft Login Portal";
            this.BackColor = Color.FromArgb(15, 15, 15);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.ClientSize = new Size(1100, 700);

            try
            {
                this.BackgroundImage = Image.FromFile("warcraft_bg.jpg");
                this.BackgroundImageLayout = ImageLayout.Stretch;
            }
            catch { }
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUser.Text.Trim();
            string password = txtPass.Text.Trim();

            var client = new MongoClient("mongodb://localhost:27017"); 
            var database = client.GetDatabase("Warcraft_LibraryDB");
            var collection = database.GetCollection<BsonDocument>("Users");

            var filter = Builders<BsonDocument>.Filter.Eq("Username", username) &
                         Builders<BsonDocument>.Filter.Eq("Password", password);

            var user = await collection.Find(filter).FirstOrDefaultAsync();

            if (user != null)
            {
                MessageBox.Show(
                    $"Welcome, {username}. The Frozen Throne awaits.",
                    "Access Granted",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            else
            {
                MessageBox.Show(
                    "Invalid credentials, mortal.",
                    "Access Denied",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void LinkSignup_Click(object sender, EventArgs e)
        {
            Form2 signUpForm = new Form2();
            signUpForm.ShowDialog();
        }
    }
}
