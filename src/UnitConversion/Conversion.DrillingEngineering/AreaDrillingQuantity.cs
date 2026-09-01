using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class AreaDrillingQuantity : AreaQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static AreaDrillingQuantity instance_ = null;

        public static new AreaDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new AreaDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public AreaDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "area (drilling)" };
            ID = new Guid("21fc0373-6eda-460b-bacb-070abf2f3add");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of area in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + AreaQuantity.Instance.GetUnitChoice(AreaQuantity.UnitChoicesEnum.SquareMetre).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(AreaQuantity.Instance.GetUnitChoice(AreaQuantity.UnitChoicesEnum.SquareMetre));
            this.UnitChoices.Add(AreaQuantity.Instance.GetUnitChoice(AreaQuantity.UnitChoicesEnum.SquareFoot));
            this.UnitChoices.Add(AreaQuantity.Instance.GetUnitChoice(AreaQuantity.UnitChoicesEnum.SquareDecimetre));
            this.UnitChoices.Add(AreaQuantity.Instance.GetUnitChoice(AreaQuantity.UnitChoicesEnum.SquareYard));
            this.UnitChoices.Add(AreaQuantity.Instance.GetUnitChoice(AreaQuantity.UnitChoicesEnum.SquareCentimetre));
            this.UnitChoices.Add(AreaQuantity.Instance.GetUnitChoice(AreaQuantity.UnitChoicesEnum.SquareInch));
            SemanticExample = GetSemanticExample();
        }
    }
}
