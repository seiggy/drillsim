using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class SpecificVolumeDrillingQuantity : SpecificVolumeQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static SpecificVolumeDrillingQuantity instance_ = null;

        public static new SpecificVolumeDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new SpecificVolumeDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public SpecificVolumeDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "specific volume (drilling)" };
            ID = new Guid("26b08c06-b8e9-40e5-9b6c-66c93e8d8723");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of specific volume in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + MassDensityQuantity.Instance.GetUnitChoice(MassDensityQuantity.UnitChoicesEnum.KilogramPerCubicMetre).UnitLabel + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
