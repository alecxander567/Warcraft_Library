using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Warcraft_Library
{
    public partial class Form4 : Form
    {
        private Label lblName, lblRace, lblBio, lblImage;
        private TextBox txtName, txtBio;
        private ComboBox cbRace;
        private Button btnSave, btnCancel, btnUploadImage;
        private Panel mainPanel;
        private PictureBox picHero;
        private string imageFilePath = null;
        private string currentUsername;
        private ObjectId? editingHeroId = null;

        public Form4(string username)
        {
            currentUsername = username;
            BuildForm();
        }

        public Form4(string username, ObjectId heroId) : this(username) 
        {
            editingHeroId = heroId;
            this.Text = "Edit Hero";
            LoadHeroData(heroId);
        }

        private void BuildForm()
        {
            this.Text = "Add New Hero";
            this.Size = new Size(700, 700);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.Black;

            mainPanel = new Panel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(25, 25, 30),
                Padding = new Padding(20)
            };
            this.Controls.Add(mainPanel);

            mainPanel.Paint += (s, e) =>
            {
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    mainPanel.ClientRectangle,
                    Color.FromArgb(25, 25, 30),
                    Color.FromArgb(40, 40, 50),
                    45f))
                {
                    e.Graphics.FillRectangle(brush, mainPanel.ClientRectangle);
                }
            };

            Label lblHeader = new Label()
            {
                Text = "⚔️ Add New Hero ⚔️",
                ForeColor = Color.Gold,
                Font = new Font("Papyrus", 22, FontStyle.Bold),
                AutoSize = false,
                Height = 60,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter
            };
            mainPanel.Controls.Add(lblHeader);

            lblImage = new Label()
            {
                Text = "Hero Image:",
                ForeColor = Color.Gold,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(50, 80)
            };
            mainPanel.Controls.Add(lblImage);

            picHero = new PictureBox()
            {
                Location = new Point(200, 80),
                Size = new Size(200, 200),
                BackColor = Color.FromArgb(60, 60, 70),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            mainPanel.Controls.Add(picHero);

            btnUploadImage = new Button()
            {
                Text = "Upload Image",
                Location = new Point(420, 140),
                Size = new Size(150, 40),
                Font = new Font("Papyrus", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnUploadImage.FlatAppearance.BorderSize = 0;
            btnUploadImage.Click += BtnUploadImage_Click;
            mainPanel.Controls.Add(btnUploadImage);

            lblName = new Label()
            {
                Text = "Hero Name:",
                ForeColor = Color.Gold,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(50, 300)
            };
            mainPanel.Controls.Add(lblName);

            txtName = new TextBox()
            {
                Location = new Point(200, 298),
                Width = 370,
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                BackColor = Color.FromArgb(60, 60, 70),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            mainPanel.Controls.Add(txtName);

            lblBio = new Label()
            {
                Text = "Biography:",
                ForeColor = Color.Gold,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(50, 360)
            };
            mainPanel.Controls.Add(lblBio);

            txtBio = new TextBox()
            {
                Location = new Point(200, 358),
                Width = 370,
                Height = 120,
                Multiline = true,
                Font = new Font("Segoe UI", 12),
                BackColor = Color.FromArgb(60, 60, 70),
                ForeColor = Color.White,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.FixedSingle
            };
            mainPanel.Controls.Add(txtBio);

            lblRace = new Label()
            {
                Text = "Race:",
                ForeColor = Color.Gold,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(50, 500)
            };
            mainPanel.Controls.Add(lblRace);

            cbRace = new ComboBox()
            {
                Location = new Point(200, 498),
                Width = 370,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 12),
                BackColor = Color.FromArgb(60, 60, 70),
                ForeColor = Color.White
            };
            cbRace.Items.AddRange(new string[] { "Human", "Orc", "Night Elf", "Elf", "Troll", "Undead", "Demon", "Worgen", "Pandaren", "Draenei" });
            mainPanel.Controls.Add(cbRace);

            btnSave = new Button()
            {
                Text = "Save",
                BackColor = Color.FromArgb(180, 30, 30),
                ForeColor = Color.White,
                Font = new Font("Papyrus", 12, FontStyle.Bold),
                Size = new Size(160, 50),
                FlatStyle = FlatStyle.Flat,
                Location = new Point(200, 600), 
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.MouseEnter += (s, e) => btnSave.BackColor = Color.FromArgb(220, 50, 50);
            btnSave.MouseLeave += (s, e) => btnSave.BackColor = Color.FromArgb(180, 30, 30);
            btnSave.Click += BtnSave_Click;
            mainPanel.Controls.Add(btnSave);

            btnCancel = new Button()
            {
                Text = "Cancel",
                BackColor = Color.Gray,
                ForeColor = Color.White,
                Font = new Font("Papyrus", 12, FontStyle.Bold),
                Size = new Size(160, 50),
                FlatStyle = FlatStyle.Flat,
                Location = new Point(380, 600), 
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.MouseEnter += (s, e) => btnCancel.BackColor = Color.FromArgb(120, 120, 120);
            btnCancel.MouseLeave += (s, e) => btnCancel.BackColor = Color.Gray;
            btnCancel.Click += (s, e) => this.Close();
            mainPanel.Controls.Add(btnCancel);
        }

        private void BtnUploadImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            ofd.Title = "Select Hero Image";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                imageFilePath = ofd.FileName;
                picHero.Image = Image.FromFile(imageFilePath);
            }
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || cbRace.SelectedIndex == -1)
            {
                MessageBox.Show("Please fill in all fields.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var client = new MongoClient("mongodb://localhost:27017");
                var db = client.GetDatabase("Warcraft_LibraryDB");
                var collection = db.GetCollection<BsonDocument>("Heroes");

                byte[] imageBytes = null;
                if (!string.IsNullOrEmpty(imageFilePath))
                    imageBytes = File.ReadAllBytes(imageFilePath);

                if (editingHeroId.HasValue) 
                {
                    var update = Builders<BsonDocument>.Update
                        .Set("Name", txtName.Text)
                        .Set("Race", cbRace.SelectedItem.ToString())
                        .Set("Biography", txtBio.Text);

                    if (imageBytes != null)
                        update = update.Set("Image", imageBytes);

                    var filter = Builders<BsonDocument>.Filter.Eq("_id", editingHeroId.Value);
                    await collection.UpdateOneAsync(filter, update);

                    MessageBox.Show("Hero updated successfully!", "Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else 
                {
                    var newHero = new BsonDocument
            {
                { "Username", currentUsername },
                { "Name", txtName.Text },
                { "Race", cbRace.SelectedItem.ToString() },
                { "Biography", txtBio.Text },
                { "Image", imageBytes ?? new byte[0] }
            };
                    await collection.InsertOneAsync(newHero);
                    MessageBox.Show("Hero added successfully!", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving hero: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void LoadHeroData(ObjectId heroId)
        {
            try
            {
                var client = new MongoClient("mongodb://localhost:27017");
                var db = client.GetDatabase("Warcraft_LibraryDB");
                var collection = db.GetCollection<BsonDocument>("Heroes");

                var filter = Builders<BsonDocument>.Filter.Eq("_id", heroId);
                var hero = await collection.Find(filter).FirstOrDefaultAsync();

                if (hero != null)
                {
                    txtName.Text = hero.GetValue("Name", "").AsString;
                    txtBio.Text = hero.GetValue("Biography", "").AsString;
                    cbRace.SelectedItem = hero.GetValue("Race", "").AsString;

                    if (hero.Contains("Image") && hero["Image"].IsBsonBinaryData && hero["Image"].AsBsonBinaryData.Bytes.Length > 0)
                    {
                        using (MemoryStream ms = new MemoryStream(hero["Image"].AsByteArray))
                            picHero.Image = Image.FromStream(ms);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load hero data: " + ex.Message);
            }
        }
    }
}
