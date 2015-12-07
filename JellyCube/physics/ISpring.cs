using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace FrenetFrame.physics
{
    public interface ISpring
    {
        void Initialize();

        Vector3D GetSecondPosition(Vector3D X0, Vector3D V0);

        Vector3D GetNextVelocity(Vector3D f, Vector3D v);

        Vector3D GetCurrentVelocity(Vector3D X, Vector3D Xp);

        Vector3D GetCurrentForce(Vector3D P1, Vector3D P2, Vector3D I0, Vector3D V);

    }
}
