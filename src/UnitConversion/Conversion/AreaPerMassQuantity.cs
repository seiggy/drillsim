using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents area per mass.
    /// </summary>
    public partial class AreaPerMassQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "A/m";
        public override string SIUnitName { get; } = "square metre per kilogram";
        public override string SIUnitLabelLatex { get; } = "\\frac{m^2}{kg}";
        public override double MassDimension { get; } = -1;
        public override double LengthDimension { get; } = 2;

        private static AreaPerMassQuantity instance_ = null;
        public static AreaPerMassQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new AreaPerMassQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "square metre per kilogram",
                UnitLabel = "m^2/kg",
                ID = new Guid("ff216a2c-2fae-423f-a1d2-d14c070ffb06"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "square centimetre per gram",
                UnitLabel = "cm^2/g",
                ID = new Guid("b61df260-ea70-48a5-aa07-c209eb0eafeb"),
                ConversionFactorFromSIFormula = "Factors.Milli/(Factors.Centi*Factors.Centi)",
            },
            new UnitChoice
            {
                UnitName = "square foot per pound",
                UnitLabel = "ft^2/lb",
                ID = new Guid("e71612bc-3e04-4768-a755-ade8184f1a26"),
                ConversionFactorFromSIFormula = "Factors.Pound/(Factors.Foot*Factors.Foot)",
            }
        };

        public AreaPerMassQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "area per mass", "specific surface area" };
            ID = new Guid("54417926-5232-4ee1-8dde-1c1e0689834d");
            DescriptionMD = "**area per mass** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is square metre per kilogram with unit label $\\frac{m^2}{kg}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
