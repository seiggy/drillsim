using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class DoseEquivalentDrillingQuantity : DoseEquivalentQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static DoseEquivalentDrillingQuantity instance_ = null;
        public static new DoseEquivalentDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new DoseEquivalentDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public DoseEquivalentDrillingQuantity() : base()
        {
            Name = "DoseEquivalentDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "DoseEquivalent (drilling)" };
            ID = new Guid("366e1528-b748-4ac9-ba08-23de485832cb");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
