using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class TorqueDrillingQuantity : TorqueQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.1;
        private static TorqueDrillingQuantity instance_ = null;

        public static new TorqueDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new TorqueDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public TorqueDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "torque (drilling)", "TOB" };
            ID = new Guid("eff33c0e-1e92-49e4-a7ab-716d9d66a157");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of torque in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + TorqueQuantity.Instance.GetUnitChoice(TorqueQuantity.UnitChoicesEnum.NewtonMetre).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(TorqueQuantity.Instance.GetUnitChoice(TorqueQuantity.UnitChoicesEnum.NewtonMetre));
            this.UnitChoices.Add(TorqueQuantity.Instance.GetUnitChoice(TorqueQuantity.UnitChoicesEnum.FootPound));
            this.UnitChoices.Add(TorqueQuantity.Instance.GetUnitChoice(TorqueQuantity.UnitChoicesEnum.KilofootPound));
            this.UnitChoices.Add(TorqueQuantity.Instance.GetUnitChoice(TorqueQuantity.UnitChoicesEnum.DecanewtonMetre));
            this.UnitChoices.Add(TorqueQuantity.Instance.GetUnitChoice(TorqueQuantity.UnitChoicesEnum.KilogramForceMetre));
            this.UnitChoices.Add(TorqueQuantity.Instance.GetUnitChoice(TorqueQuantity.UnitChoicesEnum.KilonewtonMetre));
            SemanticExample = GetSemanticExample();
        }
    }
}
