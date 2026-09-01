using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class ForceRateOfChangeDrillingQuantity : ForceRateOfChangeQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.1;

        private static ForceRateOfChangeDrillingQuantity instance_ = null;

        public static new ForceRateOfChangeDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ForceRateOfChangeDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public ForceRateOfChangeDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "force rate of change (drilling)" };
            ID = new Guid("2f93df65-edf4-46fb-b4f0-658c854b2845");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of force rate of change in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + ForceRateOfChangeQuantity.Instance.GetUnitChoice(ForceRateOfChangeQuantity.UnitChoicesEnum.NewtonPerSecond).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(ForceRateOfChangeQuantity.Instance.GetUnitChoice(ForceRateOfChangeQuantity.UnitChoicesEnum.NewtonPerSecond));
            this.UnitChoices.Add(ForceRateOfChangeQuantity.Instance.GetUnitChoice(ForceRateOfChangeQuantity.UnitChoicesEnum.DecanewtonPerSecond));
            this.UnitChoices.Add(ForceRateOfChangeQuantity.Instance.GetUnitChoice(ForceRateOfChangeQuantity.UnitChoicesEnum.KilonewtonPerSecond));
            this.UnitChoices.Add(ForceRateOfChangeQuantity.Instance.GetUnitChoice(ForceRateOfChangeQuantity.UnitChoicesEnum.KilodecanewtonPerSecond));
            this.UnitChoices.Add(ForceRateOfChangeQuantity.Instance.GetUnitChoice(ForceRateOfChangeQuantity.UnitChoicesEnum.KilogramForcePerSecond));
            this.UnitChoices.Add(ForceRateOfChangeQuantity.Instance.GetUnitChoice(ForceRateOfChangeQuantity.UnitChoicesEnum.KilopoundForcePerSecond));
            this.UnitChoices.Add(ForceRateOfChangeQuantity.Instance.GetUnitChoice(ForceRateOfChangeQuantity.UnitChoicesEnum.PoundForcePerSecond));

            this.UnitChoices.Add(ForceRateOfChangeQuantity.Instance.GetUnitChoice(ForceRateOfChangeQuantity.UnitChoicesEnum.NewtonPerMinute));
            this.UnitChoices.Add(ForceRateOfChangeQuantity.Instance.GetUnitChoice(ForceRateOfChangeQuantity.UnitChoicesEnum.DecanewtonPerMinute));
            this.UnitChoices.Add(ForceRateOfChangeQuantity.Instance.GetUnitChoice(ForceRateOfChangeQuantity.UnitChoicesEnum.KilonewtonPerMinute));
            this.UnitChoices.Add(ForceRateOfChangeQuantity.Instance.GetUnitChoice(ForceRateOfChangeQuantity.UnitChoicesEnum.KilodecanewtonPerMinute));
            this.UnitChoices.Add(ForceRateOfChangeQuantity.Instance.GetUnitChoice(ForceRateOfChangeQuantity.UnitChoicesEnum.KilogramForcePerMinute));
            this.UnitChoices.Add(ForceRateOfChangeQuantity.Instance.GetUnitChoice(ForceRateOfChangeQuantity.UnitChoicesEnum.KilopoundForcePerMinute));
            this.UnitChoices.Add(ForceRateOfChangeQuantity.Instance.GetUnitChoice(ForceRateOfChangeQuantity.UnitChoicesEnum.PoundForcePerMinute));

            this.UnitChoices.Add(ForceRateOfChangeQuantity.Instance.GetUnitChoice(ForceRateOfChangeQuantity.UnitChoicesEnum.NewtonPerHour));
            this.UnitChoices.Add(ForceRateOfChangeQuantity.Instance.GetUnitChoice(ForceRateOfChangeQuantity.UnitChoicesEnum.DecanewtonPerHour));
            this.UnitChoices.Add(ForceRateOfChangeQuantity.Instance.GetUnitChoice(ForceRateOfChangeQuantity.UnitChoicesEnum.KilonewtonPerHour));
            this.UnitChoices.Add(ForceRateOfChangeQuantity.Instance.GetUnitChoice(ForceRateOfChangeQuantity.UnitChoicesEnum.KilodecanewtonPerHour));
            this.UnitChoices.Add(ForceRateOfChangeQuantity.Instance.GetUnitChoice(ForceRateOfChangeQuantity.UnitChoicesEnum.KilogramForcePerHour));
            this.UnitChoices.Add(ForceRateOfChangeQuantity.Instance.GetUnitChoice(ForceRateOfChangeQuantity.UnitChoicesEnum.KilopoundForcePerHour));
            this.UnitChoices.Add(ForceRateOfChangeQuantity.Instance.GetUnitChoice(ForceRateOfChangeQuantity.UnitChoicesEnum.PoundForcePerHour));
            SemanticExample = GetSemanticExample();
        }
    }
}
