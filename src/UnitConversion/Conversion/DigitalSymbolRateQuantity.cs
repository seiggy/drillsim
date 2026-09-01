using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents digital symbol rate.
    /// </summary>
    public partial class DigitalSymbolRateQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "R_s";
        public override string SIUnitName { get; } = "baud";
        public override string SIUnitLabelLatex { get; } = "Bd";
        public override double TimeDimension { get; } = -1;
        public override double? MeaningfulPrecisionInSI { get; } = 1;

        private static DigitalSymbolRateQuantity instance_ = null;
        public static DigitalSymbolRateQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new DigitalSymbolRateQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "baud",
                UnitLabel = "Bd",
                ID = new Guid("386c7e7a-51c9-4169-886b-20f00d521da2"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "kilobaud",
                UnitLabel = "kBd",
                ID = new Guid("d90a7813-f811-4392-bcf2-6bb1b16bcf56"),
                ConversionFactorFromSIFormula = "1.0/Factors.Kilo",
            },
            new UnitChoice
            {
                UnitName = "megabaud",
                UnitLabel = "MBd",
                ID = new Guid("e351c060-9b56-45bf-a908-9e8d18a143a7"),
                ConversionFactorFromSIFormula = "1.0/Factors.Mega",
            }
        };

        public DigitalSymbolRateQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "digital symbol rate", "symbol rate" };
            ID = new Guid("d005f39e-8afb-40c0-a2ed-0305743e600d");
            DescriptionMD = "**digital symbol rate** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is baud with unit label $Bd$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
