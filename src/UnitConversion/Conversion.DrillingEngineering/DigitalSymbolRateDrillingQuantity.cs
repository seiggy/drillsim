using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class DigitalSymbolRateDrillingQuantity : DigitalSymbolRateQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1.0;

        private static DigitalSymbolRateDrillingQuantity instance_ = null;
        public static new DigitalSymbolRateDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new DigitalSymbolRateDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public DigitalSymbolRateDrillingQuantity() : base()
        {
            Name = "DigitalSymbolRateDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "DigitalSymbolRate (drilling)" };
            ID = new Guid("954e5757-9ba8-417c-8d4a-29ba35fc7c7d");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
