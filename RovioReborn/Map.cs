using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;


using DColor = System.Drawing.Color;
using DPoint = System.Drawing.Point;
namespace PredatorPreyAssignment
{
    class Map
    {

        enum Type
        { 
            Prey,
            Obstacle,
            Other,
        }
        Rovio.BaseRobot robot;

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

        ////////////////////////////////////////////////////
        //////////////////Initialisation////////////////////
        ////////////////////////////////////////////////////

        public Map(Rovio.BaseRobot r, Control.ControlCollection c, int x, int y)
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

        // Adds new picture box to scene and sends bitmap + dimensions.
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

        // Draws the grid to screen (only called once, at load).
        private void DrawGraphics()
        {
            Graphics g = Graphics.FromImage(bMap);
            
            g.FillRectangle(new SolidBrush(System.Drawing.Color.LightBlue), new System.Drawing.Rectangle(0, 0, bMap.Width, bMap.Height));

            g = Graphics.FromImage(bPrey);
            g.FillRectangle(new SolidBrush(System.Drawing.Color.Red), new System.Drawing.Rectangle(0, 0, 3, 3));

            g = Graphics.FromImage(bRovio);
            g.FillEllipse(new SolidBrush(System.Drawing.Color.RosyBrown), new System.Drawing.Rectangle(0, 0, 24, 27));
            g.FillRectangle(new SolidBrush(System.Drawing.Color.IndianRed), new System.Drawing.Rectangle(12, 0, 2, 13));

            g = Graphics.FromImage(bObstacle);
            g.FillRectangle(new SolidBrush(System.Drawing.Color.Green), new System.Drawing.Rectangle(0, 0, 30, 30));


            g = Graphics.FromImage(bMap);
            for (int i = 1; i < maxY; i++)
                g.DrawLine(new Pen(System.Drawing.Color.DarkBlue, 1), new System.Drawing.Point(0, i * 10), new System.Drawing.Point(260, i * 10));

            for (int i = 1; i < maxX; i++)
                g.DrawLine(new Pen(System.Drawing.Color.DarkBlue, 1), new System.Drawing.Point(i * 10, 0), new System.Drawing.Point(i * 10, 300));

            SolidBrush brush = new SolidBrush(System.Drawing.Color.DarkBlue);
            g.FillRectangle(brush, new System.Drawing.Rectangle(0, 0, 30, 100));
            g.FillRectangle(brush, new System.Drawing.Rectangle(bMap.Width - 30, 0, 30, 100));
            g.FillRectangle(brush, new System.Drawing.Rectangle(0, bMap.Height - 100, 30, 100));
            g.FillRectangle(brush, new System.Drawing.Rectangle(bMap.Width - 30, bMap.Height - 100, 30, 100));


            g = Graphics.FromImage(bCone);
            DPoint[] conePoints = { new System.Drawing.Point(bCone.Width / 2, bCone.Height), new System.Drawing.Point(0, 0), new System.Drawing.Point(bCone.Width, 0) };
            g.FillPolygon(new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(100, DColor.ForestGreen)), conePoints);
        }

        // Called from robot once initial spins are made. Sets the first localisation (always pointing north).
        // Uses the nearest two distances for each opposite direction to localise as accurately as possible.
        public void SetInitialPoint()
        {
            double x = 0;
            double y = 0;

            if ((robot as Rovio.Predator).northDist > (robot as Rovio.Predator).southDist)
                y = 3 - (robot as Rovio.Predator).southDist;
            else
                y = (robot as Rovio.Predator).northDist;

            if ((robot as Rovio.Predator).eastDist > (robot as Rovio.Predator).westDist)
                x = (robot as Rovio.Predator).westDist;
            else
                x = 2.64 - (robot as Rovio.Predator).eastDist;

            if (((robot as Rovio.Predator).northDist > 0.9 && (robot as Rovio.Predator).northDist < 2.3 && (robot as Rovio.Predator).southDist < 2.3 && (robot as Rovio.Predator).southDist > 0.9)
                || ((robot as Rovio.Predator).southDist > 0.9 && (robot as Rovio.Predator).southDist < 2.3 && (robot as Rovio.Predator).northDist < 2.3 && (robot as Rovio.Predator).northDist > 0.9)
                && !double.IsInfinity((robot as Rovio.Predator).northDist) && !double.IsInfinity((robot as Rovio.Predator).southDist))// && x < 1.0)
                x -= 0.32;
            else if (x > 2)//if ((robot as Rovio.Predator).eastDist > (robot as Rovio.Predator).westDist)
                x -= 0.32;


            //picBoxCone.Show();

            UpdatePicBox(picBoxRovio, new System.Drawing.Point((int)(x * 100), (int)(y * 100)));
            //UpdatePicBox(picBoxCone, new Point(picBoxRovio.Location.X-(picBoxCone.Size.Width/2)+(picBoxRovio.Width/2), picBoxRovio.Location.Y-picBoxCone.Size.Width));
            testBool = true;

            
            
        }


        ////////////////////////////////////////////////////
        //////////////Probabilistic methods/////////////////
        ////////////////////////////////////////////////////

        // Values being used.
        private double GetProbability(bool map, bool input)
        {
            if (map && input)
                return 0.6;
            else if (map && !input)
                return 0.4;
            else if (!map && input)
                return 0.15;
            else //if (!map && !input)
                return 0.85;
        }

        // Calculates new map for whichever input has been given
        private void Bayes(bool lookingForPrey, bool[,] inputSensor, ref double[,]probability)
        {
            double threshold = 0.95;

            bool map = false;
            bool input;
            double mapProb = 0;
            double newProb = 0;
            bool destFound = false;
            for (int i = 0; i < maxX; i++)
            {
                for (int j = 0; j < maxY; j++)
                {
                    if (!isCellVisible[i, j]) // If cell can't be seen, it is ignored.
                        continue;

                    input = inputSensor[i, j];
                    mapProb = probability[i, j];
                    map = (mapProb < threshold);
                    newProb = (GetProbability(map, input) * mapProb) / ((GetProbability(map, input) * mapProb + GetProbability(!map, input) * ((1 - mapProb))));

                    if (newProb < 0.95 && newProb > 0.05)
                        probability[i, j] = newProb;
                    else if (newProb > 0.75)
                    {
                        if (lookingForPrey)
                        {
                            destination = new DPoint(i, j);
                            destFound = true;
                        }
                        else
                            finalMap[i, j] = true;
                    }
                    else if (newProb > 0.5 && !lookingForPrey)
                        finalMap[i, j] = true;
                    else
                        finalMap[i, j] = false;
                }
            }

            if (lookingForPrey && !destFound)
                destination = new DPoint(-1, -1);
        }

        bool testBool = false;

        private DPoint destination = new DPoint(-1, -1);
        public void SetUpdate(Rovio.BaseRobot r)
        {
            robot = r;
            timer = new Timer();
            timer.Tick += Update;
            timer.Start();
        }

        

        private void Update(object sender, EventArgs e)
        {
            //picBoxCone.Location = new Point(picBoxRovio.Location.X - (picBoxCone.Width / 2) + (picBoxRovio.Width/2),  (picBoxCone.Height/2) + (picBoxRovio.Height));
            //picBoxCone.Location = new Point(0,0);

            



            
            if (testBool)
            {
                Vector2 oldP = new Vector2(picBoxRovio.Location.X, picBoxRovio.Location.Y);
                Vector2 newP = new Vector2(picBoxRovio.Location.X, (int)((robot as Rovio.Predator).wallDist * 100));

                Vector2 brandNew = Vector2.Lerp(oldP, newP, 0.1f);
                //picBoxRovio.Location = new DPoint((int)brandNew.X, (int)brandNew.Y);
                
            
            
            }
            lock (robot.mapLock)
            {
                Bayes(true, preySensor, ref preyProbability);
                Bayes(false, obstacleSensor, ref obstacleProbability);
                bBayes = new Bitmap(bMap.Size.Width, bMap.Size.Height);
                Graphics g = Graphics.FromImage(bBayes);


                
                AStar aa = new AStar(finalMap.GetLength(0), finalMap.GetLength(1));
                aa.Build(finalMap, new DPoint(destination.X, destination.Y), 
                    new DPoint((picBoxRovio.Location.X / 10) + (picBoxRovio.Width/10/2), picBoxRovio.Location.Y / 10));
                
                    for (int i = 0; i < maxX; i++)
                    {
                        for (int j = 0; j < maxY; j++)
                        {
                            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb((int)(preyProbability[i, j] * 255), System.Drawing.Color.DarkRed)), new System.Drawing.Rectangle(i * 10, j * 10, 10, 10));
                            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb((int)(obstacleProbability[i, j] * 255), System.Drawing.Color.DarkBlue)), new System.Drawing.Rectangle(i * 10, j * 10, 10, 10));
                            if (aa.inPath[i, j])
                                g.FillRectangle(new SolidBrush(System.Drawing.Color.Red), new System.Drawing.Rectangle(i * 10, j * 10, 10, 10));
                        }
                    }

                //bBayes = new Bitmap(260, 300);
                picBoxBayes.Image = bBayes;

                Graphics myg = Graphics.FromImage(bMap);
                if (viewConePoints != null)
                {
                    for (int i = 0; i < maxX; i++)
                    {
                        for (int j = 0; j < maxY; j++)
                        {
                            //preySensor[i, j] = false;
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

                //int away = (robot as Rovio.Predator).preyDistance * 
                if (robot.GetType() == typeof(Rovio.Predator))
                {
                    if ((robot as Rovio.Predator).IsPreySeen())
                    {
                        //picBoxPrey.Show();
                        picBoxPrey.Location = new System.Drawing.Point(picBoxRovio.Location.X, picBoxRovio.Location.Y - (int)((robot as Rovio.Predator).GetPreyDistance() * 26 * 3));

                        double totalFOV = (robot as Rovio.Predator).GetPreyDistance() * 100 * 0.93;
                        double percentage = (double)(robot as Rovio.Predator).preyRectangle.X / (double)robot.cameraDimensions.X * 100;
                        double newX = percentage * (totalFOV / 100);

                        //int rovLocationX = ((int)totalFOV/2) + (int)newX;
                        //int rovLocationY = 
                        try
                        {
                            //picBoxPrey.Location = new System.Drawing.Point(picBoxRovio.Location.X - ((int)totalFOV/2) + (int)newX, picBoxPrey.Location.Y);
                            DPoint newPosition = new System.Drawing.Point(picBoxRovio.Location.X - ((int)totalFOV / 2) + (int)newX, picBoxPrey.Location.Y);
                            System.Drawing.Drawing2D.Matrix m = new System.Drawing.Drawing2D.Matrix();

                            m.RotateAt((float)robot.cumulativeAngle, new System.Drawing.Point(picBoxRovio.Location.X + (picBoxRovio.Size.Width / 2), picBoxRovio.Location.Y + (picBoxRovio.Size.Height / 2)));
                            m.Translate(0f, -0f);
                            System.Drawing.Point[] aPoints = { newPosition };
                            m.TransformPoints(aPoints);

                            picBoxPrey.Location = aPoints[0];

                        }
                        catch { }


                        try
                        {
                            preySensor[(int)((picBoxPrey.Location.X / 10) + 1.5), (int)((picBoxPrey.Location.Y / 10) + 1.5)] = true;
                        }
                        catch { }
                    }
                    else
                        picBoxPrey.Hide();

                    if ((robot as Rovio.Predator).IsObstacleSeen())
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


                        picBoxObstacle.Location = new System.Drawing.Point(picBoxRovio.Location.X + (robot as Rovio.Predator).obstacleRectangle.X + (robot as Rovio.Predator).obstacleRectangle.Width, picBoxRovio.Location.Y - (int)((robot as Rovio.Predator).GetObstacleDistance() * 40 * 3));


                        double totalFOV = (robot as Rovio.Predator).GetObstacleDistance() * 100 * 0.93;
                        double percentage = (double)(robot as Rovio.Predator).obstacleRectangle.X / (double)robot.cameraDimensions.X * 100;
                        double newX = percentage * (totalFOV / 100);


                        //int rovLocationX = ((int)totalFOV/2) + (int)newX;
                        //int rovLocationY = 
                        try
                        {
                            //picBoxPrey.Location = new System.Drawing.Point(picBoxRovio.Location.X - ((int)totalFOV/2) + (int)newX, picBoxPrey.Location.Y);
                            DPoint newPosition = new System.Drawing.Point(picBoxRovio.Location.X - ((int)totalFOV / 2) + (int)newX*2, picBoxObstacle.Location.Y);
                            System.Drawing.Drawing2D.Matrix m = new System.Drawing.Drawing2D.Matrix();

                            m.RotateAt((float)robot.cumulativeAngle, new System.Drawing.Point(picBoxRovio.Location.X + (picBoxRovio.Size.Width / 2), picBoxRovio.Location.Y + (picBoxRovio.Size.Height / 2)));
                            m.Translate(0f, -0f);
                            System.Drawing.Point[] aPoints = { newPosition };
                            m.TransformPoints(aPoints);
                            picBoxObstacle.Location = aPoints[0];

                        }
                        catch { }


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
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                //preySensor[(int)((robot as Rovio.Predator).preyRectangle.X +i)/maxX , (int)((robot as Rovio.Predator).preyRectangle.Y+i)/maxY] = false;
                            }
                        }
                         picBoxObstacle.Hide();
                    }
                }

                bool bb = true;
                robot.MYTESTBOOL = false;
                if (bb)
                {
                    System.Drawing.Point p = new System.Drawing.Point(picBoxRovio.Location.X, picBoxRovio.Location.Y);
                    robot.MYTESTBOOL = false;
                    System.Drawing.Drawing2D.Matrix m = new System.Drawing.Drawing2D.Matrix();

                    m.RotateAt((float)robot.cumulativeAngle, new System.Drawing.Point(picBoxRovio.Location.X + (picBoxRovio.Size.Width / 2), picBoxRovio.Location.Y + (picBoxRovio.Size.Height / 2)));
                    m.Translate(0f, -0f);
                    DPoint[] aPoints = {new DPoint(picBoxRovio.Location.X+(picBoxRovio.Size.Width/2), picBoxRovio.Location.Y + (picBoxRovio.Size.Height/2)), 
                                                     new DPoint(picBoxRovio.Location.X+(picBoxRovio.Size.Width/2) - 69, picBoxRovio.Location.Y-150),
                                                     new DPoint(picBoxRovio.Location.X+(picBoxRovio.Size.Width/2) + 69, picBoxRovio.Location.Y-150)};

                    if ((robot as Rovio.Predator).IsObstacleSeen())
                    {
                        DPoint leftPoint = new DPoint(picBoxObstacle.Location.X-(picBoxObstacle.Width/2), picBoxObstacle.Location.Y-15);
                        DPoint rightPoint = new DPoint(picBoxObstacle.Location.X+(picBoxObstacle.Width/2), picBoxObstacle.Location.Y-15);
                        if (PointInPolygon(leftPoint, aPoints) && PointInPolygon(rightPoint, aPoints))
                        {
                            aPoints = new DPoint[] { aPoints[0], aPoints[1], leftPoint, rightPoint, aPoints[2] };
                        }
                        else
                        {
                            leftPoint = new DPoint(picBoxObstacle.Location.X - (picBoxObstacle.Width / 2), picBoxObstacle.Location.Y - 15);
                            rightPoint = new DPoint(picBoxObstacle.Location.X + (picBoxObstacle.Width / 2), picBoxObstacle.Location.Y - 15);
                            aPoints = new DPoint[] { aPoints[0], aPoints[1], leftPoint, rightPoint, aPoints[2] };
                        }
                    }
                    m.TransformPoints(aPoints);
                    viewConePoints = aPoints;
                    Bitmap rotated = new Bitmap(bRovio.Width + 70, bRovio.Height + 70);
                    rotated.SetResolution(bRovio.HorizontalResolution, bRovio.VerticalResolution);

                    Graphics gr = Graphics.FromImage(rotated);
                    gr.TranslateTransform(bRovio.Width / 2, bRovio.Height / 2);
                    gr.RotateTransform((float)robot.cumulativeAngle);
                    gr.TranslateTransform(-bRovio.Width / 2, -bRovio.Height / 2);
                    gr.DrawImage(bRovio, new DPoint(0, 0));

                    picBoxRovio.Size = new Size(27, 24);
                    picBoxRovio.Image = rotated;

                    picBoxCone.Location = new DPoint(picBoxRovio.Location.X - (bCone.Width / 2) + (bRovio.Width / 2), picBoxRovio.Location.Y - bCone.Height + (bRovio.Height / 2));
                    picBoxCone.Size = new Size(900, 500);
                    picBoxCone.Image = rotated;

                    picBoxCone.Location = new DPoint(0, 0);
                    picBoxCone.Size = new Size(260, 300);

                    Bitmap newCone = new Bitmap(260, 300);
                    gr = Graphics.FromImage(newCone);
                    gr.FillPolygon(new SolidBrush(DColor.FromArgb(100, DColor.ForestGreen)), aPoints);

                    picBoxCone.Image = newCone;

                    //picBoxRovio.Location = new System.Drawing.Point(aPoints[0].X, aPoints[0].Y);
                }

            }
             
        }

        // Finds if a point resides within the points of a polygon (for this purpose, the viewing cone).
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
