using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class SurfacePoreDrillingQuantity : AreaQuantity
    {
        
        public override string TypicalSymbol { get; } = null;
        public override double? MeaningfulPrecisionInSI { get; } = 1E-13;
        private static SurfacePoreDrillingQuantity instance_ = null;

        public static new SurfacePoreDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new SurfacePoreDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public SurfacePoreDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "surface (pore) (drilling)" };
            ID = new Guid("c88cc254-b870-44a6-b896-5863472bdcd0");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of pore surface in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + AreaQuantity.Instance.GetUnitChoice(AreaQuantity.UnitChoicesEnum.SquareMetre).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(AreaQuantity.Instance.GetUnitChoice(AreaQuantity.UnitChoicesEnum.SquareMetre));
            this.UnitChoices.Add(AreaQuantity.Instance.GetUnitChoice(AreaQuantity.UnitChoicesEnum.SquareMillimetre));
            this.UnitChoices.Add(AreaQuantity.Instance.GetUnitChoice(AreaQuantity.UnitChoicesEnum.SquareMicrometre));
            this.UnitChoices.Add(AreaQuantity.Instance.GetUnitChoice(AreaQuantity.UnitChoicesEnum.SquareFoot));
            SemanticExample = GetSemanticExample();
        }
    }
}
