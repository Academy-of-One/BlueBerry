using System;

namespace Blueberry.Dekstop.WindowsApp.Bluetooth
{

    /// <summary>
    /// Information about a BLE device
    /// </summary>
    public class DnaBluetoothLEDevice
    {
        #region Public Properties
        /// <summary>
        /// The time of the broadcast advertisement message of the device
        /// </summary>
        public DateTimeOffset BroadcastTime { get; }

        /// <summary>
        /// Address of the device
        /// </summary>
        public ulong Address { get; }

        /// <summary>
        /// The name of the device
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// The signal strength in db
        /// </summary>
        public short SignalStreangthinDB { get; }


        /// <summary>
        /// Indicates if we are connected to the device
        /// </summary>
        public bool Connected { get; }

        /// <summary>
        /// Indicates if the device supports pairing
        /// </summary>
        public bool CanPair { get; }

        /// <summary>
        /// Indicates if we are currently paired to this device
        /// </summary>
        public bool Paired { get; }

        /// <summary>
        /// The permanent unique id of this device
        /// </summary>
        public string DeviceID { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="address">The BT device address</param>
        /// <param name="name"> Device name</param>
        /// <param name="rssi">Signal Strength</param>
        /// <param name="broadcastTime">The broadcast time of the discover</param>
        /// <param name="connected">if connected to the device</param>
        /// <param name="canpair">If we are paired with the device</param>
        /// <param name="paired">If we can pair to the device</param>
        /// <param name="deviceid">Unique ID of the device</param>
        public DnaBluetoothLEDevice(ulong address,
            string name, 
            short rssi, 
            DateTimeOffset broadcastTime,
            bool connected,
            bool canpair,
            bool paired,
            string deviceid
            )
        {
            Address = address;
            Name = name;
            SignalStreangthinDB = rssi;
            BroadcastTime = broadcastTime;
            Connected = connected;
            CanPair = canpair;
            Paired = paired;
            DeviceID = deviceid;
        }

        #endregion

        /// <summary>
        /// User friendly ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{ (string.IsNullOrEmpty(Name) ? " [No Nname]" : Name ) } [{DeviceID}] ({SignalStreangthinDB})";
        }
    }
}
