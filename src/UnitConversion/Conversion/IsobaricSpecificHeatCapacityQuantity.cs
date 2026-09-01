using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class IsobaricSpecificHeatCapacityQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override string SIUnitName { get; } = "joule per kilogram kelvin";
        public override string SIUnitLabelLatex { get; } = "\\frac{J}{kg \\cdot K}";
        public override double LengthDimension { get; } = 2;
        public override double TimeDimension { get; } = -2;
        public override double TemperatureDimension { get; } = -1;
        private static IsobaricSpecificHeatCapacityQuantity instance_ = null;

        public static IsobaricSpecificHeatCapacityQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new IsobaricSpecificHeatCapacityQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public static List<UnitChoice> UnitChoiceDescriptions = new List<UnitChoice>()
        {
            new UnitChoice
        {
          UnitName = "joule per kilogram kelvin",
          UnitLabel = "J/kg•K",
          ID = new Guid("52d9523e-546b-41dd-b283-a125447433a3"),
          ConversionFactorFromSIFormula = "1.0/Factors.Unit",
          IsSI = true
        },
        new UnitChoice
        {
          UnitName = "joule per gram kelvin",
          UnitLabel = "J/g•K",
          ID = new Guid("0c38001b-ecba-4920-ac75-e4644d8feced"),
          ConversionFactorFromSIFormula = "Factors.Milli/Factors.Unit"
        },
        new UnitChoice
        {
          UnitName = "joule per gram degree celsius",
          UnitLabel = "J/g•°C",
          ID = new Guid("5b620d63-2269-42d3-8385-edca04c7ea70"),
          ConversionFactorFromSIFormula = "Factors.Milli/Factors.Unit"
        },
        new UnitChoice
        {
          UnitName = "calorie per gram degree celsius",
          UnitLabel = "cal/g•°C",
          ID = new Guid("bb241c58-e76c-4d96-81c1-356b3f2ad397"),
          ConversionFactorFromSIFormula = "Factors.Milli/Factors.Calorie"
        },
        new UnitChoice
        {
          UnitName = "british thermal unit per pound degree fahrenheit",
          UnitLabel = "BTU/lb•°F",
          ID = new Guid("ad9274f2-4c1a-45fe-97c1-710f00deca16"),
          ConversionFactorFromSIFormula = "Factors.Pound*Factors.FahrenheitSlope/Factors.BTU"
        },
        new UnitChoice
        {
          UnitName = "kilocalorie per gram degree celsius",
          UnitLabel = "Cal/g•°C",
          ID = new Guid("b283ecf7-20e4-4a6c-b62b-b07f56fa6614"),
          ConversionFactorFromSIFormula = "Factors.Milli/(Factors.Kilo*Factors.Calorie)"
        }
        };
        public IsobaricSpecificHeatCapacityQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "isobaric specific heat capacity" };
            ID = new Guid("e5c75fa9-0102-42dc-bb0c-830fe9fca2b9");
            DescriptionMD = string.Empty;
            DescriptionMD += @"Isobaric specific heat capacity is the amount of heat required to raise the temperature of one unit mass of a substance by one unit of temperature at constant pressure. It indicates how much heat energy a material can store." + Environment.NewLine;
            DescriptionMD += @"The dimension of specific heat capacity is:" + Environment.NewLine;
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
