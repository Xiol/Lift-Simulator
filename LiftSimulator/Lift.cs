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
        private bool waiting = false;           // set to true before starting wait timer, and spin until it becomes false again


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
        public Lift(PictureBox limg, int[] fy, int sf, frmLiftSim mf)
        {
            liftImage = limg;
            floor_y = fy;
            idleFloor = sf;
            mainForm = mf;

            liftImage.Top = floor_y[idleFloor];  // Move lift to the start position
            currentFloor = idleFloor;            // Set the lift's current position

            dtMove.Interval = TimeSpan.FromMilliseconds(25);
            dtMove.Tick += new EventHandler(dtMove_Tick);

            dtWait.Interval = TimeSpan.FromSeconds(3);
            dtWait.Tick += new EventHandler(dtWait_Tick);
        }

        public void Move(int floor)
        {
            if (currentFloor < floor)
            {
                currentDirection = Direction.UP;
                nextDestFloor = floor; // TEMPORARY
                dtMove.Start();
            }
            else if (currentFloor > floor)
            {
                currentDirection = Direction.DOWN;
                nextDestFloor = floor; // TEMPORARY
                dtMove.Start();
            }
            else
            {
                currentDirection = Direction.IDLE;
                return;
            }
        }

        public void AddDest(int floor)
        {
            destQueue[floor] = 1;

            if (currentDirection == Direction.IDLE)
            {
                // if the lift is doing nothing when the floor is added, start moving in that direction
                // anyway. We need to be careful here of triggering this method when the lift is waiting
                // at a floor if we're still setting currentDirection to 0 on a wait (don't do this then!)
                Move(floor);
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
            int nextFloor = -1;
            int counter = 0;
            Direction oppDir = Direction.IDLE;

            if (currentDirection == Direction.DOWN) { counter = -1; oppDir = Direction.UP; }
            else if (currentDirection == Direction.UP) { counter = 1; oppDir = Direction.DOWN; }

            while (nextFloor == -1)
            {
                int testFloor = currentFloor + counter;
                try
                {
                    if (IsDest(testFloor))
                    {
                        nextFloor = testFloor;
                        break;
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    // Nothing left in the queue in this direction
                    // Check to see if we have anything that needs servicing in the opposite direction.
                    currentDirection = oppDir;
                    if (IsQueued(currentFloor, oppDir)) { MoveNext(); } else { currentDirection = Direction.IDLE; }
                    break;
                }
                counter++;
            }

            if (nextFloor != -1)
            {
                Move(nextFloor);
            }
        }

        private void dtMove_Tick(object sender, EventArgs e)
        {
            if (liftImage.Top == floor_y[nextDestFloor])
            {
                currentFloor = nextDestFloor;
                if (currentDirection == Direction.UP)
                {
                    // If we're going up then reset the Up call button for this floor
                    mainForm.ResetCallButton(currentFloor, 0);
                }
                else
                {
                    // Else reset the down one
                    mainForm.ResetCallButton(currentFloor, 1);
                }
                dtMove.Stop();
                destQueue[currentFloor] = 0;
                dtWait.Start();
            }
            else
            {
                // If currentDirection is -1 this will move the lift down, otherwise up, every tick
                liftImage.Top = liftImage.Top + (int)currentDirection;
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

        public Direction GetCurrentDirection
        {
            get { return currentDirection; }
        }

        public bool IsIdle
        {
            get { if (GetCurrentDirection == 0) { return true; } else { return false; } }
        }
    }
}
