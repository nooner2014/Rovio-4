using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PredatorPreyAssignment;

namespace Rovio
{
    class PredatorStateMachine : BaseArena
    {

        enum Tracking
        {
            SearchForGreen,
            
            BigSpin,
            GoingAroundBlock,

            Searching,
            OnScreen,
            Approaching,
            //Roaming,
        };

        Tracking trackingState = Tracking.SearchForGreen;



        public PredatorStateMachine(string address, string user, string password, Map m, Object k)
            : base(address, user, password, m, k)
        {
           // map = m;
            
        }

        public override void Start()
        {
            System.Threading.Thread search = new System.Threading.Thread(SearchImage);
            search.Start();

            System.Threading.Thread move = new System.Threading.Thread(SetFSMAction);
            move.Start();

            while (running && connected)
            {
               // System.Threading.Thread.Sleep(1000);
            }
        }

        protected void SetFSMAction()
        {
            while (running)
            {
                //commandList = new List<Action>();
                // if (commandList.Count == 0)
                // {

                // Update state machine action based on latest readings.
               Console.WriteLine(trackingState.ToString());
               Console.WriteLine(cumulativeAngle);
               if (preyRectangle != new System.Drawing.Rectangle(0, 0, 0, 0))
               {
                   cumulativeAngle = 0;
                   searchingRotationCount = 0;
                   preyScreenPosition = preyRectangle;
               }
               // If prey is out of view but has been recently seen, search with rotation.
               if (preyRectangle == new System.Drawing.Rectangle(0, 0, 0, 0) && searchingRotationCount < 8)
                   trackingState = Tracking.Searching;

               // If prey has been out of view and hasn't been found with rotation, start roaming.
               else if (searchingRotationCount >= 8 && trackingState != Tracking.OnScreen)
               {
                   
                   trackingState = Tracking.BigSpin;

               }

               else
                   trackingState = Tracking.OnScreen;



               if (trackingState == Tracking.Searching)
                   Search();
               else if (trackingState == Tracking.BigSpin)
               {
                   if (cumulativeAngle > 80 && IsObstacleSeen())
                       trackingState = Tracking.GoingAroundBlock;
                   else
                       Rotate90(1, 5);
               }
               else if (trackingState == Tracking.GoingAroundBlock)
                   GoAroundBlock();
               else
                   Approach();


                /*
               if (IsPreySeen() && trackingState != Tracking.Approaching)
               {
                   lock(commandLock)
                    Drive.Stop();
                   trackingState = Tracking.Approaching;
               }

                if (trackingState == Tracking.SearchForGreen)
                {
                    if (IsObstacleSeen())
                    {
                        this.cumulativeAngle = 0;
                        trackingState = Tracking.BigSpin;
                    }
                    else
                        Rotate90(1, 3);

                    lock (commandLock)
                        Drive.Stop();
                }
                if (trackingState == Tracking.BigSpin)
                {
                    if (cumulativeAngle > 160 && IsObstacleSeen())
                        trackingState = Tracking.GoingAroundBlock;
                    else
                        Rotate90(1, 3);

                    lock (commandLock)
                        Drive.Stop();
                }
                if (trackingState == Tracking.GoingAroundBlock)
                    GoAroundBlock();
                if (trackingState == Tracking.Approaching)
                {
                    if (!IsPreySeen())
                        trackingState = Tracking.SearchForGreen;
                    else
                        Approach();
                }
                 * */
               // lock(commandLock)
                    //if (!(trackingState == Tracking.GoingAroundBlock))
                   // Drive.Stop();

                    /*
                else
                {
                    if (preyRectangle != new System.Drawing.Rectangle(0, 0, 0, 0))
                        preyScreenPosition = preyRectangle;

                    // If prey is out of view but has been recently seen, search with rotation.
                    if (preyRectangle == new System.Drawing.Rectangle(0, 0, 0, 0) && searchingRotationCount < 8)
                        trackingState = Tracking.SearchForGreen;
                    // If prey has been out of view and hasn't been found with rotation, start roaming.
                    else if (searchingRotationCount >= 8 && trackingState != Tracking.OnScreen)
                        trackingState = Tracking.Roaming;
                    // If it's not out of view, it must be on screen.
                    else
                        trackingState = Tracking.OnScreen;
                }
                // Perform the relevant state machine action.
                if (trackingState == Tracking.SearchForGreen)
                    Search();
                else if (trackingState == Tracking.Roaming)
                    Roam();
                else
                    Approach();
                     * 
                     * */
                //}
                //completed = true;
            }
        }

        enum GreenBlockState
        { 
            InitialAproach,
            StrafeAround,
            Rotate,
        }

        GreenBlockState greenBlockState = GreenBlockState.InitialAproach;
        protected void GoAroundBlock()
        {
            if (greenBlockState == GreenBlockState.InitialAproach)
            {
                if (IsObstacleSeen() && obstacleRectangle.Height < cameraDimensions.Y - (cameraDimensions.Y / 5))
                    MoveForward(1, 1);
                else if (IsObstacleSeen())
                    greenBlockState = GreenBlockState.StrafeAround;
            }
        }

        protected void Search()
        {
            // Rotate to look for prey - direction of rotation depends on which side of the screen prey left from.
            if (preyScreenPosition.X < cameraDimensions.X / 2)
                RotateDirection(2, -3);
            else
                RotateDirection(2, 3);
            // Keep track of how long we have been searching. If we've been looking for a while, resume roaming.
            searchingRotationCount++;
            //if (searchingRotationCount > 8)
                //trackingState = Tracking.Roaming;

        }

        protected void Approach()
        {

            // If prey is not centred, rotate to it. Otherwise move forward until it's within a certain distance.
            if (preyScreenPosition.X < 0 + cameraDimensions.X / 5)
                RotateDirection(2, -4);
            else if (preyScreenPosition.X > cameraDimensions.X - cameraDimensions.X / 5)
                RotateDirection(2, 4);
            else if (preyRectangle.Width < 80)
            {
                trackingState = Tracking.Approaching;
                if (obstacleRectangle.Height > cameraDimensions.Y - 10 && obstacleRectangle.Width > 180)
                    Strafe(5, 1);
                else if (obstacleRectangle.Height > cameraDimensions.Y - 10 && cameraDimensions.X - obstacleRectangle.Width > cameraDimensions.X - 180)
                    Strafe(5, -1);
                else
                    MoveForward(1, 1);
            }
            else
                searchingRotationCount = 0;
        }

        private void Roam()
        {
            //if (wallHeight > 20)
            //{

            // If we are near a wall or obstacle, drive back from it and then rotate in a direction depending on which way we're facing.
            if (irSensor || wallLineHeight > 15)
            {

                MoveForward(6, -1);
                if (wallLeftPoint.Y > wallRightPoint.Y)
                    RotateDirection(2, 3);
                else
                    RotateDirection(2, -3);
            }

            // If not directly in front of an obstacle, keep moving.
            else if ((obstacleRectangle.Height < 200 || obstacleRectangle.Width < 100))// && (wallLineRectangle.Height > 25 || wallLineRectangle.Height < 40))
                MoveForward(3, 1);
            // Must be in front of an obstacle, so rotate from it depending on which direction it's at.
            else
            {
                if (obstacleRectangle.X > cameraDimensions.X / 8)
                    RotateDirection(2, -3);
                else
                    RotateDirection(2, 3);
            }
           // trackingState = Tracking.Roaming;
        }
    }
}
