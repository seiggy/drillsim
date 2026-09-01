using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class ThermalConductivityGradientPerTemperatureQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "watt per metre kelvin per kelvin";
        public override string SIUnitLabelLatex { get; } = "\\frac{(\\frac{W}{(m \\cdot K)}}{K}";
        public override double LengthDimension { get; } = 1;
        public override double MassDimension { get; } = 1;
        public override double TimeDimension { get; } = -3;
        public override double TemperatureDimension { get; } = -2;
        private static ThermalConductivityGradientPerTemperatureQuantity instance_ = null;

        public static ThermalConductivityGradientPerTemperatureQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ThermalConductivityGradientPerTemperatureQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
        new UnitChoice
        {
          UnitName = "watt per metre kelvin per kelvin",
          UnitLabel = "(W/(m•K))/K",
          ID = new Guid("0459940e-d71f-4b01-9ea6-eeb05d754af2"),
          ConversionFactorFromSIFormula = "1.0/Factors.Unit",
          IsSI = true
        },
        new UnitChoice
        {
          UnitName = "calorie per metre second degree celsius squared",
          UnitLabel = "Cal/m•s•°C²",
          ID = new Guid("eb08ff8c-d542-440f-a4c7-610653018910"),
          ConversionFactorFromSIFormula = "1.0/Factors.Calorie"
        },
        new UnitChoice
        {
          UnitName = "calorie per centimetre second degree celsius squared",
          UnitLabel = "Cal/cm•s•°C²",
          ID = new Guid("6c21a6cd-61fe-4086-95a7-ad6d6820c96e"),
          ConversionFactorFromSIFormula = "Factors.Centi/Factors.Calorie"
        },
        new UnitChoice
        {
          UnitName = "british thermal unit per hour foot degree fahrenheit squared",
          UnitLabel = "BTU/h•ft•°F²",
          ID = new Guid("b79509ea-8c03-4538-9974-208f7e0ee40e"),
          ConversionFactorFromSIFormula = "Factors.Hour*Factors.Foot*Factors.FahrenheitSlope*Factors.FahrenheitSlope/Factors.BTU"
        },
        new UnitChoice
        {
          UnitName = "british thermal unit inch per hour square foot degree fahrenheit squared",
          UnitLabel = "BTU•in/h•ft²•°F²",
          ID = new Guid("918b4e34-3986-427f-8bb6-c09740a7c299"),
          ConversionFactorFromSIFormula = "Factors.Hour*Factors.Foot*Factors.Foot*Factors.FahrenheitSlope*Factors.FahrenheitSlope/(Factors.BTU*Factors.Inch)"
        }
        };
        public ThermalConductivityGradientPerTemperatureQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "thermal conductivity gradient per temperature" };
            ID = new Guid("5e509f43-8fb4-490e-b9a5-59d7393761c0");
            DescriptionMD = string.Empty;
            DescriptionMD += @"A thermal conductivity gradient per temperature is the first derivative of a thermal conductivity compared to temperature: $\frac{dk}{dT}$, where $k$ is a thermal conductivity and $T$ is temperature. " + Environment.NewLine;
            DescriptionMD += @"The dimension of thermal conductivity gradient per temperature is:" + Environment.NewLine;
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
