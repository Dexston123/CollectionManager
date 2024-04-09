using Microsoft.Maui.Controls;
using System;
using System.IO;
using System.Linq;

namespace CollectionManager
{
    public partial class EditCollectionPage : ContentPage
    {
        private Collection _collection;
        private string defaultImagePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CollectionManagerData", "default_image.jpg");

        public EditCollectionPage(Collection collection)
        {
            InitializeComponent();
            _collection = collection;
            LoadCollectionDetails();
        }

        private void LoadCollectionDetails()
        {
            NameEntry.Text = _collection.Name;
            DescriptionEntry.Text = _collection.Description;
            CollectionImage.Source = !string.IsNullOrEmpty(_collection.ImagePath) ? ImageSource.FromFile(_collection.ImagePath) : ImageSource.FromFile(defaultImagePath);
        }

        private async void SaveButton_Clicked(object sender, EventArgs e)
        {
            _collection.Name = NameEntry.Text;
            _collection.Description = DescriptionEntry.Text;

            if (string.IsNullOrWhiteSpace(_collection.ImagePath))
            {
                _collection.ImagePath = defaultImagePath;
            }

            _collection.SaveToFile();

            await DisplayAlert("Saved", "Collection has been saved successfully.", "OK");
            await Navigation.PopAsync();
        }

        private async void PickImageButton_Clicked(object sender, EventArgs e)
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images,
                PickerTitle = "Select an image for the collection"
            });

            if (result != null)
            {
                var collectionFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CollectionManagerData", _collection.Name);
                string fileName = Path.GetFileName(result.FullPath);
                string newFilePath = Path.Combine(collectionFolderPath, fileName);

                if (!Directory.Exists(collectionFolderPath))
                {
                    Directory.CreateDirectory(collectionFolderPath);
                }
                else
                {
                    var existingImage = Directory.GetFiles(collectionFolderPath).FirstOrDefault(f => f.EndsWith(Path.GetExtension(newFilePath)));
                    if (existingImage != null && existingImage != defaultImagePath)
                    {
                        File.Delete(existingImage);
                    }
                }

                File.Copy(result.FullPath, newFilePath, true);
                _collection.ImagePath = newFilePath;
                CollectionImage.Source = ImageSource.FromFile(newFilePath);
            }
            else if (string.IsNullOrWhiteSpace(_collection.ImagePath))
            {
                _collection.ImagePath = defaultImagePath;
                CollectionImage.Source = ImageSource.FromFile(defaultImagePath);
            }
        }

        private async void DeleteCollectionButton_Clicked(object sender, EventArgs e)
        {
            var confirm = await DisplayAlert("Delete", "Are you sure you want to delete this collection?", "Yes", "No");
            if (confirm)
            {
                var collectionFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CollectionManagerData", _collection.Name);
                if (Directory.Exists(collectionFolderPath))
                {
                    Directory.Delete(collectionFolderPath, true);
                }

                await DisplayAlert("Deleted", "Collection has been deleted.", "OK");
                await Navigation.PopToRootAsync();
            }
        }
    }
}
