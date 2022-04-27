using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DipsAndroidBluetoothScanner.ListView
{
    public class BluetoothListDataItem : Java.Lang.Object, IListItem
    {
        private string _name;
        private IReadOnlyCollection<string> _services;

        public BluetoothListDataItem(string address) : this(address, "", default) { }

        public BluetoothListDataItem(string address, string name) : this(address, name, default) { }

        public BluetoothListDataItem(string address, string name, IReadOnlyCollection<string> services)
        {
            if (string.IsNullOrEmpty(address))
            {
                throw new ArgumentNullException("Address cannot be null or empty");
            }

            Address = address;
            Name = name;
            Services = services;
        }

        public string Address { get; }

        public ListItemType ItemType => ListItemType.DataItem;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                Text = new StringBuilder($"{Address}")
                    .Append(string.IsNullOrEmpty(Name) ? string.Empty : $" ({Name})")
                    .ToString();
            }
        }

        public int Rssi { get; set; } = default;

        public IReadOnlyCollection<string> Services
        {
            get => _services;
            set
            {
                _services = value;
                SubText = _services is null || _services.Count == 0
                          ? string.Empty
                          : string.Join(", ", _services.Select(u => u?.ToString()));
            }
        }

        public string Text { get; set; }

        public string SubText { get; set; }
    }
}