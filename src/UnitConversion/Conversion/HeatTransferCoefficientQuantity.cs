using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class HeatTransferCoefficientQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "watt per square metre per kelvin";
        public override string SIUnitLabelLatex { get; } = "\\frac{W}{m^{2} \\cdot K}";
        public override double MassDimension { get; } = 1;
        public override double TimeDimension { get; } = -3;
        public override double TemperatureDimension { get; } = -1;
        private static HeatTransferCoefficientQuantity instance_ = null;

        public static HeatTransferCoefficientQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new HeatTransferCoefficientQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
                {
                    UnitName = "watt per square metre per kelvin",
                    UnitLabel = "W/m²/K",
                    ID = new Guid("e1737353-c10b-46cd-aa4e-9c90afb2f01e"),
                    ConversionFactorFromSIFormula = "1.0/Factors.Unit",
                    IsSI = true
                },
                new UnitChoice
                {
                    UnitName = "british thermal unit per hour per square foot per degree fahrenheit",
                    UnitLabel = "BTU/h/ft²/°F",
                    ID = new Guid("6963db25-2bd9-4017-9c83-cc578a11abbf"),
                    ConversionFactorFromSIFormula = "Factors.Hour * Factors.Foot*Factors.Foot * Factors.FahrenheitSlope / Factors.BTU"
                }
        };
        public HeatTransferCoefficientQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "heat transfer coefficient" };
            ID = new Guid("08c247bc-a55b-460e-a9a7-150faf10bdff");
            DescriptionMD = string.Empty;
            DescriptionMD += @"Heat transfer coefficient is a measure of the efficiency with which heat is transferred between a solid surface and a fluid (or between two fluids) per unit area and temperature difference." + Environment.NewLine;
            DescriptionMD += @"The dimension of heat transfer coefficient is:" + Environment.NewLine;
            DescriptionMD += "$" + GetDimensionsEnclosed() + "$." + Environment.NewLine;
            if (!string.IsNullOrEmpty(SIUnitLabelLatex) && !string.IsNullOrEmpty(SIUnitName) && UsualNames != null && UsualNames.Count > 0)
            {
                DescriptionMD += Environment.NewLine;
                DescriptionMD += @"The SI unit for **" + UsualNames.First() + "** is: " + SIUnitName + " with the associated unit label $" + SIUnitLabelLatex + "$" + Environment.NewLine;
            }
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
