using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Warcraft_Library
{
    public partial class FormAddItem : Form
    {
        private PictureBox picItem;
        private TextBox txtName, txtOwner, txtAbilities, txtDesc;
        private Button btnSave, btnCancel, btnUploadImage;
        private Panel mainPanel;
        private byte[] selectedImageBytes = null;
        private ObjectId? currentItemId = null;
        private string currentUsername;

        public string ItemName => txtName.Text;
        public string ItemOwner => txtOwner.Text;
        public string ItemAbilities => txtAbilities.Text;
        public string ItemDescription => txtDesc.Text;
        public byte[] ItemImageBytes => selectedImageBytes;

        public FormAddItem(string currentUsername)
        {
            BuildForm();
            this.currentUsername = currentUsername;
        }

        private void BuildForm()
        {
            this.Text = "Add New Item";
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
                Text = "🛡️ Add New Item 🛡️",
                ForeColor = Color.Gold,
                Font = new Font("Papyrus", 22, FontStyle.Bold),
                AutoSize = false,
                Height = 60,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter
            };
            mainPanel.Controls.Add(lblHeader);

            Label lblImage = new Label()
            {
                Text = "Item Image:",
                ForeColor = Color.Gold,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(50, 80),
                AutoSize = true
            };
            mainPanel.Controls.Add(lblImage);

            picItem = new PictureBox()
            {
                Location = new Point(200, 80),
                Size = new Size(200, 200),
                BackColor = Color.FromArgb(60, 60, 70),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            mainPanel.Controls.Add(picItem);

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

            CreateLabelAndTextbox("Item Name:", 300, out txtName);
            CreateLabelAndTextbox("Owner:", 360, out txtOwner);
            CreateLabelAndTextbox("Abilities:", 420, out txtAbilities); 

            Label lblDesc = new Label()
            {
                Text = "Description:",
                ForeColor = Color.Gold,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(50, 480)
            };
            mainPanel.Controls.Add(lblDesc);

            txtDesc = new TextBox()
            {
                Location = new Point(200, 478),
                Width = 370,
                Height = 100,
                Multiline = true,
                Font = new Font("Segoe UI", 12),
                BackColor = Color.FromArgb(60, 60, 70),
                ForeColor = Color.White,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.FixedSingle
            };
            mainPanel.Controls.Add(txtDesc);

            btnSave = new Button()
            {
                Text = "Save Item",
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

        private void CreateLabelAndTextbox(string text, int yPos, out TextBox txtBox)
        {
            Label lbl = new Label()
            {
                Text = text,
                ForeColor = Color.Gold,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(50, yPos)
            };
            mainPanel.Controls.Add(lbl);

            txtBox = new TextBox()
            {
                Location = new Point(200, yPos - 2),
                Width = 370,
                Font = new Font("Segoe UI", 12),
                BackColor = Color.FromArgb(60, 60, 70),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            mainPanel.Controls.Add(txtBox);
        }

        private void BtnUploadImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            ofd.Title = "Select Item Image";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                selectedImageBytes = File.ReadAllBytes(ofd.FileName);
                picItem.Image = Image.FromFile(ofd.FileName);
            }
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please fill in the Item Name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var client = new MongoClient("mongodb://localhost:27017");
                var db = client.GetDatabase("Warcraft_LibraryDB");
                var collection = db.GetCollection<BsonDocument>("Items");

                var doc = new BsonDocument
                {
                    { "Name", txtName.Text },
                    { "Owner", txtOwner.Text },       
                    { "Uploader", currentUsername },  
                    { "Abilities", txtAbilities.Text },
                    { "Description", txtDesc.Text },
                    { "Image", selectedImageBytes ?? new byte[0] }
                };


                if (currentItemId.HasValue)
                {
                    var filter = Builders<BsonDocument>.Filter.Eq("_id", currentItemId.Value);
                    await collection.ReplaceOneAsync(filter, doc);
                    MessageBox.Show("Item updated successfully!", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    await collection.InsertOneAsync(doc);
                    MessageBox.Show("Item added successfully!", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving item: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void SetItemData(BsonDocument item)
        {
            currentItemId = item.GetValue("_id").AsObjectId;
            txtName.Text = item.GetValue("Name", "").AsString;
            txtOwner.Text = item.GetValue("Owner", "").AsString;
            txtAbilities.Text = item.GetValue("Abilities", "").AsString;
            txtDesc.Text = item.GetValue("Description", "").AsString;

            if (item.Contains("Image") && item["Image"].AsByteArray.Length > 0)
            {
                selectedImageBytes = item["Image"].AsByteArray;
                using (var ms = new MemoryStream(selectedImageBytes))
                {
                    picItem.Image = Image.FromStream(ms);
                }
            }
        }
    }
}
