using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class PlaneAngleGeodesicQuantity : PlaneAngleQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.001 / 6371000.0;
        private static PlaneAngleGeodesicQuantity instance_ = null;

        public static new PlaneAngleGeodesicQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new PlaneAngleGeodesicQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public PlaneAngleGeodesicQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "plane angle (geodesic)" };
            ID = new Guid("fc88c9ff-fa0a-4406-b397-015f3ca062bc");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of a geodesic plane angle is typically: " + MeaningfulPrecisionInSI.ToString() + " " + PlaneAngleQuantity.Instance.GetUnitChoice(PlaneAngleQuantity.UnitChoicesEnum.Radian).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(PlaneAngleQuantity.Instance.GetUnitChoice(PlaneAngleQuantity.UnitChoicesEnum.Radian));
            this.UnitChoices.Add(PlaneAngleQuantity.Instance.GetUnitChoice(PlaneAngleQuantity.UnitChoicesEnum.Degree));
            SemanticExample = GetSemanticExample();
        }
    }
}
