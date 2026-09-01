using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    /// <summary>
    /// A power rate of change used for drilling equipment and power systems.
    /// </summary>
    public partial class PowerRateOfChangeDrillingQuantity : PowerRateOfChangeQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1.0;

        private static PowerRateOfChangeDrillingQuantity instance_ = null;
        public static new PowerRateOfChangeDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new PowerRateOfChangeDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public PowerRateOfChangeDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "power rate of change (drilling)", "power rate", "PowerRate" };
            ID = new Guid("9ea420b7-11b2-48bc-8a9e-a37fe84d4596");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of power rate of change in the drilling context is typically: " + MeaningfulPrecisionInSI + " W/s." + Environment.NewLine;
            Reset();
            UnitChoices.Add(PowerRateOfChangeQuantity.Instance.GetUnitChoice(PowerRateOfChangeQuantity.UnitChoicesEnum.WattPerSecond));
            UnitChoices.Add(PowerRateOfChangeQuantity.Instance.GetUnitChoice(PowerRateOfChangeQuantity.UnitChoicesEnum.KilowattPerSecond));
            UnitChoices.Add(PowerRateOfChangeQuantity.Instance.GetUnitChoice(PowerRateOfChangeQuantity.UnitChoicesEnum.MegawattPerSecond));
            SemanticExample = GetSemanticExample();
        }
    }
}
