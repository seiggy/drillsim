using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents radiant intensity.
    /// </summary>
    public partial class RadiantIntensityQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "I_e";
        public override string SIUnitName { get; } = "watt per steradian";
        public override string SIUnitLabelLatex { get; } = "\\frac{W}{sr}";
        public override double SolidAngleDimension { get; } = -1;
        public override double LengthDimension { get; } = 2;
        public override double TimeDimension { get; } = -3;
        public override double MassDimension { get; } = 1;

        private static RadiantIntensityQuantity instance_ = null;
        public static RadiantIntensityQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new RadiantIntensityQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "watt per steradian",
                UnitLabel = "W/sr",
                ID = new Guid("8b19fa13-bf01-4fbd-be34-5f8405f7b112"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "kilowatt per steradian",
                UnitLabel = "kW/sr",
                ID = new Guid("02423578-1c58-4c6d-9132-ee42cb9a319b"),
                ConversionFactorFromSIFormula = "1.0/Factors.Kilo",
            }
        };

        public RadiantIntensityQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "radiant intensity" };
            ID = new Guid("8884a026-e09f-4fba-8aa2-7af355d21e44");
            DescriptionMD = "**radiant intensity** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is watt per steradian with unit label $\\frac{W}{sr}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
