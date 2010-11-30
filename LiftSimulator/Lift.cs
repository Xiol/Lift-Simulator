using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Threading;

namespace LiftSimulator
{
    class Lift
    {
        /************************************************************************************************/
        private PictureBox liftImage;           // The object we need to control for this lift
        private int[] floor_y = new int[5];     // The y location of each floor
        public int idleFloor;                  // The floor which this lift starts on and returns to when idle
        private int currentFloor;               // Current location of the lift
        public enum Direction { UP = -1, IDLE, DOWN };   // Up = -1, Idle = 0, Down = 1
        private Direction currentDirection = Direction.IDLE;
        private int nextDestFloor;              // Next floor the lift is heading to
        private int[] destQueue = new int[5] { 0, 0, 0, 0, 0 };  // Array of lift buttons that have been pushed
        private frmLiftSim mainForm;            // Our parent form object so we can access methods there
        private int liftID;


        private DispatcherTimer dtMove = new DispatcherTimer();
        private DispatcherTimer dtWait = new DispatcherTimer();
        /************************************************************************************************/


        /// <summary>
        /// Create a new Lift object.
        /// </summary>
        /// <param name="limg">The PictureBox control this Lift object is associated to.</param>
        /// <param name="fy">The list of floor y coordinates</param>
        /// <param name="sf">The floor the lift will idle/start on</param>
        /// <param name="mf">The main form (pass in 'this')</param>
        public Lift(PictureBox limg, int[] fy, int sf, frmLiftSim mf, int id)
        {
            liftImage = limg;
            floor_y = fy;
            idleFloor = sf;
            mainForm = mf;
            liftID = id;

            liftImage.Top = floor_y[idleFloor];  // Move lift to the start position
            currentFloor = idleFloor;            // Set the lift's current position

            mainForm.AddToLog("Lift " + liftID + " initialised. Start floor: " + currentFloor);

            dtMove.Interval = TimeSpan.FromMilliseconds(20);
            dtMove.Tick += new EventHandler(dtMove_Tick);

            dtWait.Interval = TimeSpan.FromSeconds(2);
            dtWait.Tick += new EventHandler(dtWait_Tick);

            mainForm.UpdateFloorDisp(currentFloor, liftID);
        }

        /// <summary>
        /// Moves the lift to the specified floor.
        /// </summary>
        /// <param name="floor">The floor to move to.</param>
        public void Move(int floor)
        {
            if (currentFloor < floor)
            {
                currentDirection = Direction.UP;
                dtMove.Start();
            }
            else if (currentFloor > floor)
            {
                currentDirection = Direction.DOWN;
                dtMove.Start();
            }
            else
            {
                currentDirection = Direction.IDLE;
                destQueue[currentFloor] = 0;
                return;
            }
        }

        /// <summary>
        /// Add a destination to our queue. This function will call MoveNext() to recheck the
        /// queue and move to the new floor if it's between our current location and our original
        /// destination.
        /// </summary>
        /// <param name="floor">The floor to add to the queue.</param>
        public void AddDest(int floor)
        {
            if (currentFloor == floor) { return; }

            destQueue[floor] = 1;
            mainForm.AddToLog("Lift " + liftID + " destination added: " + floor.ToString());
            //mainForm.AddToLog(destQueue[0] + " " + destQueue[1] + " " + destQueue[2] + " " + destQueue[3] + " " + destQueue[4]);
            if (!dtWait.IsEnabled)
            {
                // If we're waiting already then the timer will call MoveNext for us.
                MoveNext();
            }
        }

        /// <summary>
        /// This will check if a floor is already in the queue.
        /// </summary>
        /// <param name="floor">The floor to check.</param>
        /// <returns>True if the floor is in the queue.</returns>
        public bool IsDest(int floor)
        {
            if (destQueue[floor] == 1) { return true; } else { return false; }
        }

        /// <summary>
        /// This will check to see if there are any floors that need servicing in the direction
        /// specified, from the floor specified. Once it reaches the end of the queue in that
        /// direction, it will return false if it does not find anything.
        /// </summary>
        /// <param name="startFloor">The floor to being our search from.</param>
        /// <param name="direction">The direction we wish to search.</param>
        /// <returns>True if floor requires service. False if nothing in that direction.</returns>
        private bool IsQueued(int startFloor, Direction direction)
        {
            if (direction == Direction.UP)
            {
                for (int i = startFloor; i <= 4; i++)
                {
                    if (destQueue[i] == 1) { return true; }
                }
            }
            else if (direction == Direction.DOWN)
            {
                for (int i = startFloor; i >= 0; i--)
                {
                    if (destQueue[i] == 1) { return true; }
                }
            }
            return false;
        }

        /// <summary>
        /// This function will check to see if there are any floors left in the queue in our
        /// current travelling direction. If there are, that will become our next destination
        /// and we shall start moving towards it. If not, it will check for floors to service
        /// in the opposite direction and start moving towards them.
        /// Failing that, the lift will go idle.
        /// </summary>
        public void MoveNext()
        {
            //if (dtMove.IsEnabled) { return; }

            int nextFloor = -1;
            int counter = 0;
            int change = 0;
            Direction oppDir = Direction.IDLE;

            if (currentDirection == Direction.DOWN) 
            {
                counter = -1; change = -1; oppDir = Direction.UP; 
            }
            else if (currentDirection == Direction.UP || currentDirection == Direction.IDLE) 
            {
                // If we're idle, start scanning up anyway.
                counter = 1; change = 1; oppDir = Direction.DOWN; 
            }

            while (nextFloor == -1)
            {
                int testFloor = currentFloor + counter;
                try
                {
                    if (IsDest(testFloor))
                    {
                        // Sanity check to make sure we've not moved past the floor already
                        // This stops the lifts from disappearing... in theory.
                        if (currentDirection == Direction.UP)
                        {
                            if (!(liftImage.Top < floor_y[testFloor])) { nextFloor = testFloor; break; }
                        }
                        else if (currentDirection == Direction.DOWN)
                        {
                            if (!(liftImage.Top > floor_y[testFloor])) { nextFloor = testFloor; break; }
                        }
                        else if (currentDirection == Direction.IDLE)
                        {
                            nextFloor = testFloor;
                            break;
                        }
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    // Nothing left in the queue in this direction
                    // Check to see if we have anything that needs servicing in the opposite direction.
                    if (IsQueued(currentFloor, oppDir))
                    {
                        currentDirection = oppDir;
                        MoveNext();
                    }
                    else
                    {
                        currentDirection = Direction.IDLE;
                        mainForm.AddToLog("Lift " + liftID + " going idle.");
                    }
                    break;
                }
                counter = counter + change;
            }

            if (nextFloor != -1)
            {
                mainForm.AddToLog("Lift " + liftID + " next destination: " + nextFloor);
                nextDestFloor = nextFloor;
                Move(nextFloor);
            }
        }

        private void dtMove_Tick(object sender, EventArgs e)
        {
            if (liftImage.Top == floor_y[nextDestFloor])
            {
                // We have arrived.
                dtMove.Stop();
                currentFloor = nextDestFloor;
                destQueue[currentFloor] = 0;    // Remove this floor from the queue
                mainForm.ResetLiftButton(currentFloor, liftID); // Reset the lift button for this floor
                mainForm.ResetCallButton(currentFloor, 0);
                mainForm.ResetCallButton(currentFloor, 1);
                dtWait.Start();
            }
            else
            {
                // We have not arrived! Carry on moving 1px per tick.
                // If currentDirection is -1 this will move the lift down, otherwise up, every tick
                liftImage.Top = liftImage.Top + (int)currentDirection;
                SetCurrentFloor();
            }
        }

        private void dtWait_Tick(object sender, EventArgs e)
        {
            // Wait for 'people' to get on/off the lift, then call MoveNext.
            dtWait.Stop();
            MoveNext();
        }

        /// <summary>
        /// Get the current floor of the lift. This does not guarentee that the lift 
        /// has, or will, stop on that floor.
        /// </summary>
        public int GetCurrentFloor
        {
            get { return currentFloor; }
        }

        /// <summary>
        /// Get the next destination of the lift.
        /// </summary>
        public int GetNextDest
        {
            get { return nextDestFloor; }
        }

        /// <summary>
        /// As the lift moves, this function should be called every tick to check the current
        /// location of the lift. As the lift moves between floors this will set the currentFloor
        /// variable to the correct value.
        /// </summary>
        public void SetCurrentFloor()
        {
            if (currentDirection == Direction.UP)
            {
                if ((floor_y[0] > liftImage.Top && floor_y[1] < liftImage.Top) || floor_y[0] == liftImage.Top)
                {
                    currentFloor = 0;
                }
                else if ((floor_y[1] > liftImage.Top && floor_y[2] < liftImage.Top) || floor_y[1] == liftImage.Top)
                {
                    currentFloor = 1;
                }
                else if ((floor_y[2] > liftImage.Top && floor_y[3] < liftImage.Top) || floor_y[2] == liftImage.Top)
                {
                    currentFloor = 2;
                }
                else if ((floor_y[3] > liftImage.Top && floor_y[4] < liftImage.Top) || floor_y[3] == liftImage.Top)
                {
                    currentFloor = 3;
                }
                else if (floor_y[4] == liftImage.Top)
                {
                    currentFloor = 4;
                }
                else
                {
                    currentFloor = -1; // this should not happen
                }
            }
            else if (currentDirection == Direction.DOWN)
            {
                if ((floor_y[4] < liftImage.Top && floor_y[3] > liftImage.Top) || floor_y[4] == liftImage.Top)
                {
                    currentFloor = 4;
                }
                else if ((floor_y[3] < liftImage.Top && floor_y[2] > liftImage.Top) || floor_y[3] == liftImage.Top)
                {
                    currentFloor = 3;
                }
                else if ((floor_y[2] < liftImage.Top && floor_y[1] > liftImage.Top) || floor_y[2] == liftImage.Top)
                {
                    currentFloor = 2;
                }
                else if ((floor_y[1] < liftImage.Top && floor_y[0] > liftImage.Top) || floor_y[1] == liftImage.Top)
                {
                    currentFloor = 1;
                }
                else if (floor_y[0] == liftImage.Top)
                {
                    currentFloor = 0;
                }
                else
                {
                    currentFloor = -1; // this should not happen
                }
            }
            mainForm.UpdateFloorDisp(currentFloor, liftID);
        }

        /// <summary>
        /// Gets the current direction of the lift.
        /// </summary>
        public Direction GetCurrentDirection
        {
            get { return currentDirection; }
        }

        /// <summary>
        /// Returns true if the lift is currently idling.
        /// </summary>
        public bool IsIdle
        {
            get { if (GetCurrentDirection == 0) { return true; } else { return false; } }
        }

        /// <summary>
        /// Returns true if the lift is moving.
        /// </summary>
        public bool IsMoving
        {
            get { if (dtMove.IsEnabled) { return true; } else { return false; } }
        }

        /// <summary>
        /// Returns true if the queue is empty.
        /// </summary>
        public bool IsQueueEmpty
        {
            get
            {
                if (destQueue.Contains(1)) { return false; } else { return true; }
            }
        }

        /// <summary>
        /// Function to check if the lift is currently travelling in the specified direction.
        /// </summary>
        /// <param name="dir">The direction of travel we wish to check.</param>
        /// <returns>True if we are travelling in the direction specified.</returns>
        public bool IsTravelling(int dir)
        {
            if (GetCurrentDirection == (Direction)dir) { return true; }
            else if (IsIdle) { return true; }
            else { return false; } 
        }

        /// <summary>
        /// Empties the queue. Because the queue is empty, this will also set
        /// the current direction to idle and stop the lift.
        /// </summary>
        public void EmptyQueue()
        {
            for (int i = 0; i < destQueue.Count(); i++)
            {
                destQueue[i] = 0;
            }
            dtMove.Stop();
            dtWait.Stop();
            currentDirection = Direction.IDLE;
            mainForm.AddToLog("Queue emptied for lift " + liftID);
        }
    }
}
