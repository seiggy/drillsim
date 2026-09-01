using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents electric field strength.
    /// </summary>
    public partial class ElectricFieldStrengthQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "E";
        public override string SIUnitName { get; } = "volt per metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{V}{m}";
        public override double LengthDimension { get; } = 1;
        public override double TimeDimension { get; } = -3;
        public override double MassDimension { get; } = 1;
        public override double ElectricCurrentDimension { get; } = -1;

        private static ElectricFieldStrengthQuantity instance_ = null;
        public static ElectricFieldStrengthQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ElectricFieldStrengthQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "volt per metre",
                UnitLabel = "V/m",
                ID = new Guid("37050dd4-52c5-401e-907a-25c7bf67bfde"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "kilovolt per metre",
                UnitLabel = "kV/m",
                ID = new Guid("6a59ee3c-d679-48cc-b6a0-eb198b749073"),
                ConversionFactorFromSIFormula = "1.0/Factors.Kilo",
            },
            new UnitChoice
            {
                UnitName = "volt per centimetre",
                UnitLabel = "V/cm",
                ID = new Guid("b477165b-7535-4549-a064-c45ab6fcbb9b"),
                ConversionFactorFromSIFormula = "Factors.Centi",
            }
        };

        public ElectricFieldStrengthQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "electric field strength" };
            ID = new Guid("9f7dc923-2e9d-46aa-95ec-8842af07ffd7");
            DescriptionMD = "**electric field strength** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is volt per metre with unit label $\\frac{V}{m}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
