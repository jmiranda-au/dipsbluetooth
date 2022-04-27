using System.Collections.Generic;
using System.Linq;
using Android;
using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Widget;
using DipsAndroidBluetoothScanner.ListView;
using Java.Util;

namespace DipsAndroidBluetoothScanner
{
    internal class AdScanCallback : ScanCallback
    {
        private static readonly string kServiceUuidFilter = "0000cafe-0000-1000-8000-00805f9b34fb";

        /// <summary>
        /// Callback invoked when a device is discovered. In this case, add the device to the list
        /// of discovered devices.
        /// </summary>
        /// <param name="callbackType"></param>
        /// <param name="result"></param>
        public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result)
        {
            var device = result.Device;
            var uuids = result.ScanRecord.ServiceUuids ?? new List<ParcelUuid>();

            // Filter the devices which have the service UUID defined above. If a UUID is not set,
            // then don't filter any device.
            if (!string.IsNullOrEmpty(kServiceUuidFilter) &&
                !uuids.Any(u => u?.ToString().CompareTo(kServiceUuidFilter) == 0))
            { 
                return;
            }

            var dataItem = new BluetoothListDataItem(device.Address,
                                                     device.Name,
                                                     uuids.Select(u => u?.ToString()).ToList())
            { Rssi = result.Rssi };

            MainActivity.GetInstance().UpdateList(dataItem);
        }
    }

    [Activity(Label = "Bluetooth Device Scanner", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private static readonly string[] kBluetoothPermissions =
        {
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.AccessFineLocation,
            Manifest.Permission.Bluetooth,
            Manifest.Permission.BluetoothAdmin
        };
        private const int kLocationPermissionsRequestCode = 1000;
        private const int kRequestBluetoothEnable = 1001;

        private BluetoothAdapter _bluetoothAdapter = default;
        private static MainActivity _instance;
        private bool _isDiscovering = false;

        private BluetoothAdapter BluetoothDefaultAdapter
        {
            get
            {
                if (_bluetoothAdapter == default)
                {
                    using var bluetoothManager = _instance.GetSystemService(BluetoothService) as BluetoothManager;
                    _bluetoothAdapter = bluetoothManager.Adapter;
                }

                return _bluetoothAdapter;
            }
        }

        private ScanCallback ScanCallbackObj { get; set; } = new AdScanCallback();

        public static MainActivity GetInstance()
        {
            return _instance;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            _instance = this;

            SetContentView(Resource.Layout.activity_main);

            if (!BluetoothDefaultAdapter.IsEnabled)
            {
                StartActivityForResult(new Intent(BluetoothAdapter.ActionRequestEnable),
                                       kRequestBluetoothEnable);
            }

            // Check if the Bluetooth permissions for the application are set. If, not, request them
            // to the user.
            var coarseLocationPermission = ContextCompat.CheckSelfPermission(this,
                                                                             Manifest.Permission.AccessCoarseLocation);
            var fineLocationPermission = ContextCompat.CheckSelfPermission(this,
                                                                           Manifest.Permission.AccessFineLocation);

            if (coarseLocationPermission != Permission.Denied
                || fineLocationPermission == Permission.Denied)
                ActivityCompat.RequestPermissions(this,
                                                  kBluetoothPermissions,
                                                  kLocationPermissionsRequestCode);

            PopulateListView();

            StartScanning();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Make sure we're not doing discovery anymore
            StopScanning();
        }

        protected override void OnPause()
        {
            base.OnPause();

            // Make sure we're not doing discovery anymore
            StopScanning();
        }

        protected override void OnResume()
        {
            base.OnResume();

            StartScanning();
        }

        private void StartScanning()
        {
            if (_isDiscovering)
            {
                return;
            }

            // Start the scanning. The ScanCallbackObj is the object that handles the devices that
            // are discovered. 
            BluetoothDefaultAdapter.BluetoothLeScanner.StartScan(ScanCallbackObj);
            _isDiscovering = true;

            Toast.MakeText(this, "Discovering devices", ToastLength.Long).Show();
        }

        private void StopScanning()
        {
            if (!_isDiscovering)
            {
                return;
            }

            BluetoothDefaultAdapter.BluetoothLeScanner.StopScan(ScanCallbackObj);
            _isDiscovering = false;

            Toast.MakeText(this, "Stopped discovering devices", ToastLength.Long).Show();
        }

        private void PopulateListView()
        {
            var items = new List<IListItem>
            {
                new HeaderListItem("PREVIOUSLY PAIRED")
            };

            // Add bonded devices to the list.
            items.AddRange(
                BluetoothDefaultAdapter.BondedDevices.Select(
                    device =>
                    {
                        // Filter out the UUIDs that are default. This is necessary for Android 12.
                        var uuids = device.GetUuids()?
                                          .Where(u => u.Uuid.CompareTo(UUID.FromString("00000000-0000-0000-0000-000000000000")) != 0)
                                          .Select(u => u?.ToString()).ToList();

                        return new BluetoothListDataItem(device.Address, device.Name, uuids);
                    }
                )
            );

            items.Add(new StatusHeaderListItem("DISCOVERED"));

            var lst = FindViewById<Android.Widget.ListView>(Resource.Id.lstview);
            lst.Adapter = new BluetoothListViewAdapter(this, items);
        }

        public void UpdateList(BluetoothListDataItem dataItem)
        {
            var lst = FindViewById<Android.Widget.ListView>(Resource.Id.lstview);
            var adapter = lst.Adapter as BluetoothListViewAdapter;

            // Find if the item is already in the list.
            var dataItemIndex = adapter?.Items.ToList().FindIndex(
                x => x.ItemType == ListItemType.DataItem
                     && ((BluetoothListDataItem)x).Address == dataItem.Address);

            // If the device was not discovered before, then it isn't on the list; thus add it.
            if (dataItemIndex == -1)
            {
                adapter.Items.Add(dataItem);
            }
            // The device was already discovered. Then update it.
            else if (dataItemIndex.HasValue)
            {
                adapter.Items[dataItemIndex.Value] = dataItem;
            }
            adapter.NotifyDataSetChanged();
        }
    }
}