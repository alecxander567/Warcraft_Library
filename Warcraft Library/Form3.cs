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

namespace Warcraft_Library
{

    public partial class Form3 : Form
    {

        private string currentUsername;

        public Form3(string username)
        {
            currentUsername = username;
            InitializeComponent();
            LoadProfileImage();
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

                if (user != null && user.Contains("ProfileImage") && user["ProfileImage"].AsBsonBinaryData.Bytes.Length > 0)
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
    }
}
