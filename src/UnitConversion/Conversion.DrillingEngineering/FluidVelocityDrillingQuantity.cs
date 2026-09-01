using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class FluidVelocityDrillingQuantity : VelocityQuantity
    {
        
        public override double? MeaningfulPrecisionInSI { get; } = 0.01;
        private static FluidVelocityDrillingQuantity instance_ = null;

        public static new FluidVelocityDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new FluidVelocityDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public FluidVelocityDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "fluid velocity (drilling)" };
            ID = new Guid("dac9cee1-59a3-42d5-98c6-0c7baf5083bb");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of fluid velocity in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.MetrePerSecond).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.MetrePerSecond));
            this.UnitChoices.Add(VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.FootPerSecond));
            SemanticExample = GetSemanticExample();
        }
    }
}
