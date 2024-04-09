using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;

namespace CollectionManager
{
    public partial class CollectionsPage : ContentPage
    {
        public ObservableCollection<Collection> Collections { get; private set; } = new ObservableCollection<Collection>();

        public CollectionsPage()
        {
            InitializeComponent();
            LoadCollections();
            BindingContext = this;
        }

        private void LoadCollections()
        {
            Collections.Clear();
            var baseFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CollectionManagerData");

            var collectionFolders = Directory.GetDirectories(baseFolderPath);

            foreach (var folder in collectionFolders)
            {
                try
                {
                    var files = Directory.GetFiles(folder, "*.txt");

                    foreach (var file in files)
                    {
                        var collection = Collection.LoadFromFile(file);
                        var imageExtensions = new List<string> { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff" };
                        var imageFile = Directory.GetFiles(folder)
                                        .FirstOrDefault(file => imageExtensions.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)));

                        if (!string.IsNullOrEmpty(imageFile))
                        {
                            collection.ImagePath = imageFile;
                        }

                        Collections.Add(collection);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading collections from folder {folder}: {ex.Message}");
                }
            }
        }


        private async void AddCollectionButton_Clicked(object sender, EventArgs e)
        {
            var newCollection = new Collection { Name = "New Collection", Description = "Describe your collection" };
            await Navigation.PushAsync(new AddCollectionPage(newCollection, true));
        }

        private async void EditCollectionButton_Clicked(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                var collection = button.BindingContext as Collection;
                if (collection != null)
                {
                    await Navigation.PushAsync(new EditCollectionPage(collection));
                }
            }
        }

        private async void OnCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedCollection = e.CurrentSelection.FirstOrDefault() as Collection;
            if (selectedCollection != null)
            {
                (sender as CollectionView).SelectedItem = null;

                await Navigation.PushAsync(new CollectionDetailsPage(selectedCollection));
            }
        }

        private async void ExportCollectionButton_Clicked(object sender, EventArgs e)
        {
            var selectedCollection = Collections.FirstOrDefault();

            if (selectedCollection == null)
            {
                await DisplayAlert("Error", "No collection selected for export.", "OK");
                return;
            }

            var zipFileName = $"{selectedCollection.Name}.zip";
            var sourceFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CollectionManagerData", selectedCollection.Name);
            var destinationFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var zipFilePath = Path.Combine(destinationFolderPath, zipFileName);

            try
            {
                if (File.Exists(zipFilePath))
                {
                    File.Delete(zipFilePath);
                }

                ZipFile.CreateFromDirectory(sourceFolderPath, zipFilePath);

                await DisplayAlert("Success", "The collection has been successfully exported.", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while exporting the collection: {ex.Message}");
                await DisplayAlert("Error", "Failed to export the collection.", "OK");
            }
        }

        private async void ImportCollectionButton_Clicked(object sender, EventArgs e)
        {
            var pickResult = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Select archive file",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { "public.zip-archive" } },
                    { DevicePlatform.Android, new[] { "application/zip" } },
                    { DevicePlatform.WinUI, new[] { ".zip" } },
                    { DevicePlatform.MacCatalyst, new[] { "public.zip-archive" } }
                })
            });


            if (pickResult != null)
            {
                var zipFilePath = pickResult.FullPath;
                var collectionName = Path.GetFileNameWithoutExtension(zipFilePath);
                var checkCollectionsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CollectionManagerData");
                var collectionExists = Directory.Exists(checkCollectionsPath) && Directory.GetDirectories(checkCollectionsPath).Any();

                string action;
                if (collectionExists)
                {
                    action = await DisplayActionSheet("Collection Import", null, null, "Import as a new collection", "Consolidate with an existing collection");

                }
                else
                {
                    action = await DisplayActionSheet("Collection Import", null, null, "Import as a new collection");
                }

                if (action == "Cancel" || action == null)
                {
                    return;
                }

                if (action == "Import as a new collection")
                {
                    try
                    {
                        await DisplayAlert("Unpacking", "We are starting the collection import...", "Understood");

                        var zipCollectionName = Path.GetFileNameWithoutExtension(zipFilePath);
                        var targetFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CollectionManagerData", zipCollectionName);

                        if (Directory.Exists(targetFolderPath))
                        {
                            var userChoice = await DisplayActionSheet($"Collection '{zipCollectionName}' already exists. What do you want to do?", null, null, "Import as new (change name)", "Abort process");

                            switch (userChoice)
                            {
                                case "Import as new":
                                    var uniqueSuffix = Guid.NewGuid().ToString().Substring(0, 8);
                                    targetFolderPath += "_" + uniqueSuffix;
                                    break;
                                case "Abort process":
                                    await DisplayAlert("Cancelled", "The import process has been aborted.", "OK");
                                    return;
                            }
                        }

                        ZipFile.ExtractToDirectory(zipFilePath, targetFolderPath, true);

                        LoadCollections();

                        await DisplayAlert("Success", "The collection was successfully imported.", "OK");
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Error", $"An error occurred while importing the collection: {ex.Message}", "OK");
                    }
                }
                else if (action == "Consolidate with an existing collection")
                {
                    var collectionsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CollectionManagerData");

                    var tempFolderPath = Path.Combine(collectionsFolderPath, Guid.NewGuid().ToString());
                    Directory.CreateDirectory(tempFolderPath);

                    try
                    {
                        ZipFile.ExtractToDirectory(zipFilePath, tempFolderPath);

                        var collectionFiles = Directory.GetFiles(tempFolderPath, "*.txt");
                        if (collectionFiles.Length == 0)
                        {
                            throw new FileNotFoundException("Collection file not found in folder.");
                        }

                        var importedCollection = Collection.LoadFromFile(collectionFiles[0]);
                        var collectionNames = Collections.Select(c => c.Name).ToList();
                        var selectedCollectionName = await DisplayActionSheet("Select collection to consolidate", "Cancel", null, collectionNames.ToArray());


                        if (selectedCollectionName == "Anuluj" || string.IsNullOrEmpty(selectedCollectionName))
                        {
                            return;
                        }

                        var selectedCollection = Collections.FirstOrDefault(c => c.Name == selectedCollectionName);

                        if (selectedCollection == null)
                        {
                            await DisplayAlert("Error", "Selected collection not found.", "OK");
                            return;
                        }

                        ConsolidateCollections(importedCollection, selectedCollection);
                    }
                    finally
                    {
                        if (Directory.Exists(tempFolderPath))
                        {
                            Directory.Delete(tempFolderPath, true);
                        }
                    }
                }
            }
        }

        private async void ConsolidateCollections(Collection importedCollection, Collection targetCollection)
        {
            foreach (var importedItem in importedCollection.Items)
            {
                var isDuplicate = targetCollection.Items.Any(existingItem =>
                    existingItem.Name == importedItem.Name &&
                    existingItem.Quantity == importedItem.Quantity &&
                    existingItem.Condition == importedItem.Condition &&
                    existingItem.OwnershipStatus == importedItem.OwnershipStatus &&
                    existingItem.Rating == importedItem.Rating &&
                    existingItem.CustomValues.Count == importedItem.CustomValues.Count &&
                    existingItem.CustomValues.All(kv => importedItem.CustomValues.ContainsKey(kv.Key) && importedItem.CustomValues[kv.Key].Value == kv.Value.Value && importedItem.CustomValues[kv.Key].DataType == kv.Value.DataType));

                if (isDuplicate)
                {
                    var confirmDuplicate = await DisplayAlert("Duplicate", $"Item '{importedItem.Name}' already exists. Do you want to add a duplicate?", "Yes", "No");
                    if (!confirmDuplicate)
                    {
                        continue;
                    }
                }

                targetCollection.Items.Add(importedItem);
            }

            targetCollection.SaveToFile();
            await DisplayAlert("Success", "Collection consolidation completed successfully.", "OK");
            LoadCollections();
        }

        private async void SummaryButton_Clicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var collection = (Collection)button.CommandParameter;
            var summary = GenerateCollectionSummary(collection);
            await DisplayAlert("Collection Summary", summary, "OK");
        }

        private string GenerateCollectionSummary(Collection collection)
        {
            int totalItems = collection.Items.Count;
            int soldItems = collection.Items.Count(item => item.OwnershipStatus == OwnershipStatus.Sold);
            int ownedItems = collection.Items.Count(item => item.OwnershipStatus == OwnershipStatus.Owned);
            int wantedItems = collection.Items.Count(item => item.OwnershipStatus == OwnershipStatus.Wanted);
            int forSaleItems = collection.Items.Count(item => item.OwnershipStatus == OwnershipStatus.ForSale);

            return $"Total number of items: {totalItems}\n" +
                   $"Items sold: {soldItems}\n" +
                   $"Items owned: {ownedItems}\n" +
                   $"Items wanted: {wantedItems}\n" +
                   $"Items for sale: {forSaleItems}";
        }
    }
}
