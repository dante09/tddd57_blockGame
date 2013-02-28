using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Kinect;

namespace BlockGame
{
    abstract class GestureRecognizer : GameComponent
    {
        protected int maxErrors;
        protected int amountOfErrors = 0;
        protected Skeleton skel;
        protected int poseKeptTime = 0;
        protected int holdFor;
        protected int timesPoseCompleted = 0;

        public GestureRecognizer(Game game,Skeleton skel,int errorCount,int holdFor) : base(game)
        {
            this.maxErrors = errorCount;
            this.holdFor = holdFor;
            this.skel = skel;
        }

        public void reset()
        {
            amountOfErrors = 0;
            poseKeptTime = 0;
            timesPoseCompleted = 0;
        }
    }
}
