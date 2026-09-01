using System.Collections.Generic;
using System;

namespace OSDC.UnitConversion.Model
{
    /// <summary>
    /// An intermediate class that contains a list of ValueConversion assigned to a given Quantity
    /// </summary>
    public class QuantityConversion
    {
        /// <summary>
        /// the ID of the physical quantity used for the conversion
        /// </summary>
        public Guid QuantityID { get; set; }
        /// <summary>
        /// a list of numerical data to be converted.
        /// </summary>
        public List<ValueConversion>? ValueConversionList { get; set; }

        /// <summary>
        /// default constructor required for parsing the data model as a json file
        /// </summary>
        public QuantityConversion() : base()
        {
        }
    }
}
