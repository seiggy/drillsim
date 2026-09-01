using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents time per mass.
    /// </summary>
    public partial class TimePerMassQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "t/m";
        public override string SIUnitName { get; } = "second per kilogram";
        public override string SIUnitLabelLatex { get; } = "\\frac{s}{kg}";
        public override double TimeDimension { get; } = 1;
        public override double MassDimension { get; } = -1;

        private static TimePerMassQuantity instance_ = null;
        public static TimePerMassQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new TimePerMassQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "second per kilogram",
                UnitLabel = "s/kg",
                ID = new Guid("1b3f1ec3-bb78-4d71-961f-748133bd0f87"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "minute per kilogram",
                UnitLabel = "min/kg",
                ID = new Guid("c26a00ce-2663-41ea-8996-c53acc7e6409"),
                ConversionFactorFromSIFormula = "1.0/Factors.Minute",
            },
            new UnitChoice
            {
                UnitName = "second per pound",
                UnitLabel = "s/lb",
                ID = new Guid("3fec2d1b-1aba-4a2b-872d-a61f7f11dc43"),
                ConversionFactorFromSIFormula = "Factors.Pound",
            }
        };

        public TimePerMassQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "time per mass" };
            ID = new Guid("6c493b1b-89dc-4bd3-997b-8080f2381b7e");
            DescriptionMD = "**time per mass** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is second per kilogram with unit label $\\frac{s}{kg}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
