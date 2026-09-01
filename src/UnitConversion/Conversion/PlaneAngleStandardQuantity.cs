using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class PlaneAngleStandardQuantity : PlaneAngleQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.01 * Math.PI / 180.0;
        private static PlaneAngleStandardQuantity instance_ = null;

        public static new PlaneAngleStandardQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new PlaneAngleStandardQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public PlaneAngleStandardQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "plane angle (standard)" };
            ID = new Guid("3773fc19-be1a-4604-91f8-6e4aefb44757");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of a standard plane angle is typically: " + MeaningfulPrecisionInSI.ToString() + " " + PlaneAngleQuantity.Instance.GetUnitChoice(PlaneAngleQuantity.UnitChoicesEnum.Radian).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(PlaneAngleQuantity.Instance.GetUnitChoice(PlaneAngleQuantity.UnitChoicesEnum.Radian));
            this.UnitChoices.Add(PlaneAngleQuantity.Instance.GetUnitChoice(PlaneAngleQuantity.UnitChoicesEnum.Degree));
            SemanticExample = GetSemanticExample();
        }
    }
}
