﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiftSimulator
{
    class LiftController
    {
        public enum Direction { UP = -1, IDLE, DOWN };   // Up = -1, Idle = 0, Down = 1
        // List of lifts we control
        private Lift[] lifts;
        // Index for adding items to the lifts array
        private int ali = 0;
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

        public LiftController(int lnum)
        {
            lifts = new Lift[lnum];
        }

        public void AddLift(Lift l)
        {
            lifts[ali] = l;
            ali++;
        }

        public bool IsLiftOnFloor(int floor)
        {
            foreach (Lift lift in lifts)
            {
                if (lift.GetCurrentFloor == floor && !lift.IsMoving) { return true; }
            }
            return false;
        }

        public bool LiftHasDest(int floor)
        {
            foreach (Lift lift in lifts)
            {
                if (lift.IsDest(floor)) { return true; }
            }

            return false;
        }

        public void SendLift(int floor, int dir)
        {
            int nl = GetNearestLift(floor, dir);

            // If nl = -1 then there is already a lift on that floor
            if (nl != -1)
            {
                lifts[nl].AddDest(floor);
            }
        }

        private int GetNearestLift(int floor, int trav)
        {
            // TODO: Check direction lift is already travelling
            int liftToSend = -1;
            int highestPrio = -1;

            for (int i = 0; i < lifts.Count(); i++)
            {
                if (lifts[i].IsTravelling(trav) && !LiftHasDest(floor))
                {
                    int liftFloor = lifts[i].GetCurrentFloor;
                    int liftPrio = liftFlrPrio[liftFloor, floor];

                    // Lift already on the floor
                    // TODO: nextDest of that lift needs to be in the same direction
                    if (liftPrio == 0) { return -1; }

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

            return liftToSend;
        }
    }
}
