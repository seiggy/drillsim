using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class PlaneAngleDrillingQuantity : PlaneAngleQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.01 * Math.PI / 180.0;
        private static PlaneAngleDrillingQuantity instance_ = null;

        public static new PlaneAngleDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new PlaneAngleDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public PlaneAngleDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "plane angle (drilling)" };
            ID = new Guid("94ad3e73-2a44-4c60-bbca-188b941f3357");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of plane angle in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + PlaneAngleQuantity.Instance.GetUnitChoice(PlaneAngleQuantity.UnitChoicesEnum.Radian).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(PlaneAngleQuantity.Instance.GetUnitChoice(PlaneAngleQuantity.UnitChoicesEnum.Radian));
            this.UnitChoices.Add(PlaneAngleQuantity.Instance.GetUnitChoice(PlaneAngleQuantity.UnitChoicesEnum.Degree));
            SemanticExample = GetSemanticExample();
        }
    }
}
