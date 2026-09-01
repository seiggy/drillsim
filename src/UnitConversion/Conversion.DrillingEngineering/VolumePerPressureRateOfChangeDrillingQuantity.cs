using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class VolumePerPressureRateOfChangeDrillingQuantity : VolumePerPressureRateOfChangeQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-8;

        private static VolumePerPressureRateOfChangeDrillingQuantity instance_ = null;
        public static new VolumePerPressureRateOfChangeDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new VolumePerPressureRateOfChangeDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public VolumePerPressureRateOfChangeDrillingQuantity() : base()
        {
            Name = "VolumePerPressureRateOfChangeDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "VolumePerPressureRateOfChange (drilling)" };
            ID = new Guid("693db64f-045d-407c-8339-e9953029fb8d");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
