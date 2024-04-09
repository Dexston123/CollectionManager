using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CollectionManager
{
    public class Collection
    {
        private string _name;
        public string Id { get; set; }

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    UpdateCollectionFolderName(_name, value);
                    _name = value;
                }
            }
        }

        public string Description { get; set; }
        public string ImagePath { get; set; }
        public List<Item> Items { get; set; } = new List<Item>();
        public int ItemsCount => Items?.Count ?? 0;

        public Collection()
        {
            Id = Guid.NewGuid().ToString();
        }

        private void UpdateCollectionFolderName(string oldName, string newName)
        {
            var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CollectionManagerData");
            var oldPath = Path.Combine(basePath, SanitizeFolderName(oldName));
            var newPath = Path.Combine(basePath, SanitizeFolderName(newName));

            try
            {
                if (Directory.Exists(oldPath))
                {
                    if (Directory.Exists(newPath))
                    {

                    }
                    Directory.Move(oldPath, newPath);
                }
                else
                {
                    Directory.CreateDirectory(newPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating folder name: {ex.Message}");
            }
        }

        private string SanitizeFolderName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return "DefaultName";
            }

            var invalidChars = Path.GetInvalidFileNameChars();
            return new string(name.Where(ch => !invalidChars.Contains(ch)).ToArray());
        }


        public void SaveToFile()
        {
            var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CollectionManagerData", Name);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var filePath = Path.Combine(folderPath, $"{Id}.txt");

            var lines = new List<string>
            {
                Id,
                Name ?? string.Empty,
                Description ?? string.Empty,
                ImagePath ?? string.Empty
            };

            foreach (var item in Items)
            {
                lines.Add(item.ToString());
            }

            File.WriteAllLines(filePath, lines);
        }

        public static Collection LoadFromFile(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            var collection = new Collection
            {
                Id = lines[0],
                Name = lines[1],
                Description = lines[2],
                ImagePath = lines[3],
                Items = new List<Item>()
            };

            for (int i = 4; i < lines.Length; i++)
            {
                collection.Items.Add(Item.FromString(lines[i]));
            }

            return collection;
        }
    }
}
