using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using FrenetFrame.physics;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace FrenetFrame.models
{
    public class BezierCubeVisual3D : IBezierCubeVisual3D
    {
        private Point3D[, ,] controlPoints;
        private int N = 4;
        private double radius = 2;
        private Random rand;
        private int iteration = 0;
        private Vector3D[, ,] Xp;
        private Vector3D[, ,] X;
        private Vector3D[, ,] Xn;
        private Vector3D[, ,] V0;
        private Vector3D[, ,] acc;
        private Point3D[, ,] controlPointsOrigin;

        private ISpring spring;
        public void Initialize()
        {
            rand = new Random();
            spring = new Spring();
            spring.Initialize();
            controlPoints = new Point3D[N, N, N];
            controlPointsOrigin = new Point3D[N, N, N];
            Xp = new Vector3D[N, N, N];
            X = new Vector3D[N, N, N];
            Xn = new Vector3D[N, N, N];
            V0 = new Vector3D[N, N, N];
            acc = new Vector3D[N, N, N];
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



        }

        public IList<Point3D> GetControlPoints()
        {
            IList<Point3D> points = new List<Point3D>();
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    for (int k = 0; k < N; k++)
                    {
                        points.Add(controlPoints[i, j, k]);
                    }
                }
            }
            return points;
        }


        public IList<Point3D> GetControlLines()
        {
            IList<Point3D> lines = new List<Point3D>();

            //X
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    for (int k = 0; k < N - 1; k++)
                    {
                        var firstPoint = controlPoints[i, j, k];
                        var secondPoint = controlPoints[i, j, k + 1];
                        lines.Add(firstPoint);
                        lines.Add(secondPoint);
                    }
                }
            }

            //Y
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N - 1; j++)
                {
                    for (int k = 0; k < N; k++)
                    {
                        var firstPoint = controlPoints[i, j, k];
                        var secondPoint = controlPoints[i, j + 1, k];
                        lines.Add(firstPoint);
                        lines.Add(secondPoint);
                    }
                }
            }

            //Z
            for (int i = 0; i < N - 1; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    for (int k = 0; k < N; k++)
                    {
                        var firstPoint = controlPoints[i, j, k];
                        var secondPoint = controlPoints[i + 1, j, k];
                        lines.Add(firstPoint);
                        lines.Add(secondPoint);
                    }
                }
            }
            return lines;
        }

        public void Update()
        {
            if (iteration == 0)
            {
                //int numberOfChanged = 20;
                //for (int i = 0; i < numberOfChanged; i++)
                //{
                //    int x, y, z;
                //    x = rand.Next(N);
                //    y = rand.Next(N);
                //    z = rand.Next(N);
                //    double val = 10 * (rand.NextDouble() - 0.5);
                //    V0[x, y, z].X += val;
                //    V0[x, y, z].Y += val;
                //    V0[x, y, z].Z += val;
                //}
                //V0[0, 0, 0].X -= 12;
                //X[0, 1, 1].X -= 3;

                for (int i = 0; i < N; i++)
                {
                    for (int j = 0; j < N; j++)
                    {
                        X[i, j, 0].X -= 3;
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
            else
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
                            Vector3D[,,] forces = new Vector3D[N,N,N];
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
                            V0[i, j, k] = spring.GetNextVelocity(force, V0[i, j, k]);
                            Xn[i, j, k] = spring.GetSecondPosition(x, V0[i, j, k]);
                        }

                    }
                }

            }
            Xp = X;
            X = Xn;
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
            iteration++;
        }
    }
}
