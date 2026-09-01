using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class DiameterPoreDrillingQuantity : LengthSmallQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.0000001;

        private static DiameterPoreDrillingQuantity instance_ = null;

        public static new DiameterPoreDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new DiameterPoreDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public DiameterPoreDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "diameter (pore) (drilling)" };
            ID = new Guid("781affbc-fae3-40a0-a110-fd3bdfd7d41f");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of pore diameter in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Metre).UnitLabel + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }

    }
}
