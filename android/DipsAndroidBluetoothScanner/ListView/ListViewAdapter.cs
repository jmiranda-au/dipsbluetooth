using System;
using System.Collections.Generic;
using System.Text;
using Android.Content;
using Android.Views;
using Android.Widget;

namespace DipsAndroidBluetoothScanner.ListView
{
    public class BluetoothListViewAdapter : ArrayAdapter<IListItem>
    {
        private readonly Context _context;
        private readonly LayoutInflater _inflater;

        public BluetoothListViewAdapter(Context context, IList<IListItem> items) : base(context, 0, items)
        {
            _context = context;
            Items = items;
            _inflater = (LayoutInflater)_context.GetSystemService(Context.LayoutInflaterService);
        }

        public override int Count => Items.Count;

        public IList<IListItem> Items { get; set; }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;

            try
            {
                var item = Items[position];

                switch (item.ItemType)
                {
                    case ListItemType.Header:
                        var headerItem = (HeaderListItem)item;
                        view = _inflater.Inflate(Resource.Layout.ListViewHeaderItem, null);
                        view.Clickable = false;

                        var headerName = view.FindViewById<TextView>(Resource.Id.txtHeader);
                        headerName.Text = headerItem.Text;
                        break;

                    case ListItemType.Status:
                        var statusItem = (StatusHeaderListItem)item;
                        view = _inflater.Inflate(Resource.Layout.ListViewStatusItem, null);
                        view.Clickable = false;

                        var statusText = view.FindViewById<TextView>(Resource.Id.txtStatus);
                        statusText.Text = statusItem.Text;
                        break;

                    case ListItemType.DataItem:
                        var contentItem = (BluetoothListDataItem)item;
                        view = _inflater.Inflate(Resource.Layout.ListViewContentItem, null);

                        var title = view.FindViewById<TextView>(Resource.Id.txtTitle);
                        var subTitle = view.FindViewById<TextView>(Resource.Id.txtSubTitle);
                        var rssi = view.FindViewById<TextView>(Resource.Id.txtRssi);

                        title.Text = contentItem.Text;
                        subTitle.Text = contentItem.SubText;
                        rssi.Text = new StringBuilder("RSSI: ").Append(contentItem.Rssi != default
                                                                       ? $"{contentItem.Rssi}"
                                                                       : "(not advertising)")
                                                               .ToString();
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(_context, ex.Message, ToastLength.Long);
            }

            return view;
        }
    }
}