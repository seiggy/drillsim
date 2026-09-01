using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents power per volume.
    /// </summary>
    public partial class PowerPerVolumeQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "q_triple_prime";
        public override string SIUnitName { get; } = "watt per cubic metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{W}{m^3}";
        public override double LengthDimension { get; } = -1;
        public override double TimeDimension { get; } = -3;
        public override double MassDimension { get; } = 1;

        private static PowerPerVolumeQuantity instance_ = null;
        public static PowerPerVolumeQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new PowerPerVolumeQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "watt per cubic metre",
                UnitLabel = "W/m^3",
                ID = new Guid("cd52ade0-1620-4ddc-ba46-cc0a9ce612dc"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "kilowatt per cubic metre",
                UnitLabel = "kW/m^3",
                ID = new Guid("775e57a0-79e4-46f0-b770-573ae94297ef"),
                ConversionFactorFromSIFormula = "1.0/Factors.Kilo",
            },
            new UnitChoice
            {
                UnitName = "watt per litre",
                UnitLabel = "W/L",
                ID = new Guid("707f3c31-9e9d-49b5-9de6-7138bc8e199f"),
                ConversionFactorFromSIFormula = "Factors.Litre",
            }
        };

        public PowerPerVolumeQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "power per volume", "volumetric power density" };
            ID = new Guid("835d08b5-e817-44ab-867a-a55f7cc3f9fd");
            DescriptionMD = "**power per volume** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is watt per cubic metre with unit label $\\frac{W}{m^3}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
