using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class ThermalConductivityGradientPerTemperatureDrillingQuantity : ThermalConductivityGradientPerTemperatureQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.01;
        private static ThermalConductivityGradientPerTemperatureDrillingQuantity instance_ = null;

        public static new ThermalConductivityGradientPerTemperatureDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ThermalConductivityGradientPerTemperatureDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public ThermalConductivityGradientPerTemperatureDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "thermal conductivity temperature gradient per temperature (drilling)" };
            ID = new Guid("559ae484-42ed-4379-86f5-67dae451a9c9");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of thermal conductivity gradient per temperature in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + ThermalConductivityGradientPerTemperatureQuantity.Instance.GetUnitChoice(ThermalConductivityGradientPerTemperatureQuantity.UnitChoicesEnum.WattPerMetreKelvinPerKelvin).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(ThermalConductivityGradientPerTemperatureQuantity.Instance.GetUnitChoice(ThermalConductivityGradientPerTemperatureQuantity.UnitChoicesEnum.WattPerMetreKelvinPerKelvin));
            this.UnitChoices.Add(ThermalConductivityGradientPerTemperatureQuantity.Instance.GetUnitChoice(ThermalConductivityGradientPerTemperatureQuantity.UnitChoicesEnum.BritishThermalUnitInchPerHourSquareFootDegreeFahrenheitSquared));
            this.UnitChoices.Add(ThermalConductivityGradientPerTemperatureQuantity.Instance.GetUnitChoice(ThermalConductivityGradientPerTemperatureQuantity.UnitChoicesEnum.BritishThermalUnitPerHourFootDegreeFahrenheitSquared));
            this.UnitChoices.Add(ThermalConductivityGradientPerTemperatureQuantity.Instance.GetUnitChoice(ThermalConductivityGradientPerTemperatureQuantity.UnitChoicesEnum.CaloriePerCentimetreSecondDegreeCelsiusSquared));
            this.UnitChoices.Add(ThermalConductivityGradientPerTemperatureQuantity.Instance.GetUnitChoice(ThermalConductivityGradientPerTemperatureQuantity.UnitChoicesEnum.CaloriePerMetreSecondDegreeCelsiusSquared));
            SemanticExample = GetSemanticExample();
        }
    }
}
