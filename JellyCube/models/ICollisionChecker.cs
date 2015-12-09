using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace JellyCube.models
{
    public interface ICollisionChecker
    {
        bool CheckCollision(Vector3D P, ref Vector3D v);

        void TrimPoint(ref Vector3D P);
    }
}
