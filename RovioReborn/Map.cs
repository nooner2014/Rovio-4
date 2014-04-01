using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;


using DColor = System.Drawing.Color;
using DMatrix = System.Drawing.Drawing2D.Matrix;
using DRectangle = System.Drawing.Rectangle;
using DPoint = System.Drawing.Point;

namespace PredatorPreyAssignment
{
    class Map
    {
        Rovio.BaseArena robot;
        
        // Different picturebox for each image element, with accompanying bitmap.
        private PictureBox picBoxMap;
        private PictureBox picBoxPath;
        private PictureBox picBoxRovio;
        private PictureBox picBoxPrey;
        private PictureBox picBoxObstacle;
        private PictureBox picBoxBayes;
        private PictureBox picBoxCone;
        private Bitmap bMap;
        private Bitmap bPath;
        private Bitmap bRovio;
        private Bitmap bObstacle;
        private Bitmap bPrey;
        private Bitmap bBayes;
        private Bitmap bCone;

        // 2D arrays for different maps
        private bool[,] finalMap;
        private bool[,] preySensor;
        private double[,] preyProbability;
        private bool[,] obstacleSensor;
        private double[,] obstacleProbability;
        private bool[,] isCellVisible;

        private int maxX = 0;
        private int maxY = 0;
        private Timer timer;
        private DPoint[] viewConePoints = new DPoint[3];
        public delegate void UpdatePictureBox(PictureBox pB, System.Drawing.Point point);
        public event UpdatePictureBox UpdatePicBox;
        public void Hide() { picBoxMap.Hide(); }

        bool testBool = false;

        private DPoint destination = new DPoint(-1, -1);
        Graphics graphics;
        DMatrix matrix;
        public Map(Rovio.BaseArena r, Control.ControlCollection c, int x, int y)
        {
            robot = r;
            maxY = 300 / 10;
            maxX = 260 / 10;
            finalMap = new bool[maxX, maxY];
            isCellVisible = new bool[maxX, maxY];
            preySensor = new bool[maxX, maxY];
            preyProbability = new double[maxX, maxY]; 
            obstacleSensor = new bool[maxX, maxY];
            obstacleProbability = new double[maxX, maxY]; 
            for (int i = 0; i < maxX; i++)
            {
                for (int j = 0; j < maxY; j++)
                {
                    finalMap[i, j] = false;
                    preyProbability[i, j] = 0.5;
                    obstacleProbability[i, j] = 0.5;
                    isCellVisible[i, j] = false;
                    preySensor[i, j] = false;
                    obstacleSensor[i, j] = false;
                }
            }

            
            SetPictureBox(c, ref picBoxBayes, ref bBayes, 260, 300, 0, 0);
            
            SetPictureBox(c, ref picBoxObstacle, ref bObstacle, 30, 30, -100, -100);
            SetPictureBox(c, ref picBoxPrey, ref bPrey, 3, 3, -100, -100);
            SetPictureBox(c, ref picBoxRovio, ref bRovio, 24, 27, -100, -100);
            SetPictureBox(c, ref picBoxCone, ref bCone, 138, 150, -500, -500);

            SetPictureBox(c, ref picBoxPath, ref bPath, 260, 300, 0, 0);
            picBoxBayes.BackColor = System.Drawing.Color.Transparent;
            picBoxPath.BackColor = System.Drawing.Color.Transparent;
            SetPictureBox(c, ref picBoxMap, ref bMap, 260, 300, x, y);
            picBoxMap.BackColor = System.Drawing.Color.Transparent;
            picBoxBayes.Location = new System.Drawing.Point(0, 0);
            DrawGraphics();
            picBoxCone.Parent = picBoxBayes;
            picBoxRovio.Parent = picBoxMap;
            picBoxObstacle.Parent = picBoxBayes;
            picBoxPrey.Parent = picBoxBayes;
            picBoxBayes.Parent = picBoxMap;
            picBoxPath.Parent = picBoxBayes;


            
        }

        /// <summary>
        /// Adds new picture box to scene and sends bitmap + dimensions.
        /// </summary>
        /// <param name="c">Form control collection.</param>
        /// <param name="p">PictureBox to assign.</param>
        /// <param name="b">Bitmap to assign to PictureBox.</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="x">Position X.</param>
        /// <param name="y">Position Y.</param>
        private void SetPictureBox(Control.ControlCollection c, ref PictureBox p, ref Bitmap b, int width, int height, int x, int y)
        {
            b = new Bitmap(width, height);
            b.MakeTransparent();
            p = new PictureBox();
            p.Location = new System.Drawing.Point(x, y);
            p.Image = b;
            p.Size = new Size(width, height);
            p.BackColor = DColor.Transparent;
            p.Show();
            c.Add(p);
        }

        /// <summary>
        /// Draws the grid to screen (only called once, at load).
        /// </summary>
        private void DrawGraphics()
        {
            

            using (graphics = Graphics.FromImage(bMap))
                graphics.FillRectangle(new SolidBrush(System.Drawing.Color.LightBlue), new DRectangle(0, 0, bMap.Width, bMap.Height));

            using (graphics = Graphics.FromImage(bPrey))
                graphics.FillRectangle(new SolidBrush(System.Drawing.Color.Red), new DRectangle(0, 0, 3, 3));

            using (graphics = Graphics.FromImage(bRovio))
            {
                graphics.FillEllipse(new SolidBrush(System.Drawing.Color.RosyBrown), new DRectangle(0, 0, 24, 27));
                graphics.FillRectangle(new SolidBrush(System.Drawing.Color.IndianRed), new DRectangle(12, 0, 2, 13));
            }

            using (graphics = Graphics.FromImage(bObstacle))
                graphics.FillRectangle(new SolidBrush(System.Drawing.Color.Green), new DRectangle(0, 0, 30, 30));

            using (graphics = Graphics.FromImage(bMap))
            {
                for (int i = 1; i < maxY; i++)
                    graphics.DrawLine(new Pen(System.Drawing.Color.DarkBlue, 1), new System.Drawing.Point(0, i * 10), new System.Drawing.Point(260, i * 10));

                for (int i = 1; i < maxX; i++)
                    graphics.DrawLine(new Pen(System.Drawing.Color.DarkBlue, 1), new System.Drawing.Point(i * 10, 0), new System.Drawing.Point(i * 10, 300));

                SolidBrush brush = new SolidBrush(System.Drawing.Color.DarkBlue);
                graphics.FillRectangle(brush, new DRectangle(0, 0, 30, 100));
                graphics.FillRectangle(brush, new DRectangle(bMap.Width - 30, 0, 30, 100));
                graphics.FillRectangle(brush, new DRectangle(0, bMap.Height - 100, 30, 100));
                graphics.FillRectangle(brush, new DRectangle(bMap.Width - 30, bMap.Height - 100, 30, 100));
            }

            using (graphics = Graphics.FromImage(bCone))
            {
                DPoint[] conePoints = { new System.Drawing.Point(bCone.Width / 2, bCone.Height), new System.Drawing.Point(0, 0), new System.Drawing.Point(bCone.Width, 0) };
                graphics.FillPolygon(new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(100, DColor.ForestGreen)), conePoints);
            }
        }

        /// <summary>
        /// Called from robot once initial spins are made. Sets the newP localisation (always pointing north).
        /// Uses the nearest two distances for each opposite direction to localise as accurately as possible.
        /// </summary>
        public void SetInitialPoint()
        {
            double x = 0;
            double y = 0;

            if ((robot as Rovio.BaseArena).northDist > (robot as Rovio.BaseArena).southDist)
                y = 3 - (robot as Rovio.BaseArena).southDist;
            else
                y = (robot as Rovio.BaseArena).northDist;

            if ((robot as Rovio.BaseArena).eastDist > (robot as Rovio.BaseArena).westDist)
                x = (robot as Rovio.BaseArena).westDist;
            else
                x = 2.64 - (robot as Rovio.BaseArena).eastDist;

            if (((robot as Rovio.BaseArena).northDist > 0.9 && (robot as Rovio.BaseArena).northDist < 2.3 && (robot as Rovio.BaseArena).southDist < 2.3 && (robot as Rovio.BaseArena).southDist > 0.9)
                || ((robot as Rovio.BaseArena).southDist > 0.9 && (robot as Rovio.BaseArena).southDist < 2.3 && (robot as Rovio.BaseArena).northDist < 2.3 && (robot as Rovio.BaseArena).northDist > 0.9)
                && !double.IsInfinity((robot as Rovio.BaseArena).northDist) && !double.IsInfinity((robot as Rovio.BaseArena).southDist))// && x < 1.0)
                x -= 0.32;
            else if (x > 2)//if ((robot as Rovio.Predator).eastDist > (robot as Rovio.Predator).westDist)
                x -= 0.32;


            //picBoxCone.Show();

            UpdatePicBox(picBoxRovio, new System.Drawing.Point((int)(x * 100), (int)(y * 100)));
            //UpdatePicBox(picBoxCone, new Point(picBoxRovio.Location.X-(picBoxCone.Size.Width/2)+(picBoxRovio.Width/2), picBoxRovio.Location.Y-picBoxCone.Size.Width));
            testBool = true;

            
            
        }


        /// <summary>
        /// Get the probability to affect new Bayesian filtering value.
        /// </summary>
        /// <param name="map">If map has detected an object.</param>
        /// <param name="input">If the input sensor has detected an object.</param>
        /// <returns>Probability based on reliability of map and sensors.</returns>
        private double GetProbability(bool map, bool input)
        {
            if (map && input)
                return 0.7;
            else if (map && !input)
                return 0.48;
            else if (!map && input)
                return 0.39;
            else //if (!map && !input)
                return 0.49;
        }

        /// <summary>
        /// Get the probability of prey, to affect new Bayesian filtering value.
        /// </summary>
        /// <param name="map">If map has detected an object.</param>
        /// <param name="input">If the input sensor has detected an object.</param>
        /// <returns>Probability based on reliability of map and sensors.</returns>
        private double GetPreyProbability(bool map, bool input)
        {
            if (map && input)
                return 0.9;
            else if (map && !input)
                return 0.48;
            else if (!map && input)
                return 0.39;
            else //if (!map && !input)
                return 0.89;
        }

        /// <summary>
        /// Sets probabalistic values to map.
        /// </summary>
        /// <param name="lookingForPrey">If the prey is being searched for, or something else.</param>
        /// <param name="inputMap">Which map is being affected (e.graphics. obstacle map, prey map).</param>
        /// <param name="probability">Which probability array to update (e.graphics. obstacle map, prey map).</param>
        private void Bayes(bool lookingForPrey, bool[,] inputMap, ref double[,]probability)
        {
            double threshold = 0.95;

            bool map = false;
            bool input;
            double mapProb = 0;
            double newProb = 0;

            double maxProb = -1;
            DPoint maxIndex = new DPoint(-1, -1);
            for (int i = 0; i < maxX; i++)
            {
                for (int j = 0; j < maxY; j++)
                {
                    if (!isCellVisible[i, j]) // If cell can't be seen, it is ignored.
                        continue;

                    input = inputMap[i, j];
                    mapProb = probability[i, j];
                    map = (mapProb < threshold);

                    if (lookingForPrey)
                        newProb = (GetPreyProbability(map, input) * mapProb) / ((GetProbability(map, input) * mapProb + GetProbability(!map, input) * ((1 - mapProb))));
                    else
                        newProb = (GetProbability(map, input) * mapProb) / ((GetProbability(map, input) * mapProb + GetProbability(!map, input) * ((1 - mapProb))));

                    if (newProb > maxProb)
                    {
                        maxIndex = new DPoint(i, j);
                        maxProb = newProb;
                    }

                    if (newProb < 0.95 && newProb > 0.05)
                        probability[i, j] = newProb;
                    else if (newProb > 0.75 && !lookingForPrey)
                        finalMap[i, j] = true;
                    else if (newProb > 0.5 && !lookingForPrey)
                        finalMap[i, j] = true;
                    else
                        finalMap[i, j] = false;
                }
            }

            if (lookingForPrey)
            {
                if (maxProb < 0.6)
                    destination = new DPoint(-1, -1);
                else if (maxProb > 0.9)
                    destination = maxIndex;
            }

            //if (lookingForPrey && !destFound)
                //destination = new DPoint(-1, -1);
        }

        /// <summary>
        /// Begin the update function and assign a robot to the map.
        /// </summary>
        /// <param name="r">Input robot.</param>
        public void SetUpdate(Rovio.BaseArena r)
        {
            robot = r;
            timer = new Timer();
            timer.Tick += Update;
            timer.Start();
        }


        private void SetNewRovioPosition()
        {
            Vector2 oldP = new Vector2(picBoxRovio.Location.X, picBoxRovio.Location.Y);
            Vector2 newP = new Vector2(0, -1);

            // Find the angle of the Rovio on the perimeter of the arena.
            newP = Vector2.Transform(-Vector2.UnitY, Matrix.CreateRotationZ(MathHelper.ToRadians((float)robot.cumulativeAngle)));
            newP /= MathHelper.Max(Math.Abs(newP.X), Math.Abs(newP.Y));
            newP += Vector2.One;
            newP *= new Vector2(260, 300) * 0.5f;

            // Translate into the arena from the perimeter.
            using (matrix = new System.Drawing.Drawing2D.Matrix())
            {
                matrix.Translate((int)newP.X, (int)newP.Y);
                matrix.RotateAt((float)robot.cumulativeAngle, new DPoint(0, 0));
                matrix.Translate(0f, (float)(robot.wallDistance * 100));
                DPoint[] newPos = { new DPoint(0, 0) };
                matrix.TransformPoints(newPos);
                newP = Vector2.Lerp(oldP, new Vector2(newPos[0].X, newPos[0].Y), 0.1f);
                picBoxRovio.Location = new DPoint((int)newP.X, (int)newP.Y);   
            }
        }


        /// <summary>
        /// Update function of map.
        /// </summary>
        /// 
        private void Update(object sender, EventArgs e)
        {
            //picBoxCone.Location = new Point(picBoxRovio.Location.X - (picBoxCone.Width / 2) + (picBoxRovio.Width/2),  (picBoxCone.Height/2) + (picBoxRovio.Height));
            //picBoxCone.Location = new Point(0,0);


            //robot.cumulativeAngle = 0;
            //picBoxRovio.Location = new DPoint(50, 10);//(int)(robot.wallDistance*100));

            lock (robot.mapLock)
            {
                Bayes(true, preySensor, ref preyProbability);
                Bayes(false, obstacleSensor, ref obstacleProbability);
                bBayes = new Bitmap(bMap.Size.Width, bMap.Size.Height);
                SetNewRovioPosition(); 

                // Run AStar if there is a suitable destination and draw it on the map.
                using (graphics = Graphics.FromImage(bBayes))
                {
                    AStar astar = new AStar(finalMap.GetLength(0), finalMap.GetLength(1));
                    astar.Build(finalMap, new DPoint(destination.X, destination.Y), new DPoint((picBoxRovio.Location.X / 10) + (picBoxRovio.Width / 10 / 2), picBoxRovio.Location.Y / 10));

                    for (int i = 0; i < maxX; i++)
                    {
                        for (int j = 0; j < maxY; j++)
                        {
                            graphics.FillRectangle(new SolidBrush(DColor.FromArgb((int)(preyProbability[i, j] * 255), System.Drawing.Color.DarkRed)), new DRectangle(i * 10, j * 10, 10, 10));
                            graphics.FillRectangle(new SolidBrush(DColor.FromArgb((int)(obstacleProbability[i, j] * 255), System.Drawing.Color.DarkBlue)), new DRectangle(i * 10, j * 10, 10, 10));
                            if (astar.inPath[i, j])
                                graphics.FillRectangle(new SolidBrush(DColor.Red), new DRectangle(i * 10, j * 10, 10, 10));
                        }
                    }
                }

                picBoxBayes.Image = bBayes;

                // Check which cells are within the viewing cone.
                if (viewConePoints != null)
                {
                    for (int i = 0; i < maxX; i++)
                    {
                        for (int j = 0; j < maxY; j++)
                        {
                            DPoint a = new DPoint(i * 10 + 1, j * 10 + 1); // Top left
                            DPoint b = new DPoint((i + 1) * 10 - 1, j * 10 + 1); // Top right
                            DPoint c = new DPoint(i * 10 + 1, (j + 1) * 10 - 1); // Bottom left
                            DPoint d = new DPoint((i + 1) * 10 - 1, (j + 1) * 10 - 1); // Bottom right
                            if (!(PointInPolygon(a, viewConePoints) || PointInPolygon(b, viewConePoints)
                                || PointInPolygon(c, viewConePoints) || PointInPolygon(d, viewConePoints)))
                            {
                                //
                                isCellVisible[i, j] = false;
                            }
                            else
                            {
                                preySensor[i, j] = false;
                                obstacleSensor[i, j] = false;
                                isCellVisible[i, j] = true;
                            }
                        }
                    }
                }

                if (robot.GetType() == typeof(Rovio.PredatorMap))
                {
                    if (robot.IsPreySeen())
                    {
                        //picBoxPrey.Show();
                        picBoxPrey.Location = new System.Drawing.Point(picBoxRovio.Location.X, picBoxRovio.Location.Y - (int)(robot.GetPreyDistance() * 26 * 3));

                        double totalFOV = robot.GetPreyDistance() * 100 * 0.93;
                        double percentage = (double)robot.preyRectangle.X / (double)robot.cameraDimensions.X * 100;
                        double newX = percentage * (totalFOV / 100);

                        DPoint newPosition = new System.Drawing.Point(picBoxRovio.Location.X - ((int)totalFOV / 2) + (int)newX, picBoxPrey.Location.Y);
                        using (matrix = new System.Drawing.Drawing2D.Matrix())
                        {
                            matrix.RotateAt((float)robot.cumulativeAngle, new System.Drawing.Point(picBoxRovio.Location.X + (picBoxRovio.Size.Width / 2), picBoxRovio.Location.Y + (picBoxRovio.Size.Height / 2)));
                            matrix.Translate(0f, -0f);
                            DPoint[] aPoints = { newPosition };
                            matrix.TransformPoints(aPoints);
                            picBoxPrey.Location = aPoints[0];
                        }

                        try 
                        { 
                            preySensor[(int)((picBoxPrey.Location.X / 10) + 1.5), (int)((picBoxPrey.Location.Y / 10) + 1.5)] = true;
                        } catch { }
                    }
                    else
                        picBoxPrey.Hide();

                    if ((robot as Rovio.BaseArena).IsObstacleSeen())
                    {

                        /*
                        picBoxObstacle.Show();
                        picBoxObstacle.Location = new System.Drawing.Point(picBoxRovio.Location.X, picBoxRovio.Location.Y - (int)((robot as Rovio.Predator).GetObstacleDistance() * 50 * 3));

                        double totalFOV = (robot as Rovio.Predator).GetObstacleDistance() * 100 * 0.93;
                        double percentage = (double)(robot as Rovio.Predator).obstacleRectangle.X / (double)robot.cameraDimensions.X * 100;
                        double newX = percentage * (totalFOV / 100);

                        try
                        {
                            picBoxObstacle.Location = new System.Drawing.Point(picBoxRovio.Location.X - ((int)totalFOV / 2) + (int)newX, picBoxObstacle.Location.Y);
                        }
                        catch { }*/


                        picBoxObstacle.Location = new System.Drawing.Point(picBoxRovio.Location.X + (robot as Rovio.BaseArena).obstacleRectangle.X + (robot as Rovio.BaseArena).obstacleRectangle.Width, picBoxRovio.Location.Y - (int)((robot as Rovio.BaseArena).GetObstacleDistance() * 20 * 3));

                        // Calculat the obstacle's position along the X.
                        double totalFOV = (robot as Rovio.BaseArena).GetObstacleDistance() * 100 * 0.93;
                        double percentage = (double)(robot as Rovio.BaseArena).obstacleRectangle.X / (double)robot.cameraDimensions.X * 100;
                        double newX = percentage * (totalFOV / 100);


                        //int rovLocationX = ((int)totalFOV/2) + (int)newX;
                        //int rovLocationY = 

                            //picBoxPrey.Location = new System.Drawing.Point(picBoxRovio.Location.X - ((int)totalFOV/2) + (int)newX, picBoxPrey.Location.Y);
                        DPoint newPosition = new System.Drawing.Point(picBoxRovio.Location.X - ((int)totalFOV / 2) + (int)newX*2, picBoxObstacle.Location.Y);
                        using (matrix = new System.Drawing.Drawing2D.Matrix())
                        {
                            matrix.RotateAt((float)robot.cumulativeAngle, new System.Drawing.Point(picBoxRovio.Location.X + (picBoxRovio.Size.Width / 2), picBoxRovio.Location.Y + (picBoxRovio.Size.Height / 2)));
                            matrix.Translate((float)-newX + 30f, -(float)(robot.GetObstacleDistance() * 40 * 3));
                            DPoint[] aPoints = { newPosition };
                            matrix.TransformPoints(aPoints);
                            picBoxObstacle.Location = aPoints[0];
                        }

                        try
                        {
                            int p = (int)((picBoxObstacle.Location.X / 10) + 0.5);
                            int q = (int)((picBoxObstacle.Location.Y / 10) + 0.5);

                            for (int i = -1; i <= 1; i++)
                            {
                                for (int j = -1; j <= 1; j++)
                                {
                                    obstacleSensor[(int)((picBoxObstacle.Location.X / 10) + i), (int)((picBoxObstacle.Location.Y / 10) + j)] = true;
                                }
                            }
                        }
                        catch { }
                    }
                    else
                        picBoxObstacle.Hide();
                }


                // Rotate the Rovio icon to the angle that the robot physically faces.
                System.Drawing.Drawing2D.Matrix matrixRovio = new System.Drawing.Drawing2D.Matrix();
                matrixRovio.RotateAt((float)robot.cumulativeAngle, new System.Drawing.Point(picBoxRovio.Location.X + (picBoxRovio.Size.Width / 2), picBoxRovio.Location.Y + (picBoxRovio.Size.Height / 2)));
                matrixRovio.Translate(0f, -0f);
                DPoint[] rovioMovementPoints = {new DPoint(picBoxRovio.Location.X+(picBoxRovio.Size.Width/2), picBoxRovio.Location.Y + (picBoxRovio.Size.Height/2)), 
                                                    new DPoint(picBoxRovio.Location.X+(picBoxRovio.Size.Width/2) - 69, picBoxRovio.Location.Y-150),
                                                    new DPoint(picBoxRovio.Location.X+(picBoxRovio.Size.Width/2) + 69, picBoxRovio.Location.Y-150)};
                matrixRovio.TransformPoints(rovioMovementPoints);

                if ((robot as Rovio.BaseArena).IsObstacleSeen())
                {
                    // Get the left and right points of the obstacle in the viewing cone and orientate accordingly.
                    
                    DPoint leftPoint = new DPoint(picBoxObstacle.Location.X - (picBoxObstacle.Width / 2), picBoxObstacle.Location.Y - 15);
                    DPoint rightPoint = leftPoint;
                    using (matrix = new System.Drawing.Drawing2D.Matrix())
                    {
                        matrix.RotateAt((float)robot.cumulativeAngle, leftPoint);
                        matrix.Translate(0, -0f);

                        DPoint[] tempPointArr = { leftPoint };
                        matrix.TransformPoints(tempPointArr);
                        leftPoint = tempPointArr[0];

                        // Transform the right point relative to the position of the left.
                        matrix = new System.Drawing.Drawing2D.Matrix();
                        matrix.RotateAt((float)robot.cumulativeAngle, leftPoint);
                        matrix.Translate(30, 0);
                        tempPointArr[0] = new DPoint(0, 0);
                        matrix.TransformPoints(tempPointArr);
                        rightPoint = tempPointArr[0];
                    }
                    // Check if all points are still within the viewing cone. If not, skip over them.
                    if (PointInPolygon(leftPoint, rovioMovementPoints) && PointInPolygon(rightPoint, rovioMovementPoints))
                        rovioMovementPoints = new DPoint[] { rovioMovementPoints[0], rovioMovementPoints[1], leftPoint, rightPoint, rovioMovementPoints[2] };
                }


                viewConePoints = rovioMovementPoints;
                Bitmap rotated = new Bitmap(bRovio.Width + 70, bRovio.Height + 70);
                rotated.SetResolution(bRovio.HorizontalResolution, bRovio.VerticalResolution);

                using (graphics = Graphics.FromImage(rotated))
                {
                    graphics.TranslateTransform(bRovio.Width / 2, bRovio.Height / 2);
                    graphics.RotateTransform((float)robot.cumulativeAngle);
                    graphics.TranslateTransform(-bRovio.Width / 2, -bRovio.Height / 2);
                    graphics.DrawImage(bRovio, new DPoint(0, 0));

                    picBoxRovio.Size = new Size(27, 24);
                    picBoxRovio.Image = rotated;

                    picBoxCone.Location = new DPoint(picBoxRovio.Location.X - (bCone.Width / 2) + (bRovio.Width / 2), picBoxRovio.Location.Y - bCone.Height + (bRovio.Height / 2));
                    picBoxCone.Size = new Size(900, 500);
                    picBoxCone.Image = rotated;

                    picBoxCone.Location = new DPoint(0, 0);
                    picBoxCone.Size = new Size(260, 300);
                }

                Bitmap newCone = new Bitmap(260, 300);
                using (graphics = Graphics.FromImage(newCone))
                {
                    graphics.FillPolygon(new SolidBrush(DColor.FromArgb(100, DColor.ForestGreen)), viewConePoints);
                    picBoxCone.Image = newCone;
                }
                //picBoxRovio.Location = new System.Drawing.Point(aPoints[0].X, aPoints[0].Y);
                

            }
             
        }

        /// <summary>
        /// Finds if a point resides within the points of a polygon.
        /// </summary>
        /// <param name="p">Point to check.</param>
        /// <param name="poly">Array of points to check against.</param>
        /// <returns>Whether the input point lies within the point array.</returns>
        bool PointInPolygon(DPoint p, DPoint[] poly)
        {
            DPoint p1, p2;

            bool inside = false;

            if (poly.Length < 3)
            {
                return inside;
            }

            DPoint oldPoint = new DPoint(
            poly[poly.Length - 1].X, poly[poly.Length - 1].Y);

            for (int i = 0; i < poly.Length; i++)
            {
                DPoint newPoint = new DPoint(poly[i].X, poly[i].Y);

                if (newPoint.X > oldPoint.X)
                {
                    p1 = oldPoint;
                    p2 = newPoint;
                }
                else
                {
                    p1 = newPoint;
                    p2 = oldPoint;
                }

                if ((newPoint.X < p.X) == (p.X <= oldPoint.X)
                && ((long)p.Y - (long)p1.Y) * (long)(p2.X - p1.X)
                 < ((long)p2.Y - (long)p1.Y) * (long)(p.X - p1.X))
                {
                    inside = !inside;
                }

                oldPoint = newPoint;
            }

            return inside;
        }
    }
}
