using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class TensionDrillingQuantity : TensionQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 100;
        private static TensionDrillingQuantity instance_ = null;

        public static new TensionDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new TensionDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public TensionDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "tension (drilling)" };
            ID = new Guid("fe8fd6fd-814c-44c9-9462-f034dd46dc85");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of tension in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + TensionQuantity.Instance.GetUnitChoice(TensionQuantity.UnitChoicesEnum.Newton).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(TensionQuantity.Instance.GetUnitChoice(TensionQuantity.UnitChoicesEnum.Newton));
            this.UnitChoices.Add(TensionQuantity.Instance.GetUnitChoice(TensionQuantity.UnitChoicesEnum.Decanewton));
            this.UnitChoices.Add(TensionQuantity.Instance.GetUnitChoice(TensionQuantity.UnitChoicesEnum.Kilodecanewton));
            this.UnitChoices.Add(TensionQuantity.Instance.GetUnitChoice(TensionQuantity.UnitChoicesEnum.KilogramForce));
            this.UnitChoices.Add(TensionQuantity.Instance.GetUnitChoice(TensionQuantity.UnitChoicesEnum.Kilonewton));
            this.UnitChoices.Add(TensionQuantity.Instance.GetUnitChoice(TensionQuantity.UnitChoicesEnum.KilopoundForce));
            this.UnitChoices.Add(TensionQuantity.Instance.GetUnitChoice(TensionQuantity.UnitChoicesEnum.PoundForce));
            SemanticExample = GetSemanticExample();
        }
    }
}
