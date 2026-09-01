using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents volumetric heat transfer coefficient.
    /// </summary>
    public partial class VolumetricHeatTransferCoefficientQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "h_V";
        public override string SIUnitName { get; } = "watt per cubic metre kelvin";
        public override string SIUnitLabelLatex { get; } = "\\frac{W}{m^3\\cdot K}";
        public override double LengthDimension { get; } = -1;
        public override double TimeDimension { get; } = -3;
        public override double MassDimension { get; } = 1;
        public override double TemperatureDimension { get; } = -1;

        private static VolumetricHeatTransferCoefficientQuantity instance_ = null;
        public static VolumetricHeatTransferCoefficientQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new VolumetricHeatTransferCoefficientQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "watt per cubic metre kelvin",
                UnitLabel = "W/(m^3*K)",
                ID = new Guid("dfacd24d-56b2-4a48-8688-4bf6087d0886"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "kilowatt per cubic metre kelvin",
                UnitLabel = "kW/(m^3*K)",
                ID = new Guid("3fc64c67-82ce-4174-9d55-d9356893b508"),
                ConversionFactorFromSIFormula = "1.0/Factors.Kilo",
            },
            new UnitChoice
            {
                UnitName = "watt per litre kelvin",
                UnitLabel = "W/(L*K)",
                ID = new Guid("d6a9930d-b0e8-4b01-84e3-a5cb3f6bc271"),
                ConversionFactorFromSIFormula = "Factors.Litre",
            }
        };

        public VolumetricHeatTransferCoefficientQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "volumetric heat transfer coefficient" };
            ID = new Guid("10e62909-a85f-42de-a18b-fc941faed451");
            DescriptionMD = "**volumetric heat transfer coefficient** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is watt per cubic metre kelvin with unit label $\\frac{W}{m^3\\cdot K}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
