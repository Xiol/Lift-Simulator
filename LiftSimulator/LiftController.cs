using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiftSimulator
{
    class LiftController
    {
        // Copy of our Direction enum from Lift.cs... Not the best way to do it
        public enum Direction { UP = -1, IDLE, DOWN };   // Up = -1, Idle = 0, Down = 1

        // Array of lifts we control
        private Lift[] lifts;

        // Lift array index variable.
        private int lai = 0;

        // Used for picking a lift if we have a clash
        Random randGen = new Random();

        // This is magic. It represents the priority of each floor when a lift is on
        // a certain floor. i.e. if the lift is on the bottom floor and a request is
        // made to send a lift to floor 2 ([0,2]) then this will get a priority of 30, 
        // whereas the lift on floor 1 ([1,2]) will get a priority of 40, and therefore
        // is closer and should be sent instead.
        private int[,] liftFlrPrio = new int[5,5] {
            {0,40,30,20,10},
            {40,0,40,30,20},
            {30,40,0,40,30},
            {20,30,40,0,40},
            {10,20,30,40,0}
        };

        /// <summary>
        /// Creates a new LiftController to handle the Call buttons.
        /// </summary>
        /// <param name="lnum">The number of lifts this controller is going to handle.</param>
        public LiftController(int lnum)
        {
            lifts = new Lift[lnum];
        }

        /// <summary>
        /// Add a lift to the controller. Do not add more lifts than the controller was intended
        /// to handle when it was instantiated.
        /// </summary>
        /// <param name="lift">A Lift object.</param>
        public void AddLift(Lift lift)
        {
            lifts[lai] = lift;
            lai++;
        }

        /// <summary>
        /// Checks to see if a lift under the control of the LiftController
        /// is already on the floor specified. This will only return True
        /// if the lift is NOT moving.
        /// </summary>
        /// <param name="floor">The floor to check.</param>
        /// <returns>True if a lift is on the floor.</returns>
        public bool IsLiftOnFloor(int floor)
        {
            foreach (Lift lift in lifts)
            {
                if (lift.GetCurrentFloor == floor && !lift.IsMoving) { return true; }
            }
            return false;
        }

        /// <summary>
        /// Checks to see if a lift under the control of the LiftController already
        /// has the floor specifed in it's queue.
        /// </summary>
        /// <param name="floor">The floor to check.</param>
        /// <returns>True if a lift has that destination.</returns>
        public bool LiftHasDest(int floor)
        {
            foreach (Lift lift in lifts)
            {
                if (lift.IsDest(floor)) { return true; }
            }

            return false;
        }

        /// <summary>
        /// Sends the nearest lift that's travelling in the specified direction to the floor requested.
        /// </summary>
        /// <param name="floor">The floor to send the lift to.</param>
        /// <param name="dir">The direction the lift should be travelling in.</param>
        public void SendLift(int floor, int dir)
        {
            int nl = GetNearestLift(floor, dir);

            // If nl = -1 then there is already a lift on that floor
            if (nl != -1)
            {
                lifts[nl].AddDest(floor);
            }
        }

        /// <summary>
        /// Get the nearest lift to the floor specifed, that's travelling in the direction requested. Uses magic.
        /// </summary>
        /// <param name="destfloor">The floor that needs the lift.</param>
        /// <param name="trav">The direction the lift should be travelling in.</param>
        /// <returns>An integer corresponding to the lifts location in the lifts array.</returns>
        private int GetNearestLift(int destfloor, int trav)
        {
            int liftToSend = -1;
            int highestPrio = -1;

            // Loop through our lifts.
            for (int i = 0; i < lifts.Count(); i++)
            {
                // If there is already a lift on that floor then we do not need to send one.
                if (lifts[i].GetCurrentFloor == destfloor)
                {
                    return -1;
                }

                // Check to see if the lift is already travelling in the same direction
                // we wish to travel, and ensure that no lift already has this floor
                // as a destination.
                if (lifts[i].IsTravelling(trav) && !LiftHasDest(destfloor))
                {
                    int liftFloor = lifts[i].GetCurrentFloor;
                    // Get the priority of this lift in relation to the floor requested
                    int liftPrio = liftFlrPrio[liftFloor, destfloor];

                    if (highestPrio == liftPrio)
                    {
                        // There are two (or more) lifts that are within range.
                        // Randomly decide which lift we're going to send.
                        if (randGen.Next(1, 101) <= 50)
                        {
                            // Keep current lift.
                            continue;
                        }
                        else
                        {
                            // Send this one instead.
                            liftToSend = i;
                         }
                    }
                    else if (highestPrio < liftPrio)
                    {
                        highestPrio = liftPrio;
                        liftToSend = i;
                    }
                }
            }

            // When we have finished looping, we will the number of the nearest lift.
            return liftToSend;
        }

        /// <summary>
        /// Moves all the lifts under control of the LiftController to the bottom floor.
        /// </summary>
        public void MoveAllToBottom()
        {
            foreach (Lift lift in lifts)
            {
                lift.EmptyQueue();
                lift.AddDest(0);
            }
        }

        /// <summary>
        /// Moves all the lifts under control of the LiftController to the top floor.
        /// </summary>
        public void MoveAllToTop()
        {
            foreach (Lift lift in lifts)
            {
                lift.EmptyQueue();
                lift.AddDest(4);
            }
        }

        /// <summary>
        /// Moves all the lifts under control of the LiftController to their starting positions.
        /// </summary>
        public void MoveAllToIdle()
        {
            foreach (Lift lift in lifts)
            {
                lift.EmptyQueue();
                lift.AddDest(lift.idleFloor);
            }
        }
    }
}
