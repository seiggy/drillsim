using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class MassDensityGradientPerPressureDrillingQuantity : MassDensityGradientPerPressureQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-9;

        private static MassDensityGradientPerPressureDrillingQuantity instance_ = null;

        public static new MassDensityGradientPerPressureDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MassDensityGradientPerPressureDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public MassDensityGradientPerPressureDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "mass density gradient per pressure (drilling)" };
            ID = new Guid("e027fcd6-bc2e-4dec-a886-f6adf4446249");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of mass density gradient per pressure in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + MassDensityGradientPerLengthQuantity.Instance.GetUnitChoice(MassDensityGradientPerLengthQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerMetre).UnitLabel + Environment.NewLine;
            Reset();
            foreach (var uc in MassDensityGradientPerPressureQuantity.Instance.UnitChoices)
            {
                this.UnitChoices.Add(uc);
            }
            SemanticExample = GetSemanticExample();
        }
    }
}
