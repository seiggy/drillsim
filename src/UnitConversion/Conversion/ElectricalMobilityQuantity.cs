using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>The drift velocity of a charged particle per unit electric field strength.</summary>
    public partial class ElectricalMobilityQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "mu_e";
        public override string SIUnitName { get; } = "square metre per volt second";
        public override string SIUnitLabelLatex { get; } = "\\frac{m^2}{V\\cdot s}";
        public override double TimeDimension { get; } = 2;
        public override double MassDimension { get; } = -1;
        public override double ElectricCurrentDimension { get; } = 1;

        private static ElectricalMobilityQuantity instance_ = null;
        public static ElectricalMobilityQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ElectricalMobilityQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "square metre per volt second", UnitLabel = "m^2/(V*s)",
                ID = new Guid("7788e7a0-8ff5-4252-bf3c-f81eada5f068"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit", IsSI = true
            },
            new UnitChoice
            {
                UnitName = "square centimetre per volt second", UnitLabel = "cm^2/(V*s)",
                ID = new Guid("1ae50b3c-e251-4012-85e7-7dbbc3a46bec"),
                ConversionFactorFromSIFormula = "1.0/(Factors.Centi*Factors.Centi)"
            }
        };

        public ElectricalMobilityQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "electrical mobility", "electric mobility" };
            ID = new Guid("e3b12404-1c06-46ea-ac83-503c168c91cd");
            DescriptionMD = "**Electrical mobility** is drift velocity per unit electric field strength." + Environment.NewLine;
            DescriptionMD += "Its physical dimension is " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is square metre per volt second with unit label $" + SIUnitLabelLatex + "$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
