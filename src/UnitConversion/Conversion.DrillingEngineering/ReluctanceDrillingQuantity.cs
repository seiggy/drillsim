using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class ReluctanceDrillingQuantity : ReluctanceQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static ReluctanceDrillingQuantity instance_ = null;
        public static new ReluctanceDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new ReluctanceDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public ReluctanceDrillingQuantity() : base()
        {
            Name = "ReluctanceDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "Reluctance (drilling)" };
            ID = new Guid("9f8ffe4e-8b77-4893-88b4-1e2d858bf7cd");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
