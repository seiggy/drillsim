using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class MassDensityGradientPerPressureTemperatureDrillingQuantity : MassDensityGradientPerPressureTemperatureQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-11;

        private static MassDensityGradientPerPressureTemperatureDrillingQuantity instance_ = null;

        public static new MassDensityGradientPerPressureTemperatureDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MassDensityGradientPerPressureTemperatureDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public MassDensityGradientPerPressureTemperatureDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "mass density gradient per pressure temperature (drilling)" };
            ID = new Guid("589f259f-28d9-405c-9b1b-18804fc3446a");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of mass density gradient per pressure and temperature in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + MassDensityGradientPerLengthQuantity.Instance.GetUnitChoice(MassDensityGradientPerLengthQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerMetre).UnitLabel + Environment.NewLine;
            Reset();
            foreach (var uc in MassDensityGradientPerPressureTemperatureQuantity.Instance.UnitChoices)
            {
                this.UnitChoices.Add(uc);
            }
            SemanticExample = GetSemanticExample();
        }
    }
}
