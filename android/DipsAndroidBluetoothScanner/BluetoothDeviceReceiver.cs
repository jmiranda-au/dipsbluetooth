using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Util;
using BluetoothDeviceScanner.ListView;

namespace BluetoothDeviceScanner
{
    public class BluetoothDeviceReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var action = intent.Action;
            var device = intent.GetParcelableExtra(BluetoothDevice.ExtraDevice) as BluetoothDevice;

            switch (action)
            {
                case BluetoothDevice.ActionUuid:
                    Log.Debug("DEBUG", $"UUIDS for device {device.Address} fetched");
                    break;

                case BluetoothDevice.ActionFound:
                    // Only update the adapter with items which are not bonded
                    if (device.BondState != Bond.Bonded)
                    {
                        MainActivity.GetInstance().UpdateAdapter(new BluetoothListDataItem(device.Address,
                                                                                           device.Name,
                                                                                           null));
                        Log.Debug("DEBUG", $"{device.Address} {device.Name} => {device.GetUuids()}");
                    }

                    break;

                case BluetoothAdapter.ActionDiscoveryStarted:
                    MainActivity.GetInstance().UpdateListAdapterStatus("Discovery Started...");
                    break;

                case BluetoothAdapter.ActionDiscoveryFinished:
                    MainActivity.GetInstance().UpdateListAdapterStatus("Discovery Finished.");
                    break;

                case BluetoothDevice.ActionBondStateChanged:
                    var previousBondState = (Bond)intent.GetIntExtra(BluetoothDevice.ExtraPreviousBondState, -1);
                    var currentBondState = (Bond)intent.GetIntExtra(BluetoothDevice.ExtraBondState, -1);

                    static string BondDescription(Bond value) => value switch
                    {
                        Bond.Bonded => "BONDED",
                        Bond.Bonding => "BONDING",
                        Bond.None => "NONE",
                        _ => "ERROR"
                    };

                    Log.Debug("BOND", $"Bond state for {device.Address} changed: "
                                      + $"{BondDescription(previousBondState)} to "
                                      + $"{BondDescription(currentBondState)}");
                    break;

                default:
                    break;
            }
        }
    }
}