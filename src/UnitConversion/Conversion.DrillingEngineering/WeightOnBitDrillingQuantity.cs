using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class WeightOnBitDrillingQuantity : ForceQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 100;
        private static WeightOnBitDrillingQuantity instance_ = null;

        public static new WeightOnBitDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new WeightOnBitDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public WeightOnBitDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "weight on bit (drilling)", "WOB" };
            ID = new Guid("5e75da44-a675-4f0e-a0fb-52b2cb6797ce");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of weight on bit in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + ForceQuantity.Instance.GetUnitChoice(ForceQuantity.UnitChoicesEnum.Newton).UnitLabel + Environment.NewLine;
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
