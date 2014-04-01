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

        private String robotURL = "http://10.82.0.41/";
        private static Rovio.BaseRobot robot;
        private static Map map;

        private Dictionary<string, float> valueDict = new Dictionary<string, float>();
        private EventHandler upDownHandler;
        private EventHandler robotButtonEventHandler;
        private List<int> currentKeys = new List<int>();
        private List<Label> filterLabels = new List<Label>();
        private List<Label> userLabels = new List<Label>();
        private List<NumericUpDown> filterUpDowns = new List<NumericUpDown>();
        private List<NumericUpDown> imageSegmentingAdjusters = new List<NumericUpDown>();

        // Form initialisation.
        private void ImageViewer_Load(object sender, EventArgs e)
        {
            textBoxIP.Text = robotURL;
            picboxCameraImage.Size = new Size(352, 288);
            Bitmap b = new Bitmap(352, 288);
            Graphics g = Graphics.FromImage(b);
            g.FillRectangle(new SolidBrush(Color.White), new Rectangle(0, 0, 352, 288));
            g.DrawString("Not connected", new Font(FontFamily.GenericSansSerif, 12), new SolidBrush(Color.Black), new Point(120, 135));
            picboxCameraImage.Image = b;
            upDownHandler += new EventHandler(UpDownHandler);
            FilterChangersSetup(filterUpDowns, "green");
            FilterChangersSetup(filterUpDowns, "red");
            FilterChangersSetup(filterUpDowns, "blue");
            FilterChangersSetup(filterUpDowns, "yellow");
            FilterChangersSetup(filterUpDowns, "white");


            SetUserControlLabels();

            robotButtonEventHandler = RobotButtonHandler;

            buttonPredator.Click += robotButtonEventHandler;
            buttonPredatorFSM.Click += robotButtonEventHandler;
            buttonUser.Click += robotButtonEventHandler;
            buttonStop.Click += robotButtonEventHandler;

            Show();
            Text = "BLA11210972 Computer Vision & Robotics Predator/Prey";
            Label l = new Label();

            //Show();
            //imageSegmentingAdjustersTest[0].ValueChanged += handler;
            // Start predator thread.
            Focus();



        }

        // Set robot based on the string from the form button pressed.
        private void InitialiseRobot(string type)
        {

            //System.Threading.Thread.Sleep(2000);
            buttonPredator.Enabled = true;
            buttonUser.Enabled = true;
            buttonPredatorFSM.Enabled = true;
            picBoxUserLabels.Visible = false;
            SetFilterChangerVisibility(false);

            if (robot != null)
            {
                robot.KillThreads();
                while (robot_thread.ThreadState != System.Threading.ThreadState.Stopped && robot_thread.ThreadState != System.Threading.ThreadState.WaitSleepJoin)
                    System.Threading.Thread.Sleep(1);

                if (map != null)
                {
                    map.Hide();
                    map = null;
                }
            }


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
                robot_thread = new System.Threading.Thread(robot.Start);
                robot_thread.Start();
            }
            else if (type == "PredatorFSM")
            {
                if (map != null)
                    map.Hide();
                robot = new Rovio.PredatorSimple(robotURL, "user", "password", map, currentKeys);
                (robot as Rovio.BaseArena).SourceImage += UpdateImage;

                updateTimer.Start();

                if (valueDict.Count == 0)
                    ReadDictValues();
                (robot as Rovio.BaseArena).SetFilters(valueDict);
                SetFilterChangerVisibility(true);
                robot_thread = new System.Threading.Thread(robot.Start);
                robot_thread.Start();
            }
            else if (type == "User")
            {
                if (map != null)
                    map.Hide();
                robot = new Rovio.UserRobot(robotURL, "user", "password", map, currentKeys);
                (robot as Rovio.UserRobot).SourceImage += UpdateImage;
                picBoxUserLabels.Visible = true;
                robot_thread = new System.Threading.Thread(robot.Start);
                robot_thread.Start();
            }

            if (type == "Predator" || type == "PredatorFSM" || type == "User")
            {
                picboxCameraImage.Location = new Point(20, 22);
                picboxCameraImage.Size = new Size((int)robot.cameraDimensions.X, (int)robot.cameraDimensions.Y);
                textBoxIP.Enabled = false;
                buttonPredator.Enabled = false;
                buttonUser.Enabled = false;
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

        // Set filter changers in form.
        private void FilterChangersSetup(List<NumericUpDown> list, string col)
        {
            Point imageSegmentingAdjustingLocation = new Point(picboxCameraImage.Location.X + picboxCameraImage.Size.Width + 30 + (120 * (list.Count / 12)/3), 85);
            int limit = list.Count;
            try
            {
                imageSegmentingAdjustingLocation = new Point(picboxCameraImage.Location.X + picboxCameraImage.Size.Width + 30 + (int)(120 * (list.Count / 6) - (12 % limit) * 30 ), 85 + (int)(12 % (limit) * 8.2f));
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
                Controls.Add(list[i]);
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
                list[i].Visible = false;

                filterLabels.Add(new Label());
                filterLabels[list.Count / 6].Location = new Point(list[limit].Location.X + 32, list[limit].Location.Y - 15);
                filterLabels[list.Count / 6].Size = new Size(40, 15);
                filterLabels[list.Count / 6].Text = col[0].ToString().ToUpper() + col.Substring(1);
                Controls.Add(filterLabels[list.Count / 6]);

                
            }
            for (int i = 0; i < filterLabels.Count; i++)
                filterLabels[i].Visible = false;
        }

        // Handler for the buttons to switch robot types.
        private void RobotButtonHandler(object sender, EventArgs e)
        { 
            InitialiseRobot((sender as Button).Text);
        }

        // Choose whether the filter changers appear on the form.
        private void SetFilterChangerVisibility(bool input)
        {
            for (int i = 0; i < filterUpDowns.Count; i++)
            {
                if (input)
                    filterUpDowns[i].Show();
                else
                    filterUpDowns[i].Hide();
                filterUpDowns[i].Enabled = input;
            }
            for (int i = 0; i < filterLabels.Count; i++)
                filterLabels[i].Visible = input;
        }

        // Set the labels to appear when user controls are enabled.
        private void SetUserControlLabels()
        {
            Bitmap b = new Bitmap(300, 300);
            
            Graphics g = Graphics.FromImage(b);
            g.FillRectangle(new SolidBrush(this.BackColor), new Rectangle(0, 0, 300, 300));
            SolidBrush brushBlack = new SolidBrush(Color.Black);
            SolidBrush brushRed = new SolidBrush(Color.Red);
            g.DrawString("Forwards:     ", new Font(FontFamily.GenericSansSerif, 15), brushRed, new Point(0, 0));
            g.DrawString("Left:         ", new Font(FontFamily.GenericSansSerif, 15), brushRed, new Point(0, 30));
            g.DrawString("Right:        ", new Font(FontFamily.GenericSansSerif, 15), brushRed, new Point(0, 60));
            g.DrawString("Backwards:    ", new Font(FontFamily.GenericSansSerif, 15), brushRed, new Point(0, 90));
            g.DrawString("Rotate left:  ", new Font(FontFamily.GenericSansSerif, 15), brushRed, new Point(0, 120));
            g.DrawString("Rotate right: ", new Font(FontFamily.GenericSansSerif, 15), brushRed, new Point(0, 150));

            g.DrawString("W", new Font(FontFamily.GenericSansSerif, 15), brushBlack, new Point(150, 0));
            g.DrawString("A", new Font(FontFamily.GenericSansSerif, 15), brushBlack, new Point(150, 30));
            g.DrawString("S", new Font(FontFamily.GenericSansSerif, 15), brushBlack, new Point(150, 60));
            g.DrawString("D", new Font(FontFamily.GenericSansSerif, 15), brushBlack, new Point(150, 90));
            g.DrawString("Q ", new Font(FontFamily.GenericSansSerif, 15), brushBlack, new Point(150, 120));
            g.DrawString("E", new Font(FontFamily.GenericSansSerif, 15), brushBlack, new Point(150, 150));

            picBoxUserLabels.Size = b.Size;
            picBoxUserLabels.Image = b;
            picBoxUserLabels.Visible = false;
        }

        // Receive a picture box image from the classes.
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
               

            if (this.InvokeRequired)
            {
                try
                {
                    picboxCameraImage.Image = image;
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
            if (robot != null)
            {
                robot.KillThreads();
                while (robot_thread.ThreadState != System.Threading.ThreadState.Stopped && robot_thread.ThreadState != System.Threading.ThreadState.WaitSleepJoin)
                    System.Threading.Thread.Sleep(1);
            }
            Application.Exit();
            Environment.Exit(0);
        }

        // Key down - adds key to dictionary if it is not already there.
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

        // Handles changing values for the filter numerical up downs.
        private void UpDownHandler(object sender, EventArgs e)
        { 
            if (valueDict.ContainsKey((sender as NumericUpDown).Name.ToString()))
            {
                valueDict[(sender as NumericUpDown).Name.ToString()] = (float)(sender as NumericUpDown).Value;
                (robot as Rovio.BaseArena).SetFilters(valueDict);
            }
        }

        // Receives new string value when the IP text box is changed.
        private void textBoxIP_TextChanged(object sender, EventArgs e)
        {
            robotURL = textBoxIP.Text;
        }

    }
}
