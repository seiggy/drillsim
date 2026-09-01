using NORCE.Drilling.DrillingFluid.ParticleSwarmOptimization;

namespace NORCE.Drilling.DrillingFluid.RheologicalModel
{
    public class RheologicalModel
    {

        public virtual double YieldStress { get; set; }
        public virtual double ViscosityInfty { get; set; }
        public virtual double CharacteristicShearRate { get; set; }
        public virtual double ConsistencyIndex { get; set; }        
        public virtual double Xi { get; set; }
            
        public virtual double ObjectiveFunction(double[] par, DataPair[] xy)
        {
            return 0.0;
        }  
    }
}
