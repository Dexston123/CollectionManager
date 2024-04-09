using System;
using System.Collections.Generic;
using System.Linq;

namespace CollectionManager
{
    public class Item
    {
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public int Quantity { get; set; }
        public Condition Condition { get; set; }
        public OwnershipStatus OwnershipStatus { get; set; }
        public int Rating { get; set; }
        public Dictionary<string, CustomFieldValue> CustomValues { get; set; } = new Dictionary<string, CustomFieldValue>();

        public override string ToString()
        {
            var customValuesSerialized = string.Join("|", CustomValues.Select(kv => $"{kv.Key}={kv.Value.DataType}:{kv.Value.Value}"));
            return $"{Name};{ImagePath};{Quantity};{(int)Condition};{(int)OwnershipStatus};{Rating};{customValuesSerialized}";
        }

        public static Item FromString(string data)
        {
            var parts = data.Split(';');
            var item = new Item
            {
                Name = parts[0],
                ImagePath = parts[1],
                Quantity = int.Parse(parts[2]),
                Condition = (Condition)int.Parse(parts[3]),
                OwnershipStatus = (OwnershipStatus)int.Parse(parts[4]),
                Rating = int.Parse(parts[5]),
                CustomValues = new Dictionary<string, CustomFieldValue>()
            };

            if (parts.Length > 6)
            {
                var customValues = parts[6].Split('|');
                foreach (var pair in customValues)
                {
                    var keyValue = pair.Split('=');
                    if (keyValue.Length == 2)
                    {
                        var dataTypeAndValue = keyValue[1].Split(':');
                        if (dataTypeAndValue.Length == 2)
                        {
                            item.CustomValues[keyValue[0]] = new CustomFieldValue { DataType = dataTypeAndValue[0], Value = dataTypeAndValue[1] };
                        }
                    }
                }
            }

            return item;
        }
    }

    public class CustomFieldValue
    {
        public string Value { get; set; }
        public string DataType { get; set; }
    }

    public enum Condition
    {
        New,
        Used,
        Refurbished
    }

    public enum OwnershipStatus
    {
        Owned,
        ForSale,
        Sold,
        Wanted
    }
}