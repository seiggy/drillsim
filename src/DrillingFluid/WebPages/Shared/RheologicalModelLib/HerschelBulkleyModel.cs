using NORCE.Drilling.DrillingFluid.ModelShared;
namespace RheologicalModelLib
{
    public class HerschelBulkleyModel
    {
        public double YieldStress;
        public double ViscosityInfty;
        public double ConsistencyIndex;
        public HerschelBulkleyModel(GenericRheologicalModel rheoModel)
        {
            this.YieldStress = (double)rheoModel.HerschelBulkleyYieldStress!;
            this.ViscosityInfty = (double)rheoModel.HerschelBulkleyViscosityInfty!;
            this.ConsistencyIndex = (double)rheoModel.HerschelBulkleyConsistencyIndex!;            
        }
        public double ShearStress(double gamma)
        {
            return this.YieldStress + this.ViscosityInfty * Math.Pow(gamma, ConsistencyIndex);
        }     
    }//class
}//namespace
