using System;
using System.Collections.Generic;
using System.Text;

namespace Blueberry.Dekstop.WindowsApp.Bluetooth
{

    /// <summary>
    /// Details about a Specific Gatt service
    /// <seealso cref="Https://www.bluethooth.com/specifications/gatt/services"/>
    /// </summary>
    public class GattService
    {
        #region Public properties

        /// <summary>
        /// Human readable name for the service
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The uniform Identifier that is unique to this service
        /// </summary>
        public string UniformTypeIdentifier { get; }

        /// <summary>
        /// The 16-bit assigned number for this service.
        /// The bluetooth GATT service UUID contains for this service
        /// </summary>
        public ushort AssignedNumber { get; }

        /// <summary>
        /// The type of profile specification that this service is
        /// <seealso cref="https://bluetooth.com/specifications/gatt/"/>
        /// </summary>
        public string ProfileSpecification { get;  }
        
        #endregion


        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public GattService(string name, string uniformIdentifier, ushort assignedNumber, string profilespecification)
        {
            Name = name;
            UniformTypeIdentifier = uniformIdentifier;
            AssignedNumber = assignedNumber;
            ProfileSpecification = profilespecification;
        }

        #endregion
    }
}
