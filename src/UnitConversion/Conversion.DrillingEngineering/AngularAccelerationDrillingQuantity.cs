using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class AngularAccelerationDrillingQuantity : AngularAccelerationQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.01;

        private static AngularAccelerationDrillingQuantity instance_ = null;

        public static new AngularAccelerationDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new AngularAccelerationDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public AngularAccelerationDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "angular acceleration (drilling)" };
            ID = new Guid("0077dbf8-bd21-4cc7-a180-b2c75229dd87");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of angular acceleration in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.RadianPerSecondSquared).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.RadianPerSecondSquared));
            this.UnitChoices.Add(AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.DegreePerSecondSquared));
            SemanticExample = GetSemanticExample();
        }
    }
}
