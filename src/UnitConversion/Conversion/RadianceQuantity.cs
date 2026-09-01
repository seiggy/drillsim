using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Represents radiance.
    /// </summary>
    public partial class RadianceQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "L_e";
        public override string SIUnitName { get; } = "watt per steradian square metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{W}{sr\\cdot m^2}";
        public override double SolidAngleDimension { get; } = -1;
        public override double TimeDimension { get; } = -3;
        public override double MassDimension { get; } = 1;

        private static RadianceQuantity instance_ = null;
        public static RadianceQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new RadianceQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
            {
                UnitName = "watt per steradian square metre",
                UnitLabel = "W/(sr*m^2)",
                ID = new Guid("9736fbeb-e61a-4e3a-adbd-9e0566c927a3"),
                ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                IsSI = true,
            },
            new UnitChoice
            {
                UnitName = "kilowatt per steradian square metre",
                UnitLabel = "kW/(sr*m^2)",
                ID = new Guid("7eedcdcb-beb6-47e6-89ac-ab3bdf136abb"),
                ConversionFactorFromSIFormula = "1.0/Factors.Kilo",
            }
        };

        public RadianceQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "radiance" };
            ID = new Guid("d721aed3-da54-469e-961a-63100b2711c9");
            DescriptionMD = "**radiance** has physical dimension " + GetDimensionsEnclosed() + "." + Environment.NewLine;
            DescriptionMD += "The SI unit is watt per steradian square metre with unit label $\\frac{W}{sr\\cdot m^2}$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
