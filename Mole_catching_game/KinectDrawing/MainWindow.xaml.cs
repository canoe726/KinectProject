using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Timers;


namespace KinectDrawing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static int numOfdigda = 6;
        private UserControl[] UC;
        static Random r = new Random();
        static int rInt;
        static int rInt2;

        private int preHandState = 0;

        private KinectSensor _sensor = null;
        private ColorFrameReader _colorReader = null;
        private BodyFrameReader _bodyReader = null;
        private IList<Body> _bodies = null;

        private int _width = 0;
        private int _height = 0;
        private byte[] _pixels = null;
        private WriteableBitmap _bitmap = null;

        private string ScoreState = "Score : ";
        private static int score = 0;
        private static int stage = 0;
        private static int stageTime = 1500;

        private static int remainSecond = 60;

        private static bool NoSenceBody = false;
        private static bool TimerStart = false;

        private static int label_left = 2000;
        private static int label_top = 2000;

        private static double DigdaDown_left = 2000;
        private static double DigdaDown_top = 2000;

        private static double DacTrio_left = 2000;
        private static double DacTrio_top = 2000;

        private static double DacTrioDown_left = 2000;
        private static double DacTrioDown_top = 2000;

        private static Timer GameTimer;
        private static Timer DigdaTimer;
        private static Timer DigdaDownTimer;
        private static Timer DacTrioTimer;
        private static Timer DacTrioTimer2;
        private static Timer DacTrioDownTimer;
        private static Timer LabelTimer;

        public MainWindow()
        {
            InitializeComponent();

            _sensor = KinectSensor.GetDefault();

            UC = new UserControl[numOfdigda];
            UC[0] = Digda_1;
            UC[1] = Digda_2;
            UC[2] = Digda_3;
            UC[3] = Digda_4;
            UC[4] = Digda_5;
            UC[5] = Digda_6;

            if (_sensor != null)
            {
                _sensor.Open();

                _width = _sensor.ColorFrameSource.FrameDescription.Width;
                _height = _sensor.ColorFrameSource.FrameDescription.Height;

                _colorReader = _sensor.ColorFrameSource.OpenReader();
                _colorReader.FrameArrived += ColorReader_FrameArrived;

                _bodyReader = _sensor.BodyFrameSource.OpenReader();
                _bodyReader.FrameArrived += BodyReader_FrameArrived;

                _pixels = new byte[_width * _height * 4];
                _bitmap = new WriteableBitmap(_width, _height, 96.0, 96.0, PixelFormats.Bgra32, null);

                _bodies = new Body[_sensor.BodyFrameSource.BodyCount];

                camera.Source = _bitmap;
            }
        }

        private static void OnDigdaEvent(Object source, ElapsedEventArgs e)
        {
            rInt = r.Next(0, numOfdigda);
            Console.WriteLine("The Elapsed event was raised at {0}", e.SignalTime);
        }

        private static void OnDigdaDownEvent(Object source, ElapsedEventArgs e)
        {
            DigdaDown_left = 2000;
            DigdaDown_top = 2000;
        }

        private static void OnDacTrioEvent(Object source, ElapsedEventArgs e)
        {
            rInt2 = r.Next(0, numOfdigda);
        }

        private static void OnDacTrioEvent2(Object source, ElapsedEventArgs e)
        {
            DacTrio_left = 2000;
            DacTrio_top = 2000;
        }

        private static void OnDacTrioDownEvent(Object source, ElapsedEventArgs e)
        {
            DacTrioDown_left = 2000;
            DacTrioDown_top = 2000;
        }

        private static void LabelTimerEvent(Object source, ElapsedEventArgs e)
        {
            label_left = 2000;
            label_top = 2000;
        }
        

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_colorReader != null)
            {
                _colorReader.Dispose();
            }

            if (_bodyReader != null)
            {
                _bodyReader.Dispose();
            }

            if (_sensor != null)
            {
                _sensor.Close();
            }
        }

        private void ColorReader_FrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    frame.CopyConvertedFrameDataToArray(_pixels, ColorImageFormat.Bgra);

                    _bitmap.Lock();
                    Marshal.Copy(_pixels, 0, _bitmap.BackBuffer, _pixels.Length);
                    _bitmap.AddDirtyRect(new Int32Rect(0, 0, _width, _height));
                    _bitmap.Unlock();
                }
            }
        }

        private void BodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            if (remainSecond == 0) { GameOver.Foreground = Brushes.Red; NoSenceBody = true; Canvas.SetLeft(GameOver, 225); Canvas.SetTop(GameOver, 300); }
            if (NoSenceBody == true) {  return ; }

            if (score >= 300 + (stage * 400))
            {
                stage += 1;
                Console.WriteLine("Speed UP!!");
                stageTime -= 200;

                LabelTimer = new System.Timers.Timer(stageTime);
                LabelTimer.Elapsed += LabelTimerEvent;
                LabelTimer.Enabled = true;

                label_left = 400;
                label_top = 370;
            }
            NextStage.Foreground = Brushes.Blue;
            Canvas.SetLeft(NextStage, label_left); Canvas.SetTop(NextStage, label_top);

            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    frame.GetAndRefreshBodyData(_bodies);

                    Body body = _bodies.Where(b => b.IsTracked).FirstOrDefault();
                    
                    if (body != null)
                    {
                        Joint handRight = body.Joints[JointType.HandRight];
                        if (TimerStart == false) { TimerFunc(); }
                        TimerStart = true;

                        for (int i = 0; i < numOfdigda; i++)
                        {
                            /*
                            Canvas.Left="350" Canvas.Top="200" 
                            Canvas.Left="800" Canvas.Top="200" 
                            Canvas.Left="1250" Canvas.Top="200"
                            Canvas.Left="350" Canvas.Top="600" 
                            Canvas.Left="800" Canvas.Top="600" 
                            Canvas.Left="1250" Canvas.Top="600"
                            */

                            if (rInt == i )
                            {
                                if (rInt < 3)
                                {
                                    Canvas.SetLeft(UC[rInt], 300 + (450 * rInt));
                                    Canvas.SetTop(UC[rInt], 200);
                                    UC[rInt].Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    Canvas.SetLeft(UC[rInt], 300 + (450 * (rInt - 3)));
                                    Canvas.SetTop(UC[rInt], 600);
                                    UC[rInt].Visibility = Visibility.Visible;
                                }
                            }
                            else
                            {
                                UC[i].Visibility = Visibility.Collapsed;
                            }
                            
                            if(rInt2 == i && rInt2 != rInt)
                            {
                                if (rInt2 < 3)
                                {
                                    Canvas.SetLeft(DacTrio, 300 + (450 * rInt2));
                                    Canvas.SetTop(DacTrio, 200);
                                    DacTrio.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    Canvas.SetLeft(DacTrio, 300 + (450 * (rInt2 - 3)));
                                    Canvas.SetTop(DacTrio, 600);
                                    DacTrio.Visibility = Visibility.Visible;
                                }
                                DacTrioTimer2 = new System.Timers.Timer(1500);
                                DacTrioTimer2.Elapsed += OnDacTrioEvent2;
                                DacTrioTimer2.Enabled = true;
                            }
                        }
                        
                        if (handRight.TrackingState != TrackingState.NotTracked)
                        {
                            CameraSpacePoint handRightPosition = handRight.Position;
                            ColorSpacePoint handRightPoint = _sensor.CoordinateMapper.MapCameraPointToColorSpace(handRightPosition);

                            float x = handRightPoint.X;
                            float y = handRightPoint.Y;

                            if (!float.IsInfinity(x) && !float.IsInfinity(y))
                            {
                                // 오른쪽 손 위치 추적
                                switch (body.HandRightState)
                                {
                                    case HandState.Lasso:
                                    case HandState.Unknown:
                                    case HandState.NotTracked:
                                    case HandState.Open:
                                        preHandState = 0;
                                        // 보이지 않게 하기
                                        //HammerDown.Visibility = Visibility.Collapsed;
                                        // 보이게 하기
                                        //Hammer.Visibility = Visibility.Visible;
                                        Canvas.SetLeft(Hammer, x - 150);
                                        Canvas.SetTop(Hammer, y - 150);
                                        
                                        Canvas.SetLeft(HammerDown, x + 2000);
                                        Canvas.SetTop(HammerDown, y + 2000);
                                        break;

                                    case HandState.Closed:
                                        Canvas.SetLeft(Hammer, x + 2000);
                                        Canvas.SetTop(Hammer, y + 2000);
                                        
                                        Canvas.SetLeft(HammerDown, x - 150);
                                        Canvas.SetTop(HammerDown, y - 200);
                                        
                                        for (int i = 0; i < numOfdigda; i++)
                                        {
                                            if ((Canvas.GetLeft(UC[i]) < x)
                                            && ((Canvas.GetLeft(UC[i]) + UC[i].Width) > x)
                                            && (Canvas.GetTop(UC[i]) < y)
                                            && ((Canvas.GetTop(UC[i]) + UC[i].Height) > y)
                                            && (UC[i].Visibility == Visibility.Visible)
                                            && preHandState == 0)
                                            {
                                                preHandState = 1;
                                                rInt = r.Next(0, numOfdigda);

                                                UC[i].Visibility = Visibility.Collapsed;

                                                DigdaDown_top = Canvas.GetTop(UC[i]);
                                                DigdaDown_left = Canvas.GetLeft(UC[i]);

                                                Canvas.SetTop(DigdaDown, DigdaDown_top);
                                                Canvas.SetLeft(DigdaDown, DigdaDown_left);

                                                DigdaDownTimer = new System.Timers.Timer(1500);
                                                DigdaDownTimer.Elapsed += OnDigdaDownEvent;
                                                DigdaDownTimer.Enabled = true;

                                                score += 100;
                                                DigdaTimer.Interval = stageTime;
                                            }
                                        }

                                        if (   (Canvas.GetLeft(DacTrio) < x)
                                            && ((Canvas.GetLeft(DacTrio) + DacTrio.Width) > x)
                                            && (Canvas.GetTop(DacTrio) < y)
                                            && ((Canvas.GetTop(DacTrio) + DacTrio.Height) > y)
                                            && (DacTrio.Visibility == Visibility.Visible)
                                            && preHandState == 0)
                                        {
                                            preHandState = 1;
                                            rInt2 = r.Next(0, numOfdigda);

                                            DacTrio.Visibility = Visibility.Collapsed;

                                            DacTrioDown_top = Canvas.GetTop(DacTrio);
                                            DacTrioDown_left = Canvas.GetLeft(DacTrio);

                                            Canvas.SetTop(DacTrioDown, DacTrioDown_top);
                                            Canvas.SetLeft(DacTrioDown, DacTrioDown_left);

                                            DacTrioDownTimer = new System.Timers.Timer(1500);
                                            DacTrioDownTimer.Elapsed += OnDacTrioDownEvent;
                                            DacTrioDownTimer.Enabled = true;

                                            remainSecond += 1;
                                        }

                                        break;

                                    default:
                                        break;
                                }
                                
                            }
                            Canvas.SetTop(DigdaDown, DigdaDown_top);
                            Canvas.SetLeft(DigdaDown, DigdaDown_left);

                            

                            Canvas.SetTop(DacTrioDown, DacTrioDown_top);
                            Canvas.SetLeft(DacTrioDown, DacTrioDown_left);

                            Score.Text = ScoreState + score.ToString();
                            GameTime.Content = remainSecond.ToString();
                            if( remainSecond <= 5 ) { GameTime.Foreground = Brushes.Red; }
                        }
                    }
                }
            }

        }

        private static void OnGameTimerEvent(Object source, ElapsedEventArgs e)
        {
            remainSecond -= 1;
        }

        private void TimerFunc()
        {
            DigdaTimer = new System.Timers.Timer(stageTime);
            DigdaTimer.Elapsed += OnDigdaEvent;
            DigdaTimer.Enabled = true;

            DacTrioTimer = new System.Timers.Timer(1000);
            DacTrioTimer.Elapsed += OnDacTrioEvent;
            DacTrioTimer.Enabled = true;

            GameTimer = new System.Timers.Timer(1000);
            GameTimer.Elapsed += OnGameTimerEvent;
            GameTimer.Enabled = true;
        }
        
    }
}