using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents luminance.
    /// </summary>
    public partial class LuminanceQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "L_v";
        public override string SIUnitName { get; } = "candela per square metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{cd}{m^2}";
        public override double LuminousIntensityDimension { get; } = 1;
        public override double LengthDimension { get; } = -2;

        private static LuminanceQuantity instance_ = null;
        public static LuminanceQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new LuminanceQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "candela per square metre",
                UnitLabel = "cd/m^2",
                ID = new Guid("8328ebbf-7267-444c-9bf8-c816ebd1b206"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "candela per square centimetre",
                UnitLabel = "cd/cm^2",
                ID = new Guid("559df3ee-9ee1-4c82-a35e-6355318f625a"),
                ConversionFactorFromSIFormula = "Factors.Centi*Factors.Centi",
            },
            new UnitChoice
            {
                UnitName = "candela per square foot",
                UnitLabel = "cd/ft^2",
                ID = new Guid("bace55d7-2c3b-4828-b466-5c246b41e648"),
                ConversionFactorFromSIFormula = "Factors.Foot*Factors.Foot",
            }
        };

        public LuminanceQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "luminance" };
            ID = new Guid("27adf373-4669-44aa-b524-2edbc27a3af3");
            DescriptionMD = "**luminance** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is candela per square metre with unit label $\\frac{cd}{m^2}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
