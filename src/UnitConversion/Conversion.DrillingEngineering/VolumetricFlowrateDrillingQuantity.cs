using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class VolumetricFlowrateDrillingQuantity : VolumetricFlowRateQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.0001;

        private static VolumetricFlowrateDrillingQuantity instance_ = null;

        public static new VolumetricFlowrateDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new VolumetricFlowrateDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public VolumetricFlowrateDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "volumetric flow rate (drilling)" };
            ID = new Guid("7b12c115-3c20-4d45-b4cf-86af29255b14");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of volumetric flowrate in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.CubicFootPerSecond).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.CubicMetrePerSecond));
            this.UnitChoices.Add(VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.LitrePerMinute));
            this.UnitChoices.Add(VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.UKGallonPerMinute));
            this.UnitChoices.Add(VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.USGallonPerMinute));
            SemanticExample = GetSemanticExample();
        }
    }
}
