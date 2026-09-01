using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class LengthPerVolumeDrillingQuantity : LengthPerVolumeQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static LengthPerVolumeDrillingQuantity instance_ = null;
        public static new LengthPerVolumeDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new LengthPerVolumeDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public LengthPerVolumeDrillingQuantity() : base()
        {
            Name = "LengthPerVolumeDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "LengthPerVolume (drilling)" };
            ID = new Guid("2d093190-a549-408a-91c0-37715547f63f");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
