using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class AreaRateOfChangeDrillingQuantity : AreaRateOfChangeQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static AreaRateOfChangeDrillingQuantity instance_ = null;
        public static new AreaRateOfChangeDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new AreaRateOfChangeDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public AreaRateOfChangeDrillingQuantity() : base()
        {
            Name = "AreaRateOfChangeDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "AreaRateOfChange (drilling)" };
            ID = new Guid("37c9e42f-60af-42ef-a035-b2be5fff84da");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
