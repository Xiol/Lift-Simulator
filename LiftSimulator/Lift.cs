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
        private int idleFloor;                  // The floor which this lift starts on and returns to when idle
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
        }

        public void Move(int floor)
        {
            if (currentFloor < floor)
            {
                currentDirection = Direction.UP;
                //nextDestFloor = floor; // TEMPORARY
                //currentFloor = -1;
                dtMove.Start();
            }
            else if (currentFloor > floor)
            {
                currentDirection = Direction.DOWN;
                //nextDestFloor = floor; // TEMPORARY
                //currentFloor = -1;
                dtMove.Start();
            }
            else
            {
                currentDirection = Direction.IDLE;
                destQueue[currentFloor] = 0;
                return;
            }
        }

        public void AddDest(int floor)
        {
            destQueue[floor] = 1;
            mainForm.AddToLog("Lift " + liftID + " destination added: " + floor.ToString());
            //mainForm.AddToLog(destQueue[0] + " " + destQueue[1] + " " + destQueue[2] + " " + destQueue[3] + " " + destQueue[4]);
            if (!dtWait.IsEnabled)
            {
                // If we're waiting already then the timer will call MoveNext for us.
                MoveNext();
            }
        }

        public bool IsDest(int floor)
        {
            if (destQueue[floor] == 1) { return true; } else { return false; }
        }

        private bool IsQueued(int startFloor, Direction direction)
        {
            // Check to see if there is any floors waiting to be serviced in the direction
            // specified, from the floor specified.
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
                currentFloor = nextDestFloor;
                dtMove.Stop();
                destQueue[currentFloor] = 0;
                mainForm.ResetLiftButton(currentFloor, liftID);

                if (IsQueued(currentFloor, currentDirection) && currentDirection == Direction.UP)
                {
                    // If we're going up then reset the Up call button for this floor
                    mainForm.ResetCallButton(currentFloor, 0);
                }
                else if (IsQueued(currentFloor, currentDirection) && currentDirection == Direction.DOWN)
                {
                    // Else reset the down one
                    mainForm.ResetCallButton(currentFloor, 1);
                }
                else if (IsQueueEmpty)
                {
                    mainForm.ResetCallButton(currentFloor, 0);
                    mainForm.ResetCallButton(currentFloor, 1);
                }
                dtWait.Start();
            }
            else
            {
                // If currentDirection is -1 this will move the lift down, otherwise up, every tick
                liftImage.Top = liftImage.Top + (int)currentDirection;
                SetCurrentFloor(currentDirection);
            }
        }

        private void dtWait_Tick(object sender, EventArgs e)
        {
            dtWait.Stop();
            MoveNext();
        }

        public int GetCurrentFloor
        {
            get { return currentFloor; }
        }

        public int GetNextDest
        {
            get { return nextDestFloor; }
        }

        public void SetCurrentFloor(Direction dir)
        {
            if (dir == Direction.UP)
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
            else if (dir == Direction.DOWN)
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
        }

        public Direction GetCurrentDirection
        {
            get { return currentDirection; }
        }

        public bool IsIdle
        {
            get { if (GetCurrentDirection == 0) { return true; } else { return false; } }
        }

        public bool IsMoving
        {
            get { if (dtMove.IsEnabled) { return true; } else { return false; } }
        }

        public bool IsQueueEmpty
        {
            get
            {
                if (destQueue.Contains(1)) { return false; } else { return true; }
            }
        }
        public bool IsTravelling(int dir)
        {
            if (GetCurrentDirection == (Direction)dir) { return true; }
            else if (IsIdle) { return true; }
            else { return false; } 
        }
    }
}
