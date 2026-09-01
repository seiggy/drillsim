using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class DurationDrillingQuantity : TimeQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.001;

        private static DurationDrillingQuantity instance_ = null;

        public static new DurationDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new DurationDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public DurationDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "duration (drilling)" };
            ID = new Guid("22443197-6bcf-45f7-9079-4f710585af60");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of time in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + TimeQuantity.Instance.GetUnitChoice(TimeQuantity.UnitChoicesEnum.Second).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(TimeQuantity.Instance.GetUnitChoice(TimeQuantity.UnitChoicesEnum.Second));
            this.UnitChoices.Add(TimeQuantity.Instance.GetUnitChoice(TimeQuantity.UnitChoicesEnum.Minute));
            this.UnitChoices.Add(TimeQuantity.Instance.GetUnitChoice(TimeQuantity.UnitChoicesEnum.Hour));
            this.UnitChoices.Add(TimeQuantity.Instance.GetUnitChoice(TimeQuantity.UnitChoicesEnum.Day));
            SemanticExample = GetSemanticExample();
        }
    }
}
