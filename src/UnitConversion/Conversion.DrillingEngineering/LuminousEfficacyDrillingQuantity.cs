using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class LuminousEfficacyDrillingQuantity : LuminousEfficacyQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-2;

        private static LuminousEfficacyDrillingQuantity instance_ = null;
        public static new LuminousEfficacyDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new LuminousEfficacyDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public LuminousEfficacyDrillingQuantity() : base()
        {
            Name = "LuminousEfficacyDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "LuminousEfficacy (drilling)" };
            ID = new Guid("919d9b81-dc58-412e-bcf4-68c493899058");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
