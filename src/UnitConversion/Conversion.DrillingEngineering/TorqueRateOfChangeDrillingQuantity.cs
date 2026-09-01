using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class TorqueRateOfChangeDrillingQuantity : TorqueRateOfChangeQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.1;

        private static TorqueRateOfChangeDrillingQuantity instance_ = null;

        public static new TorqueRateOfChangeDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new TorqueRateOfChangeDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public TorqueRateOfChangeDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "torque rate of change (drilling)" };
            ID = new Guid("a8053578-6cf0-4c46-b046-a6dbc7cb360f");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of torque rate of change in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + TorqueRateOfChangeQuantity.Instance.GetUnitChoice(TorqueRateOfChangeQuantity.UnitChoicesEnum.NewtonMetrePerSecond).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(TorqueRateOfChangeQuantity.Instance.GetUnitChoice(TorqueRateOfChangeQuantity.UnitChoicesEnum.NewtonMetrePerSecond));
            this.UnitChoices.Add(TorqueRateOfChangeQuantity.Instance.GetUnitChoice(TorqueRateOfChangeQuantity.UnitChoicesEnum.FootPoundPerSecond));
            this.UnitChoices.Add(TorqueRateOfChangeQuantity.Instance.GetUnitChoice(TorqueRateOfChangeQuantity.UnitChoicesEnum.KilofootPoundPerSecond));
            this.UnitChoices.Add(TorqueRateOfChangeQuantity.Instance.GetUnitChoice(TorqueRateOfChangeQuantity.UnitChoicesEnum.DecanewtonMetrePerSecond));
            this.UnitChoices.Add(TorqueRateOfChangeQuantity.Instance.GetUnitChoice(TorqueRateOfChangeQuantity.UnitChoicesEnum.KilogramForceMetrePerSecond));
            this.UnitChoices.Add(TorqueRateOfChangeQuantity.Instance.GetUnitChoice(TorqueRateOfChangeQuantity.UnitChoicesEnum.KilonewtonMetrePerSecond));

            this.UnitChoices.Add(TorqueRateOfChangeQuantity.Instance.GetUnitChoice(TorqueRateOfChangeQuantity.UnitChoicesEnum.NewtonMetrePerMinute));
            this.UnitChoices.Add(TorqueRateOfChangeQuantity.Instance.GetUnitChoice(TorqueRateOfChangeQuantity.UnitChoicesEnum.FootPoundPerMinute));
            this.UnitChoices.Add(TorqueRateOfChangeQuantity.Instance.GetUnitChoice(TorqueRateOfChangeQuantity.UnitChoicesEnum.KilofootPoundPerMinute));
            this.UnitChoices.Add(TorqueRateOfChangeQuantity.Instance.GetUnitChoice(TorqueRateOfChangeQuantity.UnitChoicesEnum.DecanewtonMetrePerMinute));
            this.UnitChoices.Add(TorqueRateOfChangeQuantity.Instance.GetUnitChoice(TorqueRateOfChangeQuantity.UnitChoicesEnum.KilogramForceMetrePerMinute));
            this.UnitChoices.Add(TorqueRateOfChangeQuantity.Instance.GetUnitChoice(TorqueRateOfChangeQuantity.UnitChoicesEnum.KilonewtonMetrePerMinute));

            this.UnitChoices.Add(TorqueRateOfChangeQuantity.Instance.GetUnitChoice(TorqueRateOfChangeQuantity.UnitChoicesEnum.NewtonMetrePerHour));
            this.UnitChoices.Add(TorqueRateOfChangeQuantity.Instance.GetUnitChoice(TorqueRateOfChangeQuantity.UnitChoicesEnum.FootPoundPerHour));
            this.UnitChoices.Add(TorqueRateOfChangeQuantity.Instance.GetUnitChoice(TorqueRateOfChangeQuantity.UnitChoicesEnum.KilofootPoundPerHour));
            this.UnitChoices.Add(TorqueRateOfChangeQuantity.Instance.GetUnitChoice(TorqueRateOfChangeQuantity.UnitChoicesEnum.DecanewtonMetrePerHour));
            this.UnitChoices.Add(TorqueRateOfChangeQuantity.Instance.GetUnitChoice(TorqueRateOfChangeQuantity.UnitChoicesEnum.KilogramForceMetrePerHour));
            this.UnitChoices.Add(TorqueRateOfChangeQuantity.Instance.GetUnitChoice(TorqueRateOfChangeQuantity.UnitChoicesEnum.KilonewtonMetrePerHour));

            SemanticExample = GetSemanticExample();
        }
    }
}
