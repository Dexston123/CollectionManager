using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace CollectionManager
{
    public partial class CollectionDetailsPage : ContentPage
    {
        private Collection _collection;

        public CollectionDetailsPage(Collection collection)
        {
            InitializeComponent();
            _collection = collection;
            SortAndDisplayItems();
        }

        private void SortAndDisplayItems()
        {
            _collection.Items = _collection.Items
                .OrderBy(item => item.OwnershipStatus, new OwnershipStatusComparer())
                .ToList();
            BindingContext = null;
            BindingContext = _collection;
        }

        private async void AddNewItemButton_Clicked(object sender, EventArgs e)
        {
            var newItem = new Item();
            var addItemPage = new AddItemPage(newItem, _collection);
            addItemPage.Disappearing += async (sender, e) =>
            {
                if (!string.IsNullOrWhiteSpace(newItem.Name))
                {
                    await DisplayAlert("Success", "Item added successfully.", "OK");
                    SortAndDisplayItems();
                }
            };

            await Navigation.PushAsync(addItemPage);
        }

        private async void EditItemButton_Clicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                var item = button.BindingContext as Item;
                if (item != null)
                {
                    var editItemPage = new EditItemPage(item, _collection);
                    editItemPage.Disappearing += async (sender, e) =>
                    {
                        SortAndDisplayItems();
                    };
                    await Navigation.PushAsync(editItemPage);
                }
            }
        }

        public class OwnershipStatusComparer : IComparer<OwnershipStatus>
        {
            public int Compare(OwnershipStatus x, OwnershipStatus y)
            {
                var orderMap = new Dictionary<OwnershipStatus, int>
                {
                    { OwnershipStatus.Owned, 1 },
                    { OwnershipStatus.ForSale, 2 },
                    { OwnershipStatus.Wanted, 3 },
                    { OwnershipStatus.Sold, 4 }
                };

                return orderMap[x].CompareTo(orderMap[y]);
            }
        }
    }
}
