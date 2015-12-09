using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using JellyCube.common;

namespace JellyCube.models
{
    public class BezierSurface : ParametricSurface3D
    {
        bool initialized = false;
        const int Size = 4;
        Matrix4 xMatrix;
        Matrix4 yMatrix;
        Matrix4 zMatrix;

        public void UpdateSurface(IList<Point3D> controlPoints)
        {
            xMatrix = new Matrix4(Size, Size);
            yMatrix = new Matrix4(Size, Size);
            zMatrix = new Matrix4(Size, Size);

            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    var point = controlPoints[i * Size + j];
                    xMatrix[i, j] = point.X;
                    yMatrix[i, j] = point.Y;
                    zMatrix[i, j] = point.Z;
                }
            }
            initialized = true;
        }

        protected override Point3D Evaluate(double u, double v, out Point texCoord)
        {
            if (!initialized)
            {
                texCoord = new Point(u, 0);
                return new Point3D();
            }
            var leftVector = CalculateBezierVector(u);
            var rightVector = CalculateBezierVector(v);
            texCoord = new Point(u, 0);
            var x = leftVector * xMatrix * rightVector;
            var y = leftVector * yMatrix * rightVector;
            var z = leftVector * zMatrix * rightVector;
            var pt = new Point3D(x, y, z);
            return pt;
        }

        protected Vector4 CalculateBezierVector(double t)
        {
            return new Vector4((1 - t) * (1 - t) * (1 - t), 3 * t * (1 - t) * (1 - t), 3 * t * t * (1 - t), t * t * t);
        }

    }
}
