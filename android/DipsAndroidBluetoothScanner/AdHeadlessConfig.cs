using System;
using Android.Bluetooth;
using Java.Util;

namespace BluetoothDeviceScanner
{
    public class AdHeadlessConfig : IAdGattService
    {
        /* UUIDS structure
         * adxx = Aliviate Development UUIDs
         * ad0x = Aliviate's services, where x denotes the service number
         * adcx = Aliviate's characteristics, where x denotes the characteristic number
         * addx = Aliviate's descriptors, where x denotes the descriptor number
         */

        public static readonly UUID UUID_ADHEADLESS_SERVICE = UUID.FromString("0000ad00-0000-1000-8000-00805f9b34fb");
        public static readonly UUID UUID_ADHEADLESS_CHARACTERISTIC = UUID.FromString("0000adc0-0000-1000-8000-00805f9b34fb");
        public static readonly UUID UUID_ADHEADLESS_DESCRIPTOR = UUID.FromString("0000add0-0000-1000-8000-00805f9b34fb");

        public AdHeadlessConfig()
        {
        }

        public AdGattDescription GattDescription => new()
        {
            Service = Guid.Parse("0000ad00-0000-1000-8000-00805f9b34fb"),
            PrimaryCharacteristic = Guid.Parse("0000adc0-0000-1000-8000-00805f9b34fb"),
            PrimaryDescriptor = Guid.Parse("0000add0-0000-1000-8000-00805f9b34fb")
        };

        //public AdMeasurement<T> ParseCharacteristic<T>(BluetoothGattCharacteristic characteristic)
        //{
        //    throw new NotImplementedException();
        //}

        public dynamic ParseCharacteristic(BluetoothGattCharacteristic characteristic)
            => throw new NotImplementedException();

        //void ReadCharacteristic(UUID characteristic) { }

        //void WriteCharacteristic<T>(UUID characteristic, T data) { }
    }
}
