using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LiftSimulator
{
    class Lift
    {
        PictureBox lift_image;          // The object we need to control for this lift
        int[] floor_y = new int[5];     // The y location of each floor

        /// <summary>
        /// Create a new Lift object.
        /// </summary>
        /// <param name="limg">The PictureBox control this Lift object is associated to.</param>
        /// <param name="fy">The list of floor y coordinates</param>
        public Lift(PictureBox limg, int[] fy)
        {
            lift_image = limg;
            floor_y = fy;
        }
    }
}
