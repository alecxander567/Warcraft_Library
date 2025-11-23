using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Warcraft_Library
{
    partial class Form3
    {
        private System.ComponentModel.IContainer components = null;

        private Panel sidebar;
        private Panel mainContent;
        private Button btnHeroes;
        private Button btnItems;
        private Button btnMyCharacter;
        private Button btnLibraries;
        private Button btnLogout;
        private Label lblTitle;
        private PictureBox picProfile;
        private Button activeButton = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.sidebar = new System.Windows.Forms.Panel();
            this.mainContent = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();

            this.ClientSize = new System.Drawing.Size(1350, 600);

            this.sidebar.BackColor = System.Drawing.Color.FromArgb(32, 34, 37);
            this.sidebar.Dock = DockStyle.Left;
            this.sidebar.Width = 220;

            this.lblTitle.Text = "⚔️ Warcraft Library ⚔️";
            this.lblTitle.ForeColor = System.Drawing.Color.Gold;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold);
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(20, 20);

            this.picProfile = new PictureBox();
            this.picProfile.Size = new System.Drawing.Size(100, 100);
            this.picProfile.Location = new System.Drawing.Point(60, 60);
            this.picProfile.SizeMode = PictureBoxSizeMode.Zoom;
            this.picProfile.BackColor = Color.FromArgb(45, 45, 45);
            this.picProfile.Click += PicProfile_Click;
            this.picProfile.Cursor = Cursors.Hand;

            System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
            gp.AddEllipse(0, 0, picProfile.Width - 1, picProfile.Height - 1);
            picProfile.Region = new System.Drawing.Region(gp);

            picProfile.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Color.Gray, 3))
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.DrawEllipse(pen, 1, 1, picProfile.Width - 3, picProfile.Height - 3);
                }
            };

            int startX = 20;
            int startY = 180;
            int spacing = 55;

            this.btnHeroes = MakeMenuButton("Heroes", startX, startY);
            this.btnItems = MakeMenuButton("Items", startX, startY + spacing);
            this.btnMyCharacter = MakeMenuButton("My Character", startX, startY + spacing * 2);
            this.btnLibraries = MakeMenuButton("Tavern", startX, startY + spacing * 3);

            activeButton = btnHeroes;
            activeButton.BackColor = Color.Gold;
            activeButton.ForeColor = Color.Black;

            this.btnLogout = new Button();
            this.btnLogout.Text = "Log Out";
            this.btnLogout.ForeColor = Color.White;
            this.btnLogout.BackColor = Color.FromArgb(232, 59, 59);
            this.btnLogout.FlatStyle = FlatStyle.Flat;
            this.btnLogout.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            this.btnLogout.Size = new Size(180, 45);
            this.btnLogout.Location = new Point(startX, startY + spacing * 6);
            this.btnLogout.Click += BtnLogout_Click;

            this.sidebar.Controls.Add(this.lblTitle);
            this.sidebar.Controls.Add(this.picProfile);
            this.sidebar.Controls.Add(this.btnHeroes);
            this.sidebar.Controls.Add(this.btnItems);
            this.sidebar.Controls.Add(this.btnMyCharacter);
            this.sidebar.Controls.Add(this.btnLibraries);
            this.sidebar.Controls.Add(this.btnLogout);

            this.mainContent.Dock = DockStyle.Fill;
            this.mainContent.BackColor = System.Drawing.Color.FromArgb(54, 57, 63);
            this.Controls.Add(this.mainContent);
            this.Controls.Add(this.sidebar);

            this.Text = "Warcraft Library - Home";
        }

        private Button MakeMenuButton(string txt, int x, int y)
        {
            Button b = new Button();
            b.Text = txt;
            b.ForeColor = Color.White;
            b.BackColor = Color.FromArgb(47, 49, 54);
            b.FlatStyle = FlatStyle.Flat;
            b.Font = new Font("Segoe UI", 11);
            b.Size = new Size(180, 45);
            b.Location = new Point(x, y);

            b.MouseEnter += (s, e) => { if (b != activeButton) { b.BackColor = Color.Gold; b.ForeColor = Color.Black; } };
            b.MouseLeave += (s, e) => { if (b != activeButton) { b.BackColor = Color.FromArgb(47, 49, 54); b.ForeColor = Color.White; } };

            b.Click += (s, e) =>
            {
                if (activeButton != null)
                {
                    activeButton.BackColor = Color.FromArgb(47, 49, 54);
                    activeButton.ForeColor = Color.White;
                }

                activeButton = b;
                activeButton.BackColor = Color.Gold;
                activeButton.ForeColor = Color.Black;
            };

            return b;
        }

        private async void PicProfile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            ofd.Title = "Select a new profile image";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Image newImage = Image.FromFile(ofd.FileName);
                picProfile.Image = newImage;

                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                newImage.Save(ms, newImage.RawFormat);
                byte[] imgBytes = ms.ToArray();

                try
                {
                    var client = new MongoDB.Driver.MongoClient("mongodb://localhost:27017");
                    var database = client.GetDatabase("Warcraft_LibraryDB");
                    var collection = database.GetCollection<MongoDB.Bson.BsonDocument>("Users");

                    var filter = MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Filter.Eq("Username", currentUsername);
                    var update = MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Update.Set("ProfileImage", imgBytes);

                    await collection.UpdateOneAsync(filter, update);

                    MessageBox.Show("Profile image updated successfully!", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to save image: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            Form1 loginForm = new Form1();
            loginForm.Show();

            this.Close();
        }
    }
}