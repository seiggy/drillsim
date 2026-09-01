using OSDC.DotnetLibraries.General.DataManagement;
using OSDC.UnitConversion.Conversion.UnitSystem;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using OSDC.UnitConversion.Conversion.UnitSystem.DrillingEngineering;
using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Model
{
    public class UnitSystemConversionSet
    {
        /// <summary>
        /// a MetaInfo for the UnitSystemConversionSet
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
        /// the Guid of the UnitSystem to convert the list of ValueConversion from
        /// </summary>
        public Guid UnitSystemInID { get; set; }

        /// <summary>
        /// the Guid of the UnitSystem to convert the list of ValueConversion to
        /// </summary>
        public Guid UnitSystemOutID { get; set; }

        /// <summary>
        /// an input list of QuantityConversion
        /// </summary>
        public List<QuantityConversion>? QuantityConversionList { get; set; }

        /// <summary>
        /// default constructor required for parsing the data model as a json file
        /// </summary>
        public UnitSystemConversionSet() : base()
        {
        }

        /// <summary>
        /// the main calculation method for QuantityUnitConversion
        /// </summary>
        /// <returns></returns>
        public bool Calculate()
        {
            bool success = false;
            if (QuantityConversionList != null && UnitSystemInID != Guid.Empty && UnitSystemOutID != Guid.Empty)
            {
                BaseUnitSystem unitSystemIn = DrillingUnitSystem.Get(UnitSystemInID);
                BaseUnitSystem unitSystemOut = DrillingUnitSystem.Get(UnitSystemOutID);
                if (unitSystemIn != null && unitSystemOut != null)
                {
                    foreach (QuantityConversion quantityConversion in QuantityConversionList)
                    {
                        Guid qtyID = quantityConversion.QuantityID;
                        if (quantityConversion == null || quantityConversion.ValueConversionList == null || !ValueConversion.Calculate(DrillingPhysicalQuantity.GetQuantity(qtyID), unitSystemIn.GetChoice(qtyID), unitSystemOut.GetChoice(qtyID), quantityConversion.ValueConversionList))
                            return false;
                    }
                    success = true;
                }
            }
            return success;

        }
    }
}
