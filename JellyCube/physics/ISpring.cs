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
        /// <summary>
        /// Mass of the Spring
        /// </summary>
        double Mass { get; set; }

        /// <summary>
        /// Współczynnik sprężystości
        /// </summary>
        double Springer { get; set; }

        /// <summary>
        /// Współycznnik lepkości
        /// </summary>
        double Viscosity { get; set; }

        /// <summary>
        /// Krok w czasie
        /// </summary>
        double Delta { get; set; }

        void Initialize();

        Vector3D GetSecondPosition(Vector3D X0, Vector3D V0);

        Vector3D GetNextVelocity(Vector3D f, Vector3D v);

        Vector3D GetCurrentVelocity(Vector3D X, Vector3D Xp);

        Vector3D GetCurrentForce(Vector3D P1, Vector3D P2, Vector3D I0, Vector3D V);

    }
}
