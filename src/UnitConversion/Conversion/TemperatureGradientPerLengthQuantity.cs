using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class TemperatureGradientPerLengthQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "kelvin per metre";
        public override string SIUnitLabelLatex { get; } = "\\frac{K}{m}";
        public override double LengthDimension { get; } = -1;
        public override double TemperatureDimension { get; } = 1;
        private static TemperatureGradientPerLengthQuantity instance_ = null;

        public static TemperatureGradientPerLengthQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new TemperatureGradientPerLengthQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
        new UnitChoice
        {
          UnitName = "kelvin per metre",
          UnitLabel = "K/m",
          ID = new Guid("f1fe19d2-12e3-43d1-ba97-3ef9e8ec9e73"),
          ConversionFactorFromSIFormula = "1.0/Factors.Unit",
          IsSI = true
        },
        new UnitChoice
        {
          UnitName = "celsius per metre",
          UnitLabel = "°C/m",
          ID = new Guid("40dbbdfe-b680-403a-8326-2c217ba85d52"),
          ConversionFactorFromSIFormula = "1.0/Factors.Unit"
        },
        new UnitChoice
        {
          UnitName = "celsius per 10 metre",
          UnitLabel = "°C/10m",
          ID = new Guid("5e4ff2bf-4788-4258-bd4a-7b18a13364ff"),
          ConversionFactorFromSIFormula = "Factors.Deca/Factors.Unit"
        },
        new UnitChoice
        {
          UnitName = "celsius per 30 metre",
          UnitLabel = "°C/30m",
          ID = new Guid("d17464c4-a7ef-4dcd-b783-bafe6e9b92de"),
          ConversionFactorFromSIFormula = "3.0*Factors.Deca/Factors.Unit"
        },
        new UnitChoice
        {
          UnitName = "celsius per 100 metre",
          UnitLabel = "°C/100m",
          ID = new Guid("b47f299a-913a-46b7-ad20-c683fa0f02d0"),
          ConversionFactorFromSIFormula = "Factors.Hecto/Factors.Unit"
        },
        new UnitChoice
        {
          UnitName = "celsius per foot",
          UnitLabel = "°C/ft",
          ID = new Guid("e7b05420-41f6-4812-bc54-9c14f05a9cbd"),
          ConversionFactorFromSIFormula = "Factors.Foot/Factors.Unit"
        },
        new UnitChoice
        {
          UnitName = "celsius per 30 foot",
          UnitLabel = "°C/30ft",
          ID = new Guid("bea3df4f-78e9-4e1a-bbee-22086da043b4"),
          ConversionFactorFromSIFormula = "30.0*Factors.Foot/Factors.Unit"
        },
        new UnitChoice
        {
          UnitName = "celsius per 100 foot",
          UnitLabel = "°C/100ft",
          ID = new Guid("f9bae95a-b282-44a7-8ae0-54728ef3c7a3"),
          ConversionFactorFromSIFormula = "100.0*Factors.Foot/Factors.Unit"
        },
        new UnitChoice
        {
          UnitName = "fahrenheit per foot",
          UnitLabel = "°F/ft",
          ID = new Guid("d08596f1-77c4-4a8e-9245-6bf563fa7345"),
          ConversionFactorFromSIFormula = "Factors.Foot/Factors.FahrenheitSlope"
        },
        new UnitChoice
        {
          UnitName = "fahrenheit per 30 foot",
          UnitLabel = "°F/30ft",
          ID = new Guid("a1664cb0-db5c-4933-9b57-d075c4975f46"),
          ConversionFactorFromSIFormula = "30.0*Factors.Foot/Factors.FahrenheitSlope"
        },
        new UnitChoice
        {
          UnitName = "fahrenheit per 100 foot",
          UnitLabel = "°F/100ft",
          ID = new Guid("232e2d6d-cb65-4b56-9277-457e4ff678fa"),
          ConversionFactorFromSIFormula = "100.0*Factors.Foot/Factors.FahrenheitSlope"
        }
        };
        public TemperatureGradientPerLengthQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "temperature gradient per length" };
            ID = new Guid("4c1819d5-008b-4613-b62a-3f5d91b08ee7");
            DescriptionMD = string.Empty;
            DescriptionMD += @"A temperature gradient per lenth is the first derivative of a temperature compared to a distance: $\frac{dT}{ds}$, where $T$ is a temperature and $s$ is a distance." + Environment.NewLine;
            DescriptionMD += @"The dimension of temperature gradient per length is:" + Environment.NewLine;
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
