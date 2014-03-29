using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace PredatorPreyAssignment
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private System.Threading.Thread robot_thread;
        enum RobotModes
        { 
            Predator,
            Prey,
            User,
        }

        String robotURL = "http://10.82.0.41/";
        static Rovio.BaseRobot robot;
        RobotModes robotState = RobotModes.Predator;

        List<int> currentKeys = new List<int>();
        List<Label> filterLabels = new List<Label>();
        Dictionary<string, float> valueDict = new Dictionary<string, float>();
        List<NumericUpDown> imageSegmentingAdjusters = new List<NumericUpDown>();
        EventHandler upDownHandler;

        List<NumericUpDown> filterUpDowns = new List<NumericUpDown>();
        // Set default filter values in dictionary.


        private void ReadDictValues()
        {
            string line = "";
            try
            {
                using (StreamReader r = new StreamReader("dictvalues.txt"))
                {
                    while (true)
                    {
                        line = r.ReadLine();
                        if (line == null)
                            break;
                        line = Regex.Replace(line, @"\s", "");

                        if (line != "" && line[0] != '/')
                        {
                            string key = line.Substring(0, line.IndexOf(':'));
                            float value = (float)Convert.ToDecimal(line.Substring(line.IndexOf(':') + 1));
                            valueDict.Add(key, value);
                            filterUpDowns[valueDict.Count - 1].Value = (decimal)value;
                        }
                    }
                }

                upDownHandler = new EventHandler(UpDownHandler);
                
            }
            catch
            {
                MessageBox.Show("Missing dictvalues.txt (filter values). Add file to directory and try again.");
                Environment.Exit(0);
            }
        }

        private void FilterChangersSetup(List<NumericUpDown> list, string col)
        {
            Point imageSegmentingAdjustingLocation = new Point(picboxCameraImage.Location.X + picboxCameraImage.Size.Width + 200 + (120 * (list.Count / 12)/3), 85);
            int limit = list.Count;
            try
            {
                imageSegmentingAdjustingLocation = new Point(picboxCameraImage.Location.X + picboxCameraImage.Size.Width + 200 + (int)(120 * (list.Count / 6) - (12 % limit) * 30 ), 85 + (int)(12 % (limit) * 8.2f));
            }
            catch { }
            Size imageSegmentingAdjustingSize = new Size(50, 20);

            labelHue.Hide();
            labelSat.Hide();
            labelLum.Hide();
            labelBattery.Hide();
            label7.Hide();
            for (int i = list.Count; i < limit+6; i++)
            {
                list.Add(new NumericUpDown());
                //Controls.Add(list[i]);
                list[i].ValueChanged += upDownHandler;
                if (i == limit+0)
                    list[i].Name = col + "HueMin";
                else if (i == limit+1)
                    list[i].Name = col + "HueMax";
                else if (i == limit+2)
                    list[i].Name = col + "SatMin";
                else if (i == limit+3)
                    list[i].Name = col + "SatMax";
                else if (i == limit+4)
                    list[i].Name = col + "LumMin";
                else if (i == limit+5)
                    list[i].Name = col + "LumMax";

                if (i == limit+1)
                    imageSegmentingAdjustingLocation = new Point(imageSegmentingAdjustingLocation.X + 55, imageSegmentingAdjustingLocation.Y);
                if (i % 2 == 0 && i != limit)
                    imageSegmentingAdjustingLocation = new Point(list[i - 2].Location.X, list[i - 2].Location.Y + 25);
                else if (i % 2 == 1 && i != limit+1)
                    imageSegmentingAdjustingLocation = new Point(list[i - 2].Location.X, list[i - 2].Location.Y + 25);

                if (i > limit + 1)
                {
                    list[i].Increment = 0.05M;
                    list[i].Maximum = 1;
                    list[i].DecimalPlaces = 2;
                }
                else
                    list[i].Maximum = 360;
                list[i].Location = imageSegmentingAdjustingLocation;
                list[i].Size = imageSegmentingAdjustingSize;
                list[i].TabIndex = 0;
                list[i].Value = 0;
                list[i].Anchor = AnchorStyles.Top | AnchorStyles.Left;


                filterLabels.Add(new Label());
                filterLabels[list.Count / 6].Location = new Point(list[limit].Location.X + 32, list[limit].Location.Y - 15);
                filterLabels[list.Count / 6].Size = new Size(40, 15);
                filterLabels[list.Count / 6].Text = col[0].ToString().ToUpper() + col.Substring(1);
                //Controls.Add(filterLabels[list.Count / 6]);
            }
        }

        static Map map;
        private void ImageViewer_Load(object sender, EventArgs e)
        {


            
            upDownHandler += new EventHandler(UpDownHandler);
            FilterChangersSetup(filterUpDowns, "green");
            FilterChangersSetup(filterUpDowns, "red");
            FilterChangersSetup(filterUpDowns, "blue");
            FilterChangersSetup(filterUpDowns, "yellow");
            FilterChangersSetup(filterUpDowns, "white");

            Show();
            
            


            


            //
            
            Text = "BLA11210972 Computer Vision & Robotics Predator/Prey";

            Label l = new Label();

            //Show();
            //imageSegmentingAdjustersTest[0].ValueChanged += handler;
            // Start predator thread.
            Focus();

            
            
        }

        // "User" "Predator" "FSM"
        private void InitialiseRobot(string type)
        {

            System.Threading.Thread.Sleep(2000);
            buttonPredator.Enabled = true;
            buttonUser.Enabled = true;
            for (int i = 0; i < filterUpDowns.Count; i++)
            {
                filterUpDowns[i].Hide();
                filterUpDowns[i].Enabled = false;
            }

            if (robot != null)
                robot.KillThreads();
  

            if (type == "Predator")
            {
                buttonPredator.Enabled = false;
                map = new Map((robot as Rovio.BaseArena), Controls, 387, 18);
                map.UpdatePicBox += UpdatePictureBox;
                robot = new Rovio.PredatorMap(robotURL, "user", "password", map, currentKeys);
                (robot as Rovio.BaseArena).SourceImage += UpdateImage;


                map.SetUpdate((robot as Rovio.BaseArena));
                
                updateTimer.Start();

                if (valueDict.Count == 0)
                    ReadDictValues();
                (robot as Rovio.BaseArena).SetFilters(valueDict);
                for (int i = 0; i < filterUpDowns.Count; i++)
                {
                    filterUpDowns[i].Show();
                    filterUpDowns[i].Enabled = true;
                }

                robot_thread = new System.Threading.Thread(robot.Start);
                robot_thread.Start();
            }
            else if (type == "PredatorFSM")
            {
                buttonPredatorFSM.Enabled = false;
                //map = new Map(robot, Controls, 387, 18);
                //map.UpdatePicBox += UpdatePictureBox;
                robot = new Rovio.PredatorStateMachine(robotURL, "user", "password", map, currentKeys);
                (robot as Rovio.BaseArena).SourceImage += UpdateImage;
                //map.SetUpdate(robot);

                updateTimer.Start();

                if (valueDict.Count == 0)
                    ReadDictValues();
                (robot as Rovio.BaseArena).SetFilters(valueDict);
                for (int i = 0; i < filterUpDowns.Count; i++)
                {
                    filterUpDowns[i].Show();
                    filterUpDowns[i].Enabled = true;
                }

                robot_thread = new System.Threading.Thread(robot.Start);
                robot_thread.Start();
            }
            else if (type == "User")
            {
                if (map != null)
                    map.Hide();
                robot = new Rovio.UserRobot(robotURL, "user", "password", map, currentKeys);
                (robot as Rovio.UserRobot).SourceImage += UpdateImage;
                robot_thread = new System.Threading.Thread(robot.Start);
                robot_thread.Start();
                //buttonUser.Enabled = false;

                //buttonPredator.Enabled = true;

            }

            if (type == "Predator" || type == "PredatorFSM" || type == "User")
            {
                picboxCameraImage.Location = new Point(20, 22);
                picboxCameraImage.Size = new Size((int)robot.cameraDimensions.X, (int)robot.cameraDimensions.Y);
                textBoxIP.Enabled = false;
                
                buttonPredator.Enabled = false;
                buttonUser.Enabled = false;
                buttonStop.Enabled = true;
                buttonPredatorFSM.Enabled = false;
                buttonStop.Enabled = true;
                Focus();
            }
            else
            {
                textBoxIP.Enabled = true;
                buttonStop.Enabled = false;
                robot.KillThreads();
            }
        }


        private void ImageViewer_Resize(object sender, EventArgs e)
        {
            picboxCameraImage.Size = this.ClientSize;
        }

        public void UpdatePictureBox(PictureBox pB, System.Drawing.Point point)
        {
            if (InvokeRequired)
                Invoke(new MethodInvoker(delegate { UpdatePictureBox(pB, point); }));
            else
                pB.Location = point;
                
        }

        // Update form picture box
        public void UpdateImage(Image image)
        {
                picboxCameraImage.Image = image;

            if (this.InvokeRequired)
            {
                try
                {
                    this.Invoke(new MethodInvoker(delegate { UpdateImage(image); }));
                }
                catch { }
            }
            //else
               // this.ClientSize = picboxCameraImage.Image.Size;
        }

        // Abort thread and close window.
        private void ImageViewer_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                robot_thread.Abort();
            }
            catch { }

            Application.Exit();
            Environment.Exit(0);
        }

        // Handle switch to user input state.
        private void buttonUser_Click(object sender, EventArgs e)
        {
            InitialiseRobot("User");

        }

        // Handle switch to predator state.
        private void buttonPredator_Click(object sender, EventArgs e)
        {
            
           // robot_thread.Abort();
            //while (robot_thread.ThreadState != System.Threading.ThreadState.Aborted)
            //    System.Threading.Thread.Sleep(1);

            

            InitialiseRobot("Predator");
            
            //robot.API.Movement.Update();
            //labelDocked.Text = robot.API.Movement.GetStatus();
            
        }

        // Key down - adds key to dictionary (if it is not already there).
        private void ImageViewer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Environment.Exit(0);
            else if (!currentKeys.Contains(Convert.ToInt32(e.KeyCode)))
                currentKeys.Add(Convert.ToInt32(e.KeyCode));
        }

        // Key up - removes key from dictionary.
        private void ImageViewer_KeyUp(object sender, KeyEventArgs e)
        {
            currentKeys.Remove(Convert.ToInt32(e.KeyCode));
        }

        private void UpDownHandler(object sender, EventArgs e)
        { 
            if (valueDict.ContainsKey((sender as NumericUpDown).Name.ToString()))
            {
                valueDict[(sender as NumericUpDown).Name.ToString()] = (float)(sender as NumericUpDown).Value;
                (robot as Rovio.BaseArena).SetFilters(valueDict);
            }
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            labelDirection.Text = robot.direction;

            //if (robot.direction == "North")
            //    pictureBoxRovio.Image.RotateFlip(RotateFlipType.RotateNoneFlipNone);
            //else if (robot.direction == "South")
            //    pictureBoxRovio.Image.RotateFlip(RotateFlipType.RotateNoneFlipX);
            //else if (robot.direction == "East")
            //    pictureBoxRovio.Image.RotateFlip(RotateFlipType.Rotate90FlipNone);
           // else if (robot.direction == "West")
            //    pictureBoxRovio.Image.RotateFlip(RotateFlipType.Rotate270FlipNone);
            try
            {
                //int dockedStatus = robot.API.Movement.Report.Charging;
                labelDocked.Text = robot.chargingStatus == 80 ? "Docked" : "Not docked";
                labelDirection.Text = (robot as Rovio.BaseArena).lastReadDirection.ToString();
               // int batteryCharge = robot.API.Movement.Report.BatteryLevel;
                labelBattery.Text = robot.batteryStatus.ToString();
            }
            catch { }
        }

        private void textBoxIP_TextChanged(object sender, EventArgs e)
        {
            robotURL = textBoxIP.Text;
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            InitialiseRobot("");
        }

        private void buttonPredatorFSM_Click(object sender, EventArgs e)
        {
            InitialiseRobot("PredatorFSM");
        }
    }
}
