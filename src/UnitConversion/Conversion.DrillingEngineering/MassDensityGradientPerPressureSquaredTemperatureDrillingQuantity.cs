using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class MassDensityGradientPerPressureSquaredTemperatureDrillingQuantity : MassDensityGradientPerPressureSquaredTemperatureQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-20;

        private static MassDensityGradientPerPressureSquaredTemperatureDrillingQuantity instance_ = null;

        public static new MassDensityGradientPerPressureSquaredTemperatureDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MassDensityGradientPerPressureSquaredTemperatureDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public MassDensityGradientPerPressureSquaredTemperatureDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "mass density gradient per pressure squared temperature (drilling)" };
            ID = new Guid("83831fcf-deb2-4b8e-a250-31c5c3f206cb");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of mass density gradient per pressure squared and temperature in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + MassDensityGradientPerLengthQuantity.Instance.GetUnitChoice(MassDensityGradientPerLengthQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerMetre).UnitLabel + Environment.NewLine;
            Reset();
            foreach (var uc in MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.UnitChoices)
            {
                this.UnitChoices.Add(uc);
            }
            SemanticExample = GetSemanticExample();
        }
    }
}
