using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.BuildHat.Models
{
    /// <summary>
    /// Provides data for the PropertyChanged
    /// event.
    /// </summary>
    public class PropertyChangedEventArgs : EventArgs
    {
        private readonly string propertyName;

        /// <summary>
        /// Initializes a new instance of the PropertyChangedEventArg
        /// class.
        /// </summary>
        public PropertyChangedEventArgs(string propertyName)
        {
            this.propertyName = propertyName;
        }

        /// <summary>
        /// Indicates the name of the property that changed.
        /// </summary>
        public virtual string PropertyName
        {
            get
            {
                return propertyName;
            }
        }
    }
}
