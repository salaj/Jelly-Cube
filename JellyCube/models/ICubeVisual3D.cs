using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace FrenetFrame.models
{
    public interface IBezierCubeVisual3D
    {
        void Initialize();

        IList<Point3D> GetControlPoints();
        IList<Point3D> GetControlLines();

        void Update();
    }
}
