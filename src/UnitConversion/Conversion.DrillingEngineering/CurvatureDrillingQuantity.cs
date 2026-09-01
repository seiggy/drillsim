using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class CurvatureDrillingQuantity : CurvatureQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 5.817764173314432E-06;

        private static CurvatureDrillingQuantity instance_ = null;

        public static new CurvatureDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new CurvatureDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public CurvatureDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "curvature (drilling)", "dogleg severity", "DLS" };
            ID = new Guid("0e41ce3a-a0e4-44a3-bf6e-6c2a70f4a28b");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of curvature in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.RadianPerMetre).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.RadianPerMetre));
            this.UnitChoices.Add(CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.DegreePer10m));
            this.UnitChoices.Add(CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.DegreePer30m));
            this.UnitChoices.Add(CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.DegreePer30ft));
            this.UnitChoices.Add(CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.DegreePer100ft));
            SemanticExample = GetSemanticExample();
        }
    }
}
