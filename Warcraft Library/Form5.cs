using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MongoDB.Bson;

namespace Warcraft_Library
{
    public partial class FormViewHero : Form
    {
        public FormViewHero(BsonDocument hero)
        {
            InitializeComponent();
            InitializeHeroView(hero);
        }

        private void InitializeHeroView(BsonDocument hero)
        {
            this.Text = hero.GetValue("Name", "").AsString;
            this.ClientSize = new Size(520, 650);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(35, 30, 30);

            PictureBox pic = new PictureBox
            {
                Size = new Size(400, 250),
                Location = new Point(60, 20),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle
            };
            if (hero.Contains("Image") && hero["Image"].IsBsonBinaryData)
            {
                byte[] imgBytes = hero["Image"].AsByteArray;
                using (MemoryStream ms = new MemoryStream(imgBytes))
                    pic.Image = Image.FromStream(ms);
            }

            Label lblName = new Label
            {
                Text = hero.GetValue("Name", "").AsString,
                Font = new Font("Goudy Stout", 16, FontStyle.Bold), 
                ForeColor = Color.Gold,
                Size = new Size(400, 40),
                Location = new Point(60, 280),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblRace = new Label
            {
                Text = hero.GetValue("Race", "").AsString,
                Font = new Font("Segoe UI", 13, FontStyle.Italic),
                ForeColor = Color.LightGray,
                Size = new Size(400, 25),
                Location = new Point(60, 330),
                TextAlign = ContentAlignment.MiddleCenter
            };

            TextBox txtBio = new TextBox
            {
                Text = hero.GetValue("Biography", "").AsString,
                Location = new Point(60, 370),
                Size = new Size(400, 230),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 12),
                BackColor = Color.FromArgb(45, 40, 40),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            Panel borderPanel = new Panel
            {
                Size = new Size(460, 590),
                Location = new Point(30, 15),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(50, 45, 45)
            };

            this.Controls.Add(borderPanel);
            this.Controls.Add(pic);
            this.Controls.Add(lblName);
            this.Controls.Add(lblRace);
            this.Controls.Add(txtBio);

            pic.BringToFront();
            lblName.BringToFront();
            lblRace.BringToFront();
            txtBio.BringToFront();
        }
    }
}
