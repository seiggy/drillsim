using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class TemperatureRateOfChangeQuantity : DerivedBasePhysicalQuantity
    {
        public override string TypicalSymbol { get; } = "dT/dt";
        public override string SIUnitName { get; } = "kelvin per second";
        public override string SIUnitLabelLatex { get; } = "\\frac{K}{s}";
        public override double TimeDimension { get; } = -1;
        public override double TemperatureDimension { get; } = 1;

        private static TemperatureRateOfChangeQuantity instance_ = null;
        public static TemperatureRateOfChangeQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new TemperatureRateOfChangeQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public static List<UnitChoice> UnitChoiceDescriptions = new()
        {
            new UnitChoice { UnitName = "kelvin per second", UnitLabel = "K/s", ID = new Guid("524c7149-c168-4103-a8d2-134686e3e267"), ConversionFactorFromSIFormula = "1.0/Factors.Unit", IsSI = true },
            new UnitChoice { UnitName = "kelvin per minute", UnitLabel = "K/min", ID = new Guid("a99064b2-d701-4099-947a-a6e22a14208a"), ConversionFactorFromSIFormula = "Factors.Minute" },
            new UnitChoice { UnitName = "relative celsius per minute", UnitLabel = "°C/min", ID = new Guid("3563299f-730e-4598-9c5c-a58bdce22894"), ConversionFactorFromSIFormula = "Factors.Minute" },
            new UnitChoice { UnitName = "rankine per minute", UnitLabel = "°R/min", ID = new Guid("52491162-b0ec-429c-acdc-64ba1e584de8"), ConversionFactorFromSIFormula = "Factors.Minute/Factors.FahrenheitSlope" },
            new UnitChoice { UnitName = "rankine per hour", UnitLabel = "°R/h", ID = new Guid("c2c1a22c-ad7e-4c09-9c8e-a8cbed00771e"), ConversionFactorFromSIFormula = "Factors.Hour/Factors.FahrenheitSlope" }
        };

        public TemperatureRateOfChangeQuantity() : base()
        {
            Name = GetType().Name.Split("Quantity").First();
            UsualNames = new HashSet<string>() { "temperature rate of change", "temperature rate", "TemperatureRate", "RelativeTemperatureRate" };
            ID = new Guid("57ed8659-32a4-4812-b324-67883f627836");
            DescriptionMD = "**Temperature rate of change** is a relative-temperature variation per unit time; no absolute-temperature bias applies." + Environment.NewLine;
            DescriptionMD += "Its coherent SI unit is kelvin per second with unit label $" + SIUnitLabelLatex + "$." + Environment.NewLine;
            InitializeUnitChoices();
            SemanticExample = GetSemanticExample();
        }
    }
}
