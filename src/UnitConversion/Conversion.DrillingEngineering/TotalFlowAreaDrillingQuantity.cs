using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class TotalFlowAreaDrillingQuantity : AreaQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static TotalFlowAreaDrillingQuantity instance_ = null;

        public static new TotalFlowAreaDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new TotalFlowAreaDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public TotalFlowAreaDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "total flow area (drilling)" };
            ID = new Guid("fc067c6d-1f34-4af9-a6f8-eb2bbf7acca5");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of total flow area in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + AreaQuantity.Instance.GetUnitChoice(AreaQuantity.UnitChoicesEnum.SquareMetre).UnitLabel + Environment.NewLine;
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
