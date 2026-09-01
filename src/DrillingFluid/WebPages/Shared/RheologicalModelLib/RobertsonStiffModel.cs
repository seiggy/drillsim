using NORCE.Drilling.DrillingFluid.ModelShared;
namespace RheologicalModelLib
{
    public class RobertsonStiffModel
    {


        public double YieldStress;
        public double CharacteristicShearRate;
        public double ConsistencyIndex;
        public RobertsonStiffModel(GenericRheologicalModel rheoModel)
        {
            this.YieldStress = (double)rheoModel.RobertsonStiffYieldStress!;
            this.CharacteristicShearRate = (double)rheoModel.RobertsonStiffCharacteristicShearRate!;
            this.ConsistencyIndex = (double)rheoModel.RobertsonStiffConsistencyIndex!;
        }
        public double ShearStress(double gamma)
        {
            return this.YieldStress * Math.Pow(1.0 + gamma / this.CharacteristicShearRate, this.ConsistencyIndex);
        }

    }//class
}//namespace
