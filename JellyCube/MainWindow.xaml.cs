using System.Windows.Media;
using FrenetFrame.models;
using HelixToolkit.Wpf;
using JellyCube.models;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;
//using Expression = NCalc.Expression;
using MathNet.Symbolics;
using Expression = MathNet.Symbolics.Expression;
using System.Collections.Generic;

namespace FrenetFrame
{
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

        ObservableCollection<Point3D> curvePoints;
        public ObservableCollection<Point3D> CurvePoints
        {
            get { return curvePoints; }
        }

        ObservableCollection<DataPoint> kappaPoints;
        public ObservableCollection<DataPoint> KappaPoints
        {
            get { return kappaPoints; }
        }

        ObservableCollection<DataPoint> tauPoints;
        public ObservableCollection<DataPoint> TauPoints
        {
            get { return tauPoints; }
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

        private string xParametrization;
        public string XParametrization
        {
            get { return xParametrization; }
            set
            {
                if (value != xParametrization)
                {
                    xParametrization = value;
                    OnPropertyChanged("XParametrization");
                }
            }
        }

        private string yParametrization;
        public string YParametrization
        {
            get { return yParametrization; }
            set
            {
                if (value != yParametrization)
                {
                    yParametrization = value;
                    OnPropertyChanged("YParametrization");
                }
            }
        }

        private string zParametrization;
        public string ZParametrization
        {
            get { return zParametrization; }
            set
            {
                if (value != zParametrization)
                {
                    zParametrization = value;
                    OnPropertyChanged("ZParametrization");
                }
            }
        }

        private double minParameterValue;
        public double MinParameterValue
        {
            get { return minParameterValue; }
            set
            {
                if (value != minParameterValue)
                {
                    minParameterValue = value;
                    OnPropertyChanged("MinParameterValue");
                }
            }
        }

        private double maxParameterValue;
        public double MaxParameterValue
        {
            get { return maxParameterValue; }
            set
            {
                if (value != maxParameterValue)
                {
                    maxParameterValue = value;
                    OnPropertyChanged("MaxParameterValue");
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

        Expression xExpression;
        Expression yExpression;
        Expression zExpression;
        private bool curveLoaded = false;
        private int actualParameterIndex;
        private double parameterStep;
        ArrowVisual3D arrowTangent;
        ArrowVisual3D arrowNormal;
        ArrowVisual3D arrowBinormal;
        Func<double, int> intervalFunc;
        private List<Point3D> curveArray;
        private bool animationPaused;
        private const double Epsilon = 0.0001;
        Dictionary<string, FloatingPoint> symbols;
        Expression symbol;
        private IBezierCubeVisual3D bezierCube;
        private IFrameVisual3D frameCube;
        private CombinedManipulator manipulator;
        private int cubeSize = 6;
        private RectangleVisual3D rect;

        #endregion

        #region Public Methods
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Initialize();
            InitializeScene();
        }

        #endregion

        #region Private Methods
        private void InitializeScene()
        {
            const double maxVal = 8;

            //SortingVisual3D sortingVisual3D = new SortingVisual3D();
            //sortingVisual3D.SortingFrequency = 2;
            //HelixViewport.Children.Add(sortingVisual3D);



            bezierCube = new BezierCubeVisual3D();
            bezierCube.Initialize(cubeSize);

            HelixViewport.Children.Add(bezierCube.points);
            HelixViewport.Children.Add(bezierCube.lines);

            frameCube = new FrameVisual3D();
            frameCube.Initialize(cubeSize);
            HelixViewport.Children.Add(frameCube.points);
            HelixViewport.Children.Add(frameCube.lines);
            HelixViewport.Children.Add(frameCube.GetJointsPoints(bezierCube.GetCornerPoints()));

            manipulator = new CombinedManipulator();
            HelixViewport.Children.Add(manipulator);

            rect = new RectangleVisual3D();
            rect.Fill = new SolidColorBrush(Color.FromArgb(120, 255, 0, 0));
            rect.Normal = new Vector3D(0, 1, 0);
            rect.Origin = new Point3D(-10, 0, 0);
            rect.Width = 20;
            rect.Length = 20;
            rect.LengthDirection = new Vector3D(0, 0, 1);

            HelixViewport.Children.Add(rect);
            

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

            arrowTangent = new ArrowVisual3D();
            arrowNormal = new ArrowVisual3D();
            arrowBinormal = new ArrowVisual3D();
        }

        private void Initialize()
        {
            curvePoints = new ObservableCollection<Point3D>();
            curveArray = new List<Point3D>();
            kappaPoints = new ObservableCollection<DataPoint>();
            tauPoints = new ObservableCollection<DataPoint>();
            animationSpeed = 5.5;
            xParametrization = "2 * sin(t)";
            yParametrization = "2 * cos(t)";
            zParametrization = "t^2 / 6";
            symbol = Expression.Symbol("t");
            symbols = new Dictionary<string, FloatingPoint> { { "t", 0 } };
            minParameterValue = -2 * Math.PI;
            maxParameterValue = 2 * Math.PI;
            segmentsCount = 500;
            intervalFunc = (x) => (int)(500 / Math.Pow(2, x));
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 10); //new TimeSpan(0, 0, 0, 0, intervalFunc(animationSpeed));

            dispatcherTimer.Start();
        }

        private double Norm(Vector3D v)
        {
            return Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
        }

        private Vector3D PerpendicularVector(Vector3D v)
        {
            if (Math.Abs(v.X - v.Y) < Epsilon && Math.Abs(v.Y - v.Z) < Epsilon)
                return new Vector3D(v.X, v.Y, -v.Z);
            var x = Math.Abs(v.X);
            var y = Math.Abs(v.Y);
            var z = Math.Abs(v.Z);
            if (x > y && x > z)
                return new Vector3D(v.Z, -v.X, v.Y);
            if (y > x && y > z)
                return new Vector3D(v.Z, v.X, -v.Y);
            return new Vector3D(-v.Z, -v.X, v.Y);
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

            HelixViewport.Children.Remove(rect);
            HelixViewport.Children.Add(rect);
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            //if (!curveLoaded)
            //    LoadCurveButton_Click(null, null);
            //if (animationPaused)
            //    animationPaused = false;
            //else
            //    actualParameterIndex = 2;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, intervalFunc(animationSpeed));
            dispatcherTimer.Start();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Stop();
            animationPaused = true;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Stop();
            HelixViewport.Children.Remove(arrowTangent);
            HelixViewport.Children.Remove(arrowNormal);
            HelixViewport.Children.Remove(arrowBinormal);
            kappaPoints.Clear();
            tauPoints.Clear();
        }

        private void LoadCurveButton_Click(object sender, RoutedEventArgs e)
        {
            curvePoints.Clear();
            curveArray.Clear();
            //xExpression = new Expression(xParametrization);
            //xExpression.Parameters.Add("t", 0);
            //yExpression = new Expression(yParametrization);
            //yExpression.Parameters.Add("t", 0);
            //zExpression = new Expression(zParametrization);
            //zExpression.Parameters.Add("t", 0);

            //if (xExpression.HasErrors() || yExpression.HasErrors() || zExpression.HasErrors())
            //{
            //    curveLoaded = false;
            //    MessageBox.Show("Wrong format of parametrizations.");
            //    return;
            //}

            try
            {
                xExpression = Infix.ParseOrThrow(xParametrization);
                yExpression = Infix.ParseOrThrow(yParametrization);
                zExpression = Infix.ParseOrThrow(zParametrization);
            }
            catch (Exception exception)
            {
                curveLoaded = false;
                MessageBox.Show("Wrong format of parametrization.\n" + exception.Message);
                return;
            }
            
            var flag = true;
            try
            {
                parameterStep = (maxParameterValue - minParameterValue) / segmentsCount;
                for (double t = minParameterValue; t < maxParameterValue + parameterStep / 2; t += parameterStep)
                {
                    //xExpression.Parameters["t"] = t;
                    //yExpression.Parameters["t"] = t;
                    //zExpression.Parameters["t"] = t;
                    //double xResult = (double)xExpression.Evaluate();
                    //double yResult = (double)yExpression.Evaluate();
                    //double zResult = (double)zExpression.Evaluate();
                    symbols["t"] = t;
                    double xResult = Evaluate.Evaluate(symbols, xExpression).RealValue;
                    double yResult = Evaluate.Evaluate(symbols, yExpression).RealValue;
                    double zResult = Evaluate.Evaluate(symbols, zExpression).RealValue;
                    curveArray.Add(new Point3D(xResult, yResult, zResult));
                    curvePoints.Add(new Point3D(xResult, yResult, zResult));
                    if (flag)
                        flag = false;
                    else
                        curvePoints.Add(curvePoints[curvePoints.Count - 1]);
                }
            }
            catch (Exception exception)
            {
                curveLoaded = false;
                curvePoints.Clear();
                if(exception.HResult == -2146232969)
                    MessageBox.Show("Given parametrization is not based on [t] variable.\n");
                else if(exception.HResult == -2146233088)
                    MessageBox.Show("Cannot calculate curve for given set.\n");
                else
                    MessageBox.Show("Cannot calculate curve for given set.\n" + exception.Message);
                return;
            }
            curveLoaded = true;
        }
        #endregion
    }
}
