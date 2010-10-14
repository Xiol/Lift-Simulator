using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiftSimulator
{
    class LiftController
    {
        // List of lifts we control
        private List<Lift> lifts = new List<Lift>();

        // Up and down queues for when 'people' push the up/down buttons on the floors
        private int[] upQueueWaiting = new int[5] { 0, 0, 0, 0, 0 };
        private int[] downQueueWaiting = new int[5] { 0, 0, 0, 0, 0 };

        public LiftController()
        {
        }

        public void AddLift(Lift l)
        {
            lifts.Add(l);
        }

        public void GoingUp(int calledFrom)
        {
            upQueueWaiting[calledFrom] = 1;
        }

        public void GoingDown(int calledFrom)
        {
            downQueueWaiting[calledFrom] = 1;
        }
    }
}
