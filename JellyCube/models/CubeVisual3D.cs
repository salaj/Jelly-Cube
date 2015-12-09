using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        private IList<Point3D> sphereBezierParameters; 

        public ISpring spring { get; set; }
        public ISpring springFrame { get; set; }
        public double noise { get; set; }
        public ICollisionChecker CollisionChecker { get; set; }


        public override void Initialize(double cubeSize)
        {
            base.N = 4;
            base.Initialize();
            radius = cubeSize/(double)(N - 1);
            rand = new Random();
            spring = new Spring();
            springFrame = new Spring();
            spring.Initialize();
            springFrame.Initialize();
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
            CalculateSpherePoints();
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
                        CollisionChecker.TrimPoint(ref P2);
                        Vector3D V1 = V0[3 * i, 3 * j, 3 * k];
                        Vector3D I0 = new Vector3D(0, 0, 0); //długość spoczynkowa sprężynki połączonej z ramką ma być 0
                        Vector3D diff = P1 - P2;
                        if (Math.Sqrt(diff.X*diff.X + diff.Y*diff.Y + diff.Z*diff.Z) < 0.00001)
                            jointForces[i, j, k] = new  Vector3D(0, 0, 0);
                        else
                            jointForces[i,j,k] = springFrame.GetCurrentForce(P1, P2, I0, V1);
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
                        //V0[i, j, k] = spring.GetNextVelocity(force, V0[i, j, k]);
                        //double maxLen = 2;
                        //double len = Math.Sqrt(force.X * force.X + force.Y * force.Y + force.Z * force.Z);
                        //if (len > maxLen)
                        //{
                        //    force = force / len * maxLen;
                        //}
                        //if (i == 3 && j == 0 && k == 3)
                        //{
                        //    int de = 2;
                        //}
                        Vector3D v = spring.GetNextVelocity(force, V0[i, j, k]);
                        Vector3D xn = spring.GetSecondPosition(x, v);
                        bool isCollisionDetected = CollisionChecker.CheckCollision(xn, ref v);

                        //if (isCollisionDetected)
                        //    V0[i, j, k] = new Vector3D(0, 0, 0);
                        //else
                            V0[i, j, k] = v;
                        xn = spring.GetSecondPosition(x, V0[i, j, k]);
                        //Xn[i, j, k] = spring.GetSecondPosition(x, V0[i, j, k]);
                        //CollisionChecker.TrimPoint(ref xn);
                        Xn[i, j, k] = xn;
                    }
                }
            }
        }

        private void CalculateSpherePoints()
        {
            var sphereRadius = (N - 1) * radius / 2;
            var step = 0.01;
            sphereBezierParameters = new List<Point3D>();
            for (double u = 0; u <= 1; u += step)
            {
                var uBezier = CalculateBezierVector(u);
                for (double v = 0; v <= 1; v += step)
                {
                    var vBezier = CalculateBezierVector(v);
                    for (double w = 0; w <= 1; w += step)
                    {
                        var wBezier = CalculateBezierVector(w);
                        var sum = new Point3D();
                        for (int i = 0; i < N; i++)
                        {
                            for (int j = 0; j < N; j++)
                            {
                                for (int k = 0; k < N; k++)
                                {
                                    var factor = uBezier[i] * vBezier[j] * wBezier[k];
                                    var point = controlPoints[i, j, k];
                                    sum = new Point3D(sum.X + point.X * factor, sum.Y + point.Y * factor, sum.Z + point.Z * factor);
                                }
                            }
                        }
                        if (Math.Abs(Math.Sqrt(sum.X * sum.X + sum.Y * sum.Y + sum.Z * sum.Z) - sphereRadius) < 0.0005)
                        {
                            sphereBezierParameters.Add(new Point3D(u, v, w));
                        }
                    }
                }
            }

        }

        protected double[] CalculateBezierVector(double t)
        {
            return new double[4] { (1 - t) * (1 - t) * (1 - t), 3 * t * (1 - t) * (1 - t), 3 * t * t * (1 - t), t * t * t };
        }

        public IList<Point3D> GetSpherePoints()
        {
            var spherePoints = new List<Point3D>();

            foreach (var parameters in sphereBezierParameters)
            {
                var uBezier = CalculateBezierVector(parameters.X);
                var vBezier = CalculateBezierVector(parameters.Y);
                var wBezier = CalculateBezierVector(parameters.Z);
                var sum = new Point3D();
                for (int i = 0; i < N; i++)
                {
                    for (int j = 0; j < N; j++)
                    {
                        for (int k = 0; k < N; k++)
                        {
                            var factor = uBezier[i] * vBezier[j] * wBezier[k];
                            var point = controlPoints[i, j, k];
                            sum = new Point3D(sum.X + point.X * factor, sum.Y + point.Y * factor, sum.Z + point.Z * factor);
                        }
                    }
                }
                spherePoints.Add(sum);
            }
            return spherePoints;
        }


        public IList<Point3D> GetFaceControlPoints(int faceNumber)
        {
            IList<Point3D> points = new List<Point3D>();
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    switch (faceNumber)
                    {
                        case 0:
                            points.Add(controlPoints[i, j, 0]);
                            break;
                        case 1:
                            points.Add(controlPoints[i, 0, j]);
                            break;
                        case 2:
                            points.Add(controlPoints[0, i, j]);
                            break;
                        case 3:
                            points.Add(controlPoints[i, j, 3]);
                            break;
                        case 4:
                            points.Add(controlPoints[i, 3, j]);
                            break;
                        case 5:
                            points.Add(controlPoints[3, i, j]);
                            break;
                        default:
                            break;
                    }
                }
            }
            return points;
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
                X[x, y, z].X += noise * (rand.NextDouble() - 0.5);
                X[x, y, z].Y += noise * (rand.NextDouble() - 0.5);
                X[x, y, z].Z += noise * (rand.NextDouble() - 0.5);
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
                V0[x, y, z].X += noise * (rand.NextDouble() - 0.5);
                V0[x, y, z].Y += noise * (rand.NextDouble() - 0.5);
                V0[x, y, z].Z += noise * (rand.NextDouble() - 0.5);
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
