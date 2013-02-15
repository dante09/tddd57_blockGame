
namespace BlockGame
{
    using System;
    using Microsoft.Kinect;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// A delegate method explaining how to map a SkeletonPoint from one space to another.
    /// </summary>
    /// <param name="point">The SkeletonPoint to map.</param>
    /// <returns>The Vector2 representing the target location.</returns>
    public delegate Vector2 SkeletonPointMap(SkeletonPoint point);

    /// <summary>
    /// This class is responsible for rendering a skeleton stream.
    /// </summary>
    public class SkeletonStreamManager : GameComponent
    {
        private int lastSkeletonUpdate = 0;

        public Skeleton currentSkeleton
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
                double candidateDist = Double.MaxValue;
                currentSkeleton = null;
                foreach (Skeleton skel in skeletons)
                {
                    if (skel.Position.Z < candidateDist && skel.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        candidateDist = skel.Position.Z;
                        currentSkeleton = skel;
                    }
                }
             }
            lastSkeletonUpdate = 0;
        }

    }
}
