# DiPS (Distributed and Pervasive Systems) Bluetooth tutorial

This repository is composed by an Android application written in Xamarin (C#) and a Python application, that enables Bluetooth Low Energy (BLE) advertising in the device where it is running.

The Android application (placed in the `android` directory) discovers advertising BLE devices. If set (in the code), the application can filter the devices that advertisesspecific UUIDs - in the available code, the application filter the devices that advertise the UUID *0000cafe-0000-1000-8000-00805f9b34fb*, i.e. the UUID that the Python application advertises.

The Python application (placed in the `peripheral` directory) only advertises the UUID *0000cafe-0000-1000-8000-00805f9b34fb*. This application can only be run on Linux systems, such as the Raspberry Pi, because it uses the BlueZ stack. The application is limited to Linux because, right now, there isn't yet any mulitplatform Bluetooth library that enables to turn your device into a BLE peripheral.

## Build and run the Android application

The minimum required Android version to run the application is 6; it is targeted at version 11 and it was tested in version 11 and 12. To build the application, you must download Visual Studio and install Xamarin. More inforamtion on this can be found [here](https://docs.microsoft.com/en-us/xamarin/get-started/installation/windows).

Once the application is running, it displays the device's RSSI and its advertised UUIDs.

## Run Python application

First install the package python3-dbus using the command `sudo python3 install python3-dbus`. To run the actual application, execute `sudo python3 advertise.py`. To stop the application, press CTRL+C.

For the applciation to work, the `bluetooth` service must be running in your device. To verify it, execute `sudo systemctl status bluetooth` - it must report *running*. If the application fails to start, restart the `bluetooth` service using the command `sudo systemctl restart bluetooth` - the same can be accomplished by repalcing restart by `start` or `stop`.


# Further reading

To get a better understanding of Bluetooth LowEnergy and its implementation in Android and Linux, the reading of the following articles is advised:

* [The Ultimate Guide to Android Bluetooth Low Energy](https://punchthrough.com/android-ble-guide/)
* Making Android BLE work: [part 1](https://medium.com/@martijn.van.welie/making-android-ble-work-part-1-a736dcd53b02), [part 2](https://medium.com/@martijn.van.welie/making-android-ble-work-part-2-47a3cdaade07), [part 3](https://medium.com/@martijn.van.welie/making-android-ble-work-part-3-117d3a8aee23), [part 4](https://medium.com/@martijn.van.welie/making-android-ble-work-part-4-72a0b85cb442)
* [How To Use Android BLE to Communicate with Bluetooth Devices - An Overview & Code examples](https://medium.com/@shahar_avigezer/bluetooth-low-energy-on-android-22bc7310387a)
* [Bluetooth Low Energy (BTLE) Peripherals with Raspberry Pi](https://www.raspberrypi-bluetooth.com)