using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class StickDurationDrillingQuantity : TimeQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.001;

        private static StickDurationDrillingQuantity instance_ = null;

        public static new StickDurationDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new StickDurationDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public StickDurationDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "stick duration (drilling)" };
            ID = new Guid("1e9bafaa-bbf1-4a29-9811-39b5e2280499");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of stick duration in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + TimeQuantity.Instance.GetUnitChoice(TimeQuantity.UnitChoicesEnum.Second).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(TimeQuantity.Instance.GetUnitChoice(TimeQuantity.UnitChoicesEnum.Second));
            this.UnitChoices.Add(TimeQuantity.Instance.GetUnitChoice(TimeQuantity.UnitChoicesEnum.Millisecond));
            SemanticExample = GetSemanticExample();
        }
    }
}
