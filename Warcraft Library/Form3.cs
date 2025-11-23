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
            btnLibraries.Click += (s, e) => LoadTavern();

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
            mainContent.AutoScroll = true;
            mainContent.BackgroundImage = null;

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
                int cardWidth = 250;
                int cardHeight = 420;
                int spacing = 20; 
                int leftPadding = 10;
                int cardsPerRow = 4;
                int counter = 0;

                int totalCardWidth = cardsPerRow * cardWidth + (cardsPerRow - 1) * spacing;
                int extraSpace = mainContent.Width - totalCardWidth;
                int startX = leftPadding + extraSpace / 2; 
                x = startX; 

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
                        Size = new Size(cardWidth - 20, 130), 
                        Location = new Point(10, 230),
                        BorderStyle = BorderStyle.None
                    };

                    Button btnView = new Button
                    {
                        Text = "View",
                        Size = new Size(60, 30),
                        Location = new Point(cardWidth - 210, 370), 
                        BackColor = Color.FromArgb(100, 100, 200),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat
                    };

                    btnView.Click += (s, e) =>
                    {
                        FormViewHero viewForm = new FormViewHero(hero);
                        viewForm.ShowDialog();
                    };

                    Button btnEdit = new Button
                    {
                        Text = "Edit",
                        Size = new Size(60, 30),
                        Location = new Point(cardWidth - 140, 370), 
                        BackColor = Color.FromArgb(0, 122, 204),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat
                    };

                    btnEdit.Click += async (s, e) =>
                    {
                        try
                        {
                            var heroId = hero.GetValue("_id").AsObjectId;

                            Form4 editForm = new Form4(currentUsername, heroId); 
                            editForm.ShowDialog();

                            await LoadHeroesAsync();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to open hero for editing: " + ex.Message);
                            Debug.WriteLine($"Exception: {ex}");
                        }
                    };

                    Button btnDelete = new Button
                    {
                        Text = "Delete",
                        Size = new Size(60, 30),
                        Location = new Point(cardWidth - 70, 370),
                        BackColor = Color.FromArgb(200, 50, 50),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat
                    };

                    var currentHero = hero;

                    btnDelete.Click += async (s, e) =>
                    {
                        try
                        {
                            var result = MessageBox.Show(
                                $"Are you sure you want to delete {currentHero.GetValue("Name", "").AsString}?",
                                "Confirm Delete",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning
                            );

                            if (result == DialogResult.Yes)
                            {
                                var heroId = currentHero.GetValue("_id").AsObjectId;
                                var filterDel = Builders<BsonDocument>.Filter.Eq("_id", heroId);
                                await collection.DeleteOneAsync(filterDel);

                                MessageBox.Show("Hero deleted successfully!", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                await LoadHeroesAsync();
                            }
                        }

                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to delete hero: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Debug.WriteLine($"Exception: {ex}");
                        }
                    };

                    card.Controls.Add(pic);
                    card.Controls.Add(lblName);
                    card.Controls.Add(lblRace);
                    card.Controls.Add(txtBio);
                    card.Controls.Add(btnView);
                    card.Controls.Add(btnEdit);
                    card.Controls.Add(btnDelete);

                    mainContent.Controls.Add(card);

                    counter++;
                    x += cardWidth + spacing;

                     if (counter % cardsPerRow == 0)
                    {
                        x = startX; 
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

        private async void LoadItems()
        {
            mainContent.Controls.Clear();
            mainContent.BackgroundImage = null;

            Button btnAddItem = new Button
            {
                Text = "+ Add Item",
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 122, 204),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(140, 40),
                Location = new Point(mainContent.Width - 160, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            btnAddItem.Click += (s, e) =>
            {
                FormAddItem addItemForm = new FormAddItem(currentUsername);
                if (addItemForm.ShowDialog() == DialogResult.OK)
                {
                    LoadItems();
                }
            };
            mainContent.Controls.Add(btnAddItem);

            try
                {
                var client = new MongoClient("mongodb://localhost:27017");
                var db = client.GetDatabase("Warcraft_LibraryDB");
                var collection = db.GetCollection<BsonDocument>("Items");

                var filter = Builders<BsonDocument>.Filter.Eq("Uploader", currentUsername);
                var items = await collection.Find(filter).ToListAsync();

                int x = 20, y = 80;
                int cardWidth = 300, cardHeight = 300;
                int spacing = 20;
                int cardsPerRow = Math.Max(1, (mainContent.Width - spacing) / (cardWidth + spacing));
                int count = 0;

                int btnWidth = 80, btnHeight = 30, btnSpacing = 10;
                int btnY = cardHeight - btnHeight - 10;
                int startX = 10; 

                foreach (var item in items)
                {
                    Panel card = new Panel
                    {
                        Size = new Size(cardWidth, cardHeight),
                        Location = new Point(x, y),
                        BackColor = Color.FromArgb(35, 35, 45),
                        BorderStyle = BorderStyle.FixedSingle
                    };

                    PictureBox pic = new PictureBox
                    {
                        Size = new Size(100, 100),
                        Location = new Point(10, 10),
                        SizeMode = PictureBoxSizeMode.Zoom,
                        BackColor = Color.FromArgb(60, 60, 70)
                    };
                    if (item.Contains("Image") && item["Image"].AsByteArray.Length > 0)
                    {
                        using (var ms = new MemoryStream(item["Image"].AsByteArray))
                        {
                            pic.Image = Image.FromStream(ms);
                        }
                    }
                    card.Controls.Add(pic);

                    Label lblName = new Label
                    {
                        Text = item.Contains("Name") ? item["Name"].AsString : "Unknown",
                        ForeColor = Color.Gold,
                        Font = new Font("Papyrus", 12, FontStyle.Bold),
                        Location = new Point(120, 10),
                        AutoSize = false,
                        Width = cardWidth - 130,
                        Height = 25
                    };
                    card.Controls.Add(lblName);

                    Label lblOwner = new Label
                    {
                        Text = "Owner: " + (item.Contains("Owner") ? item["Owner"].AsString : "-"),
                        ForeColor = Color.LightGray,
                        Font = new Font("Segoe UI", 10, FontStyle.Regular),
                        Location = new Point(120, 40),
                        AutoSize = false,
                        Width = cardWidth - 130,
                        Height = 20
                    };
                    card.Controls.Add(lblOwner);

                    Label lblAbilities = new Label
                    {
                        Text = "Abilities: " + (item.Contains("Abilities") ? item["Abilities"].AsString : "-"),
                        ForeColor = Color.LightGreen,
                        Font = new Font("Segoe UI", 10, FontStyle.Italic),
                        Location = new Point(10, 120),
                        AutoSize = false,
                        Width = cardWidth - 20,
                        Height = 40
                    };
                    card.Controls.Add(lblAbilities);

                    TextBox txtDesc = new TextBox
                    {
                        Text = item.Contains("Description") ? item["Description"].AsString : "-",
                        ForeColor = Color.White,
                        BackColor = Color.FromArgb(50, 50, 60),
                        Font = new Font("Segoe UI", 9),
                        Location = new Point(10, 170),
                        Width = cardWidth - 20,
                        Height = 70,
                        Multiline = true,
                        ReadOnly = true,
                        BorderStyle = BorderStyle.None,
                        ScrollBars = ScrollBars.Vertical
                    };
                    card.Controls.Add(txtDesc);

                    Button btnView = new Button
                    {
                        Text = "View",
                        Size = new Size(60, 30),
                        Location = new Point(cardWidth - 210, cardHeight - 40), 
                        BackColor = Color.FromArgb(100, 100, 200),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat,
                        Cursor = Cursors.Hand
                    };

                    btnView.Click += (s, e) =>
                    {
                        FormViewItem viewForm = new FormViewItem(item);
                        viewForm.ShowDialog();
                    };

                    card.Controls.Add(btnView);

                    Button btnEdit = new Button
                    {
                        Text = "Edit",
                        Size = new Size(60, 30),
                        Location = new Point(cardWidth - 140, cardHeight - 40),
                        BackColor = Color.FromArgb(0, 122, 204),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat,
                        Cursor = Cursors.Hand
                    };

                    var currentItem = item;

                    btnEdit.Click += async (s, e) =>
                    {
                        try
                        {
                            FormAddItem editForm = new FormAddItem(currentUsername);

                            editForm.SetItemData(item); 

                            if (editForm.ShowDialog() == DialogResult.OK)
                            {
                               client = new MongoClient("mongodb://localhost:27017");
                               db = client.GetDatabase("Warcraft_LibraryDB");
                               collection = db.GetCollection<BsonDocument>("Items");

                                var updatedDoc = new BsonDocument
                                {
                                    { "Name", editForm.ItemName },
                                    { "Owner", editForm.ItemOwner },
                                    { "Abilities", editForm.ItemAbilities },
                                    { "Description", editForm.ItemDescription },
                                    { "Image", editForm.ItemImageBytes ?? new byte[0] },
                                    { "Uploader", currentUsername } 
                                };

                                var editFilter = Builders<BsonDocument>.Filter.Eq("_id", currentItem.GetValue("_id").AsObjectId);
                                await collection.ReplaceOneAsync(editFilter, updatedDoc);

                                LoadItems();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to edit item: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    };

                    card.Controls.Add(btnEdit);

                    Button btnDelete = new Button
                    {
                        Text = "Delete",
                        Size = new Size(60, 30),
                        Location = new Point(cardWidth - 70, cardHeight - 40),
                        BackColor = Color.FromArgb(200, 50, 50),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat,
                        Cursor = Cursors.Hand
                    };

                    btnDelete.Click += async (s, e) =>
                    {
                        try
                        {
                            var result = MessageBox.Show(
                                $"Are you sure you want to delete {currentItem.GetValue("Name", "").AsString}?",
                                "Confirm Delete",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning
                            );

                            if (result == DialogResult.Yes)
                            {
                                client = new MongoClient("mongodb://localhost:27017");
                                db = client.GetDatabase("Warcraft_LibraryDB");
                                collection = db.GetCollection<BsonDocument>("Items");

                                var itemId = currentItem.GetValue("_id").AsObjectId;

                                var deleteFilter = Builders<BsonDocument>.Filter.Eq("_id", currentItem.GetValue("_id").AsObjectId);
                                await collection.DeleteOneAsync(deleteFilter);

                                MessageBox.Show("Item deleted successfully!", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                LoadItems();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to delete item: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Console.WriteLine($"Exception: {ex}");
                        }
                    };

                    card.Controls.Add(btnDelete);

                    mainContent.Controls.Add(card);

                    count++;
                    if (count % cardsPerRow == 0)
                    {
                        x = 20;
                        y += cardHeight + spacing;
                    }
                    else
                    {
                        x += cardWidth + spacing;
                    }
                }

                if (items.Count == 0)
                {
                    Label noItems = new Label
                    {
                        Text = "No items found. Click '+ Add Items' to create one!",
                        ForeColor = Color.Gray,
                        Font = new Font("Segoe UI", 14),
                        Location = new Point(mainContent.Width / 2 - 200, mainContent.Height / 2),
                        AutoSize = true
                    };
                    mainContent.Controls.Add(noItems);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading items: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadMyCharacter()
        {
            mainContent.Controls.Clear();
            mainContent.BackgroundImage = null;

            Panel container = new Panel
            {
                BackColor = Color.FromArgb(28, 28, 33),
                Location = new Point(20, 20),
                Size = new Size(mainContent.Width - 40, mainContent.Height - 40),
                BorderStyle = BorderStyle.FixedSingle
            };
            mainContent.Controls.Add(container);

            Label title = new Label
            {
                Text = "⚔ Create Your Hero ⚔",
                ForeColor = Color.Gold,
                Font = new Font("Palatino Linotype", 22, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(30, 20)
            };
            container.Controls.Add(title);

            int startX = 30;
            int startY = 100;

            Func<string, Label> MakeLabel = (text) =>
            {
                return new Label
                {
                    Text = text,
                    ForeColor = Color.FromArgb(220, 200, 160),
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    AutoSize = true
                };
            };

            Panel imageFrame = new Panel
            {
                Location = new Point(startX, startY),
                Size = new Size(180, 220),
                BackColor = Color.FromArgb(22, 22, 28),
                BorderStyle = BorderStyle.Fixed3D
            };
            container.Controls.Add(imageFrame);

            Label lblImage = MakeLabel("Hero Image:");
            lblImage.Location = new Point(startX, startY - 30);
            container.Controls.Add(lblImage);

            PictureBox picHero = new PictureBox
            {
                Name = "picHeroImage",
                Location = new Point(15, 15),
                Size = new Size(150, 150),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(35, 35, 45)
            };
            imageFrame.Controls.Add(picHero);

            Button btnUpload = new Button
            {
                Text = "Upload",
                BackColor = Color.FromArgb(0, 90, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = 150,
                Height = 35,
                Location = new Point(15, 170),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnUpload.FlatAppearance.BorderSize = 0;

            btnUpload.Click += (sender, e) =>
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        picHero.Image = Image.FromFile(ofd.FileName);
                    }
                }
            };
            imageFrame.Controls.Add(btnUpload);

            int textStartX = startX + 220;
            int textStartY = startY;

            Label lblName = MakeLabel("Hero Name:");
            lblName.Location = new Point(textStartX, textStartY);
            container.Controls.Add(lblName);

            TextBox txtName = new TextBox
            {
                Name = "txtHeroName",
                Location = new Point(textStartX + 150, textStartY - 3),
                Width = 250,
                BackColor = Color.FromArgb(40, 40, 50),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            container.Controls.Add(txtName);

            textStartY += 40;

            Label lblRace = MakeLabel("Race:");
            lblRace.Location = new Point(textStartX, textStartY);
            container.Controls.Add(lblRace);

            ComboBox cbRace = new ComboBox
            {
                Name = "cbRace",
                Location = new Point(textStartX + 150, textStartY - 3),
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(40, 40, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            cbRace.Items.AddRange(new string[]
            {
                "Human", "Orc", "Night Elf", "Undead",
                "Dwarf", "Pandaren", "Troll", "Gnome", "Tauren",
                "Draenei", "Blood Elf", "Worgen", "Goblin"
            });
            container.Controls.Add(cbRace);

            textStartY += 40;

            Label lblAbilities = MakeLabel("Abilities:");
            lblAbilities.Location = new Point(textStartX, textStartY);
            container.Controls.Add(lblAbilities);

            TextBox txtAbilities = new TextBox
            {
                Name = "txtAbilities",
                Location = new Point(textStartX + 120, textStartY),
                Width = 350,
                Height = 90,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(40, 40, 50),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            container.Controls.Add(txtAbilities);

            textStartY += 110;

            Label lblBio = MakeLabel("Biography:");
            lblBio.Location = new Point(textStartX, textStartY);
            container.Controls.Add(lblBio);

            TextBox txtBio = new TextBox
            {
                Name = "txtBiography",
                Location = new Point(textStartX + 120, textStartY),
                Width = 350,
                Height = 130,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(40, 40, 50),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            container.Controls.Add(txtBio);

            ObjectId? currentHeroId = null;

            Panel previewPanel = new Panel
            {
                Location = new Point(container.Width - 350, 60),
                Size = new Size(300, container.Height - 120),
                BackColor = Color.FromArgb(18, 18, 23),
                BorderStyle = BorderStyle.Fixed3D,
                AutoScroll = true
            };
            container.Controls.Add(previewPanel);

            Panel scrollPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(previewPanel.Width - SystemInformation.VerticalScrollBarWidth, 600),
                BackColor = Color.Transparent
            };
            previewPanel.Controls.Add(scrollPanel);

            Label previewTitle = new Label
            {
                Text = "🏹 Hero Preview",
                ForeColor = Color.Gold,
                Font = new Font("Palatino Linotype", 16, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(60, 15)
            };
            scrollPanel.Controls.Add(previewTitle);

            PictureBox previewImage = new PictureBox
            {
                Name = "previewHeroImage",
                Location = new Point(75, 60),
                Size = new Size(150, 150),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(40, 40, 50)
            };
            scrollPanel.Controls.Add(previewImage);

            Label previewName = new Label
            {
                Text = "Name: (empty)",
                ForeColor = Color.FromArgb(220, 200, 160),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 230)
            };
            scrollPanel.Controls.Add(previewName);

            Label previewRace = new Label
            {
                Text = "Race: (empty)",
                ForeColor = Color.FromArgb(220, 200, 160),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 260)
            };
            scrollPanel.Controls.Add(previewRace);

            Label previewAbilities = MakeLabel("Abilities:");
            previewAbilities.Location = new Point(20, 290);
            scrollPanel.Controls.Add(previewAbilities);

            TextBox previewAbilitiesBox = new TextBox
            {
                ReadOnly = true,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical, 
                Location = new Point(20, 310),
                Width = 250,
                Height = 90,
                BackColor = Color.FromArgb(30, 30, 40),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            scrollPanel.Controls.Add(previewAbilitiesBox);

            Label previewBio = MakeLabel("Biography:");
            previewBio.Location = new Point(20, 410);
            scrollPanel.Controls.Add(previewBio);

            TextBox previewBioBox = new TextBox
            {
                ReadOnly = true,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(20, 430),
                Width = 250,
                Height = 120,
                BackColor = Color.FromArgb(30, 30, 40),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            scrollPanel.Controls.Add(previewBioBox);

            scrollPanel.Cursor = Cursors.Hand; 
            scrollPanel.Click += async (sender, e) =>
            {
                if (currentHeroId == null) return; 

                try
                {
                    using (var client = new MongoClient("mongodb://localhost:27017"))
                    {
                        var database = client.GetDatabase("Warcraft_LibraryDB");
                        var myHeroesCollection = database.GetCollection<BsonDocument>("My_Heroes");

                        var hero = myHeroesCollection.Find(Builders<BsonDocument>.Filter.Eq("_id", currentHeroId.Value))
                                                     .FirstOrDefault();

                        if (hero != null)
                        {
                            txtName.Text = hero.GetValue("name", "").AsString;
                            cbRace.SelectedItem = hero.GetValue("race", "Human").AsString;
                            txtAbilities.Text = hero.GetValue("abilities", "").AsString;
                            txtBio.Text = hero.GetValue("biography", "").AsString;

                            string imgBase64 = hero.GetValue("imageBase64", "").AsString;
                            if (!string.IsNullOrEmpty(imgBase64))
                            {
                                byte[] imageBytes = Convert.FromBase64String(imgBase64);
                                using (MemoryStream ms = new MemoryStream(imageBytes))
                                {
                                    picHero.Image?.Dispose();
                                    picHero.Image = Image.FromStream(ms);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading hero for editing: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            Button btnSave = new Button
            {
                Text = "Save Hero",
                BackColor = Color.FromArgb(10, 100, 200),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = 180,
                Height = 45,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(startX, container.Height - 80)
            };
            btnSave.FlatAppearance.BorderSize = 0;

            btnSave.Click += async (sender, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Hero Name is required!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string base64Image = null;
                if (picHero.Image != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        picHero.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        base64Image = Convert.ToBase64String(ms.ToArray());
                    }
                }

                var myHeroDoc = new BsonDocument
                {
                    { "name", txtName.Text },
                    { "race", cbRace.SelectedItem?.ToString() ?? "Unknown" },
                    { "abilities", txtAbilities.Text },
                    { "biography", txtBio.Text },
                    { "imageBase64", base64Image ?? "" },
                    { "createdAt", DateTime.Now }
                };

                try
                {
                    using (var client = new MongoClient("mongodb://localhost:27017"))
                    {
                        var database = client.GetDatabase("Warcraft_LibraryDB");
                        var myHeroesCollection = database.GetCollection<BsonDocument>("My_Heroes");

                        await myHeroesCollection.InsertOneAsync(myHeroDoc);

                        MessageBox.Show("Hero saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        currentHeroId = myHeroDoc["_id"].AsObjectId;
                        previewName.Text = $"Name: {txtName.Text}";
                        previewRace.Text = $"Race: {cbRace.SelectedItem?.ToString() ?? "Unknown"}";
                        previewAbilitiesBox.Text = txtAbilities.Text;
                        previewBioBox.Text = txtBio.Text;
                        previewImage.Image = picHero.Image != null ? (Image)picHero.Image.Clone() : null;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving hero: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            container.Controls.Add(btnSave);

            Button btnEdit = new Button
            {
                Text = "Edit Hero",
                BackColor = Color.FromArgb(200, 150, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = 180,
                Height = 45,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(startX + 190, container.Height - 80)
            };
            btnEdit.FlatAppearance.BorderSize = 0;

            try
            {
                using (var client = new MongoClient("mongodb://localhost:27017"))
                {
                    var database = client.GetDatabase("Warcraft_LibraryDB");
                    var myHeroesCollection = database.GetCollection<BsonDocument>("My_Heroes");

                    var lastHero = myHeroesCollection.Find(new BsonDocument())
                                                    .Sort(Builders<BsonDocument>.Sort.Descending("createdAt"))
                                                    .Limit(1)
                                                    .FirstOrDefault();

                    if (lastHero != null)
                    {
                        currentHeroId = lastHero["_id"].AsObjectId;
                        previewName.Text = $"Name: {lastHero.GetValue("name", "(empty)")}";
                        previewRace.Text = $"Race: {lastHero.GetValue("race", "(empty)")}";
                        previewAbilitiesBox.Text = lastHero.GetValue("abilities", "").AsString;
                        previewBioBox.Text = lastHero.GetValue("biography", "").AsString;

                        string imgBase64 = lastHero.GetValue("imageBase64", "").AsString;
                        if (!string.IsNullOrEmpty(imgBase64))
                        {
                            try
                            {
                                byte[] imageBytes = Convert.FromBase64String(imgBase64);
                                using (MemoryStream ms = new MemoryStream(imageBytes))
                                {
                                    previewImage.Image?.Dispose();
                                    previewImage.Image = Image.FromStream(ms);
                                }
                            }
                            catch (FormatException)
                            {
                                previewImage.Image = null;
                            }
                        }
                        else
                        {
                            previewImage.Image = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading hero preview: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            btnEdit.Click += async (sender, e) =>
            {
                if (currentHeroId == null)
                {
                    MessageBox.Show("No hero to edit!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Hero Name is required!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var confirmResult = MessageBox.Show("Are you sure you want to update this hero?",
                                                    "Confirm Edit",
                                                    MessageBoxButtons.YesNo,
                                                    MessageBoxIcon.Question);
                if (confirmResult != DialogResult.Yes)
                    return;

                try
                {
                    using (var client = new MongoClient("mongodb://localhost:27017"))
                    {
                        var database = client.GetDatabase("Warcraft_LibraryDB");
                        var myHeroesCollection = database.GetCollection<BsonDocument>("My_Heroes");

                        string base64Image = null;
                        if (picHero.Image != null)
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                picHero.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                base64Image = Convert.ToBase64String(ms.ToArray());
                            }
                        }

                        var filter = Builders<BsonDocument>.Filter.Eq("_id", currentHeroId.Value);
                        var update = Builders<BsonDocument>.Update
                            .Set("name", txtName.Text)
                            .Set("race", cbRace.SelectedItem?.ToString() ?? "Unknown")
                            .Set("abilities", txtAbilities.Text)
                            .Set("biography", txtBio.Text)
                            .Set("imageBase64", base64Image ?? "")
                            .Set("updatedAt", DateTime.Now);

                        var result = await myHeroesCollection.UpdateOneAsync(filter, update);

                        if (result.ModifiedCount > 0)
                        {
                            MessageBox.Show("Hero updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            previewName.Text = $"Name: {txtName.Text}";
                            previewRace.Text = $"Race: {cbRace.SelectedItem?.ToString() ?? "Unknown"}";
                            previewAbilitiesBox.Text = txtAbilities.Text;
                            previewBioBox.Text = txtBio.Text;
                            previewImage.Image = picHero.Image != null ? (Image)picHero.Image.Clone() : null;
                        }
                        else
                        {
                            MessageBox.Show("No changes were made.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error editing hero: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            container.Controls.Add(btnEdit);

            Button btnDelete = new Button
            {
                Text = "Delete Hero",
                BackColor = Color.FromArgb(180, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = 180,
                Height = 45,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(startX + 380, container.Height - 80) 
            };
            btnDelete.FlatAppearance.BorderSize = 0;
         
            btnDelete.Click += async (sender, e) =>
            {
                if (currentHeroId == null)
                {
                    MessageBox.Show("No hero to delete!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var confirmResult = MessageBox.Show("Are you sure you want to delete this hero?",
                                                    "Confirm Delete",
                                                    MessageBoxButtons.YesNo,
                                                    MessageBoxIcon.Warning);
                if (confirmResult != DialogResult.Yes)
                    return;

                try
                {
                    using (var client = new MongoClient("mongodb://localhost:27017"))
                    {
                        var database = client.GetDatabase("Warcraft_LibraryDB");
                        var myHeroesCollection = database.GetCollection<BsonDocument>("My_Heroes");

                        var filter = Builders<BsonDocument>.Filter.Eq("_id", currentHeroId.Value);
                        var result = await myHeroesCollection.DeleteOneAsync(filter);

                        if (result.DeletedCount > 0)
                        {
                            MessageBox.Show("Hero deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            currentHeroId = null;

                            previewName.Text = "Name: (empty)";
                            previewRace.Text = "Race: (empty)";
                            previewAbilitiesBox.Text = "";
                            previewBioBox.Text = "";
                            previewImage.Image?.Dispose();
                            previewImage.Image = null;

                            txtName.Text = "";
                            cbRace.SelectedIndex = -1;
                            txtAbilities.Text = "";
                            txtBio.Text = "";
                            picHero.Image?.Dispose();
                            picHero.Image = null;
                        }
                        else
                        {
                            MessageBox.Show("Hero not found or already deleted.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting hero: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            container.Controls.Add(btnDelete);
        }

        private void LoadTavern()
        {
            mainContent.Controls.Clear();

            mainContent.BackgroundImage = Image.FromFile("C:/Users/Lenovo/Pictures/morgan-howell-img-1760.jpg");
            mainContent.BackgroundImageLayout = ImageLayout.Stretch;

            Panel cardGamePanel = new Panel
            {
                Dock = DockStyle.Fill, 
                BackColor = Color.FromArgb(120, 0, 0, 0),
                Padding = new Padding(10)
            };
            mainContent.Controls.Add(cardGamePanel);

            Panel addCardPanel = new Panel
            {
                Width = 300,
                Dock = DockStyle.Left,
                BackColor = Color.FromArgb(150, 20, 20, 20), 
                Padding = new Padding(15)
            };
            mainContent.Controls.Add(addCardPanel);

            Font titleFont = new Font("Georgia", 16, FontStyle.Bold);
            Font labelFont = new Font("Georgia", 11, FontStyle.Bold);

            Label lblTitle = new Label
            {
                Text = "Add New Card",
                Font = titleFont,
                ForeColor = Color.Gold,
                BackColor = Color.Transparent,
                AutoSize = true
            };
            addCardPanel.Controls.Add(lblTitle);

            Label lblName = new Label
            {
                Text = "Card Name:",
                Font = labelFont,
                ForeColor = Color.Gold,
                BackColor = Color.Transparent,
                Top = 50,
                Left = 10,
                AutoSize = true
            };
            addCardPanel.Controls.Add(lblName);

            TextBox txtName = new TextBox
            {
                Top = 75,
                Left = 10,
                Width = 250,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            addCardPanel.Controls.Add(txtName);

            Label lblImage = new Label
            {
                Text = "Card Image:",
                Font = labelFont,
                ForeColor = Color.Gold,
                BackColor = Color.Transparent,
                Top = 120,
                Left = 10,
                AutoSize = true
            };
            addCardPanel.Controls.Add(lblImage);

            PictureBox picCard = new PictureBox
            {
                Top = 145,
                Left = 10,
                Width = 150,
                Height = 150,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            addCardPanel.Controls.Add(picCard);

            Button btnUpload = new Button
            {
                Text = "Upload Image",
                Top = 300,
                Left = 10,
                Width = 150,
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(70, 50, 20),  
                ForeColor = Color.Gold,
                Font = new Font("Georgia", 10, FontStyle.Bold)
            };
            btnUpload.FlatAppearance.BorderColor = Color.FromArgb(120, 90, 40);
            btnUpload.FlatAppearance.BorderSize = 2;
            btnUpload.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, 70, 30);
            btnUpload.Click += (s, e) =>
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "Image Files|*.png;*.jpg;*.jpeg";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    picCard.Image = Image.FromFile(ofd.FileName);
                }
            };
            addCardPanel.Controls.Add(btnUpload);

            Label lblPower = new Label
            {
                Text = "Power (Stars):",
                Font = labelFont,
                ForeColor = Color.Gold,
                BackColor = Color.Transparent,
                Top = 345,
                Left = 10,
                AutoSize = true
            };
            addCardPanel.Controls.Add(lblPower);

            ComboBox cmbPower = new ComboBox
            {
                Top = 370,
                Left = 10,
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };
            cmbPower.Items.AddRange(new object[] { 1, 2, 3, 4, 5 });
            cmbPower.SelectedIndex = 0;
            addCardPanel.Controls.Add(cmbPower);

            Button btnAdd = new Button
            {
                Text = "Add Card",
                Top = 415,
                Left = 10,
                Width = 150,
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 70, 20), 
                ForeColor = Color.Gold,
                Font = new Font("Georgia", 10, FontStyle.Bold)
            };
            btnAdd.FlatAppearance.BorderColor = Color.FromArgb(80, 120, 40);
            btnAdd.FlatAppearance.BorderSize = 2;
            btnAdd.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 120, 30);
            btnAdd.Click += async (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Please enter a card name.");
                    return;
                }

                if (picCard.Image == null)
                {
                    MessageBox.Show("Please upload a card image.");
                    return;
                }

                try
                {
                    byte[] imageBytes;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        picCard.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        imageBytes = ms.ToArray();
                    }

                    var client = new MongoClient("mongodb://localhost:27017");
                    var database = client.GetDatabase("Warcraft_LibraryDB");
                    var cards = database.GetCollection<BsonDocument>("cards");

                    var cardDoc = new BsonDocument
                    {
                        { "name", txtName.Text },
                        { "power", Convert.ToInt32(cmbPower.SelectedItem) },
                        { "image", imageBytes },
                        { "createdAt", DateTime.Now }
                    };

                    await cards.InsertOneAsync(cardDoc);

                    MessageBox.Show("Card successfully added to the Tavern!", "Success");

                    txtName.Clear();
                    cmbPower.SelectedIndex = 0;
                    picCard.Image = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error adding card: " + ex.Message);
                }
            };

            addCardPanel.Controls.Add(btnAdd);

            Button btnRefresh = new Button
            {
                Text = "Refresh Cards",
                Top = btnAdd.Top + btnAdd.Height + 10, 
                Left = 10,
                Width = 150,
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(70, 50, 20), 
                ForeColor = Color.Gold,
                Font = new Font("Georgia", 10, FontStyle.Bold)
            };
            btnRefresh.FlatAppearance.BorderColor = Color.FromArgb(120, 90, 40);
            btnRefresh.FlatAppearance.BorderSize = 2;
            btnRefresh.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, 70, 30);
            addCardPanel.Controls.Add(btnRefresh);

            Panel playerCardsPanel = new Panel
            {
                Dock = DockStyle.Fill, 
                AutoScroll = true,     
                BackColor = Color.FromArgb(100, 0, 0, 0)
            };
            cardGamePanel.Controls.Add(playerCardsPanel);

            FlowLayoutPanel buttonsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(10)
            };
            cardGamePanel.Controls.Add(buttonsPanel);

            Button btnShowCards = new Button
            {
                Text = "Show Cards",
                Width = 120,
                Height = 40,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(70, 50, 20),
                ForeColor = Color.Gold,
                Font = new Font("Georgia", 10, FontStyle.Bold)
            };
            btnShowCards.FlatAppearance.BorderColor = Color.FromArgb(120, 90, 40);
            btnShowCards.FlatAppearance.BorderSize = 2;
            btnShowCards.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, 70, 30);
            buttonsPanel.Controls.Add(btnShowCards);

            Button btnReset = new Button
            {
                Text = "Reset",
                Width = 120,
                Height = 40,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(70, 50, 20),
                ForeColor = Color.Gold,
                Font = new Font("Georgia", 10, FontStyle.Bold)
            };
            btnReset.FlatAppearance.BorderColor = Color.FromArgb(120, 90, 40);
            btnReset.FlatAppearance.BorderSize = 2;
            btnReset.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, 70, 30);
            buttonsPanel.Controls.Add(btnReset);

            var client2 = new MongoClient("mongodb://localhost:27017");
            var database2 = client2.GetDatabase("Warcraft_LibraryDB");
            var cardsCollection = database2.GetCollection<BsonDocument>("cards");
            var allCards = cardsCollection.Find(new BsonDocument()).ToList();

            List<BsonDocument> playerPicked = new List<BsonDocument>();
            List<BsonDocument> botPicked = new List<BsonDocument>();

            int cardsPerRow = 7;
            int cardWidth = 100;
            int cardHeight = 140;
            int spacing = 15;

            Dictionary<BsonDocument, PictureBox> cardBoxes = new Dictionary<BsonDocument, PictureBox>();

            for (int i = 0; i < allCards.Count; i++)
            {
                PictureBox cardBack = new PictureBox
                {
                    Width = cardWidth,
                    Height = cardHeight,
                    Top = (i / cardsPerRow) * (cardHeight + spacing + 40), 
                    Left = (i % cardsPerRow) * (cardWidth + spacing),
                    Image = Image.FromFile("C:/Users/Lenovo/Pictures/card_back.png"),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    BorderStyle = BorderStyle.FixedSingle,
                    Tag = allCards[i]
                };

                cardBack.Click += (s, e) =>
                {
                    if (playerPicked.Count >= 2) return;
                    PictureBox pb = s as PictureBox;
                    var card = pb.Tag as BsonDocument;
                    if (!playerPicked.Contains(card))
                    {
                        playerPicked.Add(card);
                        pb.BorderStyle = BorderStyle.Fixed3D;
                    }
                };

                playerCardsPanel.Controls.Add(cardBack);
                cardBoxes[allCards[i]] = cardBack;
            }

            btnShowCards.Click += (s, e) =>
            {
                if (playerPicked.Count < 2)
                {
                    MessageBox.Show("Pick 2 cards first!");
                    return;
                }

                Random rnd = new Random();
                botPicked = allCards.Except(playerPicked).OrderBy(x => rnd.Next()).Take(2).ToList();

                void AddCardInfo(BsonDocument card)
                {
                    var pb = cardBoxes[card];

                    byte[] imgBytes = card["image"].AsByteArray;
                    using (MemoryStream ms = new MemoryStream(imgBytes))
                    {
                        pb.Image = Image.FromStream(ms);
                    }

                    Label cardNameLabel = new Label
                    {
                        Text = card["name"].AsString,
                        ForeColor = Color.Gold,
                        BackColor = Color.FromArgb(150, 0, 0, 0),
                        Font = new Font("Georgia", 10, FontStyle.Bold),
                        AutoSize = true,
                        Top = pb.Top + pb.Height + 2,
                        Left = pb.Left,
                        MaximumSize = new Size(pb.Width, 0), 
                        Tag = "name_label"
                    };
                    playerCardsPanel.Controls.Add(cardNameLabel);


                    int power = card["power"].AsInt32;
                    string stars = new string('★', power);

                    Label starsLabel = new Label
                    {
                        Text = stars,
                        ForeColor = Color.Gold,
                        BackColor = Color.FromArgb(150, 0, 0, 0),
                        Font = new Font("Georgia", 10, FontStyle.Bold),
                        AutoSize = true,
                        Top = cardNameLabel.Top + cardNameLabel.Height,
                        Left = pb.Left,
                        Tag = "star_label"
                    };
                    playerCardsPanel.Controls.Add(starsLabel);
                }

                foreach (var card in playerPicked)
                    AddCardInfo(card);

                foreach (var card in botPicked)
                {
                    AddCardInfo(card);
                    var pb = cardBoxes[card];
                    pb.BorderStyle = BorderStyle.FixedSingle; 
                }

                int playerPower = playerPicked.Sum(c => c["power"].AsInt32);
                int botPower = botPicked.Sum(c => c["power"].AsInt32);

                string winner = playerPower > botPower ? "You win!" :
                                playerPower < botPower ? "Bot wins!" : "Draw!";

                ShowBattleResult(playerPower, botPower);
            };

            btnReset.Click += (s, e) =>
            {
                playerPicked.Clear();
                botPicked.Clear();

                var labelsToRemove = playerCardsPanel.Controls.OfType<Label>()
                    .Where(l => l.Tag != null && (l.Tag.ToString() == "star_label" || l.Tag.ToString() == "name_label"))
                    .ToList();

                foreach (var label in labelsToRemove)
                {
                    playerCardsPanel.Controls.Remove(label);
                }

                Random rnd = new Random();
                var shuffledCards = allCards.OrderBy(x => rnd.Next()).ToList();

                cardBoxes.Clear();

                for (int i = 0; i < shuffledCards.Count; i++)
                {
                    var card = shuffledCards[i];
                    var pb = playerCardsPanel.Controls.OfType<PictureBox>()
                        .FirstOrDefault(p => p.Tag == card);

                    if (pb != null)
                    {
                        pb.Top = (i / cardsPerRow) * (cardHeight + spacing + 40); 
                        pb.Left = (i % cardsPerRow) * (cardWidth + spacing);

                        pb.Image = Image.FromFile("C:/Users/Lenovo/Pictures/card_back.png");
                        pb.BorderStyle = BorderStyle.FixedSingle;

                        cardBoxes[card] = pb;
                    }
                }
            };

            btnRefresh.Click += (s, e) =>
            {
                playerCardsPanel.Controls.Clear();
                cardBoxes.Clear();
                playerPicked.Clear();
                botPicked.Clear();

                var allCardsRefreshed = cardsCollection.Find(new BsonDocument()).ToList();

                cardWidth = 100;
                cardHeight = 140;
                spacing = 15;

                for (int i = 0; i < allCardsRefreshed.Count; i++)
                {
                    PictureBox cardBack = new PictureBox
                    {
                        Width = cardWidth,
                        Height = cardHeight,
                        Top = (i / cardsPerRow) * (cardHeight + spacing + 40), 
                        Left = (i % cardsPerRow) * (cardWidth + spacing),
                        Image = Image.FromFile("C:/Users/Lenovo/Pictures/card_back.png"),
                        SizeMode = PictureBoxSizeMode.StretchImage,
                        BorderStyle = BorderStyle.FixedSingle,
                        Tag = allCardsRefreshed[i]
                    };

                    cardBack.Click += (sender, args) =>
                    {
                        if (playerPicked.Count >= 2) return;
                        PictureBox pb = sender as PictureBox;
                        var card = pb.Tag as BsonDocument;
                        if (!playerPicked.Contains(card))
                        {
                            playerPicked.Add(card);
                            pb.BorderStyle = BorderStyle.Fixed3D;
                        }
                    };

                    playerCardsPanel.Controls.Add(cardBack);
                    cardBoxes[allCardsRefreshed[i]] = cardBack;
                }
            };
        }

        private void ShowBattleResult(int playerPower, int botPower)
        {
            string winner = playerPower > botPower ? "You win!" :
                            playerPower < botPower ? "Bot wins!" : "Draw!";

            Form battleResultForm = new Form
            {
                Width = 400,
                Height = 250,
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(30, 30, 30),
                Padding = new Padding(10)
            };

            Label title = new Label
            {
                Text = "⚔ Battle Result ⚔",
                ForeColor = Color.Gold,
                Font = new Font("Georgia", 16, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 50
            };
            battleResultForm.Controls.Add(title);

            Label details = new Label
            {
                Text = $"Your Total Power: {playerPower}\nBot Total Power: {botPower}\n\nWinner: {winner}",
                ForeColor = Color.White,
                Font = new Font("Georgia", 12, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Width = battleResultForm.ClientSize.Width - 20,
                Height = 120,
                Top = title.Bottom + 10,
                Left = 10
            };
            battleResultForm.Controls.Add(details);

            Button btnOK = new Button
            {
                Text = "OK",
                Width = 100,
                Height = 35,
                BackColor = Color.FromArgb(70, 50, 20),
                ForeColor = Color.Gold,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Georgia", 10, FontStyle.Bold),
                Top = details.Bottom + 10,
                Left = (battleResultForm.ClientSize.Width - 100) / 2
            };
            btnOK.FlatAppearance.BorderColor = Color.FromArgb(120, 90, 40);
            btnOK.FlatAppearance.BorderSize = 2;
            btnOK.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, 70, 30);
            btnOK.Click += (s, e) => battleResultForm.Close();
            battleResultForm.Controls.Add(btnOK);

            battleResultForm.ShowDialog();
        }
    }
}