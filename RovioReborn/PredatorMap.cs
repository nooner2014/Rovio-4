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
            System.Threading.Thread search = new System.Threading.Thread(SearchImage);
            search.Start();

            threadFindHeading = new System.Threading.Thread(FindHeading);
            threadFindHeading.Start();

            threadFindWallDistance = new System.Threading.Thread(() => FindWallDistance(ref wallDistance, false));
            threadFindWallDistance.Start();

            threadMovement = new System.Threading.Thread(Movement);

            while (running && connected)
            {
            }
        }

        /// <summary>
        /// Incomplete implementation of following an A* path.
        /// </summary>
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
