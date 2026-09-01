using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents productivity index.
    /// </summary>
    public partial class ProductivityIndexQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "J";
        public override string SIUnitName { get; } = "cubic metre per second pascal";
        public override string SIUnitLabelLatex { get; } = "\\frac{m^3}{s\\cdot Pa}";
        public override double LengthDimension { get; } = 4;
        public override double TimeDimension { get; } = 1;
        public override double MassDimension { get; } = -1;

        private static ProductivityIndexQuantity instance_ = null;
        public static ProductivityIndexQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ProductivityIndexQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "cubic metre per second pascal",
                UnitLabel = "m^3/(s*Pa)",
                ID = new Guid("36e1f025-d290-4244-8807-3662333cb0bf"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "cubic metre per day bar",
                UnitLabel = "m^3/(d*bar)",
                ID = new Guid("7b17b0c1-8a44-40c3-9ba3-13af3e0078dc"),
                ConversionFactorFromSIFormula = "Factors.Day*Factors.Bar",
            },
            new UnitChoice
            {
                UnitName = "barrel per day psi",
                UnitLabel = "bbl/(d*psi)",
                ID = new Guid("57744986-a005-4e42-aa17-8d6a6de3653f"),
                ConversionFactorFromSIFormula = "Factors.Day*Factors.PSI/Factors.Barrel",
            }
        };

        public ProductivityIndexQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "productivity index" };
            ID = new Guid("7320b04f-86cf-44b9-b386-d512fbf23240");
            DescriptionMD = "**productivity index** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is cubic metre per second pascal with unit label $\\frac{m^3}{s\\cdot Pa}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
