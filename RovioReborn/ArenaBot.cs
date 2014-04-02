//#define TESTING
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Microsoft.Xna.Framework;
using System.Timers;
using System.Diagnostics;
using System.Windows.Forms;
using PredatorPreyAssignment;
using Xna = Microsoft.Xna.Framework;

namespace Rovio
{
    abstract class BaseArena : BaseRobot
    {
        // Filters
        AForge.Imaging.Filters.HSLFiltering greenFilter;
        AForge.Imaging.Filters.HSLFiltering redFilter;
        AForge.Imaging.Filters.HSLFiltering blueFilter;
        AForge.Imaging.Filters.HSLFiltering yellowFilter;
        AForge.Imaging.Filters.HSLFiltering whiteFilter;


        // Rectangles to draw on screen.
        protected System.Drawing.Rectangle preyScreenPosition = System.Drawing.Rectangle.Empty;
        public System.Drawing.Rectangle obstacleRectangle = System.Drawing.Rectangle.Empty;
        public System.Drawing.Rectangle preyRectangle = System.Drawing.Rectangle.Empty;
        protected System.Drawing.Rectangle blueLineRectangle = System.Drawing.Rectangle.Empty;
        protected System.Drawing.Rectangle secondBlueLineRectangle = System.Drawing.Rectangle.Empty;
        protected System.Drawing.Rectangle yellowWallRectangleTop = System.Drawing.Rectangle.Empty;
        protected System.Drawing.Rectangle yellowWallRectangleBottom = System.Drawing.Rectangle.Empty;
        protected System.Drawing.Rectangle whiteWallRectangleTop = System.Drawing.Rectangle.Empty;
        protected System.Drawing.Rectangle whiteWallRectangleBottom = System.Drawing.Rectangle.Empty;


        // Segmented bitmaps.
        protected Bitmap redColourBitmap;
        protected Bitmap greenColourBitmap;
        protected Bitmap blueColourBitmap;
        protected Bitmap yellowColourBitmap;
        protected Bitmap whiteColourBitmap;


        // Line points.
        protected System.Drawing.Point wallLeftPoint = System.Drawing.Point.Empty;
        protected System.Drawing.Point wallRightPoint = System.Drawing.Point.Empty;
        protected System.Drawing.Point[] blueLinePoints = new System.Drawing.Point[4];
        protected System.Drawing.Point[] secondBlueLinePoints = new System.Drawing.Point[4];
        protected System.Drawing.Point[] yellowTopWallPoints = new System.Drawing.Point[4];
        protected System.Drawing.Point[] yellowBottomWallPoints = new System.Drawing.Point[4];
        protected System.Drawing.Point[] whiteTopWallPoints = new System.Drawing.Point[4];
        protected System.Drawing.Point[] whiteBottomWallPoints = new System.Drawing.Point[4];

        // Movement affecting variables.
        protected int wallLineHeight = 0;
        protected int searchingRotationCount = 8;
        protected Bitmap outputImage;
        protected List<char> wallDirectionList = new List<char>();
        protected List<float> wallHeightList = new List<float>();
        

        // Variables for map.
        private bool preySeen = false;
        private float preyDistance = 0;
        private bool obstacleSeen = false;
        private float obstacleDistance = 0;
        private float blueLineThickness = 0;
        private double northDist = double.PositiveInfinity;
        private double westDist = double.PositiveInfinity;
        private double southDist = double.PositiveInfinity;
        private double eastDist = double.PositiveInfinity;
        protected double wallDistance = double.PositiveInfinity;
        protected char lastReadDirection;

        // 'Get' functions for the map.
        public bool IsPreySeen() { return preySeen; }
        public bool IsObstacleSeen() { return obstacleSeen; }
        public float GetPreyDistance() { return preyDistance; }
        public float GetObstacleDistance() { return obstacleDistance; }
        public double GetNorthDist() { return northDist; }
        public double GetWestDist() { return westDist; }
        public double GetSouthDist() { return southDist; }
        public double GetEastDist() { return eastDist; }
        public double GetWallDist() { return wallDistance; }
        protected int outputImageKey = 0;


        public event ImageReady SourceImage;

#if TESTING
        Stopwatch stopwatchImageProcessing = new Stopwatch();
#endif


        public BaseArena(string address, string user, string password, Map m, Object k)
            : base(address, user, password, m, k)
        { }
        
        /// <summary>
        /// Override base class' keyboard input method.
        /// </summary>
        protected override void KeyboardInput()
        {
            try
            {
                outputImageKey = keys[0] - 48;
            }
            catch { }
        }

        /// <summary>
        /// Called from form. Set filters based on passed in dictionary values.
        /// </summary>
        /// <param name="values">Dictionary passed as object</param>
        public void SetFilters(object values)
        {
            Dictionary<string, float> dict = (Dictionary<string, float>)values;

            SetIndividualFilters(dict, out greenFilter, "green");
            SetIndividualFilters(dict, out redFilter, "red");
            SetIndividualFilters(dict, out blueFilter, "blue");
            SetIndividualFilters(dict, out yellowFilter, "yellow");
            SetIndividualFilters(dict, out whiteFilter, "white");
        }

        /// <summary>
        /// Takes in multiple readings from the wall and returns the mean to eliminate error.
        /// </summary>
        /// <param name="final">Average of all gathered distances</param>
        /// <param name="moveAfter">Whether to rotate 90 degrees after gathering data.</param>
        protected void FindWallDistance(ref double final, bool moveAfter)
        {
            while (running)
            {
                List<double> arr = new List<double>();
                Stopwatch s = new Stopwatch();
                s.Start();

                double dist = 0;

                // Take an average reading across a second.
                while (s.ElapsedMilliseconds < 1000)
                {
                    if (blueLineRectangle.Width > 30 || secondBlueLineRectangle.Width > 30)
                    {
                        dist = 6.5 / blueLineThickness;

                        if (dist != 0 && !double.IsInfinity(dist))
                            arr.Add(Math.Abs(dist));

                        if (dist > 3)
                            arr.Remove(dist);
                        System.Threading.Thread.Sleep(30);
                    }
                }

                // If there is no valid value, default to 5m.
                if (arr.Count != 0)
                {
                    if (double.IsNaN(final))
                        final = 5;
                    final = MathHelper.Lerp((float)final, (float)arr.Average(), 0.3f);
                }
                else
                    final = 5;
#if TESTING
                Console.WriteLine("Distance from wall: " + final);
#endif
                if (!running)
                    return;
                else if (moveAfter)
                    RotateByAngle(3, 3);
            }
        }

        /// <summary>
        /// Uses rectangle information to determine faced direction, and use data to assume the heading angle relative to north.
        /// </summary>
        public void FindHeading()
        {
            while (running)
            {
                // Calculate the direction based on on-screen rectangles.'
                if (whiteWallRectangleTop == System.Drawing.Rectangle.Empty && whiteWallRectangleBottom == System.Drawing.Rectangle.Empty)
                {
                    if (yellowWallRectangleBottom == System.Drawing.Rectangle.Empty)
                    {
                        if (yellowWallRectangleTop.X > 10)
                            direction = "NorthWest";
                        else
                            direction = "NorthEast";
                    }
                    else
                    {
                        if (yellowWallRectangleTop.X > 10)
                        {
                            direction = "SouthEast";
                        }
                        else
                            direction = "SouthWest";
                    }
                }
                if (yellowWallRectangleBottom == System.Drawing.Rectangle.Empty && whiteWallRectangleBottom != System.Drawing.Rectangle.Empty)
                {
                    if (yellowWallRectangleTop.X < whiteWallRectangleBottom.X)
                        direction = "NorthEast";
                    else
                        direction = "NorthWest";
                }
                else if (yellowWallRectangleTop != System.Drawing.Rectangle.Empty && yellowWallRectangleBottom != System.Drawing.Rectangle.Empty)
                {
                    if (whiteWallRectangleBottom.X < yellowWallRectangleTop.X)
                        direction = "SouthEast";
                    else
                        direction = "SouthWest";
                }

                char[] which = new char[50];
                Xna.Vector2[] pos = new Vector2[50];
                for (int i = 0; i < 352 / 7; i++)
                {
                    //      N
                    //      -
                    //      Y
                    // W -X + X+ E
                    //      Y
                    //      +
                    //      S

                    // Use the data collected from the direction to find the angle.
                    if (direction == "NorthEast")
                    {
                        if (yellowWallRectangleTop.Right > (i * 7))
                        {
                            which[i] = 'N';
                            pos[i] = Vector2.Transform(new Vector2(0, -1), Matrix.CreateRotationZ(MathHelper.ToRadians(i - 25)));
                        }
                        else
                        {
                            which[i] = 'E';
                            pos[i] = Vector2.Transform(new Vector2(-1, 0), Matrix.CreateRotationZ(MathHelper.ToRadians(i - 25)));

                        }
                    }
                    else if (direction == "NorthWest")
                    {
                        if (yellowWallRectangleTop.X < (i * 7))
                        {
                            which[i] = 'N';
                            pos[i] = Vector2.Transform(new Vector2(0, -1), Matrix.CreateRotationZ(MathHelper.ToRadians(i - 25)));
                        }
                        else
                        {
                            which[i] = 'W';
                            pos[i] = Vector2.Transform(new Vector2(1, 0), Matrix.CreateRotationZ(MathHelper.ToRadians(i - 25)));

                        }
                    }
                    else if (direction == "SouthEast")
                    {
                        if (yellowWallRectangleTop.X < (i * 7))
                        {
                            which[i] = 'S';
                            pos[i] = Vector2.Transform(new Vector2(0, 1), Matrix.CreateRotationZ(MathHelper.ToRadians(i - 25)));
                        }
                        else
                        {
                            which[i] = 'E';
                            pos[i] = Vector2.Transform(new Vector2(-1, 0), Matrix.CreateRotationZ(MathHelper.ToRadians(i - 25)));

                        }
                    }
                    else if (direction == "SouthWest")
                    {
                        if (yellowWallRectangleTop.Right > (i * 7))
                        {
                            which[i] = 'S';
                            pos[i] = Vector2.Transform(new Vector2(0, 1), Matrix.CreateRotationZ(MathHelper.ToRadians(i - 25)));
                        }
                        else
                        {
                            which[i] = 'W';
                            pos[i] = Vector2.Transform(new Vector2(1, 0), Matrix.CreateRotationZ(MathHelper.ToRadians(i - 25)));

                        }
                    }
                }

                Vector2 totalVec = Vector2.Zero;
                for (int i = 0; i < 50; i++)
                {
                    totalVec += pos[i];
                }

                totalVec.Normalize();
                //totalVec = new Vector2(0.33f, 0.68f);
                float radAngle = -(float)Math.Atan2(totalVec.X, -totalVec.Y);
                float degAngle = MathHelper.ToDegrees(radAngle);

                if (degAngle < 0)
                    degAngle += 360;
                cumulativeAngle = MathHelper.Lerp(cumulativeAngle, degAngle, 0.5f);// Lerp(new System.Drawing.Point((int)cumulativeAngle), new System.Drawing.Point((int)degAngle), 0.1f).X;
            }
        }

        /// <summary>
        /// Calls various methods to segment images, evaluate their content, and judge findings based on gathered information.
        /// </summary>
        protected void SearchImage()
        {
            while (running)
            {
#if TESTING
                stopwatchImageProcessing.Start();
#endif
                outputImage = cameraImage;

                // Only processes if the image is new.
                if (outputImage != lastProcessedImage)
                {
                    lastProcessedImage = outputImage;
                    FindRectangles();
                    MergeImages();
                    DrawRectangles();

                    // Get the distance based on height in pixels at 1m, and current height.
                    preyDistance = (float)25 / (float)preyRectangle.Height;
                    obstacleDistance = (float)130 / (float)obstacleRectangle.Height;

                    if (outputImage != null)
                    {
                        switch (outputImageKey)
                        {
                            case 1: outputImage = redColourBitmap;
                                break;
                            case 2: outputImage = greenColourBitmap;
                                break;
                            case 3: outputImage = blueColourBitmap;
                                break;
                            case 4: outputImage = yellowColourBitmap;
                                break;
                            case 5: outputImage = whiteColourBitmap;
                                break;
                        }
                    }
                    SourceImage(outputImage);
#if TESTING
                    Console.WriteLine("Image processing time: " + stopwatchImageProcessing.ElapsedMilliseconds.ToString());
                    stopwatchImageProcessing.Reset();
#endif
                }
            }
        }

        /// <summary>
        /// Called by SearchImage. Analyses blobs and 
        /// </summary>
        private void FindRectangles()
        {
            System.Drawing.Rectangle[] rectResult;

            // Pass filter and detected object from camera image. Referenced bitmap is set as binary image.
            // After image has been analysed, read through the outputImageKey array and sort.

            // Green block
            rectResult = DetectObstacle(greenFilter, outputImage, new System.Drawing.Point(35, 35), out greenColourBitmap);
            if (rectResult.Length > 0)
            {
                obstacleRectangle = rectResult[0];
                obstacleSeen = true;
            }
            else
            {
                obstacleRectangle = new System.Drawing.Rectangle(0, 0, 0, 0);
                obstacleSeen = false;
            }

            // Red block (prey)
            rectResult = DetectObstacle(redFilter, outputImage, new System.Drawing.Point(10, 10), out redColourBitmap);
            if (rectResult.Length > 0)
            {
                preyRectangle = rectResult[0];
                preySeen = true;
            }
            else
            {
                preyRectangle = new System.Drawing.Rectangle(0, 0, 0, 0);
                preySeen = false;
            }


            // Remove the newP eighty-five pixels of the camera image - only for wall processing since the top is unneeded.
            Bitmap wallImage = new Bitmap(outputImage);
            Graphics g = Graphics.FromImage(wallImage);
            g.FillRectangle(new SolidBrush(System.Drawing.Color.Black), new System.Drawing.Rectangle(0, 0, 352, 85));



            // Only need the blue line which intersects the centre to find threadFindWallDistance.
            int lowestY = 100;
            rectResult = DetectObstacle(blueFilter, wallImage, new System.Drawing.Point(60, 0), out blueColourBitmap, new System.Drawing.Point(cameraDimensions.X, 50));
            if (rectResult.Length > 0)
            {
                bool firstFound = false;
                bool secondFound = false;

                rectResult = rectResult.OrderBy(item => item.X).ToArray();
                for (int i = 0; i < rectResult.Length; i++)
                {
                    if (rectResult[i].X < cameraDimensions.X / 2 && rectResult[i].Bottom > 100)
                    {
                        if (!firstFound)
                        {
                            blueLineRectangle = rectResult[i];
                            firstFound = true;
                        }
                        else if (Math.Abs(blueLineRectangle.Right - rectResult[i].Left) < 50 && !secondFound)
                        {
                            secondBlueLineRectangle = rectResult[i];
                            secondFound = true;

                            // If both rectangles are detected, cut off the image at the highest rectangle to eliminate irrelevant data..
                            if (blueLineRectangle.Y > secondBlueLineRectangle.Y)
                                lowestY = secondBlueLineRectangle.Y;
                            else
                                lowestY = blueLineRectangle.Y;
                        }

                        if (!secondFound)
                            secondBlueLineRectangle = new System.Drawing.Rectangle(0, 0, 0, 0);
                    }
                }
            }
            else
            {
                blueLineRectangle = new System.Drawing.Rectangle(0, 0, 0, 0);
                secondBlueLineRectangle = new System.Drawing.Rectangle(0, 0, 0, 0);
            }

            g = Graphics.FromImage(wallImage);
            g.FillRectangle(new SolidBrush(System.Drawing.Color.Black), new System.Drawing.Rectangle(0, 0, 352, lowestY));


            // North or south wall
            rectResult = DetectObstacle(yellowFilter, wallImage, new System.Drawing.Point(50, 0), out yellowColourBitmap);
            if (rectResult.Length > 1 && rectResult[1].Y < rectResult[0].Y)
            {
                System.Drawing.Rectangle temp = rectResult[1];
                rectResult[1] = rectResult[0];
                rectResult[0] = temp;
            }
            if (rectResult.Length > 0)
                yellowWallRectangleTop = rectResult[0];
            else
                yellowWallRectangleTop = new System.Drawing.Rectangle(0, 0, 0, 0);

            if (rectResult.Length > 1 && rectResult[1].Height > 5)
                yellowWallRectangleBottom = rectResult[1];
            else
                yellowWallRectangleBottom = new System.Drawing.Rectangle(0, 0, 0, 0);

            if (yellowWallRectangleTop != System.Drawing.Rectangle.Empty && 
                yellowWallRectangleBottom != System.Drawing.Rectangle.Empty &&
                Math.Abs(yellowWallRectangleTop.X - yellowWallRectangleBottom.X) > 20)
            {
                yellowWallRectangleTop = yellowWallRectangleBottom;
                yellowWallRectangleBottom = System.Drawing.Rectangle.Empty;
            }

            //West or east wall
            rectResult = DetectObstacle(whiteFilter, wallImage, new System.Drawing.Point(80, 0), out whiteColourBitmap);
            if (rectResult.Length > 1 && rectResult[1].Y < rectResult[0].Y)
            {
                System.Drawing.Rectangle temp = rectResult[1];
                rectResult[1] = rectResult[0];
                rectResult[0] = temp;
            }
            if (rectResult.Length > 0)
                whiteWallRectangleTop = rectResult[0];
            else
                whiteWallRectangleTop = new System.Drawing.Rectangle(0, 0, 0, 0);

            if (rectResult.Length > 1 && rectResult[1].Height > 5)
                whiteWallRectangleBottom = rectResult[1];
            else
                whiteWallRectangleBottom = new System.Drawing.Rectangle(0, 0, 0, 0);

            if (whiteWallRectangleBottom != System.Drawing.Rectangle.Empty && whiteWallRectangleBottom.Height < 15)
                whiteWallRectangleBottom = System.Drawing.Rectangle.Empty;

            // Check accuracy of the current results.
            if (yellowWallRectangleTop != System.Drawing.Rectangle.Empty && whiteWallRectangleTop != System.Drawing.Rectangle.Empty)
            {
                if (yellowWallRectangleTop.X < 10)
                {
                    whiteWallRectangleTop.X = yellowWallRectangleTop.Right;
                }
                else if (yellowWallRectangleTop.X - whiteWallRectangleTop.X > 0)
                {
                    whiteWallRectangleTop.X = 0;
                    whiteWallRectangleTop.Width = yellowWallRectangleTop.X;
                }
                whiteWallRectangleBottom = whiteWallRectangleTop;
                whiteWallRectangleTop = System.Drawing.Rectangle.Empty;
            }
        }

        /// <summary>
        /// Called by SearchImage. Merges images together for visual output to form and gathers edge points of rectangles.
        /// </summary>
        protected void MergeImages()
        {
            redColourBitmap = ConvertImageFormat(redColourBitmap);
            redColourBitmap = ApplyColour(redColourBitmap, outputImage, System.Drawing.Color.Red);

            greenColourBitmap = ConvertImageFormat(greenColourBitmap);
            greenColourBitmap = ApplyColour(greenColourBitmap, outputImage, System.Drawing.Color.LightGreen);

            blueColourBitmap = ConvertImageFormat(blueColourBitmap);
            blueColourBitmap = ApplyColour(blueColourBitmap, outputImage, System.Drawing.Color.LightBlue);

            whiteColourBitmap = ConvertImageFormat(whiteColourBitmap);
            whiteColourBitmap = ApplyColour(whiteColourBitmap, outputImage, System.Drawing.Color.FromArgb(100, System.Drawing.Color.White));

            yellowColourBitmap = ConvertImageFormat(yellowColourBitmap);
            yellowColourBitmap = ApplyColour(yellowColourBitmap, outputImage, System.Drawing.Color.FromArgb(100, System.Drawing.Color.Yellow));

            // Find the Y positions of the left, right, top, and bottom results to give the edges.
            yellowTopWallPoints = GetWallPoints(yellowColourBitmap, yellowWallRectangleTop, false);
            yellowBottomWallPoints = GetWallPoints(yellowColourBitmap, yellowWallRectangleBottom, false);

            whiteTopWallPoints = GetWallPoints(whiteColourBitmap, whiteWallRectangleTop, false);
            whiteBottomWallPoints = GetWallPoints(whiteColourBitmap, whiteWallRectangleBottom, false);

            blueLinePoints = GetWallPoints(blueColourBitmap, blueLineRectangle, false);
            secondBlueLinePoints = GetWallPoints(blueColourBitmap, secondBlueLineRectangle, false);


            FindBlueLineThickness();
            outputImage = Greyscale(outputImage);
            outputImage = ConvertImageFormat(outputImage);


            // Merge all images of single colour on black background together.
            Bitmap mergedColourImages = new Bitmap(1, 1);

            AForge.Imaging.Filters.Merge mFilter;

            mFilter = new AForge.Imaging.Filters.Merge(greenColourBitmap);
            mergedColourImages = mFilter.Apply(blueColourBitmap);

            mFilter = new AForge.Imaging.Filters.Merge(mergedColourImages);
            mergedColourImages = mFilter.Apply(redColourBitmap);

            mFilter = new AForge.Imaging.Filters.Merge(mergedColourImages);
            mergedColourImages = mFilter.Apply(yellowColourBitmap);

            mFilter = new AForge.Imaging.Filters.Merge(mergedColourImages);
            mergedColourImages = mFilter.Apply(whiteColourBitmap);


            // Set the merged colour image on top of greyscale camera image to see what is segmented.
            mFilter = new AForge.Imaging.Filters.Merge(mergedColourImages);
            outputImage = mFilter.Apply(outputImage);
        }

        /// <summary>
        /// Called by SearchImage. Draws all rectangles to output image.
        /// </summary>
        protected void DrawRectangles()
        {
            if (obstacleRectangle != new System.Drawing.Rectangle(0, 0, 0, 0))
                outputImage = DrawRect(outputImage, obstacleRectangle, System.Drawing.Color.DarkGreen, 3f);

            if (preyRectangle != new System.Drawing.Rectangle(0, 0, 0, 0))
            {
                searchingRotationCount = 0;
                outputImage = DrawRect(outputImage, preyRectangle, System.Drawing.Color.IndianRed, 3f);
            }
            System.Drawing.Point p = new System.Drawing.Point(0, 0);
            if (yellowWallRectangleBottom.Y + yellowWallRectangleBottom.Height < yellowWallRectangleTop.Y)
            {
                yellowWallRectangleBottom = new System.Drawing.Rectangle(0, 0, 0, 0);
                yellowBottomWallPoints = new System.Drawing.Point[] { p, p, p, p };
            }


            if (yellowTopWallPoints != null)
                outputImage = DrawLineFromPoints(outputImage, yellowTopWallPoints, System.Drawing.Color.DarkOrange, 3f);

            if (yellowBottomWallPoints != null)
                outputImage = DrawLineFromPoints(outputImage, yellowBottomWallPoints, System.Drawing.Color.DarkOrange, 3f);
            if (yellowWallRectangleTop.Width < 200)
            {
                if (whiteTopWallPoints != null)
                    outputImage = DrawLineFromPoints(outputImage, whiteTopWallPoints, System.Drawing.Color.Black, 3f);

                if (whiteBottomWallPoints != null)
                    outputImage = DrawLineFromPoints(outputImage, whiteBottomWallPoints, System.Drawing.Color.Black, 3f);
            }

            if (blueLinePoints != null)
                outputImage = DrawLineFromPoints(outputImage, blueLinePoints, System.Drawing.Color.DarkBlue, 3f);
            if (secondBlueLinePoints != null)
                outputImage = DrawLineFromPoints(outputImage, secondBlueLinePoints, System.Drawing.Color.DarkBlue, 15f);
        }

        /// <summary>
        /// Searches down from X position of rectangle to find actual height of segmented object in rectangle.
        /// </summary>
        /// <param name="bmp">Binary image on black background.</param>
        /// <param name="rect">Rectangle for search region.</param>
        /// <param name="minimumLimit">Make use of a hard minimum size limit of 5,5.</param>
        /// <returns>Actual corners rectangle.</returns>
        protected System.Drawing.Point[] GetWallPoints(Bitmap bmp, System.Drawing.Rectangle rect, bool minimumLimit)
        {
            System.Drawing.Point topRight = new System.Drawing.Point(0, 0);
            System.Drawing.Point topLeft = new System.Drawing.Point(0, 0);
            System.Drawing.Point bottomRight = new System.Drawing.Point(0, 0);
            System.Drawing.Point bottomLeft = new System.Drawing.Point(0, 0);
            for (int j = 0; j <= 2; j++)
            {
                int lowestPoint = 0;
                int highestPoint = 0;
                int x = 0;
                if (j == 0)
                    x = rect.X + 10;
                else if (j == 1)
                    x = (int)cameraDimensions.X / 2;
                else if (j == 2)
                    x = rect.X + rect.Width - 10;
                
                for (int i = rect.Y; i < rect.Y + rect.Height; i++)
                {
                    if (x >= cameraDimensions.X)
                        x = cameraDimensions.X-1;
                    if (i >= cameraDimensions.Y)
                        i = cameraDimensions.Y - 1;
                    System.Drawing.Color col = bmp.GetPixel(x, i);
                    System.Drawing.Color checkCol = System.Drawing.Color.FromArgb(255, 0, 0, 0);
                    if (bmp.GetPixel(x, i) != checkCol)
                    {
                        if (col != System.Drawing.Color.Cyan)
                        {
                            if (highestPoint == 0)
                                lowestPoint = i;
                            highestPoint = i;
                        }
                    }
                }

                if (j == 0)
                {
                    topLeft = new System.Drawing.Point(rect.X, lowestPoint);
                    bottomLeft = new System.Drawing.Point(rect.X, highestPoint);
                }
                else if (j == 1)
                    wallLineHeight += highestPoint - lowestPoint;
                else if (j == 2)
                {
                    topRight = new System.Drawing.Point(rect.X + rect.Width, lowestPoint);
                    bottomRight = new System.Drawing.Point(rect.X + rect.Width, highestPoint);
                }
            }
            if (!minimumLimit)
                return new System.Drawing.Point[] { topLeft, topRight, bottomRight, bottomLeft };
            else if (bottomLeft.Y - topLeft.Y > 5 && bottomRight.Y - topRight.Y > 5)
                return new System.Drawing.Point[] { topLeft, topRight, bottomRight, bottomLeft };

            System.Drawing.Point emptyPoint = new System.Drawing.Point(0, 0);
            return new System.Drawing.Point[] { emptyPoint, emptyPoint, emptyPoint, emptyPoint };
        }

        /// <summary>
        /// Analyses the blue line rectangle to find its thickness.
        /// </summary>
        protected void FindBlueLineThickness()
        {
            float topPoint = 0;
            float bottomPoint = 0;

            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, 0, 0);
            System.Drawing.Point[] points = { new System.Drawing.Point(0, 0) };
            if (blueLineRectangle.Left < cameraDimensions.X / 2 && blueLineRectangle.Right > cameraDimensions.X / 2)
            {
                rect = blueLineRectangle;
                points = blueLinePoints;
            }
            else if (secondBlueLineRectangle.Left < cameraDimensions.X / 2 && secondBlueLineRectangle.Right > cameraDimensions.X / 2)
            {
                rect = secondBlueLineRectangle;
                points = secondBlueLinePoints;
            }
            else
                return;
            for (int i = 0; i < rect.Height; i++)
            {
                System.Drawing.Color col = blueColourBitmap.GetPixel(cameraDimensions.X / 2, rect.Y + i);
                if (col != System.Drawing.Color.FromArgb(255, 0, 0, 0))
                {
                    if (topPoint == 0)
                        topPoint = i;
                    else
                        bottomPoint = i;
                }
            }
            if (!(rect.Height > 200 && Math.Abs(points[0].Y - points[1].Y) < 40))
                blueLineThickness = bottomPoint - topPoint;
            else
                blueLineThickness = 0;
        }
    }
}
