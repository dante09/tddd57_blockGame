using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Kinect;

namespace BlockGame
{
    class RightArmUp : GestureRecognizer
    {

        public RightArmUp(Game game, GetSkeleton skel, int errorCount, int holdFor)
            : base(game, skel,errorCount,holdFor)
        {
        }

        public override void Update(GameTime gameTime)
        {
            Skeleton skel = getSkel();
            if (!gestureComplete && skel!=null)
            {           
                if (skel.Joints[JointType.WristRight].Position.Y
                        > skel.Joints[JointType.ShoulderCenter].Position.Y)
                {
                    amountOfErrors = 0;
                    gestureKeptTime += gameTime.ElapsedGameTime.Milliseconds;
                }
                else if (amountOfErrors >= maxErrors)
                {
                    gestureKeptTime = 0;
                    amountOfErrors = 0;
                }
                else
                {
                    amountOfErrors++;
                }
                if (gestureKeptTime >= holdFor)
                {
                    gestureKeptTime = holdFor;
                    gestureComplete = true;
                }
            }
            base.Update(gameTime);
        }
    }
}
