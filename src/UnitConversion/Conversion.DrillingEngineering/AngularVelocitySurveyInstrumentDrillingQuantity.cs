using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class AngularVelocitySurveyInstrumentDrillingQuantity : AngularVelocityQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-7;

        private static AngularVelocitySurveyInstrumentDrillingQuantity instance_ = null;

        public static new AngularVelocitySurveyInstrumentDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new AngularVelocitySurveyInstrumentDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public AngularVelocitySurveyInstrumentDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "angular velocity (survey instrument) (drilling)" };
            ID = new Guid("76dd6ac8-8b67-416c-b41f-07bbf4065cdb");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of angular velocity related to survey instrument performance models in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.RadianTesla).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(AngularVelocityQuantity.Instance.GetUnitChoice(AngularVelocityQuantity.UnitChoicesEnum.RadianPerSecond));
            this.UnitChoices.Add(AngularVelocityQuantity.Instance.GetUnitChoice(AngularVelocityQuantity.UnitChoicesEnum.DegreePerSecond));
            this.UnitChoices.Add(AngularVelocityQuantity.Instance.GetUnitChoice(AngularVelocityQuantity.UnitChoicesEnum.DegreePerMinute));
            this.UnitChoices.Add(AngularVelocityQuantity.Instance.GetUnitChoice(AngularVelocityQuantity.UnitChoicesEnum.DegreePerHour));
            this.UnitChoices.Add(AngularVelocityQuantity.Instance.GetUnitChoice(AngularVelocityQuantity.UnitChoicesEnum.DegreePerDay));
            SemanticExample = GetSemanticExample();
        }
    }
}
