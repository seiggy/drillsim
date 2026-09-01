using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class PowerDrillingQuantity : PowerQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.1;
        private static PowerDrillingQuantity instance_ = null;

        public static new PowerDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new PowerDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public PowerDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "power (drilling)" };
            ID = new Guid("d7f0d3a8-2d15-4ae9-897a-5d1ef7feef8a");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of power in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + PowerQuantity.Instance.GetUnitChoice(PowerQuantity.UnitChoicesEnum.Watt).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(PowerQuantity.Instance.GetUnitChoice(PowerQuantity.UnitChoicesEnum.Watt));
            this.UnitChoices.Add(PowerQuantity.Instance.GetUnitChoice(PowerQuantity.UnitChoicesEnum.Kilowatt));
            this.UnitChoices.Add(PowerQuantity.Instance.GetUnitChoice(PowerQuantity.UnitChoicesEnum.Megawatt));
            SemanticExample = GetSemanticExample();
        }
    }
}
