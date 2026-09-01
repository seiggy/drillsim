using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class AreaPerMassDrillingQuantity : AreaPerMassQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static AreaPerMassDrillingQuantity instance_ = null;
        public static new AreaPerMassDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new AreaPerMassDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public AreaPerMassDrillingQuantity() : base()
        {
            Name = "AreaPerMassDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "AreaPerMass (drilling)" };
            ID = new Guid("1c765b7f-c07c-4610-9cae-9c05423b3e6b");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
