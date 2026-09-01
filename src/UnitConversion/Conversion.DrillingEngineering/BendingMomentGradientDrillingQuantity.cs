using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class BendingMomentGradientDrillingQuantity : BendingMomentGradientQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1.0;

        private static BendingMomentGradientDrillingQuantity instance_ = null;
        public static new BendingMomentGradientDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new BendingMomentGradientDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public BendingMomentGradientDrillingQuantity() : base()
        {
            Name = "BendingMomentGradientDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "BendingMomentGradient (drilling)" };
            ID = new Guid("d827e7f0-6381-4617-9b04-e28d2f90b9e1");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
