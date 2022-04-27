namespace DipsAndroidBluetoothScanner.ListView
{
    public class HeaderListItem : IListItem
    {
        public HeaderListItem(string text)
        {
            Text = text;
        }

        public ListItemType ItemType => ListItemType.Header;

        public string Text { get; set; }
    }
}