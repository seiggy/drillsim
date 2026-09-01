using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class ForceDrillingQuantity : ForceQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.1;
        private static ForceDrillingQuantity instance_ = null;

        public static new ForceDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ForceDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public ForceDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "force (drilling)" };
            ID = new Guid("30c08b42-6a89-4d99-879b-882eb7ed46d0");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of force in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + ForceQuantity.Instance.GetUnitChoice(ForceQuantity.UnitChoicesEnum.Newton).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(ForceQuantity.Instance.GetUnitChoice(ForceQuantity.UnitChoicesEnum.Newton));
            this.UnitChoices.Add(ForceQuantity.Instance.GetUnitChoice(ForceQuantity.UnitChoicesEnum.Decanewton));
            this.UnitChoices.Add(ForceQuantity.Instance.GetUnitChoice(ForceQuantity.UnitChoicesEnum.Kilodecanewton));
            this.UnitChoices.Add(ForceQuantity.Instance.GetUnitChoice(ForceQuantity.UnitChoicesEnum.KilogramForce));
            this.UnitChoices.Add(ForceQuantity.Instance.GetUnitChoice(ForceQuantity.UnitChoicesEnum.Kilonewton));
            this.UnitChoices.Add(ForceQuantity.Instance.GetUnitChoice(ForceQuantity.UnitChoicesEnum.KilopoundForce));
            this.UnitChoices.Add(ForceQuantity.Instance.GetUnitChoice(ForceQuantity.UnitChoicesEnum.PoundForce));
            SemanticExample = GetSemanticExample();
        }
    }
}
