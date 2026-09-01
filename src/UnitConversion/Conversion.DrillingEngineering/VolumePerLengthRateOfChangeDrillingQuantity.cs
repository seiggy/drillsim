using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class VolumePerLengthRateOfChangeDrillingQuantity : VolumePerLengthRateOfChangeQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-8;

        private static VolumePerLengthRateOfChangeDrillingQuantity instance_ = null;
        public static new VolumePerLengthRateOfChangeDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new VolumePerLengthRateOfChangeDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public VolumePerLengthRateOfChangeDrillingQuantity() : base()
        {
            Name = "VolumePerLengthRateOfChangeDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "VolumePerLengthRateOfChange (drilling)" };
            ID = new Guid("56a93665-e1a0-4555-96df-5ec585e1c386");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
