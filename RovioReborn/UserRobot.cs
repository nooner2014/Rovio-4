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
        public event ImageReady SourceImage;
        bool moving;

        public UserRobot(string address, string user, string password, Map m, Object k)
            : base(address, user, password, m, k)
        {

        }

        /// <summary>
        /// Override 
        /// </summary>
        public override void Start()
        {
            Bitmap outputImage = new Bitmap(cameraDimensions.X, cameraDimensions.Y);
            while (running)
            {
                //lock (commandLock)
                //{
                    outputImage = cameraImage;
                lock(commandLock)
                    SourceImage(outputImage);
                //}
            }
        }


        // = Convert.ToInt32(a);

        // Take pressed keys for user movement. 
        protected override void KeyboardInput()
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
