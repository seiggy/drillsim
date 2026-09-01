using OSDC.DotnetLibraries.General.DataManagement;
using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Model
{
    public class UnitConversionSet
    {
        /// <summary>
        /// a MetaInfo for the UnitConversionSet
        /// </summary>
        public MetaInfo? MetaInfo { get; set; }

        /// <summary>
        /// name of the data
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// a description of the data
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// the date when the data was created
        /// </summary>
        public DateTimeOffset? CreationDate { get; set; }

        /// <summary>
        /// the date when the data was last modified
        /// </summary>
        public DateTimeOffset? LastModificationDate { get; set; }

        /// <summary>
        /// an input list of QuantityUnitConversion
        /// </summary>
        public List<QuantityUnitConversion>? QuantityUnitConversionList { get; set; }

        /// <summary>
        /// default constructor required for parsing the data model as a json file
        /// </summary>
        public UnitConversionSet() : base()
        {
        }

        /// <summary>
        /// the main calculation method for UnitConversionSet
        /// </summary>
        /// <returns></returns>
        public bool Calculate()
        {
            if (QuantityUnitConversionList != null)
            {
                foreach (QuantityUnitConversion quantityUnitConversion in QuantityUnitConversionList)
                {
                    if (!quantityUnitConversion.Calculate())
                        return false;
                }
                return true;
            }
            return false;
        }
    }
}
