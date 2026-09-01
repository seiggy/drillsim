using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents volumetric flow rate gradient per length.
    /// </summary>
    public partial class VolumetricFlowRateGradientPerLengthQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "dQ/dL";
        public override string SIUnitName { get; } = "cubic metre per second metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{m^3}{s\\cdot m}";
        public override double TimeDimension { get; } = -1;
        public override double LengthDimension { get; } = 2;

        private static VolumetricFlowRateGradientPerLengthQuantity instance_ = null;
        public static VolumetricFlowRateGradientPerLengthQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new VolumetricFlowRateGradientPerLengthQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "cubic metre per second metre",
                UnitLabel = "m^3/(s*m)",
                ID = new Guid("705d1e45-8f4f-4672-9696-cfb1b697282a"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "litre per minute metre",
                UnitLabel = "L/(min*m)",
                ID = new Guid("5404064e-c0f2-4eb4-b5cd-99119ba89b07"),
                ConversionFactorFromSIFormula = "Factors.Minute/Factors.Litre",
            },
            new UnitChoice
            {
                UnitName = "gallon US per minute foot",
                UnitLabel = "galUS/(min*ft)",
                ID = new Guid("bf104618-b79a-49a1-82d9-ed6ee32d0aa4"),
                ConversionFactorFromSIFormula = "Factors.Minute*Factors.Foot/Factors.GallonUS",
            },
            new UnitChoice
            {
                UnitName = "gallon UK per minute foot",
                UnitLabel = "galUK/(min*ft)",
                ID = new Guid("179a2d8d-a6b3-4a01-87c9-95987cf5b25c"),
                ConversionFactorFromSIFormula = "Factors.Minute*Factors.Foot/Factors.GallonUK",
            }
        };

        public VolumetricFlowRateGradientPerLengthQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "volumetric flow rate gradient per length" };
            ID = new Guid("7bb02c43-2159-4ca4-b6cd-3e6addd90534");
            DescriptionMD = "**volumetric flow rate gradient per length** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is " + SIUnitName + " with unit label $" + SIUnitLabelLatex + "$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
