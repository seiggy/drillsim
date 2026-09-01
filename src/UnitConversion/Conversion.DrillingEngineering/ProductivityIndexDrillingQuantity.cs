using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class ProductivityIndexDrillingQuantity : ProductivityIndexQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-10;

        private static ProductivityIndexDrillingQuantity instance_ = null;
        public static new ProductivityIndexDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new ProductivityIndexDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public ProductivityIndexDrillingQuantity() : base()
        {
            Name = "ProductivityIndexDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "ProductivityIndex (drilling)" };
            ID = new Guid("8ab966e5-1159-417d-ae64-18f8aef375b9");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
