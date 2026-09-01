using System;
using System.Collections.Generic;
using NORCE.Drilling.DrillingFluid.ModelShared;
using NORCE.Drilling.DrillingFluid.ParticleSwarmOptimization;

namespace NORCE.Drilling.DrillingFluid.RheologicalModel
{
    public class PowerLawModel : RheologicalModel
    {
        public ModelConstraints Constraints = new ModelConstraints()
        {
            Dim = 2,
            minValues = new double[2] { -1.0, - 1.0 }, // {min ViscosityInfty, min ConsistencyIndex}
            maxValues = new double[2] { 1.0, 1.0 } // {max ViscosityInfty, max ConsistencyIndex}
        };


        private readonly int DIM = 2;

        public double ShearStress(double gamma)
        {
            return this.ViscosityInfty * Math.Pow(gamma, this.ConsistencyIndex);
        }
        public override double ObjectiveFunction(double[] par, DataPair[] xy)
        {                
            double residue = 0.0;
            for (int i = 0; i < xy.Length; i++)
            {
                //par[0] = ViscosityInfty
                //par[1] = ConsistencyIndex
                residue += Math.Pow((par[0] * Math.Pow(xy[i].X, par[1]) - xy[i].Y), 2);
            }
            return residue;
        }
        public PowerLawModel(double[] parameters)
        {
            this.ViscosityInfty = parameters[0];   
            this.ConsistencyIndex = parameters[1];  
        }      
        public PowerLawModel(List<RheometerMeasurement> rheoMeasurements)
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
        }
        public PowerLawModel(DataPair[] xy, ParticleSwarmSolverOptions options)
        {
            double[] var = ParticleSwarmMethod.Main(options, this.Constraints, xy, ObjectiveFunction);
            this.ViscosityInfty = var[0];
            this.ConsistencyIndex = var[1];
        }//constructor
    }//class
}//namespace
