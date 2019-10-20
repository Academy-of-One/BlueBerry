using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Bluetooth.Advertisement;

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
        private readonly Dictionary<ulong, DnaBluetoothLEDevice> mDeiscoverdDevices = new Dictionary<ulong, DnaBluetoothLEDevice>();

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
        public DnaBlueberryBluetoothLEAdvertisementWatcher()
        {
            // Create bluetooth listener
            mWatcher = new BluetoothLEAdvertisementWatcher
            {
                ScanningMode = BluetoothLEScanningMode.Active
            };

            // Listen out for new advertisements
            mWatcher.Received += WatcherAdvertisementRecieved;

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
        private void WatcherAdvertisementRecieved(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            // Cleanup Timeouts
            CleanupTimeouts(); 

            DnaBluetoothLEDevice device = null;

            // Is new discovery?
            var newDiscovery = !mDeiscoverdDevices.ContainsKey(args.BluetoothAddress);

            // Name changed?
            var nameChanged =
                // If it already exists
                !newDiscovery &&
                // And it is not a blank name
                !string.IsNullOrEmpty(args.Advertisement.LocalName) &&
                // And the Name is different
                mDeiscoverdDevices[args.BluetoothAddress].Name != args.Advertisement.LocalName;

            lock (mThreadLock)
            {
                // Get the name of the device
                var name = args.Advertisement.LocalName;

                // If new name is blank, and we already have a device..
                if (string.IsNullOrEmpty(name) && !newDiscovery)
                    // Don't override what could actually be a name arleady
                    name = mDeiscoverdDevices[args.BluetoothAddress].Name;

                // Create a new device info class
                device = new DnaBluetoothLEDevice
                (
                    // Bluetooth address
                    address: args.BluetoothAddress,

                    //Name
                    name: name,

                    //Brodcast Time
                    broadcastTime: args.Timestamp,

                    // Signal Strength
                    rssi: args.RawSignalStrengthInDBm
                );

                // Add /Update the device in the dictionary
                mDeiscoverdDevices[args.BluetoothAddress] = device;
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
        /// Prune any timed out devices that we have not heard of
        /// </summary>
        private void CleanupTimeouts()
        {
            lock (mThreadLock)
            {
                // The date in time that if less than means a device has timed out
                var threashold = DateTime.UtcNow - TimeSpan.FromSeconds(HeartbeatTimeout);

                // Any devices that have not sent out a new brodcast within the heartbeat time
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
            // If already listening...
            if (Listening)
                // Do nothing more
                return;

            // start the underlying watcher
            mWatcher.Start();

            // Inform listeners

            StartedListening();
        }

        /// <summary>
        /// Stops listening for advertisements
        /// </summary>
        public void StopListening()
        {
            // If we have no listening...
            if (!Listening)
                // Do nothing More
                return;

            // Stop listening
            mWatcher.Stop();

            lock (mThreadLock)
            {
                // Clear devices
                mDeiscoverdDevices.Clear();
            }
        }

        #endregion
    }
}
