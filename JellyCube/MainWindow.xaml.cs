using System.Collections;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using FrenetFrame.models;
using HelixToolkit.Wpf;
using JellyCube.models;
using MIConvexHull;
//using OxyPlot;
//using OxyPlot.Series;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;
//using Expression = NCalc.Expression;
using MathNet.Symbolics;
using Petzold.Media3D;
using Expression = MathNet.Symbolics.Expression;
using System.Collections.Generic;

namespace FrenetFrame
{

    //    /// <summary>
    ///// Simple interface to unify different types of triangulations in the future.
    ///// </summary>
    ///// <typeparam name="TVertex"></typeparam>
    ///// <typeparam name="TCell"></typeparam>
    //public interface ITriangulation<TVertex, TCell>
    //    where TCell : TriangulationCell<TVertex, TCell>, new()
    //    where TVertex : IVertex
    //{
    //    /// <summary>
    //    /// Triangulation simplexes. For 2D - triangles, 3D - tetrahedrons, etc ...
    //    /// </summary>
    //    IEnumerable<TCell> Cells { get; }
    //}

    //class Vertex : IVertex
    //    {
    //        public double[] Position { get; set; }

    //        public Vertex(Point3D point)
    //        {
    //            Position = new double[3] { point.X, point.Y, point.Z };
    //        }
    //    }

    public class Vertex : ModelVisual3D, IVertex
    {
        static readonly Material material = new DiffuseMaterial(Brushes.Black);
        static readonly SphereMesh mesh = new SphereMesh { Slices = 6, Stacks = 4, Radius = 0.5 };

        static readonly Material hullMaterial = new DiffuseMaterial(Brushes.Yellow);
        static readonly SphereMesh hullMesh = new SphereMesh { Slices = 6, Stacks = 4, Radius = 1.0 };

        /// <summary>
        /// Initializes a new instance of the <see cref="Vertex"/> class.
        /// </summary>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        /// <param name="z">The z position.</param>
        /// <param name="isHull"></param>
        public Vertex(double x, double y, double z, bool isHull = false)
        {
            Content = new GeometryModel3D
            {
                Geometry = isHull ? hullMesh.Geometry : mesh.Geometry,
                Material = isHull ? hullMaterial : material,
                Transform = new TranslateTransform3D(x, y, z)
            };
            Position = new double[] { x, y, z };
        }

        public Vertex AsHullVertex()
        {
            return new Vertex(Position[0], Position[1], Position[2], true);
        }

        public Point3D Center { get { return new Point3D(Position[0], Position[1], Position[2]); } }

        /// <summary>
        /// Gets or sets the coordinates.
        /// </summary>
        /// <value>The coordinates.</value>
        public double[] Position
        {
            get;
            set;
        }
    }

    public class Face : ConvexFace<Vertex, Face>
    {

    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool isSurfaceVisible;
        public bool IsSurfaceVisible
        {
            get { return isSurfaceVisible; }
            set
            {
                if (value != isSurfaceVisible)
                {
                    isSurfaceVisible = value;
                    OnPropertyChanged("IsSurfaceVisible");
                }
            }
        }

        private bool isDeformingObjectVisible;
        public bool IsDeformingObjectVisible
        {
            get { return isDeformingObjectVisible; }
            set
            {
                if (value != isDeformingObjectVisible)
                {
                    isDeformingObjectVisible = value;
                    OnPropertyChanged("IsDeformingObjectVisible");
                }
            }
        }

        private double animationSpeed;
        public double AnimationSpeed
        {
            get { return animationSpeed; }
            set
            {
                if (value != animationSpeed)
                {
                    animationSpeed = value;
                    OnPropertyChanged("AnimationSpeed");
                }
            }
        }

        private double noise;
        public double Noise
        {
            get { return noise; }
            set
            {
                if (value != noise)
                {
                    noise = value;
                    OnPropertyChanged("Noise");
                }
            }
        }

        private double mass;
        public double Mass
        {
            get { return mass; }
            set
            {
                if (value != mass)
                {
                    mass = value;
                    OnPropertyChanged("Mass");
                }
            }
        }

        private double springer;
        public double Springer
        {
            get { return springer; }
            set
            {
                if (value != springer)
                {
                    springer = value;
                    OnPropertyChanged("Springer");
                }
            }
        }
        private double springerFrame;
        public double SpringerFrame
        {
            get { return springerFrame; }
            set
            {
                if (value != springerFrame)
                {
                    springerFrame = value;
                    OnPropertyChanged("SpringerFrame");
                }
            }
        }

        private double viscosity;
        public double Viscosity
        {
            get { return viscosity; }
            set
            {
                if (value != viscosity)
                {
                    viscosity = value;
                    OnPropertyChanged("Viscosity");
                }
            }
        }

        private double delta;
        public double Delta
        {
            get { return delta; }
            set
            {
                if (value != delta)
                {
                    delta = value;
                    OnPropertyChanged("Delta");
                }
            }
        }

        private double segmentsCount;
        public double SegmentsCount
        {
            get { return segmentsCount; }
            set
            {
                if (value != segmentsCount)
                {
                    segmentsCount = value;
                    OnPropertyChanged("SegmentsCount");
                }
            }
        }

        Random rnd = new Random(DateTime.Now.Millisecond);
        System.Windows.Threading.DispatcherTimer dispatcherTimer;


        private IBezierCubeVisual3D bezierCube;
        private IFrameVisual3D frameCube;
        private CombinedManipulator manipulator;
        private int cubeSize = 6;
        private IRoomVisual3D room;
        private BezierSurface[] surfaces;
        private PointsVisual3D spherePoints;
        private List<DefaultTriangulationCell<Vertex>> Del_tetras;
        private MeshGeometryVisual3D geometry = new MeshGeometryVisual3D();

        #endregion

        #region Public Methods
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Initialize();
            InitializeScene();
            updateSpringData();
        }

        #endregion

        #region Private Methods
        private void InitializeScene()
        {
            const double maxVal = 8;

            room = new RoomVisual3D();
            room.Initialize();

            bezierCube = new BezierCubeVisual3D();
            bezierCube.Initialize(cubeSize);
            bezierCube.CollisionChecker = room;

            HelixViewport.Children.Add(bezierCube.points);
            HelixViewport.Children.Add(bezierCube.lines);

            frameCube = new FrameVisual3D();
            frameCube.Initialize(cubeSize);
            HelixViewport.Children.Add(frameCube.points);
            HelixViewport.Children.Add(frameCube.lines);
            HelixViewport.Children.Add(frameCube.GetJointsPoints(bezierCube.GetCornerPoints()));

            manipulator = new CombinedManipulator();
            manipulator.Offset = new Vector3D(0, 0, 7);
            HelixViewport.Children.Add(manipulator);



            surfaces = new BezierSurface[6];
            for (int i = 0; i < 6; i++)
            {
                surfaces[i] = new BezierSurface()
                {
                    Fill = new SolidColorBrush(Color.FromArgb(100, 255, 0, 255)),//Brushes.BlueViolet,
                    MeshSizeU = 20,
                    MeshSizeV = 20
                };
                surfaces[i].UpdateSurface(bezierCube.GetFaceControlPoints(i));
                surfaces[i].UpdateModel();
            }

            IList<Vertex> vertices = bezierCube.GetSpherePoints().Select(p => new Vertex(p.X, p.Y, p.Z)).ToList();
            var convexHull = ConvexHull.Create<Vertex, Face>(vertices);
            List<Vertex> convexHullVertices = convexHull.Points.ToList();
            List<Face> faces = convexHull.Faces.ToList();

            geometry = new MeshGeometryVisual3D();
            var builder = new MeshBuilder(false, false);

            IList<Point3D> t = new List<Point3D>();
            faces.ForEach(node =>
            {
                double[] pos = node.Vertices[0].Position;
                t.Add(new Point3D(pos[0], pos[1], pos[2]));
                pos = node.Vertices[1].Position;
                t.Add(new Point3D(pos[0], pos[1], pos[2]));
                pos = node.Vertices[2].Position;
                t.Add(new Point3D(pos[0], pos[1], pos[2]));
            });
            builder.AddTriangles(t);
            geometry.MeshGeometry = builder.ToMesh(true);


            spherePoints = new PointsVisual3D();
            spherePoints.Points = bezierCube.GetSpherePoints();
            spherePoints.Size = 3;




            ParametricSurface3D v;


            room.UpdateViewport(HelixViewport);
            

            double arrowsSize = 0.05;


            var arrowX = new ArrowVisual3D();
            arrowX.Direction = new Vector3D(1, 0, 0);
            arrowX.Point1 = new Point3D(0, 0, 0);
            arrowX.Point2 = new Point3D(maxVal, 0, 0);
            arrowX.Diameter = arrowsSize;
            arrowX.Fill = System.Windows.Media.Brushes.Black;
            HelixViewport.Children.Add(arrowX);
            HelixToolkit.Wpf.MeshVisual3D mesh;
    
            var arrowMX = new ArrowVisual3D();
            arrowMX.Direction = new Vector3D(-1, 0, 0);
            arrowMX.Point1 = new Point3D(0, 0, 0);
            arrowMX.Point2 = new Point3D(-maxVal, 0, 0);
            arrowMX.Diameter = arrowsSize;
            arrowMX.Fill = System.Windows.Media.Brushes.Black;
            HelixViewport.Children.Add(arrowMX);

            var arrowY = new ArrowVisual3D();
            arrowY.Direction = new Vector3D(0, 1, 0);
            arrowY.Point1 = new Point3D(0, 0, 0);
            arrowY.Point2 = new Point3D(0, maxVal, 0);
            arrowY.Diameter = arrowsSize;
            arrowY.Fill = System.Windows.Media.Brushes.Black;
            HelixViewport.Children.Add(arrowY);

            var arrowMY = new ArrowVisual3D();
            arrowMY.Direction = new Vector3D(0, -1, 0);
            arrowMY.Point1 = new Point3D(0, 0, 0);
            arrowMY.Point2 = new Point3D(0, -maxVal, 0);
            arrowMY.Diameter = arrowsSize;
            arrowMY.Fill = System.Windows.Media.Brushes.Black;
            HelixViewport.Children.Add(arrowMY);

            var arrowZ = new ArrowVisual3D();
            arrowZ.Direction = new Vector3D(0, 0, 1);
            arrowZ.Point1 = new Point3D(0, 0, 0);
            arrowZ.Point2 = new Point3D(0, 0, maxVal);
            arrowZ.Diameter = arrowsSize;
            arrowZ.Fill = System.Windows.Media.Brushes.Black;
            HelixViewport.Children.Add(arrowZ);

            var arrowMZ = new ArrowVisual3D();
            arrowMZ.Direction = new Vector3D(0, 0, -1);
            arrowMZ.Point1 = new Point3D(0, 0, 0);
            arrowMZ.Point2 = new Point3D(0, 0, -maxVal);
            arrowMZ.Diameter = arrowsSize;
            arrowMZ.Fill = System.Windows.Media.Brushes.Black;
            HelixViewport.Children.Add(arrowMZ);

            var xArrowText = new TextVisual3D();
            xArrowText.Text = "X";
            xArrowText.Position = new Point3D(maxVal - 0.5, 0, 0.5);
            xArrowText.Height = 0.5;
            xArrowText.FontWeight = System.Windows.FontWeights.Bold;
            HelixViewport.Children.Add(xArrowText);

            var yArrowText = new TextVisual3D();
            yArrowText.Text = "Y";
            yArrowText.Position = new Point3D(0, maxVal - 0.5, 0.5);
            yArrowText.Height = 0.5;
            yArrowText.FontWeight = System.Windows.FontWeights.Bold;
            HelixViewport.Children.Add(yArrowText);

            var zArrowText = new TextVisual3D();
            zArrowText.Text = "Z";
            zArrowText.Position = new Point3D(0.5, 0, maxVal - 0.5);
            zArrowText.Height = 0.5;
            zArrowText.FontWeight = System.Windows.FontWeights.Bold;
            HelixViewport.Children.Add(zArrowText);

        }

        private void Initialize()
        {
            Noise = 5;
            IsSurfaceVisible = true;
            IsDeformingObjectVisible = true;

            Mass = 1;
            Springer = 0.5; //c1  sprężystość
            SpringerFrame = 0.2; // c2 sprężystość
            Viscosity = 0.2; //k lepkość
            Delta = 0.5;

            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, (int)animationSpeed); //new TimeSpan(0, 0, 0, 0, intervalFunc(animationSpeed));

           dispatcherTimer.Start();
        }


        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            HelixViewport.Children.Remove(bezierCube.points);
            HelixViewport.Children.Remove(bezierCube.lines);
            HelixViewport.Children.Remove(frameCube.points);
            HelixViewport.Children.Remove(frameCube.lines);

            frameCube.SetTransform(manipulator.TargetTransform);

            bezierCube.CalculateJointForces(frameCube.GetFramePoints());
            bezierCube.Update();
            LinesVisual3D frameLines = frameCube.GetJointsPoints(bezierCube.GetCornerPoints());
            HelixViewport.Children.Remove(frameLines);
            HelixViewport.Children.Add(frameLines);


            bezierCube.points.Points = bezierCube.GetControlPoints();
            bezierCube.lines.Points = bezierCube.GetControlLines();
            HelixViewport.Children.Add(bezierCube.lines);
            HelixViewport.Children.Add(bezierCube.points);

            frameCube.points.Points = frameCube.GetControlPoints();
            frameCube.lines.Points = frameCube.GetControlLines();
            HelixViewport.Children.Add(frameCube.lines);
            HelixViewport.Children.Add(frameCube.points);

          
            if (IsDeformingObjectVisible)
            {
                HelixViewport.Children.Remove(geometry);
                IList<Vertex> vertices = bezierCube.GetSpherePoints().Select(p => new Vertex(p.X, p.Y, p.Z)).ToList();
                var convexHull = ConvexHull.Create<Vertex, Face>(vertices);
                List<Vertex> convexHullVertices = convexHull.Points.ToList();
                List<Face> faces = convexHull.Faces.ToList();

                geometry = new MeshGeometryVisual3D();
                var builder = new MeshBuilder(false, false);

                IList<Point3D> t = new List<Point3D>();
                faces.ForEach(node =>
                {
                    double[] pos = node.Vertices[0].Position;
                    t.Add(new Point3D(pos[0], pos[1], pos[2]));
                    pos = node.Vertices[1].Position;
                    t.Add(new Point3D(pos[0], pos[1], pos[2]));
                    pos = node.Vertices[2].Position;
                    t.Add(new Point3D(pos[0], pos[1], pos[2]));
                });
                builder.AddTriangles(t);
                geometry.MeshGeometry = builder.ToMesh(true);
                HelixViewport.Children.Add(geometry);


                HelixViewport.Children.Remove(spherePoints);
                spherePoints.Points = bezierCube.GetSpherePoints();
                HelixViewport.Children.Add(spherePoints);
            }
            if (IsSurfaceVisible)
            {
                for (int i = 0; i < 6; i++)
                {
                    surfaces[i].UpdateSurface(bezierCube.GetFaceControlPoints(i));
                    surfaces[i].UpdateModel();
                    HelixViewport.Children.Remove(surfaces[i]);
                    HelixViewport.Children.Add(surfaces[i]);
                }
            }
            room.UpdateViewport(HelixViewport);
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Start();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Stop();
        }

        private void RefreshButtonClick(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0,(int) animationSpeed);
            updateSpringData();

        }

        private void updateSpringData()
        {
            bezierCube.noise = Noise;

            bezierCube.spring.Mass = Mass;
            bezierCube.spring.Springer = Springer;
            bezierCube.spring.Viscosity = Viscosity;
            bezierCube.spring.Delta = Delta;


            bezierCube.springFrame.Mass = Mass;
            bezierCube.springFrame.Springer = SpringerFrame;
            bezierCube.springFrame.Viscosity = Viscosity;
            bezierCube.springFrame.Delta = Delta;
        }

        #endregion

        private void ResetButtonClick(object sender, RoutedEventArgs e)
        {
            HelixViewport.Children.Remove(bezierCube.points);
            HelixViewport.Children.Remove(bezierCube.lines);
            HelixViewport.Children.Remove(frameCube.points);
            HelixViewport.Children.Remove(frameCube.lines);
            HelixViewport.Children.Remove(spherePoints);
            LinesVisual3D frameLines = frameCube.GetJointsPoints(bezierCube.GetCornerPoints());
            HelixViewport.Children.Remove(frameLines);
            HelixViewport.Children.Remove(manipulator);
            HelixViewport.Children.Remove(spherePoints);
            HelixViewport.Children.Remove(geometry);

            room.RemoveWalls(HelixViewport);
            for (int i = 0; i < 6; i++)
            {
                 HelixViewport.Children.Remove(surfaces[i]);
            }

            InitializeScene();
            updateSpringData();
            if (IsSurfaceVisible)
            {
                for (int i = 0; i < 6; i++)
                {
                    HelixViewport.Children.Add(surfaces[i]);
                }
            }
            if (IsDeformingObjectVisible)
            {
                HelixViewport.Children.Add(geometry);
                HelixViewport.Children.Add(spherePoints);
            }
        }

        private void SurfaceVisibilityCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 6; i++)
            {
                HelixViewport.Children.Add(surfaces[i]);
            }
        }

        private void SurfaceVisibilityCheckBox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 6; i++)
            {
                HelixViewport.Children.Remove(surfaces[i]);
            }
        }

        private void DeformingObjectCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            HelixViewport.Children.Add(geometry);
            HelixViewport.Children.Add(spherePoints);
        }

        private void DeformingObjectCheckBox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            HelixViewport.Children.Remove(geometry);
            HelixViewport.Children.Remove(spherePoints);
        }
    }
}
