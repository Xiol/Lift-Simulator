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
        private int currentDirection = 0;       // 1 to go down, -1 to go up
        private int destFloor;                  // Destination floor -- TEMPORARY
        private int[] destQueue = new int[5] { 0, 0, 0, 0, 0 };  // Array of lift buttons that have been pushed

        private DispatcherTimer dtMove = new DispatcherTimer();
        /************************************************************************************************/


        /// <summary>
        /// Create a new Lift object.
        /// </summary>
        /// <param name="limg">The PictureBox control this Lift object is associated to.</param>
        /// <param name="fy">The list of floor y coordinates</param>
        public Lift(PictureBox limg, int[] fy, int sf)
        {
            liftImage = limg;
            floor_y = fy;
            idleFloor = sf;

            liftImage.Top = floor_y[idleFloor];  // Move lift to the start position
            currentFloor = idleFloor;            // Set the lift's current position

            dtMove.Interval = TimeSpan.FromMilliseconds(25);
            dtMove.Tick += new EventHandler(dtMove_Tick);
        }

        public void Move(int floor)
        {
            if (currentFloor < floor)
            {
                currentDirection = -1;
                destFloor = floor; // TEMPORARY
                dtMove.Start();
            }
            else if (currentFloor > floor)
            {
                currentDirection = 1;
                destFloor = floor; // TEMPORARY
                dtMove.Start();
            }
            else
            {
                return;
            }
        }

        private void dtMove_Tick(object sender, EventArgs e)
        {
            if (liftImage.Top == floor_y[destFloor])
            {
                // TEMPORARY - CHANGE ME
                currentFloor = destFloor;
                currentDirection = 0;
                dtMove.Stop();
            }
            else
            {
                liftImage.Top = liftImage.Top + currentDirection;
            }
        }

        public int GetCurrentFloor()
        {
            return currentFloor;
        }
    }
}
