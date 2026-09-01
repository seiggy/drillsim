using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class HookLoadDrillingQuantity : ForceQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1000;
        private static HookLoadDrillingQuantity instance_ = null;

        public static new HookLoadDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new HookLoadDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public HookLoadDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "hook load (drilling)" };
            ID = new Guid("126be5e9-ed09-459e-92ce-32a5fcd81f0a");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of hook load in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + ForceQuantity.Instance.GetUnitChoice(ForceQuantity.UnitChoicesEnum.Newton).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(ForceQuantity.Instance.GetUnitChoice(ForceQuantity.UnitChoicesEnum.KilogramForce));
            this.UnitChoices.Add(ForceQuantity.Instance.GetUnitChoice(ForceQuantity.UnitChoicesEnum.Decanewton));
            this.UnitChoices.Add(ForceQuantity.Instance.GetUnitChoice(ForceQuantity.UnitChoicesEnum.PoundForce));
            this.UnitChoices.Add(ForceQuantity.Instance.GetUnitChoice(ForceQuantity.UnitChoicesEnum.KilopoundForce));
            this.UnitChoices.Add(ForceQuantity.Instance.GetUnitChoice(ForceQuantity.UnitChoicesEnum.Newton));
            this.UnitChoices.Add(ForceQuantity.Instance.GetUnitChoice(ForceQuantity.UnitChoicesEnum.TonneForce));
            SemanticExample = GetSemanticExample();
        }
    }
}
