using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class MassDensityGradientPerPressureSquaredDrillingQuantity : MassDensityGradientPerPressureSquaredQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-18;

        private static MassDensityGradientPerPressureSquaredDrillingQuantity instance_ = null;

        public static new MassDensityGradientPerPressureSquaredDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MassDensityGradientPerPressureSquaredDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public MassDensityGradientPerPressureSquaredDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "mass density gradient per pressure squared (drilling)" };
            ID = new Guid("b10b7f56-ee6a-449e-badb-156b8466e6cb");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of mass density gradient per pressure squared in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + MassDensityGradientPerLengthQuantity.Instance.GetUnitChoice(MassDensityGradientPerLengthQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerMetre).UnitLabel + Environment.NewLine;
            Reset();
            foreach (var uc in MassDensityGradientPerPressureSquaredQuantity.Instance.UnitChoices)
            {
                this.UnitChoices.Add(uc);
            }
            SemanticExample = GetSemanticExample();
        }
    }
}
