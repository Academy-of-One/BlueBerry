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
        /// The time of the brodcast advertisement message of the device
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
        /// The signal streanght in db
        /// </summary>
        public short SignalStreangthinDB { get; }
        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DnaBluetoothLEDevice(ulong address, string name, short rssi, DateTimeOffset broadcastTime)
        {
            Address = address;
            Name = name;
            SignalStreangthinDB = rssi;
            BroadcastTime = broadcastTime;
        }

        #endregion

        /// <summary>
        /// User friendly ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{ (string.IsNullOrEmpty(Name) ? " [No Nname]" : Name ) } {Address} ({SignalStreangthinDB})";
        }
    }
}
