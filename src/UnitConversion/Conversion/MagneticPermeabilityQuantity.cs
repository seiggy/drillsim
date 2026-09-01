using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents magnetic permeability.
    /// </summary>
    public partial class MagneticPermeabilityQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "mu";
        public override string SIUnitName { get; } = "henry per metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{H}{m}";
        public override double LengthDimension { get; } = 1;
        public override double TimeDimension { get; } = -2;
        public override double MassDimension { get; } = 1;
        public override double ElectricCurrentDimension { get; } = -2;

        private static MagneticPermeabilityQuantity instance_ = null;
        public static MagneticPermeabilityQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MagneticPermeabilityQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "henry per metre",
                UnitLabel = "H/m",
                ID = new Guid("1fa6ee1b-6e92-4ee2-8cd8-224b2183246f"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "microhenry per metre",
                UnitLabel = "uH/m",
                ID = new Guid("fd5ed30d-289a-420f-83bb-732284846e91"),
                ConversionFactorFromSIFormula = "1.0/Factors.Micro",
            }
        };

        public MagneticPermeabilityQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "magnetic permeability" };
            ID = new Guid("406ec51a-b1a9-42f1-9ec8-3aeaec3fb7f3");
            DescriptionMD = "**magnetic permeability** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is henry per metre with unit label $\\frac{H}{m}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
