using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class MomentOfInertiaDrillingQuantity : MomentOfInertiaQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-11;
        private static MomentOfInertiaDrillingQuantity instance_ = null;

        public static new MomentOfInertiaDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MomentOfInertiaDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public MomentOfInertiaDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "moment of inertia (drilling)" };
            ID = new Guid("5b4e4820-9b88-43f9-9856-155846afee0e");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of the moment of inertia in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + ForceQuantity.Instance.GetUnitChoice(ForceQuantity.UnitChoicesEnum.Newton).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(MomentOfInertiaQuantity.Instance.GetUnitChoice(MomentOfInertiaQuantity.UnitChoicesEnum.GramCentimetreSquared));
            this.UnitChoices.Add(MomentOfInertiaQuantity.Instance.GetUnitChoice(MomentOfInertiaQuantity.UnitChoicesEnum.KilogramMetreSquared));
            this.UnitChoices.Add(MomentOfInertiaQuantity.Instance.GetUnitChoice(MomentOfInertiaQuantity.UnitChoicesEnum.PoundFootSquared));
            this.UnitChoices.Add(MomentOfInertiaQuantity.Instance.GetUnitChoice(MomentOfInertiaQuantity.UnitChoicesEnum.PoundInchSquared));
            SemanticExample = GetSemanticExample();
        }

    }
}
