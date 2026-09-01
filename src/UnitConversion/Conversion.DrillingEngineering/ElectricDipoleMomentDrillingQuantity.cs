using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class ElectricDipoleMomentDrillingQuantity : ElectricDipoleMomentQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-30;

        private static ElectricDipoleMomentDrillingQuantity instance_ = null;
        public static new ElectricDipoleMomentDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new ElectricDipoleMomentDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public ElectricDipoleMomentDrillingQuantity() : base()
        {
            Name = "ElectricDipoleMomentDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "ElectricDipoleMoment (drilling)" };
            ID = new Guid("d9cb8792-b9e4-4292-b101-78c64f3bb6bf");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
