using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class VolumePerVolumeRateOfChangeDrillingQuantity : VolumePerVolumeRateOfChangeQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-8;

        private static VolumePerVolumeRateOfChangeDrillingQuantity instance_ = null;
        public static new VolumePerVolumeRateOfChangeDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new VolumePerVolumeRateOfChangeDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public VolumePerVolumeRateOfChangeDrillingQuantity() : base()
        {
            Name = "VolumePerVolumeRateOfChangeDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "VolumePerVolumeRateOfChange (drilling)" };
            ID = new Guid("a87ad857-bc0c-42e6-8628-302acaaef829");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
