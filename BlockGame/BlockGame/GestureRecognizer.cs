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
        protected int amountOfErrors;
        protected GetSkeleton getSkel;
        public int gestureKeptTime { get; protected set; }

        public int holdFor
        {
            get;
            protected set;
        }
        public bool gestureComplete
        {
            get;
            protected set;
        }

        public GestureRecognizer(Game game,GetSkeleton skel,int errorCount,int holdFor) : base(game)
        {
            this.maxErrors = errorCount;
            this.holdFor = holdFor;
            getSkel = skel;
            Reset();
        }

        public bool GestureStarted()
        {
            return gestureKeptTime > 0;
        }

        public void Reset()
        {
            amountOfErrors = 0;
            gestureKeptTime = 0;
            gestureComplete = false;
        }
    }
}
