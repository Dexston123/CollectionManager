using Microsoft.Maui.Controls;
using System;
using System.Linq;
using System.Globalization;

namespace CollectionManager
{
    public partial class EditItemPage : ContentPage
    {
        private Item _item;
        private int _additionalFieldsCount = 0;
        private const int MaxAdditionalFields = 30;
        private Collection _collection;

        public EditItemPage(Item item, Collection collection)
        {
            InitializeComponent();
            _item = item;
            _collection = collection;
            BindingContext = _item;

            NameEntry.Text = _item.Name;
            QuantityEntry.Text = _item.Quantity.ToString();
            ConditionPicker.SelectedItem = _item.Condition.ToString();
            OwnershipStatusPicker.SelectedItem = _item.OwnershipStatus.ToString();
            RatingEntry.Text = _item.Rating.ToString();
            ItemImage.Source = ImageSource.FromFile(_item.ImagePath);

            foreach (var customValue in _item.CustomValues)
            {
                AddCustomField(customValue.Key, customValue.Value.Value, customValue.Value.DataType);
            }
        }

        private void AddCustomField(string key, string value, string dataType)
        {
            var keyEntry = new Entry { Text = key, Placeholder = "Key" };
            var typePicker = new Picker { Title = "Select type" };
            typePicker.ItemsSource = new List<string> { "Text", "Number", "Date" };
            var valueEntry = new Entry { Text = value, Placeholder = "Value" };

            if (!string.IsNullOrEmpty(dataType))
            {
                typePicker.SelectedItem = dataType;
            }
            else
            {
                typePicker.SelectedIndex = 0;
            }

            var removeButton = new Button { Text = "Remove", BackgroundColor = Colors.Red, TextColor = Colors.White };
            removeButton.Clicked += (s, e) =>
            {
                AdditionalValuesLayout.Children.Remove((s as Button).Parent as StackLayout);
            };

            var fieldLayout = new StackLayout { Orientation = StackOrientation.Horizontal };
            fieldLayout.Children.Add(keyEntry);
            fieldLayout.Children.Add(typePicker);
            fieldLayout.Children.Add(valueEntry);
            fieldLayout.Children.Add(removeButton);

            AdditionalValuesLayout.Children.Add(fieldLayout);
        }


        private void AddAdditionalFieldButton_Clicked(object sender, EventArgs e)
        {
            if (_additionalFieldsCount >= MaxAdditionalFields) return;

            var keyEntry = new Entry { Placeholder = "Key" };
            var typePicker = new Picker { Title = "Select type" };
            typePicker.ItemsSource = new List<string> { "Text", "Number", "Date" };
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
            if (string.IsNullOrWhiteSpace(NameEntry.Text))
            {
                await DisplayAlert("B³¹d", "Nazwa przedmiotu jest wymagana.", "OK");
                return;
            }

            if (!int.TryParse(QuantityEntry.Text, out int quantity) || quantity <= 0)
            {
                await DisplayAlert("B³¹d", "Iloœæ musi byæ liczb¹ wiêksz¹ od 0.", "OK");
                return;
            }

            if (!int.TryParse(RatingEntry.Text, out int rating) || rating < 1 || rating > 10)
            {
                await DisplayAlert("B³¹d", "Ocena musi byæ liczb¹ pomiêdzy 1 a 10.", "OK");
                return;
            }

            if (ConditionPicker.SelectedIndex == -1)
            {
                await DisplayAlert("B³¹d", "Wybór stanu przedmiotu jest wymagany.", "OK");
                return;
            }

            if (OwnershipStatusPicker.SelectedIndex == -1)
            {
                await DisplayAlert("B³¹d", "Wybór statusu posiadania przedmiotu jest wymagany.", "OK");
                return;
            }

            _item.Name = NameEntry.Text;
            _item.Quantity = quantity;
            _item.Rating = rating;
            _item.Condition = (Condition)Enum.Parse(typeof(Condition), ConditionPicker.SelectedItem.ToString());
            _item.OwnershipStatus = (OwnershipStatus)Enum.Parse(typeof(OwnershipStatus), OwnershipStatusPicker.SelectedItem.ToString());

            _item.CustomValues.Clear();
            foreach (var child in AdditionalValuesLayout.Children.OfType<StackLayout>())
            {
                var keyEntry = child.Children[0] as Entry;
                var typePicker = child.Children[1] as Picker;
                var valueEntry = child.Children[2] as Entry;

                if (keyEntry == null || valueEntry == null || typePicker == null || string.IsNullOrWhiteSpace(keyEntry.Text) || string.IsNullOrWhiteSpace(valueEntry.Text) || typePicker.SelectedIndex == -1)
                {
                    await DisplayAlert("B³¹d", "Wszystkie pola niestandardowe musz¹ byæ wype³nione, w tym wybór typu danych.", "OK");
                    return;
                }

                _item.CustomValues[keyEntry.Text] = new CustomFieldValue { Value = valueEntry.Text, DataType = typePicker.Items[typePicker.SelectedIndex] };
            }

            bool isDuplicate = _collection.Items.Any(existingItem =>
                existingItem != _item &&
                existingItem.Name == _item.Name &&
                existingItem.Quantity == _item.Quantity &&
                existingItem.Condition == _item.Condition &&
                existingItem.OwnershipStatus == _item.OwnershipStatus &&
                existingItem.Rating == _item.Rating &&
                existingItem.CustomValues.SequenceEqual(_item.CustomValues)
            );

            if (isDuplicate)
            {
                bool confirm = await DisplayAlert("Duplicate", "An item with the same data already exists in the collection. Do you want to add a duplicate?", "Yes", "No");
                if (!confirm) return;
            }

            _collection.SaveToFile();
            await DisplayAlert("Success", "Changes have been saved.", "OK");
            await Navigation.PopAsync();
        }

        private async void DeleteItemButton_Clicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Confirmation", "Are you sure you want to delete this item?", "Yes", "No");
            if (confirm)
            {
                _collection.Items.Remove(_item);
                _collection.SaveToFile();
                await DisplayAlert("Deleted", "The item has been removed.", "OK");
                await Navigation.PopAsync();
            }
        }
    }
}
