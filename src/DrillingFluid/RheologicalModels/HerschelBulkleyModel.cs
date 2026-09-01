using System;
using System.Collections.Generic;
using NORCE.Drilling.DrillingFluid.ModelShared;
using NORCE.Drilling.DrillingFluid.ParticleSwarmOptimization;


namespace NORCE.Drilling.DrillingFluid.RheologicalModel
{
    public class HerschelBulkleyModel : RheologicalModel
    {
        public ModelConstraints Constraints = new ModelConstraints()
        {
            Dim = 3,
            minValues = new double[3] { -1.0, -1.0, -1.0}, // {min ViscosityInfty, min ConsistencyIndex, min YieldStress}
            maxValues = new double[3] { 1.0, 1.0, 1.0 } // {max ViscosityInfty, max ConsistencyIndex, max YieldStress}
        };
        private readonly int DIM = 3;    

        public double ShearStress(double gamma)
        {
            return this.YieldStress + this.ViscosityInfty * Math.Pow(gamma, ConsistencyIndex);
        }     

        public override double ObjectiveFunction(double[] par, DataPair[] xy)
        {                
            //par[0] = ViscosityInfty
            //par[1] = ConsistencyIndex
            //par[2] = YieldStress
            double residue = 0.0;
            for (int i = 0; i < xy.Length; i++)
            {
                residue += Math.Pow((par[0] * Math.Pow(xy[i].X, par[1]) + par[2] - xy[i].Y), 2);
            }
            return residue;
        }
        public HerschelBulkleyModel(){}
        public HerschelBulkleyModel(DataPair[] xy, ParticleSwarmSolverOptions options)
        {                         
            double[] var = ParticleSwarmMethod.Main(options, this.Constraints, xy, ObjectiveFunction);
            this.ViscosityInfty = var[0];
            this.ConsistencyIndex = var[1];    
            this.YieldStress = var[2];            
        }//constructor
        public HerschelBulkleyModel(List<RheometerMeasurement> rheoMeasurements)
        {
            int Nmeasurements = rheoMeasurements.Count;
            DataPair[] xy = new DataPair[Nmeasurements];
            for (int i = 0; i < Nmeasurements; i++)
            {
                xy[i] = new DataPair
                {
                    X = rheoMeasurements[i].IsoNewtonianShearRate,
                    Y = rheoMeasurements[i].IsoNewtonianShearStress
                };
            }
            double[] var = ParticleSwarmMethod.Main(DIM, xy, ObjectiveFunction);
            this.ViscosityInfty = var[0];
            this.ConsistencyIndex = var[1];
            this.YieldStress = var[2];
        }//constructor
    }//class
}//namespace
