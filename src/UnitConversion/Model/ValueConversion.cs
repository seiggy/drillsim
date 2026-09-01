using OSDC.UnitConversion.Conversion;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Model
{
    /// <summary>
    /// A class that contains a data of type double that needs to be converted, either from SI or to SI
    /// </summary>
    public class ValueConversion
    {
        /// <summary>
        /// the input data either in SI or end-user unit, depending on the conversion type
        /// </summary>
        public double DataIn { get; set; }
        /// <summary>
        /// the out data either in end-user or SI unit, depending on the conversion type
        /// </summary>
        public double? DataOut { get; set; }
        /// <summary>
        /// the formatted string to represent the converted data at meaningful precision
        /// </summary>
        public string? DataOutString { get; set; }
        /// <summary>
        /// default constructor
        /// </summary>
        public ValueConversion() : base()
        {

        }

        public static bool Calculate(BasePhysicalQuantity quantity, UnitChoice unitChoiceIn, UnitChoice unitChoiceOut, List<ValueConversion> valueConversionList)
        {
            bool success = false;
            if (quantity != null && unitChoiceIn != null && unitChoiceOut != null && valueConversionList != null)
            {
                // cases are split to avoid useless computations
                if (unitChoiceIn.IsSI && unitChoiceOut.IsSI)
                {
                    foreach (ValueConversion valueConversion in valueConversionList)
                    {
                        valueConversion.DataOut = valueConversion.DataIn;
                        valueConversion.DataOutString = unitChoiceOut.ToStringInSI((double)valueConversion.DataOut, quantity.MeaningfulPrecisionInSI);
                    }
                }
                else if (unitChoiceIn.IsSI)
                {
                    foreach (ValueConversion valueConversion in valueConversionList)
                    {
                        valueConversion.DataOut = unitChoiceOut.FromSI(valueConversion.DataIn);
                        valueConversion.DataOutString = unitChoiceOut.ToStringInUnit((double)valueConversion.DataOut, quantity.MeaningfulPrecisionInSI);
                    }
                }
                else if (unitChoiceOut.IsSI)
                {
                    foreach (ValueConversion valueConversion in valueConversionList)
                    {
                        valueConversion.DataOut = unitChoiceIn.ToSI(valueConversion.DataIn);
                        valueConversion.DataOutString = unitChoiceOut.ToStringInSI((double)valueConversion.DataOut, quantity.MeaningfulPrecisionInSI);
                    }
                }
                else
                {
                    foreach (ValueConversion valueConversion in valueConversionList)
                    {
                        valueConversion.DataOut = unitChoiceOut.FromSI(unitChoiceIn.ToSI(valueConversion.DataIn));
                        valueConversion.DataOutString = unitChoiceOut.ToStringInUnit((double)valueConversion.DataOut, quantity.MeaningfulPrecisionInSI);
                    }
                }
                success = true;
            }
            return success;
        }
    }
}
