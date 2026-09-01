using NORCE.Drilling.DrillingFluid.ModelShared;

namespace RheologicalModelLib
{
    public class QuemadaModel
    {

        public double ViscosityInfty; 
        public double CharacteristicShearRate; 
        public double ConsistencyIndex; 
        public double Xi;

        public QuemadaModel(GenericRheologicalModel rheoModel)
        {
            this.ViscosityInfty = (double)rheoModel.QuemadaViscosityInfty!;
            this.CharacteristicShearRate = (double)rheoModel.QuemadaCharacteristicShearRate!;
            this.ConsistencyIndex = (double)rheoModel.QuemadaConsistencyIndex!;
            this.Xi = (double)rheoModel.QuemadaXi!;             
        }
        public double ShearStress(double gamma)
        {
            double admGammaN = Math.Pow(gamma / this.CharacteristicShearRate, this.ConsistencyIndex);
            return this.ViscosityInfty * Math.Pow((1.0 + admGammaN) / (this.Xi + admGammaN), 2);
        }
    }//class
}//namespace
