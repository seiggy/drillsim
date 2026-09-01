using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class AngularVelocityDrillingQuantity : AngularVelocityQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.01;

        private static AngularVelocityDrillingQuantity instance_ = null;

        public static new AngularVelocityDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new AngularVelocityDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public AngularVelocityDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "angular velocity (drilling)" };
            ID = new Guid("046cb449-8cab-4d0c-bb28-3e2060f292e5");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of angular velocity in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + AngularVelocityQuantity.Instance.GetUnitChoice(AngularVelocityQuantity.UnitChoicesEnum.RadianPerSecond).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(AngularVelocityQuantity.Instance.GetUnitChoice(AngularVelocityQuantity.UnitChoicesEnum.RadianPerSecond));
            this.UnitChoices.Add(AngularVelocityQuantity.Instance.GetUnitChoice(AngularVelocityQuantity.UnitChoicesEnum.DegreePerSecond));
            this.UnitChoices.Add(AngularVelocityQuantity.Instance.GetUnitChoice(AngularVelocityQuantity.UnitChoicesEnum.RevolutionPerMinute));
            this.UnitChoices.Add(AngularVelocityQuantity.Instance.GetUnitChoice(AngularVelocityQuantity.UnitChoicesEnum.StrokePerMinute));
            SemanticExample = GetSemanticExample();
        }
    }
}
