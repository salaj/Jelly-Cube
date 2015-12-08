using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using JellyCube.common;

namespace JellyCube.models
{
    public abstract class BaseVisual3D : IVisual3dProvidable
    {

        public PointsVisual3D points { get; set; }
        public LinesVisual3D lines { get; set; }
        protected Point3D[,,] controlPoints;
        protected int N;

        protected void Initialize()
        {
            controlPoints = new Point3D[N,N,N];
            points = new PointsVisual3D();
            points.Size = 3;
            lines = new LinesVisual3D();
        }

        public abstract void Initialize(double cubeSize);

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
    }
}
