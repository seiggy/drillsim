using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents volume per volume rate of change.
    /// </summary>
    public partial class VolumePerVolumeRateOfChangeQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "d(V/V)/dt";
        public override string SIUnitName { get; } = "cubic metre per cubic metre second";
        public override string SIUnitLabelLatex { get; } = "\\frac{m^3}{m^3\\cdot s}";
        public override double TimeDimension { get; } = -1;

        private static VolumePerVolumeRateOfChangeQuantity instance_ = null;
        public static VolumePerVolumeRateOfChangeQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new VolumePerVolumeRateOfChangeQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "cubic metre per cubic metre second",
                UnitLabel = "m^3/(m^3*s)",
                ID = new Guid("ece407e6-967f-45be-b971-e63af2116db1"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "percent per second",
                UnitLabel = "%/s",
                ID = new Guid("8d8aa27b-7657-49d7-abd7-007057a1641f"),
                ConversionFactorFromSIFormula = "1.0/Factors.Centi",
            },
            new UnitChoice
            {
                UnitName = "percent per minute",
                UnitLabel = "%/min",
                ID = new Guid("e1f92239-0b02-44dd-8cad-2d9df45e5854"),
                ConversionFactorFromSIFormula = "Factors.Minute/Factors.Centi",
            }
        };

        public VolumePerVolumeRateOfChangeQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "volume per volume rate of change", "volumetric strain rate" };
            ID = new Guid("af6ccc7a-2ae6-4dd6-becb-ea425e461991");
            DescriptionMD = "**volume per volume rate of change** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is " + SIUnitName + " with unit label $" + SIUnitLabelLatex + "$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
