using System;
using System.Collections.Generic;
using NORCE.Drilling.DrillingFluid.ModelShared;
using NORCE.Drilling.DrillingFluid.ParticleSwarmOptimization;


namespace NORCE.Drilling.DrillingFluid.RheologicalModel
{
    public class RobertsonStiffModel : RheologicalModel
    {
        public ModelConstraints Constraints = new ModelConstraints()
        {
            Dim = 3,
            minValues = new double[3] { -100.0, -100.0, -100.0}, // {min YieldStress, min CharacteristicShearRate, min ConsistencyIndex}
            maxValues = new double[3] { 100.0, 100.0, 100.0 } // {max YieldStress, max CharacteristicShearRate, max ConsistencyIndex}
        };

        private readonly int DIM = 3;        
        public double ShearStress(double gamma)
        {
            return this.YieldStress * Math.Pow(1.0 + gamma / this.CharacteristicShearRate, this.ConsistencyIndex);
        }
     
        public override double ObjectiveFunction(double[] par, DataPair[] xy)
        {                
            //par[0] = YieldStress
            //par[1] = CharacteristicShearRate
            //par[2] = ConsistencyIndex
            double residue = 0.0;
            for (int i = 0; i < xy.Length; i++)
            {              
                double calcTau = par[0] * Math.Pow(1.0 + xy[i].X/par[1], par[2]);    
                residue += Math.Pow(calcTau - xy[i].Y, 2);      
            }
            return residue;
        }
        public RobertsonStiffModel(){}
        public RobertsonStiffModel(DataPair[] xy, ParticleSwarmSolverOptions options)
        {                         
            double[] var = ParticleSwarmMethod.Main(options, this.Constraints, xy, ObjectiveFunction);
            this.YieldStress = var[0];
            this.CharacteristicShearRate = var[1];
            this.ConsistencyIndex = var[2];     
        }//constructor
        public RobertsonStiffModel(List<RheometerMeasurement> rheoMeasurements)
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
            this.YieldStress = var[0];
            this.CharacteristicShearRate = var[1];
            this.ConsistencyIndex = var[2];
        }//constructor
    }//class
}//namespace
