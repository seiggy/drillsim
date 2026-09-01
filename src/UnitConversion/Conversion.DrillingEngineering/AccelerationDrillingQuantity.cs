using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class AccelerationDrillingQuantity : AccelerationQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.001;

        private static AccelerationDrillingQuantity instance_ = null;

        public static new AccelerationDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new AccelerationDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public AccelerationDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "acceleration (drilling)" };
            ID = new Guid("b6c99136-8e57-4eea-9a31-fb804bc8ae4b");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += @"The meaningful precision of acceleration in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + AccelerationQuantity.Instance.GetUnitChoice(AccelerationQuantity.UnitChoicesEnum.MetrePerSecondSquared).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(AccelerationQuantity.Instance.GetUnitChoice(AccelerationQuantity.UnitChoicesEnum.MetrePerSecondSquared));
            this.UnitChoices.Add(AccelerationQuantity.Instance.GetUnitChoice(AccelerationQuantity.UnitChoicesEnum.FootPerSecondSquared));
            SemanticExample = GetSemanticExample();
        }
    }
}
