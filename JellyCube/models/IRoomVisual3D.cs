using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using HelixToolkit.Wpf;

namespace JellyCube.models
{
    public interface IRoomVisual3D : ICollisionChecker
    {
        double roomSize { get; set; }

        bool IsDampingActive { get; set; }

        void Initialize();

        void UpdateViewport(HelixViewport3D viewport3D);

        void RemoveWalls(HelixViewport3D viewport3D);
    }
}
