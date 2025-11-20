using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MongoDB.Bson;

namespace Warcraft_Library
{
    public partial class FormViewItem : Form
    {
        public FormViewItem(BsonDocument item)
        {
            InitializeComponent();
            InitializeItemView(item);
        }

        private void InitializeItemView(BsonDocument item)
        {
            this.Text = item.GetValue("Name", "").AsString;
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
            if (item.Contains("Image") && item["Image"].IsBsonBinaryData)
            {
                byte[] imgBytes = item["Image"].AsByteArray;
                using (MemoryStream ms = new MemoryStream(imgBytes))
                    pic.Image = Image.FromStream(ms);
            }

            Label lblName = new Label
            {
                Text = item.GetValue("Name", "").AsString,
                Font = new Font("Papyrus", 16, FontStyle.Bold),
                ForeColor = Color.Gold,
                Size = new Size(400, 40),
                Location = new Point(60, 280),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblOwner = new Label
            {
                Text = "Owner: " + item.GetValue("Owner", "").AsString,
                Font = new Font("Segoe UI", 13, FontStyle.Italic),
                ForeColor = Color.LightGray,
                Size = new Size(400, 25),
                Location = new Point(60, 330),
                TextAlign = ContentAlignment.MiddleCenter
            };

            TextBox txtAbilities = new TextBox
            {
                Text = item.GetValue("Abilities", "").AsString,
                Location = new Point(60, 370),
                Size = new Size(400, 80),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 12, FontStyle.Italic),
                BackColor = Color.FromArgb(60, 60, 70), 
                ForeColor = Color.LightGreen,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtAbilities.TabStop = false;

            TextBox txtDesc = new TextBox
            {
                Text = item.GetValue("Description", "").AsString,
                Location = new Point(60, 460),
                Size = new Size(400, 140),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 12),
                BackColor = Color.FromArgb(45, 40, 40),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            txtDesc.TabStop = false;

            this.Controls.Add(pic);
            this.Controls.Add(lblName);
            this.Controls.Add(lblOwner);
            this.Controls.Add(txtAbilities);
            this.Controls.Add(txtDesc);

            pic.BringToFront();
            lblName.BringToFront();
            lblOwner.BringToFront();
            txtAbilities.BringToFront();
            txtDesc.BringToFront();
        }
    }
}
