using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Microsoft.Xna.Framework;
using PredatorPreyAssignment;
namespace Rovio
{
    class UserRobot : BaseRobot
    {

        public delegate void ImageReady(Image image);
        public event ImageReady SourceImage;
        bool moving;

        public UserRobot(string address, string user, string password, Map m)
            : base(address, user, password, m)
        {

        }

        // Call this at thread start, passing pointer to list of pressed keys
        public void User(object keys)
        {
            while (running)
            {
                lock (commandLock)
                {
                    try
                    {
                        SourceImage(cameraImage);
                    }
                    catch { }
                    Input((List<int>)keys);
                }
            }

        }

        // = Convert.ToInt32(a);

        // Take pressed keys for user movement. 
        public void Input(List<int> keys)
        {
            // while (true)
            //{

                if (keys.Count > 0)
                    moving = true;
                if (keys.Contains(87))
                {
                    Drive.Forward(1);
                }
                else if (keys.Contains(81))
                {
                    Drive.RotateLeft(5);
                }
                else if (keys.Contains(69))
                {
                    Drive.RotateRight(5);
                }
                else if (keys.Contains(83))
                {
                    Drive.Backward(1);
                }
                else if (keys.Contains(65))
                {
                    Drive.DiagForwardLeft(1);
                }
                else if (keys.Contains(68))
                {
                    Drive.DiagForwardRight(1);
                }
                else if (keys.Count == 0 && moving)
                {
                    Drive.Stop();
                    moving = false;
                }
            
        }
        
    }
}
