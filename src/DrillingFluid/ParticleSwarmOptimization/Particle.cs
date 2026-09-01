namespace NORCE.Drilling.DrillingFluid.ParticleSwarmOptimization
{
    public class Particle
    {
        public double[] position;
        public double fitness;
        public double[] velocity;
        public double[] bestPosition;
        public double bestFitness;
        public Particle(double[] position, double fitness,
        double[] velocity, double[] bestPosition, double bestFitness)
        {
          this.position = new double[position.Length];
          position.CopyTo(this.position, 0);
          this.fitness = fitness;
          this.velocity = new double[velocity.Length];
          velocity.CopyTo(this.velocity, 0);
          this.bestPosition = new double[bestPosition.Length];
          bestPosition.CopyTo(this.bestPosition, 0);
          this.bestFitness = bestFitness;
        }
    } // class Particle
}