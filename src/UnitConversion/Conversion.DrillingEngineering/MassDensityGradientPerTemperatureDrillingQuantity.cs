using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class MassDensityGradientPerTemperatureDrillingQuantity : MassDensityGradientPerTemperatureQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.001;

        private static MassDensityGradientPerTemperatureDrillingQuantity instance_ = null;

        public static new MassDensityGradientPerTemperatureDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MassDensityGradientPerTemperatureDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public MassDensityGradientPerTemperatureDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "mass density gradient per temperature (drilling)" };
            ID = new Guid("b5ccd49c-a9fd-4cfd-8dd1-fb4d3f8c3ad5");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of mass density gradient per temperature in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + MassDensityGradientPerTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerTemperatureQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerKelvin).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(MassDensityGradientPerTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerTemperatureQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerKelvin));
            this.UnitChoices.Add(MassDensityGradientPerTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerTemperatureQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerCelsius));
            this.UnitChoices.Add(MassDensityGradientPerTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUKPerCelsius));
            this.UnitChoices.Add(MassDensityGradientPerTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUSPerFahrenheit));
            this.UnitChoices.Add(MassDensityGradientPerTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerTemperatureQuantity.UnitChoicesEnum.SpecificGravityPerCelsius));
            SemanticExample = GetSemanticExample();
        }
    }
}
