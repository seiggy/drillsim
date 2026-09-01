using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class ElectromagneticMomentDrillingQuantity : ElectromagneticMomentQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static ElectromagneticMomentDrillingQuantity instance_ = null;
        public static new ElectromagneticMomentDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new ElectromagneticMomentDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public ElectromagneticMomentDrillingQuantity() : base()
        {
            Name = "ElectromagneticMomentDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "ElectromagneticMoment (drilling)" };
            ID = new Guid("fab86f29-17b5-4faf-b267-07021ffc7f2a");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
