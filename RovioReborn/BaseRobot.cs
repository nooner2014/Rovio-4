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

        protected System.Threading.Thread threadCameraImage;
        public string direction = "Unknown";
        protected Map map;
        public double cumulativeAngle = 0;
        protected List<int> keys = new List<int>();
        public Object mapLock = new Object();

        public delegate void ImageReady(Image image);


        protected System.Threading.Thread threadKeyboardInput;
        protected bool connected = true;

        /// <summary>
        /// Constructor - initialises variables for derived classes.
        /// </summary>
        /// <param name="address">URL of robot.</param>
        /// <param name="user">Username for robot.</param>
        /// <param name="password">Password for robot.</param>
        /// <param name="matrix">Map to be used (null value is acceptable).</param>
        /// <param name="k">Keyboard dictionary.</param>
        public BaseRobot(string address, string user, string password, Map m, Object k)
            : base(address, user, password)
        {
            map = m;
            keys = (List<int>)k;

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

            threadKeyboardInput = new System.Threading.Thread(KeyboardStart);
            threadKeyboardInput.Start();
            threadCameraImage = new System.Threading.Thread(ImageGet);
            threadCameraImage.Start();
        }

        /// <summary>
        /// Method to be implemented by the derived class.
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Linearly interpolate between two positions.
        /// </summary>
        /// <param name="oldPos">Original position.</param>
        /// <param name="newPos">New position.</param>
        /// <param name="multiplier">Weighting between the two positions (eg. 0.25 is a 75% weighting to the new position).</param>
        /// <returns>New position with linear interpolation between input positions.</returns>
        protected Point Lerp(Point oldPos, Point newPos, float multiplier)
        {
            Microsoft.Xna.Framework.Vector2 oldP = new Microsoft.Xna.Framework.Vector2(oldPos.X, oldPos.Y);
            Microsoft.Xna.Framework.Vector2 newP = new Microsoft.Xna.Framework.Vector2(newPos.X, newPos.Y);
            Microsoft.Xna.Framework.Vector2 brandNew = Microsoft.Xna.Framework.Vector2.Lerp(oldP, newP, multiplier);
            return new Point((int)brandNew.X, (int)brandNew.Y);
        }

        /// <summary>
        /// Updates the camera image.
        /// </summary>
        protected void ImageGet()
        {
            while (running)
                lock (commandLock)
                    cameraImage = Camera.Image;
        }

        /// <summary>
        /// Keyboard input to be overridden in derived classes.
        /// </summary>
        protected abstract void KeyboardInput();

        /// <summary>
        /// Enters the overridden keyboard input method when a key is pressed.
        /// </summary>
        protected void KeyboardStart()
        {
            while (running)
                lock (commandLock)
                    if (keys.Count == 1)
                        KeyboardInput();
        }

        /// <summary>
        /// Find the modal value within a list (returns newP occurance in the case of equal values).
        /// </summary>
        /// <typeparam name="T">Type of elements in the list.</typeparam>
        /// <param name="list">List of elements within which to find the modal value</param>
        /// <returns>The modal value of the list.</returns>
        private T Mode<T>(List<T> list)
        {
            return list.GroupBy(x => x)
                .Select(g => new { Value = g.Key, Count = g.Count() })
                .OrderByDescending(a => a.Count).First().Value;
        }

        //public delegate void ImageReady(Image image);
       // public event ImageReady SourceImage;

        
        /// <summary>
        /// Begin the end of threads by setting the condition of all threads to false.
        /// </summary>
        public void KillThreads()
        {
            running = false;
        }

        /// <summary>
        /// Move Rovio forwards and backwards.
        /// </summary>
        /// <param name="iterations">Number of times to loop through.</param>
        /// <param name="speed">Movement speed (1-10 highest to lowest). Negative value to move backwards.</param>
        protected void MoveForward(int iterations, int speed)
        {
            lock (commandLock)
                for (int i = 0; i < iterations; i++)
                    if (speed < 0)
                        Drive.Backward(Math.Abs(speed));
                    else
                        Drive.Forward(speed);
        }

        /// <summary>
        /// Strafe left or right.
        /// </summary>
        /// <param name="iterations">Number of times to loop through.</param>
        /// <param name="speed">Strafing speed (1-10 highest to lowest). Negative moves left, positive moves right.</param>
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
                //Drive.Stop();
            }
        }

        /// <summary>
        /// Rotate the Rovio.
        /// </summary>
        /// <param name="iterations">Number of times to loop through.</param>
        /// <param name="speed">Rotation speed (1-10 highest to lowest). Negative rotates counter-clockwise.</param>
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
                }
                Drive.Stop();
            }
        }

        /// <summary>
        /// Rotate using the 'angle' web API command.
        /// </summary>
        /// <param name="iterations">
        /// 1 = 15  degrees. 
        /// 2 = 45  degrees. 
        /// 3 = 90  degrees.
        /// 4 = 180 degrees.
        /// 5 = 360 degrees.</param>
        /// <param name="speed">Speed of rotation (1-10 highest to lowest). Negative rotates counter-clockwise.</param>
        protected void RotateByAngle(int iterations=1, int speed=3)
        {
            
            bool pos = true;
            if (iterations < 0)
                pos = false;
            if (Math.Abs(iterations) == 1)
                iterations = 1;
            else if (Math.Abs(iterations) == 2)
                iterations = 3;
            else if (Math.Abs(iterations) == 3)
                iterations = 7;
            else if (Math.Abs(iterations) == 4)
                iterations = 15;
            else if (Math.Abs(iterations) == 5)
                iterations = 30;

            int direction = 18;
            if (!pos)
                direction = 17;

            lock(commandLock)
                Request("rev.cgi?Cmd=nav&action=18&drive=" + direction.ToString() + "&speed=" + speed.ToString() + "&angle=" + iterations.ToString());
        }

        /// <summary>
        /// Get extra information that may be useful to display on the form.
        /// </summary>
        protected void GetFormInformation()
        {
            batteryStatus = API.Movement.Report.BatteryLevel;
            chargingStatus = API.Movement.Report.Charging;
            irSensor = IRSensor.Detection;        
        }

        /// <summary>
        /// Pass in a filter to set up.
        /// </summary>
        /// <param name="dict">Dictionary of filter values.</param>
        /// <param name="filter">Filter to be set.</param>
        /// <param name="colour">Name of the colour filter being modified.</param>
        public void SetIndividualFilters(Dictionary<string, float> dict, out AForge.Imaging.Filters.HSLFiltering filter, string colour)
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

        /// <summary>
        /// Takes in an image and rewrites it to 24bpp RGB (e.graphics. for converting binary image to a compatible pixel format for further work).
        /// </summary>
        /// <param name="bmp">Bitmap to be converted.</param>
        /// <returns>24bpp RGB bitmap, visually identical to input Bitmap.</returns>
        protected Bitmap ConvertImageFormat(Bitmap bmp)
        {
            Bitmap outImage = new Bitmap((int)cameraDimensions.X, (int)cameraDimensions.Y, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(outImage))
                g.DrawImage(bmp, new System.Drawing.Point(0, 0));

            return outImage;
        }

        /// <summary>
        /// Takes in black and white binary image and changes white to colour.
        /// </summary>
        /// <param name="overBmp">Top binary image (to be coloured).</param>
        /// <param name="underBmp">Bottom image (to be overlayed).</param>
        /// <param name="colour">Colour to change the white of the binary image to.</param>
        /// <returns>Output image with the white of the original binary image overlayed and coloured.</returns>
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

        /// <summary>
        /// Change input to greyscale.
        /// </summary>
        /// <param name="image">Image to convert to greyscale.</param>
        /// <returns>Converted greyscale image.</returns>
        protected Bitmap Greyscale(Bitmap image)
        {
            return AForge.Imaging.Filters.Grayscale.CommonAlgorithms.Y.Apply(image);
        }

        /// <summary>
        /// Change input to a binary image.
        /// </summary>
        /// <param name="image">Image to convert to binary.</param>
        /// <returns>Converted binary image (white and black).</returns>
        protected Bitmap BinaryImage(Bitmap image)
        {
            AForge.Imaging.Filters.Threshold filter = new AForge.Imaging.Filters.Threshold(1);
            return filter.Apply(AForge.Imaging.Filters.Grayscale.CommonAlgorithms.Y.Apply(image));
        }

        /// <summary>
        /// Look at the input image and find a suitable blob.
        /// </summary>
        /// <param name="filter">HSL filter to use for image segmentation.</param>
        /// <param name="image">Input image to segment.</param>
        /// <param name="minRect">Minimum size of output blob.</param>
        /// <param name="colourBitmap">Bitmap to write binary image to.</param>
        /// <param name="maxSize">Maximum size of output blob (default value will allow any size).</param>
        /// <returns>A rectangle array of all suitable blobs, or an empty array if no suitable blobs are found.</returns>
        protected System.Drawing.Rectangle[] DetectObstacle(AForge.Imaging.Filters.HSLFiltering filter, Bitmap image, System.Drawing.Point minRect, out Bitmap colourBitmap, System.Drawing.Point maxSize = default(System.Drawing.Point))
        {
            Bitmap filtered;

                filtered = filter.Apply(image);

            //short[,] structuringElement = new short[,] { { 0, 1, 0 }, { 1, 1, 1 }, { 0, 1, 0 } };

            filtered = BinaryImage(filtered);
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

        /// <summary>
        /// Draw a rectangle on the input image.
        /// </summary>
        /// <param name="image">Bitmap to be drawn on.</param>
        /// <param name="rect">Rectangle to be drawn.</param>
        /// <param name="colour">Colour to draw rectacle.</param>
        /// <param name="lineWeight">Weight of rectangle lines.</param>
        /// <returns>Bitmap of input bitmap overlayed by rectangle.</returns>
        protected Bitmap DrawRect(Bitmap image, System.Drawing.Rectangle rect, System.Drawing.Color colour, float lineWeight)
        {
            Graphics rectPen = Graphics.FromImage(image);
            rectPen.DrawRectangle(new Pen(colour, lineWeight), rect);
            return image;
        }

        /// <summary>
        /// Draw a polygon with points from input array (e.graphics. for an irregular quadrilateral instead of a rectangle).
        /// </summary>
        /// <param name="image">Bitmap image to be drawn on.</param>
        /// <param name="pointArr">Array of points to be drawn to (in order of connections).</param>
        /// <param name="colour">Colour to draw lines.</param>
        /// <param name="lineWeight">Weight of lines to be drawn.</param>
        /// <returns>Bitmap of input bitmap with point array lines drawn over.</returns>
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
            filtered = BinaryImage(filtered, 1);
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
            filtered = BinaryImage(filtered, 1);
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
