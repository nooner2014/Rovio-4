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
        public double northDist = double.PositiveInfinity;
        public double westDist = double.PositiveInfinity;
        public double southDist = double.PositiveInfinity;
        public double eastDist = double.PositiveInfinity;
        public double wallDistance = double.PositiveInfinity;
        protected char lastReadDirection;

        public bool IsPreySeen() { return preySeen; }
        public bool IsObstacleSeen() { return obstacleSeen; }
        public float GetPreyDistance() { return preyDistance; }
        public float GetObstacleDistance() { return obstacleDistance; }

        protected int outputImageKey = 0;


        public event ImageReady SourceImage;
        ////////////////////////////////////////////////////
        //////////////////Initialisation////////////////////
        ////////////////////////////////////////////////////
        
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
        protected void FindWallDistance(out double final, bool moveAfter)
        {
            List<double> arr = new List<double>();
            Stopwatch s = new Stopwatch();
            s.Start();

            double dist = 0;
            while (s.ElapsedMilliseconds < 3000)
            {
              
                dist = 6.5 / blueLineThickness;

                if (dist != 0 && !double.IsInfinity(dist))
                    arr.Add(Math.Abs(dist));

                if (dist > 3)
                    arr.Remove(dist);
                System.Threading.Thread.Sleep(100);
            }

            if (arr.Count != 0)
                final = arr.Average();
            else
                final = 15;

            if (!running)
                return;
            else if (moveAfter)
                RotateByAngle(3, 3);
        }

        /// <summary>
        /// Uses rectangle information to determine faced direction, and use data to assume the heading angle relative to north.
        /// </summary>
        public void FindHeading()
        {
            while (running)
            {

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

                Console.WriteLine(direction);


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
                Console.WriteLine(degAngle);
                cumulativeAngle = degAngle;// Lerp(new System.Drawing.Point((int)cumulativeAngle), new System.Drawing.Point((int)degAngle), 0.1f).X;
            }
        }
        

        /* Old method for getting angle
        public void MyNewTest()
        {
            while (running)
            {
                CalculateDirection();


                char[] which = new char[50];
                Xna.Vector2[] pos = new Vector2[50];

                //Console.WriteLine("Yellow top: " + yellowWallRectangleTop);
                //Console.WriteLine("Yellow bottom: " + yellowWallRectangleBottom);
                //Console.WriteLine("White top: " + whiteWallRectangleTop);
                //Console.WriteLine("Yellow top: " + whiteWallRectangleBottom);


                for (int i = 0; i < 352 / 7; i++)
                {
                    //      N
                    //      -
                    //      Y
                    // W -X + X+ E
                    //      Y
                    //      +
                    //      S

                    if (direction == "North")
                    {
                        if (yellowWallRectangleTop.Right > (i * 7))
                        {
                            which[i] = 'N';
                            pos[i] = Vector2.Transform(new Vector2(0, -1), Matrix.CreateRotationZ(MathHelper.ToRadians(i - 25)));
                        }
                        else if (whiteWallRectangleTop.X < 8)
                        {
                            which[i] = 'E';
                            pos[i] = Vector2.Transform(new Vector2(-1, 0), Matrix.CreateRotationZ(MathHelper.ToRadians(i - 25)));
                        }
                        else
                        {
                            which[i] = 'W';
                            pos[i] = Vector2.Transform(new Vector2(1, 0), Matrix.CreateRotationZ(MathHelper.ToRadians(i - 25)));
                        }
                    }
                    else if (direction == "East")
                    { 
                        
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
                Console.WriteLine(MathHelper.ToDegrees(radAngle));
            }
        }
        */

        /* Old method for calculating direction
        // Once ten direction readings are calculated, find the mode (eliminates error).
        private void CalculateDirection()
        {
            if (wallHeightList.Count > 10)
            {
                float actualWallHeight = 0;
                for (int i = 0; i < wallHeightList.Count; i++)
                    actualWallHeight += wallHeightList[i];

                actualWallHeight /= wallHeightList.Count;

                double sum = wallHeightList.Sum(d => Math.Pow(d - actualWallHeight, 2));
                double ret = Math.Sqrt((sum) / (wallHeightList.Count() - 1));

                wallHeightList = new List<float>();

                int n = 0;
                int s = 0;
                int e = 0;
                int w = 0;
                for (int i = 0; i < wallDirectionList.Count; i++)
                {
                    if (wallDirectionList[i] == 'N')
                        n++;
                    else if (wallDirectionList[i] == 'S')
                        s++;
                    else if (wallDirectionList[i] == 'E')
                        e++;
                    else if (wallDirectionList[i] == 'W')
                        w++;
                }

                //Get
                //Console.WriteLine("Cumulative old: " + cumulativeAngle);

                //Console.Write("    Cumulative new: " + cumulativeAngle);
                wallDirectionList = new List<char>();
            }
        }
        */
        
        /* Old initial localisation
        // First movements - rotate 90 degrees four times to localise.
        protected void InitialMovements()
        {
            FindInitialDistance(ref northDist, true);
            FindInitialDistance(ref eastDist, true);
            FindInitialDistance(ref southDist, true);
            FindInitialDistance(ref westDist, true);

            map.SetInitialPoint();
        }
        */      

        /// <summary>
        /// Calls various methods to segment images, evaluate their content, and judge findings based on gathered information.
        /// </summary>
        protected void SearchImage()
        {
            while (running)
            {
                outputImage = cameraImage;

                // Only processes if the image is new.
                if (outputImage != lastProcessedImage)
                {
                    lastProcessedImage = outputImage;
                    FindRectangles();
                    MergeImages();
                    DrawRectangles();
                    //FindDirection();


                    /*if (rectResult.Length > 1 && rectResult[1].Height > 5)
                        whiteWallRectangleBottom = rectResult[1];
                    else
                        whiteWallRectangleBottom = new System.Drawing.Rectangle(0, 0, 0, 0);*/
                    // Convert thresholded image back to 24bpp, the convert from black and white binary to colour on black background

                    //wallLineHeight += (to
                    //if (wallLineRectangle != new System.Drawing.System.Drawing.Rectangle(0, 0, 0, 0))
                    //    cameraImage = DrawRect(cameraImage, wallLineRectangle, System.Drawing.Color.Green, 3f);

                    //DrawLine(outputImage, wallLeftPoint, wallRightPoint, System.Drawing.Color.Red, wallLineHeight);

                    //cameraImage = redColourBitmap;
                    //SourceImage(cameraImage);


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


            // Remove the first eighty-five pixels of the camera image - only for wall processing since the top is unneeded.
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
            if (yellowWallRectangleTop != System.Drawing.Rectangle.Empty && whiteWallRectangleTop != System.Drawing.Rectangle.Empty)
            {
               // whiteWallRectangleTop = new System.Drawing.Rectangle(yellowWallRectangleTop.X, whiteWallRectangleTop.Y, whiteWallRectangleTop.Width, whiteWallRectangleTop.Height);

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
            /*
            if (rectResult.Length > 1 && rectResult[1].Y < rectResult[0].Y)
            {
                System.Drawing.Rectangle temp = rectResult[1];
                rectResult[1] = rectResult[0];
                rectResult[0] = temp;
            }

            if (rectResult.Length > 0)
            {
                int index = 0;
                int tallest = 0;
                for (int i = 0; i < rectResult.Length; i++)
                {
                    if (rectResult[i].Height > tallest)
                    {
                        tallest = rectResult[i].Height;
                        index = i;
                    }
                }

                int secondIndex = 0;

                for (int i = 0; i < rectResult.Length; i++)
                {
                    if (i != index && Math.Abs(rectResult[i].X - rectResult[index].X) < 10)
                        secondIndex = i;
                }

                if (rectResult[index].Y > rectResult[secondIndex].Y)
                {
                    int temp = secondIndex;
                    secondIndex = index;
                    index = temp;
                }

                whiteWallRectangleTop = rectResult[index];
                whiteWallRectangleBottom = rectResult[secondIndex];
                whiteWallRectangleTop = rectResult[0];
            }
            else
                whiteWallRectangleTop = new System.Drawing.Rectangle(0, 0, 0, 0);
             * 
             * /
             */
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

            //ConvertImageFormat(yellowColourBitmap);
            // Find the Y position of wall on left and right hand side (to gauge the perspective and find where robot is facing). 

            
            yellowTopWallPoints = GetWallPoints(yellowColourBitmap, yellowWallRectangleTop, false);
            yellowBottomWallPoints = GetWallPoints(yellowColourBitmap, yellowWallRectangleBottom, false);

            whiteTopWallPoints = GetWallPoints(whiteColourBitmap, whiteWallRectangleTop, false);
            whiteBottomWallPoints = GetWallPoints(whiteColourBitmap, whiteWallRectangleBottom, false);

           // if (Math.Abs(whiteTopWallPoints[0].Y - whiteTopWallPoints[3].Y) <1)
            //    whiteWallRectangleTop = System.Drawing.Rectangle.Empty;
           // if (Math.Abs(whiteTopWallPoints[1].Y - whiteTopWallPoints[2].Y) < 1)
               // whiteWallRectangleTop = System.Drawing.Rectangle.Empty;

            //if (Math.Abs(whiteBottomWallPoints[0].Y - whiteBottomWallPoints[3].Y) <1 )
                //whiteWallRectangleBottom = System.Drawing.Rectangle.Empty;
            //if (Math.Abs(whiteBottomWallPoints[1].Y - whiteBottomWallPoints[2].Y) < 1)
                //whiteWallRectangleBottom = System.Drawing.Rectangle.Empty;
            blueLinePoints = GetWallPoints(blueColourBitmap, blueLineRectangle, false);
            secondBlueLinePoints = GetWallPoints(blueColourBitmap, secondBlueLineRectangle, false);
            // Call GetPoint again to set line height (so we know how far away we are from the wall).
            //GetPoint(blueColourBitmap, wallLineRectangle.X + wallLineRectangle.Width - (wallLineRectangle.Width / 2), wallLineRectangle);


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
            //if (whiteWallRectangleTop != new System.Drawing.System.Drawing.Rectangle(0, 0, 0, 0))
            //outputImage = DrawRect(outputImage, whiteWallRectangleTop, System.Drawing.Color.Green, 3f);
            //if (yellowWallRectangle != new System.Drawing.System.Drawing.Rectangle(0, 0, 0, 0))
            //    outputImage = DrawRect(outputImage, yellowWallRectangle, System.Drawing.Color.Red, 3f);

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

        /*
        /// <summary>
        /// Called by SearchImage. 
        /// </summary>
        protected void FindDirection()
        {
            if (wallLineHeight != 0)
            {
                
                ////if (yellowWallRectangleBottom.Width != 0 && yellowWallRectangleBottom.Height < 2 && yellowWallRectangleTop.Height > 2)
                //if (yellowWallRectangleTop.Width != 0 && yellowWallRectangleBottom.Height < 1 && wallLineRectangle.Y-5 > yellowWallRectangleTop.Y)
                //wallDirectionList.Add('N');
                //else if (yellowWallRectangleTop.Width != 0 && yellowWallRectangleTop.Height > yellowWallRectangleBottom.Height && yellowWallRectangleBottom.Height > 2)
                //    wallDirectionList.Add('S');
                //else if (whiteWallRectangleTop.X < wallLineRectangle.X + wallLineRectangle.Height)
                //    wallDirectionList.Add('E');
                //else if ((whiteWallRectangleTop.Width != 0 && whiteWallRectangleTop.Height > wallLineRectangle.Height) ||
                //    (whiteWallRectangleTop.Width != 0 && whiteWallRectangleTop.Height < whiteWallRectangleBottom.Height && whiteWallRectangleBottom.Height > 2))
                //    wallDirectionList.Add('W');
                //wallHeightList.Add(wallLineHeight);
                //wallLineHeight = 0;
                
                //if (yellowWallRectangleTop.Width != 0 && yellowWallRectangleBottom.Width == 0 && yellowWallRectangleTop.Y < blueLineRectangle.Y)
                //    wallDirectionList.Add('N');
                //else if ((whiteWallRectangleTop.Height != 0 && whiteWallRectangleTop.Height < whiteWallRectangleBottom.Height)
                //    && Math.Abs(whiteWallRectangleTop.X-whiteWallRectangleBottom.X) < 15)
                //    wallDirectionList.Add('W');
                //else if (yellowWallRectangleBottom.Width != 0 && yellowWallRectangleTop.Height > yellowWallRectangleBottom.Height)
                //    wallDirectionList.Add('S');
                //else if(whiteTopWallPoints[0].Y > blueLinePoints[0].Y)
                //    wallDirectionList.Add('E');
                

                if (yellowWallRectangleTop.Width > whiteWallRectangleBottom.Width)
                {
                    if (yellowWallRectangleTop.Width != 0 && yellowWallRectangleBottom.Width == 0 && yellowWallRectangleTop.Y < blueLineRectangle.Y)
                        wallDirectionList.Add('N');
                    else if (yellowWallRectangleBottom.Width != 0 && yellowWallRectangleTop.Height > yellowWallRectangleBottom.Height)
                        wallDirectionList.Add('S');
                }
                else
                {
                    if (((whiteWallRectangleTop.Height != 0 && whiteWallRectangleTop.Height < whiteWallRectangleBottom.Height)
                    && Math.Abs(whiteWallRectangleTop.X - whiteWallRectangleBottom.X) < 15) || whiteWallRectangleTop.X + 10 < blueLineRectangle.X || (whiteWallRectangleTop.X < whiteWallRectangleBottom.X && whiteWallRectangleBottom.X + 10 < blueLineRectangle.X))
                        wallDirectionList.Add('W');
                    else if (whiteTopWallPoints[0].Y > blueLinePoints[0].Y || whiteTopWallPoints[3].Y > blueLinePoints[3].Y)
                        wallDirectionList.Add('E');
                }
                if (wallDirectionList.Count > 0)
                    lastReadDirection = wallDirectionList.Last();
                else
                    lastReadDirection = 'Q';
                wallHeightList.Add(wallLineHeight);
                wallLineHeight = 0;
            }
        }
        */

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
            for (int j = 0; j < 3; j++)
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
                    //if (x >= rect.Width)
                    //    break;
                    if (x >= 352)
                        x = 351;
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
