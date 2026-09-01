using System;
///
/// Code modified from https://learn.microsoft.com/en-us/archive/msdn-magazine/2011/august/artificial-intelligence-particle-swarm-optimization
///
/// 
namespace NORCE.Drilling.DrillingFluid.ParticleSwarmOptimization
{

  public class ParticleSwarmMethod
  {
    static Random ran = null;
    static public double[] Main(ParticleSwarmSolverOptions Options, ModelConstraints Constraints, DataPair[] xy,  Func<double[], DataPair[], double> ObjectiveFunction)
    {
      {
        //Set number of particles and iteratioms
        ran = new Random(0);
        int iteration = 0;
        //Can be moved to the outside of this function
        double[] minX = Constraints.minValues;
        double[] maxX = Constraints.maxValues;
        int Dim = Constraints.Dim;
        Particle[] swarm = new Particle[Options.NumberOfParticles];
        double[] bestGlobalPosition = new double[Dim]; // best solution found by any particle in the swarm. implicit initialization to all 0.0
        double bestGlobalFitness = double.MaxValue; // smaller values better

        double[] minV = new double[Dim];//-1.0 * maxX;
        double[] maxV = new double[Dim];//maxX;
        for (int j = 0; j < Dim; ++j)
        {
          minV[j] = -1.0 * maxX[j];
          maxV[j] = maxX[j];
        }

        #region Initialization of the method
        //Initializing swarm with random positions/solutions
        for (int i = 0; i < swarm.Length; ++i) // initialize each Particle in the swarm
        {
          double[] randomPosition = new double[Dim];
          for (int j = 0; j < randomPosition.Length; ++j)
          {
            double lo = minX[j];
            double hi = maxX[j];
            randomPosition[j] = (hi - lo) * ran.NextDouble() + lo; // 
          }
          //double fitness = SphereFunction(randomPosition); // smaller values are better
          //double fitness = GP(randomPosition); // smaller values are better
          double fitness = ObjectiveFunction(randomPosition, xy);
          double[] randomVelocity = new double[Dim];
          for (int j = 0; j < randomVelocity.Length; ++j)
          {
            double lo = -1.0 * Math.Abs(maxX[j] - minX[j]);
            double hi = Math.Abs(maxX[j] - minX[j]);
            randomVelocity[j] = (hi - lo) * ran.NextDouble() + lo;
          }
          swarm[i] = new Particle(randomPosition, fitness, randomVelocity, randomPosition, fitness);

          // does current Particle have global best position/solution?
          if (swarm[i].fitness < bestGlobalFitness)
          {
            bestGlobalFitness = swarm[i].fitness;
            swarm[i].position.CopyTo(bestGlobalPosition, 0);
          }
        } // initialization
        #endregion

        double w = 0.729; // inertia weight. see http://ieeexplore.ieee.org/stamp/stamp.jsp?arnumber=00870279
        double c1 = 1.49445; // cognitive/local weight
        double c2 = 1.49445; // social/global weight
        double r1, r2; // cognitive and social randomizations
        #region Main loop
        while (iteration < Options.NumberOfIterations)
        {
          ++iteration;
          double[] newVelocity = new double[Dim];
          double[] newPosition = new double[Dim];
          double newFitness;

          for (int i = 0; i < swarm.Length; ++i) // each Particle
          {
            Particle currP = swarm[i];

            for (int j = 0; j < currP.velocity.Length; ++j) // each x value of the velocity
            {
              r1 = ran.NextDouble();
              r2 = ran.NextDouble();

              newVelocity[j] = (w * currP.velocity[j]) +
                (c1 * r1 * (currP.bestPosition[j] - currP.position[j])) +
                (c2 * r2 * (bestGlobalPosition[j] - currP.position[j]));

              if (newVelocity[j] < minV[j])
                newVelocity[j] = minV[j];
              else if (newVelocity[j] > maxV[j])
                newVelocity[j] = maxV[j];
            }

            newVelocity.CopyTo(currP.velocity, 0);

            for (int j = 0; j < currP.position.Length; ++j)
            {
              newPosition[j] = currP.position[j] + newVelocity[j];
              if (newPosition[j] < minX[j])
                newPosition[j] = minX[j];
              else if (newPosition[j] > maxX[j])
                newPosition[j] = maxX[j];
            }

            newPosition.CopyTo(currP.position, 0);
            newFitness = ObjectiveFunction(newPosition, xy);
            currP.fitness = newFitness;

            if (newFitness < currP.bestFitness)
            {
              newPosition.CopyTo(currP.bestPosition, 0);
              currP.bestFitness = newFitness;
            }

            if (newFitness < bestGlobalFitness)
            {
              newPosition.CopyTo(bestGlobalPosition, 0);
              bestGlobalFitness = newFitness;
            }

          } // each Particle
        } // while
        #endregion       
        return bestGlobalPosition;
      }
    } // Main()
    static public double[] Main(int Dim, DataPair[] xy, Func<double[], DataPair[], double> ObjectiveFunction)
    {
      {
        //Set number of particles and iteratioms
        ran = new Random(0);
        int numberOfParticles = 10;
        int numberIterations = 10000;
        int iteration = 0;
        //Can be moved to the outside of this function
        double minX = -200;
        double maxX = 200;

        Particle[] swarm = new Particle[numberOfParticles];
        double[] bestGlobalPosition = new double[Dim]; // best solution found by any particle in the swarm. implicit initialization to all 0.0
        double bestGlobalFitness = double.MaxValue; // smaller values better

        double minV = -1.0 * maxX;
        double maxV = maxX;
        #region Initialization of the method
        //Initializing swarm with random positions/solutions
        for (int i = 0; i < swarm.Length; ++i) // initialize each Particle in the swarm
        {
          double[] randomPosition = new double[Dim];
          for (int j = 0; j < randomPosition.Length; ++j)
          {
            double lo = minX;
            double hi = maxX;
            randomPosition[j] = (hi - lo) * ran.NextDouble() + lo; // 
          }
          //double fitness = SphereFunction(randomPosition); // smaller values are better
          //double fitness = GP(randomPosition); // smaller values are better
          double fitness = ObjectiveFunction(randomPosition, xy);
          double[] randomVelocity = new double[Dim];
          for (int j = 0; j < randomVelocity.Length; ++j)
          {
            double lo = -1.0 * Math.Abs(maxX - minX);
            double hi = Math.Abs(maxX - minX);
            randomVelocity[j] = (hi - lo) * ran.NextDouble() + lo;
          }
          swarm[i] = new Particle(randomPosition, fitness, randomVelocity, randomPosition, fitness);

          // does current Particle have global best position/solution?
          if (swarm[i].fitness < bestGlobalFitness)
          {
            bestGlobalFitness = swarm[i].fitness;
            swarm[i].position.CopyTo(bestGlobalPosition, 0);
          }
        } // initialization
        #endregion

        double w = 0.729; // inertia weight. see http://ieeexplore.ieee.org/stamp/stamp.jsp?arnumber=00870279
        double c1 = 1.49445; // cognitive/local weight
        double c2 = 1.49445; // social/global weight
        double r1, r2; // cognitive and social randomizations
        #region Main loop
        while (iteration < numberIterations)
        {
          ++iteration;
          double[] newVelocity = new double[Dim];
          double[] newPosition = new double[Dim];
          double newFitness;

          for (int i = 0; i < swarm.Length; ++i) // each Particle
          {
            Particle currP = swarm[i];

            for (int j = 0; j < currP.velocity.Length; ++j) // each x value of the velocity
            {
              r1 = ran.NextDouble();
              r2 = ran.NextDouble();

              newVelocity[j] = (w * currP.velocity[j]) +
                (c1 * r1 * (currP.bestPosition[j] - currP.position[j])) +
                (c2 * r2 * (bestGlobalPosition[j] - currP.position[j]));

              if (newVelocity[j] < minV)
                newVelocity[j] = minV;
              else if (newVelocity[j] > maxV)
                newVelocity[j] = maxV;
            }

            newVelocity.CopyTo(currP.velocity, 0);

            for (int j = 0; j < currP.position.Length; ++j)
            {
              newPosition[j] = currP.position[j] + newVelocity[j];
              if (newPosition[j] < minX)
                newPosition[j] = minX;
              else if (newPosition[j] > maxX)
                newPosition[j] = maxX;
            }

            newPosition.CopyTo(currP.position, 0);
            newFitness = ObjectiveFunction(newPosition, xy);
            currP.fitness = newFitness;

            if (newFitness < currP.bestFitness)
            {
              newPosition.CopyTo(currP.bestPosition, 0);
              currP.bestFitness = newFitness;
            }

            if (newFitness < bestGlobalFitness)
            {
              newPosition.CopyTo(bestGlobalPosition, 0);
              bestGlobalFitness = newFitness;
            }

          } // each Particle
        } // while
        #endregion       
        return bestGlobalPosition;
      }
    } // Main()
  } // class Program
} // ns
