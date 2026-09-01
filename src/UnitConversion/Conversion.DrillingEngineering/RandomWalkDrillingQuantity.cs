using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class RandomWalkDrillingQuantity : RandomWalkQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.00001;

        private static RandomWalkDrillingQuantity instance_ = null;

        public static new RandomWalkDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new RandomWalkDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public RandomWalkDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "random walk (drilling)" };
            ID = new Guid("8817dc80-eb46-42d5-b85f-703fa8845f32");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of random walk in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + RandomWalkQuantity.Instance.GetUnitChoice(RandomWalkQuantity.UnitChoicesEnum.RadianPerSquareRootSecond).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(RandomWalkQuantity.Instance.GetUnitChoice(RandomWalkQuantity.UnitChoicesEnum.RadianPerSquareRootSecond));
            this.UnitChoices.Add(RandomWalkQuantity.Instance.GetUnitChoice(RandomWalkQuantity.UnitChoicesEnum.RadianPerSquareRootMinute));
            this.UnitChoices.Add(RandomWalkQuantity.Instance.GetUnitChoice(RandomWalkQuantity.UnitChoicesEnum.RadianPerSquareRootHour));
            this.UnitChoices.Add(RandomWalkQuantity.Instance.GetUnitChoice(RandomWalkQuantity.UnitChoicesEnum.RadianPerSquareRootDay));
            this.UnitChoices.Add(RandomWalkQuantity.Instance.GetUnitChoice(RandomWalkQuantity.UnitChoicesEnum.DegreePerSquareRootSecond));
            this.UnitChoices.Add(RandomWalkQuantity.Instance.GetUnitChoice(RandomWalkQuantity.UnitChoicesEnum.DegreePerSquareRootMinute));
            this.UnitChoices.Add(RandomWalkQuantity.Instance.GetUnitChoice(RandomWalkQuantity.UnitChoicesEnum.DegreePerSquareRootHour));
            this.UnitChoices.Add(RandomWalkQuantity.Instance.GetUnitChoice(RandomWalkQuantity.UnitChoicesEnum.DegreePerSquareRootDay));
            SemanticExample = GetSemanticExample();
        }
    }
}
