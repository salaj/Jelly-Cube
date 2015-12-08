using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using JellyCube.common;

namespace FrenetFrame.models
{
    public interface IBezierCubeVisual3D : IVisual3dProvidable
    {

        void Update();

        Vector3D[,,] GetCornerPoints();

        void CalculateJointForces(Point3D[,,] framePoints);
    }
}
