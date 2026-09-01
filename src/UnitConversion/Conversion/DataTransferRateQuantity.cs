using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents data transfer rate.
    /// </summary>
    public partial class DataTransferRateQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "R_b";
        public override string SIUnitName { get; } = "bit per second";
        public override string SIUnitLabelLatex { get; } = "\\frac{bit}{s}";
        public override double TimeDimension { get; } = -1;
        public override double? MeaningfulPrecisionInSI { get; } = 1;

        private static DataTransferRateQuantity instance_ = null;
        public static DataTransferRateQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new DataTransferRateQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "bit per second",
                UnitLabel = "bit/s",
                ID = new Guid("ff3db29b-631c-4dd6-807b-fa1da189f88c"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "kilobit per second",
                UnitLabel = "kbit/s",
                ID = new Guid("8602813f-8dd7-44d5-bfb8-a5500d61535b"),
                ConversionFactorFromSIFormula = "1.0/Factors.Kilo",
            },
            new UnitChoice
            {
                UnitName = "megabit per second",
                UnitLabel = "Mbit/s",
                ID = new Guid("cc4b2ee1-fa42-4d27-8a7d-0ce2cdd553dc"),
                ConversionFactorFromSIFormula = "1.0/Factors.Mega",
            },
            new UnitChoice
            {
                UnitName = "megabyte per second",
                UnitLabel = "MB/s",
                ID = new Guid("773f4b75-1292-46ec-9c27-cafa7e00280a"),
                ConversionFactorFromSIFormula = "1.0/(Factors.BitsPerByte*Factors.Mega)",
            }
        };

        public DataTransferRateQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "data transfer rate", "bit rate" };
            ID = new Guid("f7b2946a-8d28-4d5f-9a88-b255c6811b59");
            DescriptionMD = "**data transfer rate** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is bit per second with unit label $\\frac{bit}{s}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
