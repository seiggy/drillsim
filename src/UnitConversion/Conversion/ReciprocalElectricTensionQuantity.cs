using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents reciprocal electric tension.
    /// </summary>
    public partial class ReciprocalElectricTensionQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "U^-1";
        public override string SIUnitName { get; } = "reciprocal volt";
        public override string SIUnitLabelLatex { get; } = "\\frac{1}{V}";
        public override double LengthDimension { get; } = -2;
        public override double TimeDimension { get; } = 3;
        public override double MassDimension { get; } = -1;
        public override double ElectricCurrentDimension { get; } = 1;

        private static ReciprocalElectricTensionQuantity instance_ = null;
        public static ReciprocalElectricTensionQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ReciprocalElectricTensionQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "reciprocal volt",
                UnitLabel = "1/V",
                ID = new Guid("35bd64fa-bb15-4901-a93c-47491c421de7"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "reciprocal millivolt",
                UnitLabel = "1/mV",
                ID = new Guid("2471bdb0-a6af-49af-afbc-9fa958e8403b"),
                ConversionFactorFromSIFormula = "Factors.Milli",
            },
            new UnitChoice
            {
                UnitName = "reciprocal kilovolt",
                UnitLabel = "1/kV",
                ID = new Guid("9adac12e-661e-4c25-9789-38ea2b7d5f84"),
                ConversionFactorFromSIFormula = "Factors.Kilo",
            }
        };

        public ReciprocalElectricTensionQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "reciprocal electric tension", "reciprocal voltage" };
            ID = new Guid("4f8a80aa-8424-4daf-ba9e-a3096f25f758");
            DescriptionMD = "**reciprocal electric tension** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is reciprocal volt with unit label $\\frac{1}{V}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
