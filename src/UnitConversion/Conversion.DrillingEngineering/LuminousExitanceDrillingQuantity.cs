using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class LuminousExitanceDrillingQuantity : LuminousExitanceQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-2;

        private static LuminousExitanceDrillingQuantity instance_ = null;
        public static new LuminousExitanceDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new LuminousExitanceDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public LuminousExitanceDrillingQuantity() : base()
        {
            Name = "LuminousExitanceDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "LuminousExitance (drilling)" };
            ID = new Guid("00cce824-b705-4cc9-a862-cb40f47d8ced");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
