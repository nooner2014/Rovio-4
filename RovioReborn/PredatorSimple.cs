using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PredatorPreyAssignment;

namespace Rovio
{
    class PredatorSimple : BaseArena
    {

        enum PredatorState
        {
            SearchForObstacle,
            GoingAroundObstacle,
            ApproachingPrey,
            OnScreen,
            SearchingForPrey,
            //Roaming,
        };

        PredatorState trackingState = PredatorState.SearchForObstacle;
        GreenBlockState greenBlockState = GreenBlockState.InitialAproach;
        System.Threading.Thread threadSearch;
        System.Threading.Thread threadMove;


        public PredatorSimple(string address, string user, string password, Map m, Object k)
            : base(address, user, password, m, k)
        {
           // map = m;
            
        }

        /// <summary>
        /// Overriden start method to be called from form and start threads.
        /// </summary>
        public override void Start()
        {
            threadSearch = new System.Threading.Thread(SearchImage);
            threadSearch.Start();

            threadMove = new System.Threading.Thread(SetFiniteStateMachine);
            threadMove.Start();

            while (running && connected)
            {
               // System.Threading.Thread.Sleep(1000);
            }

            threadSearch.Join();
            threadMove.Join();
        }

        /// <summary>
        /// Begin the state machine.
        /// </summary>
        protected void SetFiniteStateMachine()
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
                   trackingState = PredatorState.OnScreen;
               }
               // If prey is out of view but has been recently seen, threadSearch with rotation.
               if (preyRectangle == new System.Drawing.Rectangle(0, 0, 0, 0) && searchingRotationCount < 8)
                   trackingState = PredatorState.SearchingForPrey;

               // If prey has been out of view and hasn't been found with rotation, start roaming.
               else if (searchingRotationCount >= 8 && trackingState != PredatorState.OnScreen && trackingState != PredatorState.GoingAroundObstacle)
               {
                   
                   trackingState = PredatorState.SearchForObstacle;

               }

               else if (trackingState != PredatorState.GoingAroundObstacle)
                   trackingState = PredatorState.OnScreen;



               if (trackingState == PredatorState.SearchingForPrey)
                   Search();
               else if (trackingState == PredatorState.SearchForObstacle)
               {
                   if (cumulativeAngle > 80 && IsObstacleSeen())
                       trackingState = PredatorState.GoingAroundObstacle;
                   else
                       RotateByAngle(1, 5);
               }
               else if (trackingState == PredatorState.GoingAroundObstacle)
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
                        RotateByAngle(1, 3);

                    lock (commandLock)
                        Drive.Stop();
                }
                if (trackingState == Tracking.BigSpin)
                {
                    if (cumulativeAngle > 160 && IsObstacleSeen())
                        trackingState = Tracking.GoingAroundBlock;
                    else
                        RotateByAngle(1, 3);

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

                    // If prey is out of view but has been recently seen, threadSearch with rotation.
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

        /// <summary>
        /// States exclusive to moving around the green block.
        /// </summary>
        enum GreenBlockState
        { 
            InitialAproach,
            StrafeAround,
            MoveForwardSome,
            Rotate,
        }

        /// <summary>
        /// Move around the green block (obstacle).
        /// </summary>
        protected void GoAroundBlock()
        {
            if (greenBlockState == GreenBlockState.InitialAproach)
            {
                if (IsObstacleSeen() && obstacleRectangle.Height < cameraDimensions.Y - (cameraDimensions.Y / 5))
                    MoveForward(1, 1);
                else if (IsObstacleSeen())
                    greenBlockState = GreenBlockState.StrafeAround;
                else
                    trackingState = PredatorState.SearchingForPrey;
            }
            else if (greenBlockState == GreenBlockState.StrafeAround)
            {
                if ((IsObstacleSeen() && obstacleRectangle.Width > 15))//|| !(obstacleRectangle == System.Drawing.Rectangle.Empty))//obstacleRectangle.X < cameraDimensions.Y - (cameraDimensions.Y / 5))
                    Strafe(4, -1);
                else
                greenBlockState = GreenBlockState.MoveForwardSome;
            }
            else if (greenBlockState == GreenBlockState.MoveForwardSome)
            {
                MoveForward(28, 1);
                greenBlockState = GreenBlockState.Rotate;
            }
            else if (greenBlockState == GreenBlockState.Rotate)
            { 
                RotateByAngle(2, 2);
                greenBlockState = GreenBlockState.InitialAproach;
                trackingState = PredatorState.SearchingForPrey;

            }
        }

        /// <summary>
        /// After losing prey, look around in the direction it was last seen.
        /// </summary>
        protected void Search()
        {
            // Rotate to look for prey - direction of rotation depends on which side of the screen prey left from.
            if (preyScreenPosition.X < cameraDimensions.X / 2)
                RotateDirection(1, -3);
            else
                RotateDirection(1, 3);
            lock (commandLock)
                Drive.Stop();
            // Keep track of how long we have been searching. If we've been looking for a while, resume roaming.
            searchingRotationCount++;
            //if (searchingRotationCount > 8)
                //trackingState = Tracking.Roaming;

        }

        /// <summary>
        /// Move forward towards the prey if it is in view.
        /// </summary>
        protected void Approach()
        {

            // If prey is not centred, rotate to it. Otherwise threadMove forward until it's within a certain threadFindWallDistance.

            searchingRotationCount = 0;
            if (preyScreenPosition.X < 0 + cameraDimensions.X / 5)
                RotateDirection(2, -4);
            else if (preyScreenPosition.X > cameraDimensions.X - cameraDimensions.X / 5)
                RotateDirection(2, 4);
            else if (preyRectangle.Width < 80)
            {
                trackingState = PredatorState.ApproachingPrey;
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

        /*
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
        }*/
    }
}
