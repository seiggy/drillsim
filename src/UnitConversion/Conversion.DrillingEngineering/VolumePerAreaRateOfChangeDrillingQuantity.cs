using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class VolumePerAreaRateOfChangeDrillingQuantity : VolumePerAreaRateOfChangeQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-8;

        private static VolumePerAreaRateOfChangeDrillingQuantity instance_ = null;
        public static new VolumePerAreaRateOfChangeDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new VolumePerAreaRateOfChangeDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public VolumePerAreaRateOfChangeDrillingQuantity() : base()
        {
            Name = "VolumePerAreaRateOfChangeDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "VolumePerAreaRateOfChange (drilling)" };
            ID = new Guid("f79e17ea-ce34-4e97-891a-fdc7a27d21d6");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
