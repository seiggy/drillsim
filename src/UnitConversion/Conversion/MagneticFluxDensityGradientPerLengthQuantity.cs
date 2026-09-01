using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents magnetic flux density gradient per length.
    /// </summary>
    public partial class MagneticFluxDensityGradientPerLengthQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "dB/dL";
        public override string SIUnitName { get; } = "tesla per metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{T}{m}";
        public override double LengthDimension { get; } = -1;
        public override double TimeDimension { get; } = -2;
        public override double MassDimension { get; } = 1;
        public override double ElectricCurrentDimension { get; } = -1;

        private static MagneticFluxDensityGradientPerLengthQuantity instance_ = null;
        public static MagneticFluxDensityGradientPerLengthQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MagneticFluxDensityGradientPerLengthQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "tesla per metre",
                UnitLabel = "T/m",
                ID = new Guid("69a7f57d-70ed-411d-bcd7-9fa6b18bffbe"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "microtesla per metre",
                UnitLabel = "uT/m",
                ID = new Guid("5aa9532f-42a9-46e2-9fc5-43be50648ffb"),
                ConversionFactorFromSIFormula = "1.0/Factors.Micro",
            },
            new UnitChoice
            {
                UnitName = "gauss per centimetre",
                UnitLabel = "G/cm",
                ID = new Guid("5d3a9041-0a5d-473e-b26e-5c2a526bce84"),
                ConversionFactorFromSIFormula = "Factors.Centi/Factors.Gauss",
            }
        };

        public MagneticFluxDensityGradientPerLengthQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "magnetic flux density gradient per length" };
            ID = new Guid("6ed74495-1719-440b-88b9-be740ef4d5b0");
            DescriptionMD = "**magnetic flux density gradient per length** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is tesla per metre with unit label $\\frac{T}{m}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
