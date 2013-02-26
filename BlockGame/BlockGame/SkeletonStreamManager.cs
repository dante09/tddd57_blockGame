
namespace BlockGame
{
    using System;
    using Microsoft.Kinect;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// This class is responsible for rendering a skeleton stream.
    /// </summary>
    public class SkeletonStreamManager : GameComponent
    {
        private int lastSkeletonUpdate = 0;

        public Skeleton creatorPlayer
        {
            get;
            private set;
        }

        public Skeleton placerPlayer
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the SkeletonStreamRenderer class.
        /// </summary>
        /// <param name="game">The related game object.</param>
        /// <param name="map">The method used to map the SkeletonPoint to the target space.</param>
        public SkeletonStreamManager(Game game)
            : base(game)
        {
        }

        /// <summary>
        /// This method initializes necessary values.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// This method retrieves a new skeleton frame if necessary.
        /// </summary>
        /// <param name="gameTime">The elapsed game time.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            lastSkeletonUpdate += gameTime.ElapsedGameTime.Milliseconds;
            if (lastSkeletonUpdate < 30) 
                return;
            KinectChooser chooser = (KinectChooser)this.Game.Services.GetService(typeof(KinectChooser));

            // If the sensor is not found, not running, or not connected, stop now
            if (null == chooser.Sensor ||
                false == chooser.Sensor.IsRunning ||
                KinectStatus.Connected != chooser.Sensor.Status)
            {
                return;
            }

            using (var skeletonFrame = chooser.Sensor.SkeletonStream.OpenNextFrame(0))
            {
             // Sometimes we get a null frame back if no data is ready
                if (null == skeletonFrame)
                {
                    return;
                }

                Skeleton[] skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                skeletonFrame.CopySkeletonDataTo(skeletons);
                double candidateDist1, candidateDist2 = candidateDist1 = Double.MaxValue;
                creatorPlayer = null;
                placerPlayer = null;
                foreach (Skeleton skel in skeletons)
                {
                    if (skel.Position.Z < candidateDist1 && skel.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        candidateDist2 = candidateDist1;
                        placerPlayer = creatorPlayer;
                        candidateDist1 = skel.Position.Z;
                        creatorPlayer = skel;
                    }
                    else if (skel.Position.Z < candidateDist2 && skel.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        candidateDist2 = skel.Position.Z;
                        placerPlayer = skel;
                    }
                }
                if (creatorPlayer!=null && placerPlayer != null && creatorPlayer.Position.X > placerPlayer.Position.X)
                {
                    Skeleton temp = creatorPlayer;
                    creatorPlayer = placerPlayer;
                    placerPlayer = temp;
                }
                placerPlayer = creatorPlayer;
             }
            lastSkeletonUpdate = 0;
        }

    }
}
