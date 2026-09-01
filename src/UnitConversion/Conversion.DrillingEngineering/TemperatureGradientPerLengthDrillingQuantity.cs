using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class TemperatureGradientPerLengthDrillingQuantity : TemperatureGradientPerLengthQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.001;
        private static TemperatureGradientPerLengthDrillingQuantity instance_ = null;

        public static new TemperatureGradientPerLengthDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new TemperatureGradientPerLengthDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public TemperatureGradientPerLengthDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "temperature gradient per length (drilling)" };
            ID = new Guid("82b91f3f-d1ec-476b-98e0-eedbba6281ec");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of temperature gradient per length in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + TemperatureGradientPerLengthQuantity.Instance.GetUnitChoice(TemperatureGradientPerLengthQuantity.UnitChoicesEnum.KelvinPerMetre).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(TemperatureGradientPerLengthQuantity.Instance.GetUnitChoice(TemperatureGradientPerLengthQuantity.UnitChoicesEnum.KelvinPerMetre));
            this.UnitChoices.Add(TemperatureGradientPerLengthQuantity.Instance.GetUnitChoice(TemperatureGradientPerLengthQuantity.UnitChoicesEnum.CelsiusPerMetre));
            this.UnitChoices.Add(TemperatureGradientPerLengthQuantity.Instance.GetUnitChoice(TemperatureGradientPerLengthQuantity.UnitChoicesEnum.CelsiusPer10Metre));
            this.UnitChoices.Add(TemperatureGradientPerLengthQuantity.Instance.GetUnitChoice(TemperatureGradientPerLengthQuantity.UnitChoicesEnum.CelsiusPer30Metre));
            this.UnitChoices.Add(TemperatureGradientPerLengthQuantity.Instance.GetUnitChoice(TemperatureGradientPerLengthQuantity.UnitChoicesEnum.CelsiusPer100Metre));
            this.UnitChoices.Add(TemperatureGradientPerLengthQuantity.Instance.GetUnitChoice(TemperatureGradientPerLengthQuantity.UnitChoicesEnum.CelsiusPerFoot));
            this.UnitChoices.Add(TemperatureGradientPerLengthQuantity.Instance.GetUnitChoice(TemperatureGradientPerLengthQuantity.UnitChoicesEnum.CelsiusPer30Foot));
            this.UnitChoices.Add(TemperatureGradientPerLengthQuantity.Instance.GetUnitChoice(TemperatureGradientPerLengthQuantity.UnitChoicesEnum.CelsiusPer100Foot));
            this.UnitChoices.Add(TemperatureGradientPerLengthQuantity.Instance.GetUnitChoice(TemperatureGradientPerLengthQuantity.UnitChoicesEnum.FahrenheitPerFoot));
            this.UnitChoices.Add(TemperatureGradientPerLengthQuantity.Instance.GetUnitChoice(TemperatureGradientPerLengthQuantity.UnitChoicesEnum.FahrenheitPer30Foot));
            this.UnitChoices.Add(TemperatureGradientPerLengthQuantity.Instance.GetUnitChoice(TemperatureGradientPerLengthQuantity.UnitChoicesEnum.FahrenheitPer100Foot));
            SemanticExample = GetSemanticExample();
        }
    }
}
