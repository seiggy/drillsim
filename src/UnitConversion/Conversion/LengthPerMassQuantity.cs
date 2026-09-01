using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents length per mass.
    /// </summary>
    public partial class LengthPerMassQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "L/m";
        public override string SIUnitName { get; } = "metre per kilogram";
        public override string SIUnitLabelLatex { get; } = "\\frac{m}{kg}";
        public override double MassDimension { get; } = -1;
        public override double LengthDimension { get; } = 1;

        private static LengthPerMassQuantity instance_ = null;
        public static LengthPerMassQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new LengthPerMassQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "metre per kilogram",
                UnitLabel = "m/kg",
                ID = new Guid("0e731160-fee5-409d-be70-423aa958cbb0"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "millimetre per kilogram",
                UnitLabel = "mm/kg",
                ID = new Guid("eb7b01ca-b0f0-4850-9abe-0a3a42111a1f"),
                ConversionFactorFromSIFormula = "1.0/Factors.Milli",
            },
            new UnitChoice
            {
                UnitName = "foot per pound",
                UnitLabel = "ft/lb",
                ID = new Guid("5bbc1d13-0f4e-4f61-b9cc-b05e91f7ad5b"),
                ConversionFactorFromSIFormula = "Factors.Pound/Factors.Foot",
            }
        };

        public LengthPerMassQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "length per mass" };
            ID = new Guid("b0dbfd93-df83-4351-abd4-a4afb4da3309");
            DescriptionMD = "**length per mass** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is metre per kilogram with unit label $\\frac{m}{kg}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
