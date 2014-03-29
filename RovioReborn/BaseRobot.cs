using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Timers;
using System.Windows.Forms;
using PredatorPreyAssignment;

namespace Rovio
{
    abstract class BaseRobot : Robot
    {
        public Bitmap lastProcessedImage;
        public Bitmap cameraImage;
        public System.Drawing.Point cameraDimensions;


        public int batteryStatus;
        public int chargingStatus;
        protected bool irSensor = false;
        protected bool running = true;
        protected Object commandLock = new Object();

        private System.Threading.Thread cameraImageThread;
        public string direction = "Unknown";
        protected Map map;
        public double cumulativeAngle = 0;
        protected List<int> keys = new List<int>();
        public Object mapLock = new Object();

        public delegate void ImageReady(Image image);
        


        protected bool connected = true;
        public BaseRobot(string address, string user, string password, Map m, Object k)
            : base(address, user, password)
        {
            map = m;
            keys = (List<int>)k;
            Init();
        }

        public abstract void Start();


        protected void ImageGet()
        {
            while (running)
            {
                lock (commandLock)
                {
                    cameraImage = Camera.Image;
                    irSensor = IRSensor.Detection;
                }
            }
        }

        public abstract void KeyboardInput();

        public void KeyboardStart()
        {
            while (running)
            {
                lock (commandLock)
                {
                    if (keys.Count == 1)
                        KeyboardInput();
                }
            }
        }

        public void Init()
        {
            try { API.Movement.GetLibNSVersion(); } // a dummy request
            catch (Exception)
            {
                //simple way of getting feedback in the form mode
                System.Windows.Forms.MessageBox.Show("Could not connect to the robot");
                cameraDimensions = new System.Drawing.Point(352, 288);
                connected = false;
                //cameraImage = new Bitmap("example.jpg");
                Environment.Exit(0);
            }

            
            cameraDimensions = System.Drawing.Point.Empty;

            lock (commandLock)
            {
                Camera.Resolution = Rovio.API.Camera.ImageResolution.CIF;

                if (Camera.Resolution == Rovio.API.Camera.ImageResolution.CIF)
                    cameraDimensions = new System.Drawing.Point(352, 288);
                else if (Camera.Resolution == Rovio.API.Camera.ImageResolution.QCIF)
                    cameraDimensions = new System.Drawing.Point(176, 114);
                else if (Camera.Resolution == Rovio.API.Camera.ImageResolution.CGA)
                    cameraDimensions = new System.Drawing.Point(320, 240);
                else
                    cameraDimensions = new System.Drawing.Point(640, 480);
            }
            
            System.Threading.Thread keyboard = new System.Threading.Thread(KeyboardStart);
            keyboard.Start();
            cameraImageThread = new System.Threading.Thread(ImageGet);
            cameraImageThread.Start();
        }

        //public delegate void ImageReady(Image image);
       // public event ImageReady SourceImage;

        protected bool completed = false;

        public void KillThreads()
        {
            running = false;
            /*
            lock (commandLock)
            {
                actions.Abort();
                search.Abort();
                source.Abort();
                move.Abort();
            }*/
        }

        ////////////////////////////////////////////////////
        ///////////////////////Movement/////////////////////
        ////////////////////////////////////////////////////

        protected void MoveForward(int iterations, int speed)
        {
            lock (commandLock)
            {
                for (int i = 0; i < iterations; i++)
                {
                    if (speed < 0)
                        Drive.Backward(Math.Abs(speed));
                    else
                        Drive.Forward(speed);
                }
            }
            //Drive.Stop();
        }

        protected void Strafe(int iterations, int speed)
        {
            lock (commandLock)
            {
                for (int i = 0; i < iterations; i++)
                {
                    if (speed < 0)
                        Drive.DiagForwardLeft(Math.Abs(speed));
                    else
                        Drive.DiagBackwardRight(speed);

                    //cameraImage = Camera.Image;
                }
                Drive.Stop();
            }
        }

        protected void RotateDirection(int iterations, int speed)
        {
            lock (commandLock)
            {
                for (int i = 0; i < iterations; i++)
                {
                    if (speed < 0)
                        Drive.RotateLeft(Math.Abs(speed));
                    else
                        Drive.RotateRight(speed);

                    //cameraImage = Camera.Image;
                }
                Drive.Stop();
            }
        }

        protected void Rotate90(int iterations=1, int speed=3)
        {
            bool pos = true;
            if (iterations < 0)
                pos = false;
            double angle = 0;
            if (Math.Abs(iterations) == 1)
            {
                iterations = 1;
                angle = 22;
            }
            else if (Math.Abs(iterations) == 2)
            {
                iterations = 3;
                angle = 45;
            }
            else if (Math.Abs(iterations) == 3)
            {
                iterations = 7;
                angle = 90;
            }
            else if (Math.Abs(iterations) == 4)
            {
                iterations = 15;
                angle = 180;
            }
            else if (Math.Abs(iterations) == 5)
            {
                iterations = 30;
                angle = 360;
            }

            int direction = 18;
            if (!pos)
            {
                direction = 17;
                angle = -angle;
            }

            lock(commandLock)
                Request("rev.cgi?Cmd=nav&action=18&drive=" + direction.ToString() + "&speed=" + speed.ToString() + "&angle=" + iterations.ToString());

            cumulativeAngle += angle;

            if (cumulativeAngle > 360)
                cumulativeAngle -= 360;
            else if (cumulativeAngle < 0)
                cumulativeAngle += 360;
                //cameraImage = Camera.Image;
            
        }
  



        protected void GetFormInformation()
        {
            batteryStatus = API.Movement.Report.BatteryLevel;
            chargingStatus = API.Movement.Report.Charging;
            irSensor = IRSensor.Detection;        
        }

        public void SetIndividualFilters(Dictionary<string, float> dict, ref AForge.Imaging.Filters.HSLFiltering filter, string colour)
        {
            filter = new AForge.Imaging.Filters.HSLFiltering();
            float left = 0f;
            float right = 0f;

            dict.TryGetValue(colour + "HueMin", out left);
            dict.TryGetValue(colour + "HueMax", out right);
            filter.Hue = new AForge.IntRange((int)left, (int)right);
            dict.TryGetValue(colour + "SatMin", out left);
            dict.TryGetValue(colour + "SatMax", out right);
            filter.Saturation = new AForge.Range(left, right);
            dict.TryGetValue(colour + "LumMin", out left);
            dict.TryGetValue(colour + "LumMax", out right);
            filter.Luminance = new AForge.Range(left, right);
        }


        ////////////////////////////////////////////////////
        //////////////////Image processing//////////////////
        ////////////////////////////////////////////////////

        // Takes in an image and rewrites it to 24bpp RGB (e.g. for converting binary image to a compatible pixel format for further work).
        protected Bitmap ConvertImageFormat(Bitmap bmp)
        {
            Bitmap outImage = new Bitmap((int)cameraDimensions.X, (int)cameraDimensions.Y, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(outImage))
                g.DrawImage(bmp, new System.Drawing.Point(0, 0));

            return outImage;
        }

        // Takes in black and white binary image and changes white to colour.
        protected Bitmap ApplyColour(Bitmap overBmp, Bitmap underBmp, System.Drawing.Color colour)
        {
            Bitmap outImage = ConvertImageFormat(overBmp);
            AForge.Imaging.Filters.EuclideanColorFiltering filter = new AForge.Imaging.Filters.EuclideanColorFiltering();
            filter.CenterColor = new AForge.Imaging.RGB(System.Drawing.Color.White); 
            filter.Radius = 0;
            filter.FillColor = new AForge.Imaging.RGB(colour);
            filter.FillOutside = false;
            filter.ApplyInPlace(outImage);
            return outImage;
        }

        // Change input to greyscale.
        protected Bitmap Greyscale(Bitmap image)
        {
            return AForge.Imaging.Filters.Grayscale.CommonAlgorithms.Y.Apply(image);
        }

        // Change input to a binary image.
        protected Bitmap Threshold(Bitmap image)
        {
            AForge.Imaging.Filters.Threshold filter = new AForge.Imaging.Filters.Threshold(1);
            return filter.Apply(AForge.Imaging.Filters.Grayscale.CommonAlgorithms.Y.Apply(image));
        }

        protected System.Drawing.Rectangle[] DetectObstacle(AForge.Imaging.Filters.HSLFiltering filter, Bitmap image, System.Drawing.Point minRect, ref Bitmap colourBitmap, System.Drawing.Point maxSize = default(System.Drawing.Point))
        {
            Bitmap filtered;

                filtered = filter.Apply(image);

            //short[,] structuringElement = new short[,] { { 0, 1, 0 }, { 1, 1, 1 }, { 0, 1, 0 } };

            filtered = Threshold(filtered);
            //AForge.Imaging.Filters.Opening openingFilter = new AForge.Imaging.Filters.Opening(structuringElement);
            //filtered = openingFilter.Apply(filtered);
            colourBitmap = filtered;

            AForge.Imaging.BlobCounter blobs = new AForge.Imaging.BlobCounter();
            blobs.MinWidth = (int)minRect.X;
            blobs.MinHeight = (int)minRect.Y;
            if (!maxSize.IsEmpty)
            {
                blobs.MaxWidth = maxSize.X;
                blobs.MaxHeight = maxSize.Y;
            }
            blobs.FilterBlobs = true;
            blobs.ObjectsOrder = AForge.Imaging.ObjectsOrder.Size;
            blobs.ProcessImage(filtered);

            System.Drawing.Rectangle[] rectangles = blobs.GetObjectsRectangles();

            return rectangles;

            //return new System.Drawing.Rectangle[] {new System.Drawing.System.Drawing.Rectangle(0, 0, 0, 0)};
        }

        // Draw a rectangle on the input image.
        protected Bitmap DrawRect(Bitmap image, System.Drawing.Rectangle rect, System.Drawing.Color colour, float lineWeight)
        {
            Graphics rectPen = Graphics.FromImage(image);
            rectPen.DrawRectangle(new Pen(colour, lineWeight), rect);
            return image;
        }

        // Draw a polygon with points from input array (e.g. for an irregular quadrilateral instead of a rectangle).
        protected Bitmap DrawLineFromPoints(Bitmap image, System.Drawing.Point[] pointArr, System.Drawing.Color colour, float lineWeight)
        {
            Graphics rectPen = Graphics.FromImage(image);
            //rectPen.FillPolygon(new SolidBrush(colour), pointArr, System.Drawing.Drawing2D.FillMode.Winding);
            rectPen.DrawPolygon(new Pen(colour, 3.0f), pointArr);
            return image;
        }

        public bool MYTESTBOOL = false;
        // Old
        /*
        public System.Drawing.Rectangle DetectPrey(Bitmap image, Vector2 minRect)
        {
            AForge.Imaging.Filters.HSLFiltering filter = new AForge.Imaging.Filters.HSLFiltering();
            filter.Hue = new AForge.IntRange(355, 20);
            filter.Saturation = new AForge.Range(0.5f, 1.8f);
            filter.Luminance = new AForge.Range(0.15f, 1.0f);
            Bitmap filtered = filter.Apply(image);

            short[,] structuringElement = new short[,] { { 0, 1, 0 }, { 1, 1, 1 }, { 0, 1, 0 } };
            filtered = Threshold(filtered, 1);
            AForge.Imaging.Filters.Opening openingFilter = new AForge.Imaging.Filters.Opening(structuringElement);
            filtered = openingFilter.Apply(filtered);

            AForge.Imaging.BlobCounter blobs = new AForge.Imaging.BlobCounter(filtered);
            System.Drawing.Rectangle[] rectangles = blobs.GetObjectsRectangles();

            int size = 0;
            int chosen = 0;

            for (int i = 0; i < rectangles.Length; i++)
            {
                int newSize = rectangles[i].Height * rectangles[i].Width;

                if (size < newSize)
                {
                    chosen = i;
                    size = newSize;
                }
            }
            Graphics rect = Graphics.FromImage(image);

            if (rectangles.Length != 0)
            {
                if (rectangles[chosen].Width > minRect.X && rectangles[chosen].Height > minRect.Y)
                {
                    trackingState = Tracking.OnScreen;
                    Hunt(rectangles[chosen]);
                    return rectangles[chosen];
                }
            }
            return new System.Drawing.System.Drawing.Rectangle(0, 0, 0, 0);
        }

       


        public Bitmap DetectObject(Bitmap image)
        {            
            AForge.Imaging.Filters.HSLFiltering filter = new AForge.Imaging.Filters.HSLFiltering();
            filter.Hue = new AForge.IntRange(355, 20);
            filter.Saturation = new AForge.Range(0.5f, 1.8f);
            filter.Luminance = new AForge.Range(0.15f, 1.0f);
            Bitmap filtered = filter.Apply(image);

            
            short[,] se = new short[,] {{0, 1, 0}, {1, 1, 1}, {0, 1, 0}};
            filtered = Threshold(filtered, 1);
            AForge.Imaging.Filters.Opening ffFilter = new AForge.Imaging.Filters.Opening(se);
            filtered = ffFilter.Apply(filtered);


            AForge.Imaging.BlobCounter blobs = new AForge.Imaging.BlobCounter(filtered);
            System.Drawing.Rectangle[] rectangles = blobs.GetObjectsRectangles();

            int size = 0;
            int chosen = 0;

            for (int i = 0; i < rectangles.Length; i++)
            {
                int newSize = rectangles[i].Height * rectangles[i].Width;

                if (size < newSize)
                {
                    chosen = i;
                    size = newSize;
                }
            }

           //return filtered;
            Graphics rect = Graphics.FromImage(image);

           

            if (rectangles.Length != 0)
            {
                if (rectangles[chosen].Width > 15 && rectangles[chosen].Height > 10)
                {
                    preyScreenPosition = new System.Drawing.System.Drawing.Rectangle(rectangles[chosen].X, rectangles[chosen].Y, rectangles[chosen].Width, rectangles[chosen].Height);
                    trackingState = Tracking.OnScreen;
                    rect.DrawSystem.Drawing.Rectangle(new Pen(System.Drawing.Color.Green, 3f), rectangles[chosen]);
                    totalTime = 0;
                    Hunt(rectangles[chosen]);
                }
                else if (checkingOnScreenTimer < 5)
                {
                    checkingOnScreenTimer++;
                }                
            }
            else if (rectangles.Length == 0)
            {
                if (roamingRotationCount < 20)
                    trackingState = Tracking.Searching;
                else
                    trackingState = Tracking.Roaming;
            }

            if (trackingState == Tracking.Searching)
            {
                roamingRotationCount++;
                for (int i = 0; i < 2; i++)
                    if (preyScreenPosition.X < cameraWidth.X/2)
                        Drive.RotateLeft(3);
                    else
                        Drive.RotateRight(3);
                Drive.Stop();

                Console.WriteLine(circleCount);
            }
            return image;
        }

        
         * 
         * */

    }
}
