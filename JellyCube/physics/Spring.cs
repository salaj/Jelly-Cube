using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace FrenetFrame.physics
{
    public class Spring : ISpring
    {
        /// <summary>
        /// Mass of the Spring
        /// </summary>
        public double Mass { get; set; }

        /// <summary>
        /// Współczynnik sprężystości
        /// </summary>
        public double Springer { get; set; }

        /// <summary>
        /// Współycznnik lepkości
        /// </summary>
        public double Viscosity { get; set; }

        /// <summary>
        /// Krok w czasie
        /// </summary>
        public double Delta { get; set; }

        public void Initialize()
        {
            Mass = 1;
            Springer = 0.3; //c  sprężystość
            Viscosity = 0.05; //k lepkość
            Delta = 0.1;
        }

        public double GetVectorLength(Vector3D a)
        {
            return Math.Sqrt(a.X * a.X + a.Y * a.Y + a.Z * a.Z);
        }

        public Vector3D NormalizeVector3D(Vector3D a)
        {
            double len = GetVectorLength(a);
            a.X /= len;
            a.Y /= len;
            a.Z /= len;
            return a;
        }

        public Vector3D GetSecondPosition(Vector3D X0, Vector3D V0)
        {
            return X0 + V0 * Delta;
        }

        public Vector3D GetCurrentVelocity(Vector3D X, Vector3D Xp)
        {
            return (X - Xp) / Delta;
        }

        public Vector3D GetNextVelocity(Vector3D f, Vector3D v)
        {
            return f / Mass * Delta + v;
        }

        private Vector3D GetElasticForce(Vector3D P1, Vector3D P2, Vector3D I0)
        {
            double c = Springer;
            double m = Mass;
            double k = Viscosity;
            Vector3D L = P1 - P2;
            double len = GetVectorLength(L);
            double Rlength = GetVectorLength(I0);
            double dX = len - Rlength;
            double FstructMag = -1 * c * dX;
            L = NormalizeVector3D(L);
            return L * FstructMag;
        }

        private Vector3D GetDampingForce(Vector3D P1, Vector3D P2, Vector3D V)
        {
            double k = Viscosity;
            Vector3D L = P2 - P1;
            double len = GetVectorLength(L);
            double vDotL = Vector3D.DotProduct(V, L);
            double FdampMag = (-1 * k * vDotL) / len;
            L = NormalizeVector3D(L);
            return L * FdampMag;
        }

        public Vector3D GetCurrentForce(Vector3D P1, Vector3D P2, Vector3D I0, Vector3D V)
        {
            return GetElasticForce(P1, P2, I0) + GetDampingForce(P1, P2, V);
        }
    }
}
