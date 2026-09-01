using System;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public class UnitSystemLight
    {
        /// <summary>
        /// a guid for the light version of a unit system
        /// </summary>
        public Guid ID { get; set; }
        /// <summary>
        /// a name for the light version of a unit system
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// a description of the light version of a unit system
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// true if it is one the default light version of a unit system
        /// </summary>
        public bool IsDefault { get; set; }
        /// <summary>
        /// true if it is the SI light version of a unit system
        /// </summary>
        public bool IsSI { get; set; }
    }
}