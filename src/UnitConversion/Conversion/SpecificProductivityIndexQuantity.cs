using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents specific productivity index.
    /// </summary>
    public partial class SpecificProductivityIndexQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "J_s";
        public override string SIUnitName { get; } = "cubic metre per second pascal metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{m^3}{s\\cdot Pa\\cdot m}";
        public override double LengthDimension { get; } = 3;
        public override double TimeDimension { get; } = 1;
        public override double MassDimension { get; } = -1;

        private static SpecificProductivityIndexQuantity instance_ = null;
        public static SpecificProductivityIndexQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new SpecificProductivityIndexQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "cubic metre per second pascal metre",
                UnitLabel = "m^3/(s*Pa*m)",
                ID = new Guid("c83c9872-6216-4bdd-855e-fbe531413b42"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "cubic metre per day bar metre",
                UnitLabel = "m^3/(d*bar*m)",
                ID = new Guid("a79418be-824b-4fe6-90c2-db429970e96e"),
                ConversionFactorFromSIFormula = "Factors.Day*Factors.Bar",
            },
            new UnitChoice
            {
                UnitName = "barrel per day psi foot",
                UnitLabel = "bbl/(d*psi*ft)",
                ID = new Guid("2fd4ffeb-0fee-4f9e-8482-080b0402d7c8"),
                ConversionFactorFromSIFormula = "Factors.Day*Factors.PSI*Factors.Foot/Factors.Barrel",
            }
        };

        public SpecificProductivityIndexQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "specific productivity index" };
            ID = new Guid("9d2c471c-4adc-42e9-8941-755fe16e4a2c");
            DescriptionMD = "**specific productivity index** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is cubic metre per second pascal metre with unit label $\\frac{m^3}{s\\cdot Pa\\cdot m}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
