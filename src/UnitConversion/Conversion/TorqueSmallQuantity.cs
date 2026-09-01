using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion
{
    public partial class TorqueSmallQuantity :TorqueQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.00001;

        private static TorqueSmallQuantity instance_ = null;

        public static new TorqueSmallQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new TorqueSmallQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public TorqueSmallQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "torque (small)" };
            ID = new Guid("adf7b170-8d24-4c9f-93e1-40179f361d8c");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += @"The meaningful precision of small torque is typically: " + MeaningfulPrecisionInSI.ToString() + " " + TorqueQuantity.Instance.GetUnitChoice(TorqueQuantity.UnitChoicesEnum.NewtonMetre).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(TorqueQuantity.Instance.GetUnitChoice(TorqueQuantity.UnitChoicesEnum.NewtonMetre));
            this.UnitChoices.Add(TorqueQuantity.Instance.GetUnitChoice(TorqueQuantity.UnitChoicesEnum.FootPound));
            this.UnitChoices.Add(TorqueQuantity.Instance.GetUnitChoice(TorqueQuantity.UnitChoicesEnum.NewtonDecimetre));
            this.UnitChoices.Add(TorqueQuantity.Instance.GetUnitChoice(TorqueQuantity.UnitChoicesEnum.NewtonCentimetre));
            this.UnitChoices.Add(TorqueQuantity.Instance.GetUnitChoice(TorqueQuantity.UnitChoicesEnum.NewtonMillimetre));
            this.UnitChoices.Add(TorqueQuantity.Instance.GetUnitChoice(TorqueQuantity.UnitChoicesEnum.InchPound));
            SemanticExample = GetSemanticExample();
        }
    }
}
