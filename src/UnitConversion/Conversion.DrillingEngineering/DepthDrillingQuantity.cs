using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class DepthDrillingQuantity : LengthQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.001;

        private static DepthDrillingQuantity instance_ = null;

        public static new DepthDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new DepthDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public DepthDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "depth (drilling)" };
            ID = new Guid("c0d965b2-a153-420a-9d03-7a2a272d619e");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of depth in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Metre).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Metre));
            this.UnitChoices.Add(LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Foot));
            SemanticExample = GetSemanticExample();
        }
    }
}
