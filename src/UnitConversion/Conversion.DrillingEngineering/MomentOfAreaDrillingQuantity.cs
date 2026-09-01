using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class MomentOfAreaDrillingQuantity : MomentOfAreaQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-10;
        private static MomentOfAreaDrillingQuantity instance_ = null;

        public static new MomentOfAreaDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MomentOfAreaDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public MomentOfAreaDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "moment of area (drilling)" };
            ID = new Guid("1805e50f-3bf0-4347-9e5a-cc169a124b7e");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of the moment of area in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + ForceQuantity.Instance.GetUnitChoice(ForceQuantity.UnitChoicesEnum.Newton).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(MomentOfAreaQuantity.Instance.GetUnitChoice(MomentOfAreaQuantity.UnitChoicesEnum.CentimetresToTheFourthPower));
            this.UnitChoices.Add(MomentOfAreaQuantity.Instance.GetUnitChoice(MomentOfAreaQuantity.UnitChoicesEnum.FeetToTheFourthPower));
            this.UnitChoices.Add(MomentOfAreaQuantity.Instance.GetUnitChoice(MomentOfAreaQuantity.UnitChoicesEnum.InchesToTheFourthPower));
            this.UnitChoices.Add(MomentOfAreaQuantity.Instance.GetUnitChoice(MomentOfAreaQuantity.UnitChoicesEnum.MetresToTheFourthPower));
            SemanticExample = GetSemanticExample();
        }
    }
}
