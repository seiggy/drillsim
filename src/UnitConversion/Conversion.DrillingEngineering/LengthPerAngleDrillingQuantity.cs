using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class LengthPerAngleDrillingQuantity : LengthPerAngleQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static LengthPerAngleDrillingQuantity instance_ = null;
        public static new LengthPerAngleDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new LengthPerAngleDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public LengthPerAngleDrillingQuantity() : base()
        {
            Name = "LengthPerAngleDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "LengthPerAngle (drilling)" };
            ID = new Guid("35dc5b46-0aaf-4bd3-86b8-e186931acc11");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
