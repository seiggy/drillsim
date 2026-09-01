using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class PorousMediumPermeabilityDrillingQuantity : PorousMediumPermeabilityQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.0000000001;

        private static PorousMediumPermeabilityDrillingQuantity instance_ = null;

        public static new PorousMediumPermeabilityDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new PorousMediumPermeabilityDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public PorousMediumPermeabilityDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "porous medium permeability (drilling)" };
            ID = new Guid("040ff93a-d03a-4d01-9c52-8e456647ccb9");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of permeability in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + PorousMediumPermeabilityQuantity.Instance.GetUnitChoice(PorousMediumPermeabilityQuantity.UnitChoicesEnum.SquareMetre).UnitLabel + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }

    }
}
