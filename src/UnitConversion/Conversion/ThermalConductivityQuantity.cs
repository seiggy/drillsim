using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class ThermalConductivityQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "k";
        public override string SIUnitName { get; } = "watt per metre kelvin";
        public override string SIUnitLabelLatex { get; } = "\\frac{W}{m \\cdot K}";
        public override double LengthDimension { get; } = 1;
        public override double MassDimension { get; } = 1;
        public override double TimeDimension { get; } = -3;
        public override double TemperatureDimension { get; } = -1;

        private static ThermalConductivityQuantity instance_ = null;

        public static ThermalConductivityQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ThermalConductivityQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
        new UnitChoice
        {
          UnitName = "watt per metre kelvin",
          UnitLabel = "W/m•K",
          ID = new Guid("3ddba24f-4ccf-4cb1-af6c-2829cac3b88f"),
          ConversionFactorFromSIFormula = "1.0/Factors.Unit",
          IsSI = true
        },
        new UnitChoice
        {
          UnitName = "calorie per metre second degree celsius",
          UnitLabel = "cal/m•s•°C",
          ID = new Guid("d0386fc4-b97b-4874-8c8d-66e093c391ea"),
          ConversionFactorFromSIFormula = "1.0/Factors.Calorie"
        },
        new UnitChoice
        {
          UnitName = "calorie per centimetre second degree celsius",
          UnitLabel = "cal/cm•s•°C",
          ID = new Guid("5f8706ed-d938-4715-a0ca-2afff423f6e6"),
          ConversionFactorFromSIFormula = "Factors.Centi/Factors.Calorie"
        },
        new UnitChoice
        {
          UnitName = "british thermal unit per hour foot degree fahrenheit",
          UnitLabel = "BTU/h•ft•°F",
          ID = new Guid("43169695-8f6e-42ad-8c07-566dc7651edb"),
          ConversionFactorFromSIFormula = "Factors.Hour*Factors.Foot*Factors.FahrenheitSlope/Factors.BTU"
        },
        new UnitChoice
        {
          UnitName = "british thermal unit inch per hour square foot degree fahrenheit",
          UnitLabel = "BTU•in/h•ft²•°F",
          ID = new Guid("c79c2b27-c956-49a3-9caf-8653017777ca"),
          ConversionFactorFromSIFormula = "Factors.Hour*Factors.Foot*Factors.Foot*Factors.FahrenheitSlope/(Factors.BTU*Factors.Inch)"
        }
        };
        public ThermalConductivityQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "thermal conductivity" };
            ID = new Guid("ca23212e-8f2d-4041-89f6-ac8bfa8604fa");
            DescriptionMD = string.Empty;
            DescriptionMD += @"Thermal conductivity is a material's ability to conduct heat. It measures how efficiently heat is transferred through a material when there is a temperature difference." + Environment.NewLine;
            DescriptionMD += @"The dimension of thermal conductivity is:" + Environment.NewLine;
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
