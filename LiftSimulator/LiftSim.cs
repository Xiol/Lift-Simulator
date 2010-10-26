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

        private Button[][] liftButtons = new Button[3][];

        Lift lift1;
        Lift lift2;
        Lift lift3;

        LiftController lc = new LiftController(3);
        /*********************************************/

        public frmLiftSim()
        {
            InitializeComponent();
        }

        private void frmLiftSim_Load(object sender, EventArgs e)
        {
            // Create our lift objects, assigning them the image they need to control,
            // the array of floor locations, and which floors they should 'idle' on.
            lift1 = new Lift(pbxLift1, FLOOR_Y, IDLE_FLOORS[0], this, 1);
            lift2 = new Lift(pbxLift2, FLOOR_Y, IDLE_FLOORS[1], this, 2);
            lift3 = new Lift(pbxLift3, FLOOR_Y, IDLE_FLOORS[2], this, 3);

            // Hand our lifts over to our LiftController object
            lc.AddLift(lift1);
            lc.AddLift(lift2);
            lc.AddLift(lift3);

            // Stick our lift buttons into our array so we can control them easier (well, that's the plan)
            liftButtons[0] = new Button[5] { btnL1FG, btnL1F1, btnL1F2, btnL1F3, btnL1F4 };
            liftButtons[1] = new Button[5] { btnL2FG, btnL2F1, btnL2F2, btnL2F3, btnL2F4 };
            liftButtons[2] = new Button[5] { btnL3FG, btnL3F1, btnL3F2, btnL3F3, btnL3F4 };
        }

        public void LiftButtonHandler(object sender, EventArgs e)
        {
            // Each button has a tag associated with it. The tag is two digits - the first specifies
            // the lift the button belongs to, the second is the floor specified. e.g. 23 is lift 2, floor 3.
            // This handler will handle all the lift buttons without having to write a Click event handler
            // for each individual button.

            // The sender will be one of the lift buttons, so cast the sender to a Button object.
            Button buttonSender = (Button)sender;

            // Cast the Tag to a string 
            string buttonTag = (string)buttonSender.Tag;

            // Get the lift and floor that the sender button is referring to.
            // Strings are arrays of characters, so we can get the individual characters we need
            // FIXME: Convert.ToInt32 can parse characters, but returns the decimal ASCII value of those characters
            // So we need to get the character, convert it to a string, then convert it to an int.
            int lift = Int32.Parse(buttonTag[0].ToString());
            int floor = Int32.Parse(buttonTag[1].ToString());

            switch (lift)
            {
                case 1:
                    lift1.AddDest(floor);
                    break;
                case 2:
                    lift2.AddDest(floor);
                    break;
                case 3:
                    lift3.AddDest(floor);
                    break;
                default:
                    break;
            }

            buttonSender.BackColor = Color.DarkRed;
            buttonSender.ForeColor = Color.White;
        }

        public void CallButtonHandler(object sender, EventArgs e)
        {
            // Works on the same principle that LiftButtonHandler does above.
            PictureBox picSender = (PictureBox)sender;
            string picTag = (string)picSender.Tag;

            int floor = Int32.Parse(picTag[0].ToString());
            int direction = Int32.Parse(picTag[1].ToString());

            // this may need to be changed
            if (lc.IsLiftOnFloor(floor))
            {
                return;
            }

            if (direction == 0)
            {
                // Going down
                picSender.Image = Properties.Resources.down_arrow_active;
                lc.GoingDown(floor);
                lc.SendLift(floor);
            }
            else
            {
                // Going up
                picSender.Image = Properties.Resources.up_arrow_active;
                lc.GoingUp(floor);
                lc.SendLift(floor);
            }
        }

        public void ResetCallButton(int floor, int direction)
        {
            switch (floor)
            {
                case 0:
                    if (direction == 0) { pbx0Up.Image = Properties.Resources.up_arrow; }
                    break;
                case 1:
                    if (direction == 0) { pbx1Up.Image = Properties.Resources.up_arrow; }
                    else { pbx1Down.Image = Properties.Resources.down_arrow; }
                    break;
                case 2:
                    if (direction == 0) { pbx2Up.Image = Properties.Resources.up_arrow; }
                    else { pbx2Down.Image = Properties.Resources.down_arrow; }
                    break;
                case 3:
                    if (direction == 0) { pbx3Up.Image = Properties.Resources.up_arrow; }
                    else { pbx3Down.Image = Properties.Resources.down_arrow; }
                    break;
                case 4:
                    if (direction == 1) { pbx4Down.Image = Properties.Resources.down_arrow; }
                    break;
                default:
                    break;
            }
        }

        public void ResetLiftButton(int floor, int lift)
        {
            switch (lift)
            {
                case 1:
                    _ResetLiftButton(liftButtons[0][floor]);
                    break;
                case 2:
                    _ResetLiftButton(liftButtons[1][floor]);
                    break;
                case 3:
                    _ResetLiftButton(liftButtons[2][floor]);
                    break;
                default:
                    break;
            }
        }

        private void _ResetLiftButton(Button btn)
        {
            btn.ForeColor = Color.Black;
            btn.BackColor = SystemColors.Control;
        }
    }
}
