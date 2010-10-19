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
        private enum Direction { UP = -1, IDLE, DOWN };   // Up = -1, Idle = 0, Down = 1
        private int currentDirection = (int)Direction.IDLE;
        private int nextDestFloor;              // Next floor the lift is heading to
        private int[] destQueue = new int[5] { 0, 0, 0, 0, 0 };  // Array of lift buttons that have been pushed
        private frmLiftSim mainForm;            // Our parent form object so we can access methods there

        private DispatcherTimer dtMove = new DispatcherTimer();
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
        }

        public void Move(int floor)
        {
            if (currentFloor < floor)
            {
                currentDirection = (int)Direction.UP;
                nextDestFloor = floor; // TEMPORARY
                dtMove.Start();
            }
            else if (currentFloor > floor)
            {
                currentDirection = (int)Direction.DOWN;
                nextDestFloor = floor; // TEMPORARY
                dtMove.Start();
            }
            else
            {
                return;
            }
        }

        private void dtMove_Tick(object sender, EventArgs e)
        {
            if (liftImage.Top == floor_y[nextDestFloor])
            {
                // TEMPORARY - CHANGE ME
                currentFloor = nextDestFloor;
                if (currentDirection == (int)Direction.UP)
                {
                    // If we're going up then reset the Up call button for this floor
                    mainForm.ResetCallButton(currentFloor, 0);
                }
                else
                {
                    // Else reset the down one
                    mainForm.ResetCallButton(currentFloor, 1);
                }
                currentDirection = (int)Direction.IDLE;
                dtMove.Stop();
            }
            else
            {
                // If currentDirection is -1 this will move the lift down, otherwise up, every tick
                liftImage.Top = liftImage.Top + currentDirection;
            }
        }

        public int GetCurrentFloor
        {
            get { return currentFloor; }
        }

        public int GetCurrentDirection
        {
            get { return currentDirection; }
        }

        public bool IsIdle
        {
            get { if (GetCurrentDirection == 0) { return true; } else { return false; } }
        }
    }
}
