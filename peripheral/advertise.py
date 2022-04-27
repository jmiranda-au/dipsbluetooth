import signal
from enum import Enum
from typing import Iterable, List, Union
import dbus
import dbus.exceptions
import dbus.mainloop.glib
import dbus.service
from gi.repository import GLib

# List with the services to be advertised. A maximum of two services can be advertised
# simultaneously. If you try to register more, an error will be thrown.
SERVICES_TO_ADVERTISE = ['cafe']

BLUEZ_SERVICE_NAME = 'org.bluez'
DBUS_OM_IFACE = 'org.freedesktop.DBus.ObjectManager'
DBUS_PROP_IFACE = 'org.freedesktop.DBus.Properties'
LE_ADVERTISEMENT_IFACE = 'org.bluez.LEAdvertisement1'
LE_ADVERTISING_MANAGER_IFACE = 'org.bluez.LEAdvertisingManager1'


class GapRole(Enum):
    BROADCAST = "broadcast"
    PERIPHERAL = "peripheral"

    def __str__(self) -> str:
        return self._value_


class InvalidArgsException(dbus.exceptions.DBusException):
    _dbus_error_name = 'org.freedesktop.DBus.Error.InvalidArgs'


class NotSupportedException(dbus.exceptions.DBusException):
    _dbus_error_name = 'org.bluez.Error.NotSupported'


class NotPermittedException(dbus.exceptions.DBusException):
    _dbus_error_name = 'org.bluez.Error.NotPermitted'


class InvalidValueLengthException(dbus.exceptions.DBusException):
    _dbus_error_name = 'org.bluez.Error.InvalidValueLength'


class FailedException(dbus.exceptions.DBusException):
    _dbus_error_name = 'org.bluez.Error.Failed'


class Advertisement(dbus.service.Object):
    PATH_BASE = '/org/bluez/example/advertisement'

    def __init__(self, bus, index, advertising_type):
        self.__path = f"{self.PATH_BASE}{index}"
        self.__service_uuid = None
        self.bus = bus
        self.ad_type = dbus.String(advertising_type)
        self.manufacturer_data = None
        self.solicit_uuids = None
        self.service_data = None
        self.local_name = None
        self.include_tx_power = False
        self.data = None
        super().__init__(bus, self.__path)

    def get_properties(self):
        properties = dict()

        properties['Type'] = self.ad_type

        if self.service_uuid:
            properties['ServiceUUIDs'] = dbus.Array(self.service_uuid,
                                                    signature='s')
        if self.solicit_uuids:
            properties['SolicitUUIDs'] = dbus.Array(self.solicit_uuids,
                                                    signature='s')
        if self.manufacturer_data:
            properties['ManufacturerData'] = dbus.Dictionary(self.manufacturer_data,
                                                             signature='qv')
        if self.service_data:
            properties['ServiceData'] = dbus.Dictionary(self.service_data,
                                                        signature='sv')
        if self.local_name:
            properties['LocalName'] = dbus.String(self.local_name)
        if self.include_tx_power:
            properties['Includes'] = dbus.Array(["tx-power"],
                                                signature='s')
        if self.data:
            properties['Data'] = dbus.Dictionary(self.data,
                                                 signature='yv')

        return {LE_ADVERTISEMENT_IFACE: properties}

    @property
    def path(self):
        return dbus.ObjectPath(self.__path)

    @property
    def service_uuid(self) -> List[str]:
        return self.__service_uuid or []

    @service_uuid.setter
    def service_uuid(self, value: Union[str, Iterable[str]]) -> None:
        if not self.__service_uuid:
            self.__service_uuid = []

        if isinstance(value, str):
            self.__service_uuid = [value]
        elif len(value) <= 2:
            self.__service_uuid = value
        elif len(value) > 2:
            # As defined in the "Supplement to the Bluetooth Core Specification", section 1.1
            raise ValueError("Only two service UUIDs can be advertised simultaneously")

    def add_solicit_uuid(self, uuid):
        if not self.solicit_uuids:
            self.solicit_uuids = []
        self.solicit_uuids.append(uuid)

    def add_manufacturer_data(self, manuf_code, data):
        if not self.manufacturer_data:
            self.manufacturer_data = dbus.Dictionary({}, signature='qv')
        self.manufacturer_data[manuf_code] = dbus.Array(data, signature='y')

    def add_service_data(self, uuid, data):
        if not self.service_data:
            self.service_data = dbus.Dictionary({}, signature='sv')
        self.service_data[uuid] = dbus.Array(data, signature='y')

    def add_data(self, ad_type, data):
        if not self.data:
            self.data = dbus.Dictionary({}, signature='yv')
        self.data[ad_type] = dbus.Array(data, signature='y')

    @dbus.service.method(DBUS_PROP_IFACE,
                         in_signature='s',
                         out_signature='a{sv}')
    def GetAll(self, interface):
        print('GetAll')
        if interface != LE_ADVERTISEMENT_IFACE:
            raise InvalidArgsException()

        return self.get_properties()[LE_ADVERTISEMENT_IFACE]

    @dbus.service.method(LE_ADVERTISEMENT_IFACE,
                         in_signature='',
                         out_signature='')
    def Release(self):
        print(f'{self.__path}: released')


class TestAdvertisement(Advertisement):
    def __init__(self, local_name: str, bus, index: int):
        super().__init__(bus, index, str(GapRole.PERIPHERAL))
        self.service_uuid = SERVICES_TO_ADVERTISE
        self.add_manufacturer_data(0xffff, [0x00, 0x01, 0x02, 0x03])
        self.add_service_data('9999', [0x00, 0x01, 0x02, 0x03, 0x04])
        self.local_name = local_name  # 'TestAdvertisement'
        self.include_tx_power = True
        self.add_data(0x26, [0x01, 0x01, 0x00])


def find_adapter(bus):
    remote_om = dbus.Interface(bus.get_object(BLUEZ_SERVICE_NAME, '/'),
                               DBUS_OM_IFACE)
    objects = remote_om.GetManagedObjects()

    for adapter, props in objects.items():
        if LE_ADVERTISING_MANAGER_IFACE in props:
            return adapter

    return None


def main():
    mainloop = GLib.MainLoop()

    def shutdown(signal_, frame_):
        mainloop.quit()

    def register_ad_cb():
        print('Advertisement registered')

    def register_ad_error_cb(error):
        print(f'Failed to register advertisement: {error}')
        mainloop.quit()

    signal.signal(signal.SIGINT, shutdown)
    signal.signal(signal.SIGTERM, shutdown)

    dbus.mainloop.glib.DBusGMainLoop(set_as_default=True)

    bus = dbus.SystemBus()

    adapter = find_adapter(bus)
    if not adapter:
        print('LEAdvertisingManager1 interface not found')
        return

    # Power on the Bluetooth adapter
    adapter_props = dbus.Interface(bus.get_object(BLUEZ_SERVICE_NAME, adapter),
                                   'org.freedesktop.DBus.Properties')
    adapter_props.Set('org.bluez.Adapter1', 'Powered', dbus.Boolean(1))
    print(f"{adapter_props.Get('org.bluez.Adapter1', 'Roles') = }")

    # Setup the advertisment manager
    test_advertisement = TestAdvertisement("test_advertisement0", bus, 0)

    ad_manager = dbus.Interface(bus.get_object(BLUEZ_SERVICE_NAME, adapter),
                                LE_ADVERTISING_MANAGER_IFACE)
    ad_manager.RegisterAdvertisement(test_advertisement.path, {},
                                     reply_handler=register_ad_cb,
                                     error_handler=register_ad_error_cb)

    print('Advertising')
    mainloop.run()  # blocks until mainloop.quit() is called

    ad_manager.UnregisterAdvertisement(test_advertisement)
    print('Advertisement unregistered')
    dbus.service.Object.remove_from_connection(test_advertisement)


if __name__ == '__main__':
    main()
