using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class TemperatureDrillingQuantity : TemperatureQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.01;
        private static TemperatureDrillingQuantity instance_ = null;

        public static new TemperatureDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new TemperatureDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public TemperatureDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "temperature (drilling)" };
            ID = new Guid("84ce5a77-fd76-4014-ad8e-03f194c3e329");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of temperature in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + TemperatureQuantity.Instance.GetUnitChoice(TemperatureQuantity.UnitChoicesEnum.Kelvin).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(TemperatureQuantity.Instance.GetUnitChoice(TemperatureQuantity.UnitChoicesEnum.Kelvin));
            this.UnitChoices.Add(TemperatureQuantity.Instance.GetUnitChoice(TemperatureQuantity.UnitChoicesEnum.Celsius));
            this.UnitChoices.Add(TemperatureQuantity.Instance.GetUnitChoice(TemperatureQuantity.UnitChoicesEnum.Fahrenheit));
            SemanticExample = GetSemanticExample();
        }
    }
}
