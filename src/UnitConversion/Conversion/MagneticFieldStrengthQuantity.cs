using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents magnetic field strength.
    /// </summary>
    public partial class MagneticFieldStrengthQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "H";
        public override string SIUnitName { get; } = "ampere per metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{A}{m}";
        public override double ElectricCurrentDimension { get; } = 1;
        public override double LengthDimension { get; } = -1;

        private static MagneticFieldStrengthQuantity instance_ = null;
        public static MagneticFieldStrengthQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MagneticFieldStrengthQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "ampere per metre",
                UnitLabel = "A/m",
                ID = new Guid("9385dd68-2cd1-477c-b0d3-c1f7968d9f9b"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "ampere per centimetre",
                UnitLabel = "A/cm",
                ID = new Guid("01270a8e-a2d3-47dc-aa9f-2d63f1fb34ef"),
                ConversionFactorFromSIFormula = "Factors.Centi",
            },
            new UnitChoice
            {
                UnitName = "kiloampere per metre",
                UnitLabel = "kA/m",
                ID = new Guid("265bf325-609d-46ea-b4e6-7ad1bfa57f49"),
                ConversionFactorFromSIFormula = "1.0/Factors.Kilo",
            }
        };

        public MagneticFieldStrengthQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "magnetic field strength" };
            ID = new Guid("db6a2e38-bbca-4ef0-b466-beee214160a6");
            DescriptionMD = "**magnetic field strength** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is ampere per metre with unit label $\\frac{A}{m}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
