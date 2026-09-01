using NORCE.Drilling.DrillingFluid.ModelShared;

namespace RheologicalModelLib
{
    public class PowerLawModel 
    {
        public double ViscosityInfty;
        public double ConsistencyIndex;

        public PowerLawModel(GenericRheologicalModel rheoModel)
        {
            this.ViscosityInfty = (double)rheoModel.PowerLawViscosityInfty!;
            this.ConsistencyIndex = (double)rheoModel.PowerLawConsistencyIndex!;        
        }

        public double ShearStress(double gamma)
        {
            return this.ViscosityInfty * Math.Pow(gamma, this.ConsistencyIndex);
        }
    }//class
}//namespace
