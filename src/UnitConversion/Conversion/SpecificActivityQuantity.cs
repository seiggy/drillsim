using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents specific activity.
    /// </summary>
    public partial class SpecificActivityQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "a";
        public override string SIUnitName { get; } = "becquerel per kilogram";
        public override string SIUnitLabelLatex { get; } = "\\frac{Bq}{kg}";
        public override double TimeDimension { get; } = -1;
        public override double MassDimension { get; } = -1;

        private static SpecificActivityQuantity instance_ = null;
        public static SpecificActivityQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new SpecificActivityQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "becquerel per kilogram",
                UnitLabel = "Bq/kg",
                ID = new Guid("cf522a5e-e624-4cb5-8333-218453cbc7ce"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "kilobecquerel per kilogram",
                UnitLabel = "kBq/kg",
                ID = new Guid("5f3fd488-cf0d-4fd3-ab1d-32b9546c3e10"),
                ConversionFactorFromSIFormula = "1.0/Factors.Kilo",
            },
            new UnitChoice
            {
                UnitName = "becquerel per gram",
                UnitLabel = "Bq/g",
                ID = new Guid("fdd53991-2c85-475c-a9b9-569307e258ac"),
                ConversionFactorFromSIFormula = "Factors.Milli",
            }
        };

        public SpecificActivityQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "specific activity", "radioactivity per mass" };
            ID = new Guid("7f6b0d3a-02e2-48af-9476-f303c62fd52c");
            DescriptionMD = "**specific activity** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is becquerel per kilogram with unit label $\\frac{Bq}{kg}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
