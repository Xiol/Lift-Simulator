using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiftSimulator
{
    class LiftController
    {
        List<Lift> lifts = new List<Lift>();

        public LiftController()
        {
        }

        public void AddLift(Lift l)
        {
            lifts.Add(l);
        }
    }
}
