using Microsoft.Maui.Controls;
using System;
using System.IO;

namespace CollectionManager
{
    public partial class AddCollectionPage : ContentPage
    {
        private Collection _collection;
        private bool _isNewCollection;

        public AddCollectionPage(Collection collection, bool isNewCollection)
        {
            InitializeComponent();

            _collection = collection;
            _isNewCollection = isNewCollection;
            NameEntry.Text = _collection.Name;
            DescriptionEntry.Text = _collection.Description;

            if (string.IsNullOrEmpty(_collection.ImagePath))
            {
                _collection.ImagePath = @"C:\Users\lambo\source\repos\CollectionManager\CollectionManager\Resources\Images\default_image.png";
                CollectionImageView.Source = ImageSource.FromFile(_collection.ImagePath);
            }
            else
            {
                CollectionImageView.Source = ImageSource.FromFile(_collection.ImagePath);
            }

            this.Title = _isNewCollection ? "Add New Collection" : "Edit Collection";
        }

        private async void SaveButton_Clicked(object sender, EventArgs e)
        {
            _collection.Name = NameEntry.Text;
            _collection.Description = DescriptionEntry.Text;

            if (string.IsNullOrEmpty(_collection.ImagePath))
            {
                _collection.ImagePath = @"C:\Users\lambo\source\repos\CollectionManager\CollectionManager\Resources\Images\default_image.png";
                CollectionImageView.Source = ImageSource.FromFile(_collection.ImagePath);
            }

            _collection.SaveToFile();

            await DisplayAlert("Saved", "Collection has been saved successfully.", "OK");

            await Navigation.PopAsync();
            MessagingCenter.Send(this, "CollectionUpdated");
        }

        private async void PickImageButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle = "Select an image for the collection"
                });

                if (result != null)
                {
                    var collectionFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CollectionManagerData", _collection.Name);

                    if (!Directory.Exists(collectionFolderPath))
                    {
                        Directory.CreateDirectory(collectionFolderPath);
                    }

                    var fileName = Path.GetFileName(result.FullPath);
                    var newFilePath = Path.Combine(collectionFolderPath, fileName);

                    if (File.Exists(newFilePath))
                    {
                        File.Delete(newFilePath);
                    }

                    File.Copy(result.FullPath, newFilePath);

                    _collection.ImagePath = newFilePath;
                    CollectionImageView.Source = ImageSource.FromFile(newFilePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while picking the image: {ex.Message}");
                await DisplayAlert("Error", "Failed to pick the image.", "OK");
            }
        }
    }
}