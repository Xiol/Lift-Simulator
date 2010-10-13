using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LiftSimulator
{
    public partial class frmLiftSim : Form
    {
        /*********************************************/
        readonly int[] FLOOR_Y = new int[5] { 344, 259, 173, 87, 3 }; // y axis of floor -> G,1,2,3,4
        readonly int[] IDLE_FLOORS = new int[3] { 0, 2, 4 }; // floors the the lift's idle/start on

        Lift lift1;
        Lift lift2;
        Lift lift3;


        /*********************************************/

        public frmLiftSim()
        {
            InitializeComponent();
        }

        private void frmLiftSim_Load(object sender, EventArgs e)
        {
            // Create our lift objects, assigning them the image they need to control,
            // the array of floor locations, and which floors they should 'idle' on.
            lift1 = new Lift(pbxLift1, FLOOR_Y, IDLE_FLOORS[0]);
            lift2 = new Lift(pbxLift2, FLOOR_Y, IDLE_FLOORS[1]);
            lift3 = new Lift(pbxLift3, FLOOR_Y, IDLE_FLOORS[2]);

            lift1.Move(4);
            lift2.Move(3);
            lift3.Move(2);
        }
    }
}
