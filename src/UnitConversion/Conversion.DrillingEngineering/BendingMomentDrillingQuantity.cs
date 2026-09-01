using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class BendingMomentDrillingQuantity : BendingMomentQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.1;
        private static BendingMomentDrillingQuantity instance_ = null;

        public static new BendingMomentDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new BendingMomentDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public BendingMomentDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "bending moment (drilling)", "BOB" };
            ID = new Guid("4f3baf27-eb18-480a-a0d8-851b5e4692fb");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of bending moment in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + BendingMomentQuantity.Instance.GetUnitChoice(BendingMomentQuantity.UnitChoicesEnum.NewtonMetre).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(BendingMomentQuantity.Instance.GetUnitChoice(BendingMomentQuantity.UnitChoicesEnum.NewtonMetre));
            this.UnitChoices.Add(BendingMomentQuantity.Instance.GetUnitChoice(BendingMomentQuantity.UnitChoicesEnum.FootPound));
            this.UnitChoices.Add(BendingMomentQuantity.Instance.GetUnitChoice(BendingMomentQuantity.UnitChoicesEnum.KilofootPound));
            this.UnitChoices.Add(BendingMomentQuantity.Instance.GetUnitChoice(BendingMomentQuantity.UnitChoicesEnum.DecanewtonMetre));
            this.UnitChoices.Add(BendingMomentQuantity.Instance.GetUnitChoice(BendingMomentQuantity.UnitChoicesEnum.KilogramForceMetre));
            this.UnitChoices.Add(BendingMomentQuantity.Instance.GetUnitChoice(BendingMomentQuantity.UnitChoicesEnum.KilonewtonMetre));
            SemanticExample = GetSemanticExample();
        }
    }
}
