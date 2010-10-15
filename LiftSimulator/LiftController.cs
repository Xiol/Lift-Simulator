using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiftSimulator
{
    class LiftController
    {
        // List of lifts we control
        private Lift[] lifts;
        private int ali = 0;

        // Up and down queues for when 'people' push the up/down buttons on the floors
        private int[] upQueueWaiting = new int[5] { 0, 0, 0, 0, 0 };
        private int[] downQueueWaiting = new int[5] { 0, 0, 0, 0, 0 };

        public LiftController(int lnum)
        {
            lifts = new Lift[lnum];
        }

        public void AddLift(Lift l)
        {
            lifts[ali] = l;
            ali++;
        }

        public void GoingUp(int calledFrom)
        {
            upQueueWaiting[calledFrom] = 1;
        }

        public void GoingDown(int calledFrom)
        {
            downQueueWaiting[calledFrom] = 1;
        }

        public void SendLift(int floor)
        {
            int closest = -1;
            int lastdist = 0;

            for (int i = 0; i < lifts.Count() ; i++)
            {
                if (closest != -1)
                {
                    
                }
                else
                {
                    closest = i;
                }
            }
        }
    }
}
