using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class ReciprocalLengthSurveyInstrumentDrillingQuantity : WaveNumberQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-8;

        private static ReciprocalLengthSurveyInstrumentDrillingQuantity instance_ = null;

        public static new ReciprocalLengthSurveyInstrumentDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ReciprocalLengthSurveyInstrumentDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public ReciprocalLengthSurveyInstrumentDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "reciprocal length (survey instrument ) (drilling)" };
            ID = new Guid("c198aa3b-3b24-402d-b60b-f54ff9430f33");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of reciprocal length related to survey instrument performance models in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalMetre).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalMetre));
            this.UnitChoices.Add(WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalDecametre));
            this.UnitChoices.Add(WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalHectometre));
            this.UnitChoices.Add(WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalKilometre));
            this.UnitChoices.Add(WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalFoot));
            this.UnitChoices.Add(WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalYard));
            this.UnitChoices.Add(WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalMile));
            SemanticExample = GetSemanticExample();
        }
    }
}
