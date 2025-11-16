using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace Warcraft_Library
{
    public partial class Form3 : Form
    {
        private string currentUsername;

        public Form3(string username)
        {
            currentUsername = username;
            InitializeComponent();

            btnHeroes.Click += async (s, e) => await LoadHeroesAsync();
            btnItems.Click += (s, e) => LoadItems();
            btnMyCharacter.Click += (s, e) => LoadMyCharacter();
            btnLibraries.Click += (s, e) => LoadLibraries();

            LoadProfileImage();

            this.Load += async (s, e) => await LoadHeroesAsync();
        }

        private async void LoadProfileImage()
        {
            try
            {
                var client = new MongoClient("mongodb://localhost:27017");
                var database = client.GetDatabase("Warcraft_LibraryDB");
                var collection = database.GetCollection<BsonDocument>("Users");

                var filter = Builders<BsonDocument>.Filter.Eq("Username", currentUsername);
                var user = await collection.Find(filter).FirstOrDefaultAsync();

                if (user != null && user.Contains("ProfileImage") && user["ProfileImage"].IsBsonBinaryData)
                {
                    byte[] imgBytes = user["ProfileImage"].AsByteArray;
                    using (var ms = new System.IO.MemoryStream(imgBytes))
                    {
                        picProfile.Image = Image.FromStream(ms);
                    }
                    picProfile.Paint += (s, e) =>
                    {
                        using (Pen pen = new Pen(Color.Gold, 3))
                        {
                            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                            e.Graphics.DrawEllipse(pen, 1, 1, picProfile.Width - 3, picProfile.Height - 3);
                        }
                    };
                }
                else
                {
                    string defaultImagePath = Application.StartupPath + @"\Assets\default_profile.png";
                    if (System.IO.File.Exists(defaultImagePath))
                    {
                        picProfile.Image = Image.FromFile(defaultImagePath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading profile: " + ex.Message);
            }
        }

        private async Task LoadHeroesAsync()
        {
            mainContent.Controls.Clear();

            Button btnAddHero = new Button
            {
                Text = "+ Add Hero",
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 122, 204),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(140, 40),
                Location = new Point(mainContent.Width - 160, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnAddHero.Click += async (s, e) =>
            {
                Form4 addForm = new Form4(currentUsername);
                addForm.ShowDialog();
                await LoadHeroesAsync();
            };
            mainContent.Controls.Add(btnAddHero);

            try
            {
                var client = new MongoClient("mongodb://localhost:27017");
                var db = client.GetDatabase("Warcraft_LibraryDB");
                var collection = db.GetCollection<BsonDocument>("Heroes");

                var filter = Builders<BsonDocument>.Filter.Eq("Username", currentUsername);
                var heroes = await collection.Find(filter).ToListAsync();

                Debug.WriteLine($"Found {heroes.Count} heroes for {currentUsername}");

                if (heroes.Count == 0)
                {
                    Label noHeroes = new Label
                    {
                        Text = "No heroes found. Click '+ Add Hero' to create one!",
                        ForeColor = Color.Gray,
                        Font = new Font("Segoe UI", 14),
                        Location = new Point(mainContent.Width / 2 - 200, mainContent.Height / 2),
                        AutoSize = true
                    };
                    mainContent.Controls.Add(noHeroes);
                    return;
                }

                int x = 20, y = 80;
                int cardWidth = 250, cardHeight = 350, spacing = 20;
                int cardsPerRow = 4;
                int counter = 0;

                foreach (var hero in heroes)
                {
                    Panel card = new Panel
                    {
                        Size = new Size(cardWidth, cardHeight),
                        BackColor = Color.FromArgb(60, 60, 70),
                        BorderStyle = BorderStyle.FixedSingle,
                        Location = new Point(x, y)
                    };

                    PictureBox pic = new PictureBox
                    {
                        Size = new Size(200, 150),
                        Location = new Point((cardWidth - 200) / 2, 10),
                        SizeMode = PictureBoxSizeMode.Zoom
                    };

                    if (hero.Contains("Image") && hero["Image"].IsBsonBinaryData && hero["Image"].AsBsonBinaryData.Bytes.Length > 0)
                    {
                        byte[] imgBytes = hero["Image"].AsByteArray;
                        using (MemoryStream ms = new MemoryStream(imgBytes))
                            pic.Image = Image.FromStream(ms);
                    }

                    Label lblName = new Label
                    {
                        Text = hero.GetValue("Name", "").AsString,
                        ForeColor = Color.Gold,
                        Font = new Font("Segoe UI", 12, FontStyle.Bold),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Size = new Size(cardWidth, 25),
                        Location = new Point(0, 170)
                    };

                    Label lblRace = new Label
                    {
                        Text = hero.GetValue("Race", "").AsString,
                        ForeColor = Color.White,
                        Font = new Font("Segoe UI", 11),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Size = new Size(cardWidth, 20),
                        Location = new Point(0, 200)
                    };

                    TextBox txtBio = new TextBox
                    {
                        Text = hero.GetValue("Biography", "").AsString,
                        ForeColor = Color.White,
                        BackColor = Color.FromArgb(60, 60, 70),
                        Multiline = true,
                        ReadOnly = true,
                        ScrollBars = ScrollBars.Vertical,
                        Size = new Size(cardWidth - 20, 100),
                        Location = new Point(10, 230),
                        BorderStyle = BorderStyle.None
                    };

                    card.Controls.Add(pic);
                    card.Controls.Add(lblName);
                    card.Controls.Add(lblRace);
                    card.Controls.Add(txtBio);

                    mainContent.Controls.Add(card);

                    counter++;
                    x += cardWidth + spacing;
                    if (counter % cardsPerRow == 0)
                    {
                        x = 20;
                        y += cardHeight + spacing;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load heroes: " + ex.Message);
                Debug.WriteLine($"Exception: {ex}");
            }
        }

        private void LoadItems()
        {
            mainContent.Controls.Clear();
            Label lbl = new Label { Text = "Items view - Coming soon!", ForeColor = Color.White, Location = new Point(50, 50), AutoSize = true };
            mainContent.Controls.Add(lbl);
        }

        private void LoadMyCharacter()
        {
            mainContent.Controls.Clear();
            Label lbl = new Label { Text = "My Character view - Coming soon!", ForeColor = Color.White, Location = new Point(50, 50), AutoSize = true };
            mainContent.Controls.Add(lbl);
        }

        private void LoadLibraries()
        {
            mainContent.Controls.Clear();
            Label lbl = new Label { Text = "Libraries view - Coming soon!", ForeColor = Color.White, Location = new Point(50, 50), AutoSize = true };
            mainContent.Controls.Add(lbl);
        }
    }
}