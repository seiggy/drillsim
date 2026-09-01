using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class IsobaricSpecificHeatCapacityGradientPerTemperatureDrillingQuantity : IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.01;
        private static IsobaricSpecificHeatCapacityGradientPerTemperatureDrillingQuantity instance_ = null;

        public static new IsobaricSpecificHeatCapacityGradientPerTemperatureDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new IsobaricSpecificHeatCapacityGradientPerTemperatureDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public IsobaricSpecificHeatCapacityGradientPerTemperatureDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "specific heat capacity gradient per temperature (drilling)" };
            ID = new Guid("5f180166-bc44-4855-916f-236a5a31893d");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of isobaric specific heat capacity gradient per temperature in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.UnitChoicesEnum.JoulePerKilogramSquaredKelvin).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.UnitChoicesEnum.JoulePerKilogramSquaredKelvin));
            this.UnitChoices.Add(IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.UnitChoicesEnum.JoulePerGramSquaredKelvin));
            this.UnitChoices.Add(IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.UnitChoicesEnum.JoulePerGramDegreeSquaredCelsius));
            this.UnitChoices.Add(IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.UnitChoicesEnum.CaloriePerGramDegreeSquaredCelsius));
            this.UnitChoices.Add(IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.UnitChoicesEnum.BritishThermalUnitPerPoundSquaredDegreeFahrenheit));
            SemanticExample = GetSemanticExample();
        }
    }
}
