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
        readonly int[] IDLE_FLOORS = new int[3] { 0, 0, 4 }; // floors the the lift's idle/start on

        private Button[][] liftButtons = new Button[3][];
        private PictureBox[][] callButtons = new PictureBox[5][];

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

            // Same thing for our call 'buttons'
            callButtons[0] = new PictureBox[2] { null,     pbx0Up };
            callButtons[1] = new PictureBox[2] { pbx1Down, pbx1Up };
            callButtons[2] = new PictureBox[2] { pbx2Down, pbx2Up };
            callButtons[3] = new PictureBox[2] { pbx3Down, pbx3Up };
            callButtons[4] = new PictureBox[2] { pbx4Down, null };
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
            // Casting to an integer doesn't work either...
            int lift = Int32.Parse(buttonTag[0].ToString());
            int floor = Int32.Parse(buttonTag[1].ToString());

            bool destOK = false;

            switch (lift)
            {
                case 1:
                    if (lift1.GetCurrentFloor != floor && !lift1.IsDest(floor)) 
                    { 
                        lift1.AddDest(floor); destOK = true; 
                    }
                    break;
                case 2:
                    if (lift2.GetCurrentFloor != floor && !lift2.IsDest(floor)) 
                    {
                        lift2.AddDest(floor); destOK = true; 
                    }
                    break;
                case 3:
                    if (lift3.GetCurrentFloor != floor && !lift3.IsDest(floor)) 
                    { 
                        lift3.AddDest(floor); destOK = true; 
                    }
                    break;
                default:
                    break;
            }

            if (destOK)
            {
                buttonSender.BackColor = Color.DarkRed;
                buttonSender.ForeColor = Color.White;
            }
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
                lc.SendLift(floor, 1);
            }
            else
            {
                // Going up
                picSender.Image = Properties.Resources.up_arrow_active;
                lc.SendLift(floor, -1);
            }
        }

        /// <summary>
        /// Reset the specifed call button by changing the image back to the intial one.
        /// </summary>
        /// <param name="floor">The floor of the call button.</param>
        /// <param name="direction">1 for the Up call button, 0 for the Down call button.</param>
        public void ResetCallButton(int floor, int direction)
        {
            if (callButtons[floor][direction] != null && direction < 2)
            {
                if (direction == 0)
                {
                    callButtons[floor][direction].Image = Properties.Resources.down_arrow;
                }
                else
                {
                    callButtons[floor][direction].Image = Properties.Resources.up_arrow;
                }
            }
        }

        /// <summary>
        /// Reset the foreground and background colours of the specified lift button.
        /// </summary>
        /// <param name="floor">The number of the floor whose button we need to reset.</param>
        /// <param name="lift">Which Lift does this button correspond to.</param>
        public void ResetLiftButton(int floor, int lift)
        {
            Button btn = liftButtons[lift - 1][floor];
            btn.ForeColor = Color.Black;
            btn.BackColor = Color.Gainsboro;
        }

        /// <summary>
        /// Add a line of text to the log.
        /// </summary>
        /// <param name="text">The text to add. Carriage returns added automatically.</param>
        public void AddToLog(string text)
        {
            tbxLog.AppendText(text + "\r\n");
        }

        /// <summary>
        /// Updates the lift floor labels.
        /// </summary>
        /// <param name="floor">The floor number to display.</param>
        /// <param name="lift">The lift whose number to update.</param>
        public void UpdateFloorDisp(int floor, int lift)
        {
            switch (lift)
            {
                case 1:
                    lblFlrDisp1.Text = floor.ToString();
                    break;
                case 2:
                    lblFlrDisp2.Text = floor.ToString();
                    break;
                case 3:
                    lblFlrDisp3.Text = floor.ToString();
                    break;
                default:
                    break;
            }
        }

        private void btnToBottom_Click(object sender, EventArgs e)
        {
            // Moves all lifts to the bottom floor.
            _ResetButtons();
            lc.MoveAllToBottom();
            AddToLog("Moving all lifts to 0");
        }

        private void btnToTop_Click(object sender, EventArgs e)
        {
            // Moves all lifts to the top floor.
            _ResetButtons();
            lc.MoveAllToTop();
            AddToLog("Moving all lifts to 4");
        }

        private void btnResetAll_Click(object sender, EventArgs e)
        {
            // Moves all lifts to their starting/idle positions.
            _ResetButtons();
            lc.MoveAllToIdle();
            AddToLog("Moving all lifts to starting floors");
        }

        private void btnResetButtons_Click(object sender, EventArgs e)
        {
            _ResetButtons();   
        }

        private void _ResetButtons()
        {
            // Loops through all the buttons on the form and resets
            // them back to their original state.
            foreach (Button[] liftBtn in liftButtons)
            {
                foreach (Button btn in liftBtn)
                {
                    btn.ForeColor = Color.Black;
                    btn.BackColor = Color.Gainsboro;
                }
            }

            for (int i = 0; i < 5; i++)
            {
                for (int n = 0; n < 2; n++)
                {
                    ResetCallButton(i, n);
                }  
            }
        }

        private void tbxLog_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
