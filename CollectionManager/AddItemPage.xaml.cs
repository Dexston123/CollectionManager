using Microsoft.Maui.Controls;
using System;
using System.Linq;
using System.Globalization;

namespace CollectionManager
{
    public partial class AddItemPage : ContentPage
    {
        private Item _item;
        private int _additionalFieldsCount = 0;
        private const int MaxAdditionalFields = 30;
        private Collection _collection;

        public AddItemPage(Item item, Collection collection)
        {
            InitializeComponent();
            _item = item ?? new Item();
            _collection = collection;
            BindingContext = _item;
        }

        private void AddAdditionalFieldButton_Clicked(object sender, EventArgs e)
        {
            if (_additionalFieldsCount >= MaxAdditionalFields) return;

            var keyEntry = new Entry { Placeholder = "Key" };
            var typePicker = new Picker { Title = "Select type" };
            typePicker.ItemsSource = new List<string> { "Text", "Number", "Date"};
            var valueEntry = new Entry { Placeholder = "Value" };
            var removeButton = new Button { Text = "Remove", BackgroundColor = Colors.Red, TextColor = Colors.White };

            typePicker.SelectedIndexChanged += (sender, e) =>
            {
                var selectedType = typePicker.SelectedItem as string;
                switch (selectedType)
                {
                    case "Number":
                        valueEntry.Keyboard = Keyboard.Numeric;
                        break;
                    case "Date":
                        valueEntry.Placeholder = "Select date...";
                        valueEntry.IsReadOnly = true;
                        valueEntry.Focused += (sender, e) => ShowDatePicker(valueEntry);
                        break;
                    default:
                        valueEntry.Keyboard = Keyboard.Text;
                        break;
                }
            };

            var fieldLayout = new StackLayout { Orientation = StackOrientation.Horizontal };
            fieldLayout.Children.Add(keyEntry);
            fieldLayout.Children.Add(typePicker);
            fieldLayout.Children.Add(valueEntry);
            fieldLayout.Children.Add(removeButton);

            removeButton.Clicked += (s, e) =>
            {
                AdditionalValuesLayout.Children.Remove(fieldLayout);
                _additionalFieldsCount--;
            };

            AdditionalValuesLayout.Children.Add(fieldLayout);
            _additionalFieldsCount++;
        }


        private async void PickItemImage_Clicked(object sender, EventArgs e)
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images,
                PickerTitle = "Select an image for the item"
            });

            if (result != null)
            {
                var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CollectionManagerData", _collection.Name, "ItemPhotos");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var defaultImagePath = @"C:\Users\lambo\source\repos\CollectionManager\CollectionManager\Resources\Images\default_image.png";

                if (!string.IsNullOrWhiteSpace(_item.ImagePath) && File.Exists(_item.ImagePath) && _item.ImagePath != defaultImagePath)
                {
                    File.Delete(_item.ImagePath);
                }

                var fileName = Path.GetFileName(result.FullPath);
                var filePath = Path.Combine(folderPath, fileName);

                var uniqueFilePath = filePath;
                if (File.Exists(filePath))
                {
                    var fileExtension = Path.GetExtension(filePath);
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                    uniqueFilePath = Path.Combine(folderPath, $"{fileNameWithoutExtension}_{Guid.NewGuid()}{fileExtension}");
                }

                File.Copy(result.FullPath, uniqueFilePath, true);

                _item.ImagePath = uniqueFilePath;

                ItemImage.Source = ImageSource.FromFile(uniqueFilePath);
            }
        }


        private void ShowDatePicker(Entry entry)
        {
            var modalPage = new ContentPage();
            var datePicker = new DatePicker { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };

            datePicker.DateSelected += (sender, e) =>
            {
                entry.Text = datePicker.Date.ToString("d", CultureInfo.InvariantCulture);
                Navigation.PopModalAsync();
            };

            modalPage.Content = datePicker;
            Navigation.PushModalAsync(modalPage);
        }

        private async void SaveButton_Clicked(object sender, EventArgs e)
        {
            _item.Name = NameEntry.Text;

            if (string.IsNullOrWhiteSpace(_item.Name))
            {
                await DisplayAlert("Error", "The item name is required.", "OK");
                return;
            }

            if (!int.TryParse(QuantityEntry.Text, out int quantity) || quantity <= 0)
            {
                await DisplayAlert("Error", "Quantity must be a number greater than 0.", "OK");
                return;
            }
            else
            {
                _item.Quantity = quantity;
            }

            if (!int.TryParse(RatingEntry.Text, out int rating) || rating < 1 || rating > 10)
            {
                await DisplayAlert("Error", "Rating must be a number from 1 to 10.", "OK");
                return;
            }
            else
            {
                _item.Rating = rating;
            }

            if (ConditionPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Error", "Selecting the condition of the item is required.", "OK");
                return;
            }
            else
            {
                _item.Condition = (Condition)Enum.Parse(typeof(Condition), ConditionPicker.SelectedItem.ToString());
            }

            if (OwnershipStatusPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Error", "Selecting the ownership status of the item is required.", "OK");
                return;
            }
            else
            {
                _item.OwnershipStatus = (OwnershipStatus)Enum.Parse(typeof(OwnershipStatus), OwnershipStatusPicker.SelectedItem.ToString());
            }

            _item.CustomValues.Clear();

            foreach (var child in AdditionalValuesLayout.Children.OfType<StackLayout>())
            {
                var keyEntry = child.Children[0] as Entry;
                var typePicker = child.Children[1] as Picker;
                var valueEntry = child.Children[2] as Entry;

                if (keyEntry == null || valueEntry == null || typePicker == null)
                {
                    continue;
                }

                var key = keyEntry.Text;
                var value = valueEntry.Text;
                var typeSelectedIndex = typePicker.SelectedIndex;

                if (string.IsNullOrWhiteSpace(key))
                {
                    await DisplayAlert("Error", "Every field must have a specified key.", "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(value))
                {
                    await DisplayAlert("Error", $"Field '{key}' requires a value.", "OK");
                    return;
                }

                if (typeSelectedIndex == -1)
                {
                    await DisplayAlert("Error", $"Field '{key}' requires selecting a data type.", "OK");
                    return;
                }


                var type = typePicker.Items[typeSelectedIndex];

                _item.CustomValues[key] = new CustomFieldValue { Value = value, DataType = type };
            }

            if (string.IsNullOrWhiteSpace(_item.ImagePath))
            {
                string devImagePath = @"C:\Users\lambo\source\repos\CollectionManager\CollectionManager\Resources\Images\default_image.png";
                _item.ImagePath = devImagePath;
            }

            bool isDuplicate = _collection.Items.Any(existingItem =>
                existingItem.Name == _item.Name &&
                existingItem.Quantity == _item.Quantity &&
                existingItem.Condition == _item.Condition &&
                existingItem.OwnershipStatus == _item.OwnershipStatus &&
                existingItem.Rating == _item.Rating &&
                existingItem.CustomValues.Count == _item.CustomValues.Count &&
                existingItem.CustomValues.All(kv => _item.CustomValues.ContainsKey(kv.Key) && _item.CustomValues[kv.Key].Value == kv.Value.Value && _item.CustomValues[kv.Key].DataType == kv.Value.DataType)
            );

            if (isDuplicate)
            {
                bool confirm = await DisplayAlert("Duplicate", "An item with the same data already exists in the collection. Do you want to add a duplicate?", "Yes", "No");
                if (!confirm) return;
            }

            _collection.Items.Add(_item);
            _collection.SaveToFile();

            await DisplayAlert("Success", "The item has been successfully saved.", "OK");
            await Navigation.PopAsync();
        }
    }
}