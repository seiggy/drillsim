using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class VolumeLargeQuantity : VolumeQuantity
    {

        public override double? MeaningfulPrecisionInSI { get; } = 0.1;
        private static VolumeLargeQuantity instance_ = null;

        public static new VolumeLargeQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new VolumeLargeQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public VolumeLargeQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "volume (large)" };
            ID = new Guid("f8ab1afa-7b99-403b-9410-93598bbb4089");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += @"The meaningful precision of a large volume is typically: " + MeaningfulPrecisionInSI.ToString() + " " + VolumeQuantity.Instance.GetUnitChoice(VolumeQuantity.UnitChoicesEnum.CubicMetre).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(VolumeQuantity.Instance.GetUnitChoice(VolumeQuantity.UnitChoicesEnum.CubicMetre));
            this.UnitChoices.Add(VolumeQuantity.Instance.GetUnitChoice(VolumeQuantity.UnitChoicesEnum.Litre));
            this.UnitChoices.Add(VolumeQuantity.Instance.GetUnitChoice(VolumeQuantity.UnitChoicesEnum.USGallon));
            this.UnitChoices.Add(VolumeQuantity.Instance.GetUnitChoice(VolumeQuantity.UnitChoicesEnum.UKGallon));
            this.UnitChoices.Add(VolumeQuantity.Instance.GetUnitChoice(VolumeQuantity.UnitChoicesEnum.Barrel));
            this.UnitChoices.Add(VolumeQuantity.Instance.GetUnitChoice(VolumeQuantity.UnitChoicesEnum.MillionCubicMetre));
            this.UnitChoices.Add(VolumeQuantity.Instance.GetUnitChoice(VolumeQuantity.UnitChoicesEnum.MillionLitre));
            this.UnitChoices.Add(VolumeQuantity.Instance.GetUnitChoice(VolumeQuantity.UnitChoicesEnum.MillionUKGallon));
            this.UnitChoices.Add(VolumeQuantity.Instance.GetUnitChoice(VolumeQuantity.UnitChoicesEnum.MillionBarrel));
            this.UnitChoices.Add(VolumeQuantity.Instance.GetUnitChoice(VolumeQuantity.UnitChoicesEnum.MillionStandardCubicFoot));
            this.UnitChoices.Add(VolumeQuantity.Instance.GetUnitChoice(VolumeQuantity.UnitChoicesEnum.ThousandStandardCubicFoot));
            SemanticExample = GetSemanticExample();
        }
    }
}
