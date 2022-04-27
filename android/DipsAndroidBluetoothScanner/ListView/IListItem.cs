namespace DipsAndroidBluetoothScanner.ListView
{
    public interface IListItem
    {
        ListItemType ItemType { get; }

        string Text { get; set; }
    }
}