using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class AmountSubstancePerVolumeDrillingQuantity : AmountSubstancePerVolumeQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-3;

        private static AmountSubstancePerVolumeDrillingQuantity instance_ = null;
        public static new AmountSubstancePerVolumeDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new AmountSubstancePerVolumeDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public AmountSubstancePerVolumeDrillingQuantity() : base()
        {
            Name = "AmountSubstancePerVolumeDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "AmountSubstancePerVolume (drilling)" };
            ID = new Guid("f6514264-f0b2-41cd-8dc6-19e6a16afee7");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
