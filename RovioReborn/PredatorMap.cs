using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using PredatorPreyAssignment;

namespace Rovio
{
    class PredatorMap : BaseArena
    {

        System.Threading.Thread threadFindHeading;
        System.Threading.Thread threadFindWallDistance;
        System.Threading.Thread threadMovement;

        public PredatorMap(string address, string user, string password, Map m, Object k)
            : base(address, user, password, m, k)
        {
            
        }

        /// <summary>
        /// Overriden start method to call from main and begin threads.
        /// </summary>
        public override void Start()
        {
            //System.Threading.Thread getImage = new System.Threading.Thread(GetImage);
            //getImage.Start();

            System.Threading.Thread search = new System.Threading.Thread(SearchImage);
            search.Start();

            // System.Threading.Thread source = new System.Threading.Thread(ImageGet);
            //source.Start();

            //System.Threading.Thread threadMove = new System.Threading.Thread(SetFiniteStateMachine);
            //System.Threading.Thread move = new System.Threading.Thread(InitialMovements);
            //threadMove.Start();



            threadFindHeading = new System.Threading.Thread(FindHeading);
            threadFindHeading.Start();

            threadFindWallDistance = new System.Threading.Thread(() => FindWallDistance(ref wallDistance, false));
            threadFindWallDistance.Start();

            threadMovement = new System.Threading.Thread(Movement);
           // threadMovement.Start();
            while (running && connected)
            {
                //wallLineHeight = 0;
                //if (preyRectangle.Height != 0)
                //Console.WriteLine((float)25/preyRectangle.Height);

                //wallLineHeight -= 0.2f;

                //System.Threading.Thread.Sleep(1000);
                //lock (commandLock)
                //lock (mapLock)
                //if (!(trackingState == Tracking.Initial))
                //RotateByAngle(1, 6);

                
                //FindDirection();

            }
        }

        private void Movement()
        {
            while (running)
            {
                double targetAngle = 0;
                if (map.GetDestination() != new Point(-1, -1))
                {
                    while (Math.Abs(cumulativeAngle - targetAngle) > 35)
                    {
                        if (targetAngle > cumulativeAngle)
                            RotateByAngle(1, 3);
                        else
                            RotateByAngle(-1, 3);
                    }

                    MoveForward(5, 1);
                }

            }
        }
    }
}
