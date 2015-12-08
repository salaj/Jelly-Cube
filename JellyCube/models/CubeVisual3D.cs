using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using FrenetFrame.physics;
using JellyCube.models;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace FrenetFrame.models
{
    public class BezierCubeVisual3D : BaseVisual3D, IBezierCubeVisual3D
    {
        private double radius;
        private Random rand;
        private int iteration = 0;
        private Vector3D[, ,] X;
        private Vector3D[, ,] Xn;
        private Vector3D[, ,] V0;
        private Vector3D[, ,] cornerPoints;
        private Vector3D[, ,] jointForces;
        private Point3D[, ,] controlPointsOrigin;

        private ISpring spring;
        public override void Initialize(double cubeSize)
        {
            base.N = 4;
            base.Initialize();
            radius = cubeSize/(double)(N - 1);
            rand = new Random();
            spring = new Spring();
            spring.Initialize();
            controlPointsOrigin = new Point3D[N, N, N];
            X = new Vector3D[N, N, N];
            Xn = new Vector3D[N, N, N];
            V0 = new Vector3D[N, N, N];
            cornerPoints = new Vector3D[2, 2, 2];
            double cubeLength = (N - 1) * radius;
            double halfCubeLength = cubeLength / 2;
            double x, y, z;
            for (int i = 0; i < N; i++)
            {
                z = i * radius - halfCubeLength;
                for (int j = 0; j < N; j++)
                {
                    y = j * radius - halfCubeLength;
                    for (int k = 0; k < N; k++)
                    {
                        x = k * radius - halfCubeLength;
                        controlPoints[i, j, k] = new Point3D(x, y, z);
                        controlPointsOrigin[i, j, k] = new Point3D(x, y, z);
                        X[i, j, k] = new Vector3D(x, y, z);
                    }
                }
            }
            points.Points = GetControlPoints();
            lines.Points = GetControlLines();
            lines.Color = Color.FromArgb(255,0, 0, 255);
        }


        private void simulateFirstIteration()
        {
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    for (int k = 0; k < N; k++)
                    {
                        Vector3D X0 = X[i, j, k];
                        Vector3D v0 = V0[i, j, k];
                        Xn[i, j, k] = spring.GetSecondPosition(X0, v0);
                        Vector3D v1 = spring.GetCurrentVelocity(Xn[i, j, k], X0);
                        V0[i, j, k] = v1;
                    }
                }
            }
        }

        public Vector3D[, ,] GetCornerPoints()
        {
            cornerPoints[0, 0, 0] = X[0, 0, 0];
            cornerPoints[0, 0, 1] = X[0, 0, 3];
            cornerPoints[0, 1, 0] = X[0, 3, 0];
            cornerPoints[0, 1, 1] = X[0, 3, 3];

            cornerPoints[1, 0, 0] = X[3, 0, 0];
            cornerPoints[1, 0, 1] = X[3, 0, 3];
            cornerPoints[1, 1, 0] = X[3, 3, 0];
            cornerPoints[1, 1, 1] = X[3, 3, 3];

            return cornerPoints;
        }

        public void CalculateJointForces(Point3D[, ,] framePoints)
        {
            int dim = 2;
            jointForces = new Vector3D[dim, dim, dim];
            for (int i = 0; i < dim; i++)
            {
                for (int j = 0; j < dim; j++)
                {
                    for (int k = 0; k < dim; k++)
                    {
                        Vector3D P1;//kostka
                        Vector3D P2;//ramka
                        P1 = X[3 * i, 3 * j, 3 * k];
                        Point3D p = framePoints[i, j, k];
                        P2 = new Vector3D(p.X, p.Y, p.Z);
                        Vector3D V1 = V0[3 * i, 3 * j, 3 * k];
                        Vector3D I0 = new Vector3D(0, 0, 0); //długość spoczynkowa sprężynki połączonej z ramką ma być 0
                        Vector3D diff = P1 - P2;
                        if (Math.Sqrt(diff.X*diff.X + diff.Y*diff.Y + diff.Z*diff.Z) < 0.00001)
                            jointForces[i, j, k] = new  Vector3D(0, 0, 0);
                        else
                            jointForces[i,j,k] = spring.GetCurrentForce(P1, P2, I0, V1);
                    }
                }
            }
            int debug = 10;
        }

        private Vector3D applyFrameForce(int a, int b, int c)
        {
            if (a == 0 && b == 0 && c == 0)
                return jointForces[0, 0, 0];
            if (a == 0 && b == 0 && c == 3)
                return jointForces[0, 0, 1];
            if (a == 0 && b == 3 && c == 0)
                return jointForces[0, 1, 0];
            if (a == 0 && b == 3 && c == 3)
                return jointForces[0, 1, 1];

            if (a == 3 && b == 0 && c == 0)
                return jointForces[1, 0, 0];
            if (a == 3 && b == 0 && c == 3)
                return jointForces[1, 0, 1];
            if (a == 3 && b == 3 && c == 0)
                return jointForces[1, 1, 0];
            if (a == 3 && b == 3 && c == 3)
                return jointForces[1, 1, 1];

            return new Vector3D(0, 0, 0);
        }

        private void simulateNextIterations()
        {
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    for (int k = 0; k < N; k++)
                    {
                        Vector3D x = X[i, j, k];
                        Vector3D P1 = x;
                        Vector3D P2;
                        Vector3D[, ,] forces = new Vector3D[N, N, N];
                        Vector3D V1 = V0[i, j, k];
                        Vector3D force = new Vector3D(0, 0, 0);

                        for (int a = i - 1; a <= i + 1; a++)
                        {
                            for (int b = j - 1; b <= j + 1; b++)
                            {
                                for (int c = k - 1; c <= k + 1; c++)
                                {
                                    if (a == i && b == j && c == k)
                                        continue;
                                    if (a < 0 || b < 0 || c < 0 || a > N - 1 || b > N - 1 || c > N - 1)
                                        continue;
                                    P2 = X[a, b, c];
                                    Vector3D I0 = new Vector3D(
                                        Math.Abs(controlPointsOrigin[i, j, k].X - controlPointsOrigin[a, b, c].X),
                                        Math.Abs(controlPointsOrigin[i, j, k].Y - controlPointsOrigin[a, b, c].Y),
                                        Math.Abs(controlPointsOrigin[i, j, k].Z - controlPointsOrigin[a, b, c].Z)
                                        );
                                    forces[a, b, c] = spring.GetCurrentForce(P1, P2, I0, V1);
                                    force += forces[a, b, c];
                                }
                            }
                        }
                        force += applyFrameForce(i, j, k);
                        V0[i, j, k] = spring.GetNextVelocity(force, V0[i, j, k]);
                        Xn[i, j, k] = spring.GetSecondPosition(x, V0[i, j, k]);
                    }

                }
            }
        }

        private void copyActualPointsToVisualObject()
        {
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    for (int k = 0; k < N; k++)
                    {
                        controlPoints[i, j, k].X = X[i, j, k].X;
                        controlPoints[i, j, k].Y = X[i, j, k].Y;
                        controlPoints[i, j, k].Z = X[i, j, k].Z;
                    }
                }
            }
        }

        private void randomizeInitialPositions()
        {
            const int numberOfPositionsToChange = 5;
            for (int i = 0; i < numberOfPositionsToChange; i++)
            {
                int x, y, z;
                x = rand.Next(0, N);
                y = rand.Next(0, N);
                z = rand.Next(0, N);
                double factor = 5.0f;
                X[x, y, z].X += factor * (rand.NextDouble() - 0.5);
                X[x, y, z].Y += factor * (rand.NextDouble() - 0.5);
                X[x, y, z].Z += factor * (rand.NextDouble() - 0.5);
            }
        }

        private void randomizeInitialVelocities()
        {
            const int numberOfPositionsToChange = 5;
            for (int i = 0; i < numberOfPositionsToChange; i++)
            {
                int x, y, z;
                x = rand.Next(0, N);
                y = rand.Next(0, N);
                z = rand.Next(0, N);
                double factor = 5.0f;
                V0[x, y, z].X += factor * (rand.NextDouble() - 0.5);
                V0[x, y, z].Y += factor * (rand.NextDouble() - 0.5);
                V0[x, y, z].Z += factor * (rand.NextDouble() - 0.5);
            }
        }

        public void Update()
        {
            if (iteration == 0)
            {
                randomizeInitialPositions();
                randomizeInitialVelocities();
                simulateFirstIteration();
            }
            else
            {
                simulateNextIterations();
            }

            X = Xn;
            copyActualPointsToVisualObject();
            iteration++;
        }
    }
}
