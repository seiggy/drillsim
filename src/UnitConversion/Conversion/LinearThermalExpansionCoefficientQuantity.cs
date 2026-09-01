using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents linear thermal expansion coefficient.
    /// </summary>
    public partial class LinearThermalExpansionCoefficientQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "alpha_L";
        public override string SIUnitName { get; } = "reciprocal kelvin";
        public override string SIUnitLabelLatex { get; } = "\\frac{1}{K}";
        public override double TemperatureDimension { get; } = -1;

        private static LinearThermalExpansionCoefficientQuantity instance_ = null;
        public static LinearThermalExpansionCoefficientQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new LinearThermalExpansionCoefficientQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "reciprocal kelvin",
                UnitLabel = "1/K",
                ID = new Guid("6bb7d280-79f1-4082-931b-95745b88df0e"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "micrometre per metre kelvin",
                UnitLabel = "um/(m*K)",
                ID = new Guid("27d5ba90-d3de-44f8-bd80-554ecbd78d2d"),
                ConversionFactorFromSIFormula = "1.0/Factors.Micro",
            },
            new UnitChoice
            {
                UnitName = "reciprocal degree fahrenheit",
                UnitLabel = "1/degF",
                ID = new Guid("be900f4d-4861-4942-892d-0864b3cbcb1a"),
                ConversionFactorFromSIFormula = "Factors.FahrenheitSlope",
            }
        };

        public LinearThermalExpansionCoefficientQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "linear thermal expansion coefficient" };
            ID = new Guid("e2a898d5-5110-415e-84e9-4923003781da");
            DescriptionMD = "**linear thermal expansion coefficient** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is reciprocal kelvin with unit label $\\frac{1}{K}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
