using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class SpecificVolumeSquaredDrillingQuantity : SpecificVolumeSquaredQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-12;

        private static SpecificVolumeSquaredDrillingQuantity instance_ = null;

        public static new SpecificVolumeSquaredDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new SpecificVolumeSquaredDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public SpecificVolumeSquaredDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "specific volume squared (drilling)" };
            ID = new Guid("d6500752-b69c-4011-b8e3-b219434e690d");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of specific volume squared in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + MassDensityQuantity.Instance.GetUnitChoice(MassDensityQuantity.UnitChoicesEnum.KilogramPerCubicMetre).UnitLabel + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
