using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class CableDiameterDrillingQuantity : LengthSmallQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.0005;

        private static CableDiameterDrillingQuantity instance_ = null;

        public static new CableDiameterDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new CableDiameterDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public CableDiameterDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "cable diameter (drilling)" };
            ID = new Guid("6b3db5b2-7e30-47f7-91c5-5951dd3f9246");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of a cable diameter in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Metre).UnitLabel + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
