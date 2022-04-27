namespace DipsAndroidBluetoothScanner.ListView
{
    public class StatusHeaderListItem : IListItem
    {
        public StatusHeaderListItem(string text)
        {
            Text = text;
        }

        public ListItemType ItemType => ListItemType.Status;

        public string Text { get; set; }
    }
}