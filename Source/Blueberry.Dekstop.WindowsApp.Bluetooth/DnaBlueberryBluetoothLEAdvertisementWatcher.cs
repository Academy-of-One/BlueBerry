using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Blueberry.Dekstop.WindowsApp.Bluetooth
{
    /// <summary>
    /// Wraps and makes use of the <see cref="BluetoothLEAdvertisementWatcher"/>
    /// for easier consumption
    /// </summary>
    public class DnaBlueberryBluetoothLEAdvertisementWatcher
    {
        #region Private Members

        /// <summary>
        ///  The underlyning bluetooth watcher class
        /// </summary>
        private readonly BluetoothLEAdvertisementWatcher mWatcher;


        /// <summary>
        /// A list of discovered devices
        /// </summary>
        private readonly Dictionary<string, DnaBluetoothLEDevice> mDeiscoverdDevices = new Dictionary<string, DnaBluetoothLEDevice>();

        /// <summary>
        /// The details about GATT Services
        /// </summary>
        private readonly GattServiceIds mGattServiceIds;

        /// <summary>
        /// A thread lock object for this class
        /// </summary>
        private readonly object mThreadLock = new object();

        #endregion

        #region Public Properties

        /// <summary>
        /// Indicates if this watcher is listening for advertisements
        /// </summary>
        public bool Listening => mWatcher.Status == BluetoothLEAdvertisementWatcherStatus.Started;

        /// <summary>
        /// A list of discovered devices
        /// </summary>
        public IReadOnlyCollection<DnaBluetoothLEDevice> DiscoverdDevices
        {
            get
            {
                // Clean up any timeouts
                CleanupTimeouts();

                // Practice thread-saftey
                lock (mThreadLock)
                {
                    // Convert to read-only list
                    return mDeiscoverdDevices.Values.ToList().AsReadOnly();
                }
            }
        }

        /// <summary>
        /// The timeout in seconds that a device is removed from the <see cref="DiscoverdDevices"/>
        /// list if it is not re-advertised within this time
        /// </summary>
        public int HeartbeatTimeout { get; set; } = 30;

        #endregion


        #region Public Events

        /// <summary>
        /// Fire when the bluetooth watcher stops listening
        /// </summary>
        public event Action StoppedListening = () => { };

        /// <summary>
        /// Fire when the bluetooth watcher starts listening
        /// </summary>
        public event Action StartedListening = () => { };


        /// <summary>
        /// Fired when a new device is discovered
        /// </summary>
        public event Action<DnaBluetoothLEDevice> NewDeviceDiscovered = (device) => {};

        /// <summary>
        /// Fired when a device is discovered
        /// </summary>
        public event Action<DnaBluetoothLEDevice> DeviceDiscovered = (device) => { };

        /// <summary>
        /// Fired when a device name changes
        /// </summary>
        public event Action<DnaBluetoothLEDevice> DeviceNameChanged = (device) => { };

        /// <summary>
        /// Fired when a device is removed fro timing out
        /// </summary>
        public event Action<DnaBluetoothLEDevice> DeviceTimedOut = (device) => { };

        #endregion 


        #region Constructor

        /// <summary>
        /// The default constructor
        /// </summary>
        public DnaBlueberryBluetoothLEAdvertisementWatcher(GattServiceIds gattIds)
        {

            // Null guard
            mGattServiceIds = gattIds ?? throw new ArgumentNullException(nameof(gattIds));

            // Create bluetooth listener
            mWatcher = new BluetoothLEAdvertisementWatcher
            {
                ScanningMode = BluetoothLEScanningMode.Active
            };

            // Listen out for new advertisements
            mWatcher.Received += WatcherAdvertisementRecievedAsync;

            // Listen out for when the watcher stops listening
            mWatcher.Stopped += (watcher, e) =>
            {
                // Informs listeners
                StoppedListening();
            };
        }
        #endregion

        #region Private Methods


        /// <summary>
        /// Listens out for watcher advertisements
        /// </summary>
        /// <param name="sender">The watcher</param>
        /// <param name="args">The arguments</param>
        private async void WatcherAdvertisementRecievedAsync(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            // Cleanup Timeouts
            CleanupTimeouts();


            // Get BLE info
            var device = await GetBluetoothLEDeviceAsync(
                args.BluetoothAddress, 
                args.Timestamp, 
                args.RawSignalStrengthInDBm).ConfigureAwait(true);

            // Null guard

            if (device == null)
                return;

            // Is new discovery?
            var newDiscovery = false;
            var existingName = default(string);

            // Lock the door
            lock (mThreadLock)
            {
                // Check if this is a new discovery
                newDiscovery = !mDeiscoverdDevices.ContainsKey(device.DeviceID);

                if (!newDiscovery)
                    existingName = mDeiscoverdDevices[device.DeviceID].Name;
            }

            // Name changed?
            var nameChanged =
                // If it already exists
                !newDiscovery &&
                // And it is not a blank name
                !string.IsNullOrEmpty(device.Name) &&
                // And the Name is different
                existingName != device.Name;

            lock (mThreadLock)
            {
                // Add /Update the device in the dictionary
                mDeiscoverdDevices[device.DeviceID] = device;
            }

            // Inform listener
            DeviceDiscovered(device);

            // If name changed...
            if (nameChanged)
                // Inform Listeners
                DeviceNameChanged(device);

            // If new discovery...
            if (newDiscovery)
                // inform listener
                NewDeviceDiscovered(device);
        }

        /// <summary>
        /// Connect to the BLE device and extracts information from the device
        /// </summary>
        /// <returns></returns>
        private async Task<DnaBluetoothLEDevice> GetBluetoothLEDeviceAsync(ulong address, DateTimeOffset brodcastTime, short rssi)
        {
            // Get bluetooth device info
            var device = await BluetoothLEDevice.FromBluetoothAddressAsync(address).AsTask();

            // Null guard
            if (device == null)
                return null;

            // Get GATT services that are available
            var gatt = await device.GetGattServicesAsync().AsTask();

            // If we have any services...
            if (gatt.Status == GattCommunicationStatus.Success)
            {
                // loop each GATT Service
                foreach (var services in gatt.Services)
                {
                    // This Id contains the GATT profile assigned number we want!
                    // TODO: get more info and connect
                    var gattProfileId = services.Uuid;
                }
            };

            return new DnaBluetoothLEDevice
                (
                // Device Id
                deviceid: device.DeviceId,

                // BlueTooth Address
                address: device.BluetoothAddress,

                // Device name
                name: device.Name,

                // Time of broadcast
                broadcastTime: brodcastTime,

                // Signal strength
                rssi: rssi,

                connected: device.ConnectionStatus == BluetoothConnectionStatus.Connected,

                canpair: device.DeviceInformation.Pairing.CanPair,

                paired: device.DeviceInformation.Pairing.IsPaired
                );

        }

        /// <summary>
        /// Prune any timed out devices that we have not heard of
        /// </summary>
        private void CleanupTimeouts()
        {
            lock (mThreadLock)
            {
                // The date in time that if less than means a device has timed out
                var threashold = DateTime.UtcNow - TimeSpan.FromSeconds(HeartbeatTimeout);

                // Any devices that have not sent out a new broadcast within the heartbeat time
                mDeiscoverdDevices.Where(f => f.Value.BroadcastTime < threashold).ToList().ForEach(device =>
                {
                    // Remove device
                    mDeiscoverdDevices.Remove(device.Key);

                    // Inform Listeners
                    DeviceTimedOut(device.Value);
                });
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts listening for Advertisements
        /// </summary>
        public void StartListening()
        {
            lock (mThreadLock)
            {
                // If already listening...
                if (Listening)
                    // Do nothing more
                    return;

                // start the underlying watcher
                mWatcher.Start();

            }
            
            // Inform listeners
            StartedListening();
        }

        /// <summary>
        /// Stops listening for advertisements
        /// </summary>
        public void StopListening()
        {
            lock (mThreadLock)
            { 
                // If we have no listening...
                if (!Listening)
                    // Do nothing More
                    return;

                // Stop listening
                mWatcher.Stop();
                
                // Clear devices
                mDeiscoverdDevices.Clear();
            }
        }

        #endregion
    }
}
