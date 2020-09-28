using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;

namespace BassClefStudio.SkiaSharp.Helpers
{
    /// <summary>
    /// Represents a dynamic viewport for displaying SkiaSharp graphics.
    /// </summary>
    public interface ICamera
    {
        /// <summary>
        /// The size of the game, in pixels, as a <see cref="Vector2"/> (width, height).
        /// </summary>
        Vector2 ViewSize { get; }

        /// <summary>
        /// The scale of the game view, the size of which is defined by <see cref="ViewSize"/>, as a float.
        /// </summary>
        float ViewScale { get; }

        /// <summary>
        /// The position of the camera relative to the world, as a <see cref="Vector2"/>.
        /// </summary>
        Vector2 CameraPosition { get; }

        /// <summary>
        /// The zoom factor of the camera relative to the world.
        /// </summary>
        float CameraScale { get; }
    }


    /// <summary>
    /// A base version of <see cref="ICamera"/> that supports various zooms, lerping, and movement commands.
    /// </summary>
    public class Camera : ICamera
    {
        /// <inheritdoc/>
        public Vector2 ViewSize { get; private set; }

        /// <inheritdoc/>
        public float ViewScale { get; private set; }

        /// <inheritdoc/>
        public Vector2 CameraPosition { get; private set; }

        /// <inheritdoc/>
        public float CameraScale { get; private set; }

        /// <summary>
        /// Creates a new <see cref="Camera"/>.
        /// </summary>
        public Camera()
        { }

        #region Movement

        /// <summary>
        /// Sets the <see cref="Camera"/>'s position and zoom relative to the world.
        /// </summary>
        /// <param name="pos">The position of the <see cref="Camera"/>.</param>
        /// <param name="scale">The zoom factor of the <see cref="Camera"/>.</param>
        public void SetCamera(Vector2 pos, float scale = 1)
        {
            CameraPosition = pos;
            CameraScale = scale;
        }

        /// <summary>
        /// Smoothly interpolates the <see cref="Camera"/> position and zoom towards given values at a specified speed.
        /// </summary>
        /// <param name="pos">The position of the <see cref="Camera"/>.</param>
        /// <param name="scale">The zoom factor of the <see cref="Camera"/>.</param>
        /// <param name="lerpSpeed">The inverse of the distance between the current value and the desired value to travel. Must be greater than 1.</param>
        public void LerpCamera(Vector2 pos, float scale = 1, float lerpSpeed = 10f)
        {
            if(lerpSpeed < 1)
            {
                throw new ArgumentOutOfRangeException("lerpSpeed", "The value of lerpSpeed must be a positive float greater than or equal to 1.");
            }

            CameraPosition += (pos - CameraPosition) / lerpSpeed;
            CameraScale += (scale - CameraScale) / lerpSpeed;
        }

        /// <summary>
        /// Sets the size and scale of the game view.
        /// </summary>
        /// <param name="size">The size of the game view as a <see cref="Vector2"/> (width, height).</param>
        /// <param name="scale">The scale factor of the game view.</param>
        public void SetView(Vector2 size, float scale = 1)
        {
            ViewSize = size;
            ViewScale = scale;
        }

        #endregion
        #region Main

        /// <summary>
        /// Moves the camera towards a given position with a specified speed while obeying the given <see cref="CameraBehavior"/> ruleset.
        /// </summary>
        /// <param name="camBehavior">The behavior of the <see cref="Camera"/> as a <see cref="CameraBehavior"/>, specifying desired zoom, bounds, and movement type.</param>
        /// <param name="position">The desired focus position for the <see cref="Camera"/> to move towards.</param>
        /// <param name="lerpSpeed">The inverse of the distance between the current value and the desired value to travel. Must be greater than 1.</param>
        public void MoveCamera(CameraBehavior camBehavior, Vector2 position, float lerpSpeed = 10f)
        {
            Bounds bounds = camBehavior.Bounds;
            if (camBehavior.BehaviorType == CameraBehaviorType.Fit)
            {
                float zoom = camBehavior.DesiredZoom ?? MinZoom(bounds);
                LerpCamera(bounds.Center(), zoom, lerpSpeed);
            }
            else if (camBehavior.BehaviorType == CameraBehaviorType.Fill)
            {
                float zoom = camBehavior.DesiredZoom ?? FullZoom(bounds);
                LerpCamera(BoundPosition(position, bounds, zoom), zoom, lerpSpeed);
            }
            else if (camBehavior.BehaviorType == CameraBehaviorType.Free)
            {
                float zoom = camBehavior.DesiredZoom ?? FullZoom(bounds);
                LerpCamera(position, zoom, lerpSpeed);
            }
        }

        #endregion
        #region Zoom

        private float MinZoom(Bounds bounds)
        {
            Vector2 size = bounds.Size();
            float xZoom = (ViewSize.X / ViewScale) / size.X;
            float yZoom = (ViewSize.Y / ViewScale) / size.Y;
            return xZoom < yZoom ? xZoom : yZoom;
        }

        private float FullZoom(Bounds bounds)
        {
            Vector2 size = bounds.Size();
            float xZoom = (ViewSize.X / ViewScale) / size.X;
            float yZoom = (ViewSize.Y / ViewScale) / size.Y;
            return xZoom < yZoom ? yZoom : xZoom;
        }

        #endregion
        #region Bounding

        private Vector2 BoundPosition(Vector2 pos, Bounds bounds, float camZoom)
        {
            Vector2 size = bounds.Size();
            float dX = (size.X - (ViewSize.X / (camZoom * ViewScale))) / 2;
            float dY = (size.Y - (ViewSize.Y / (camZoom * ViewScale))) / 2;

            dX = dX < 0 ? 0 : dX;
            dY = dY < 0 ? 0 : dY;

            Vector2 center = bounds.Center();
            Vector2 currentPos = pos;
            if (currentPos.X - center.X > dX)
            {
                currentPos.X = center.X + dX;
            }
            if (center.X - currentPos.X > dX)
            {
                currentPos.X = center.X - dX;
            }
            if (currentPos.Y - center.Y > dY)
            {
                currentPos.Y = center.Y + dY;
            }
            if (center.Y - currentPos.Y > dY)
            {
                currentPos.Y = center.Y - dY;
            }

            return currentPos;
        }

        #endregion
        #region Update

        private Stopwatch stopwatch = new Stopwatch();
        /// <summary>
        /// Updates the <see cref="Camera"/>'s position and scale (view and camera) according to a given <see cref="CameraBehavior"/> and view size, adjusting the <paramref name="lerpSpeed"/> to match a constant <paramref name="frameRate"/>.
        /// </summary>
        /// <param name="canvasSize">The size of the canvas.</param>
        /// <param name="focus">The focal point of the view, which the <see cref="Camera"/> will move towards.</param>
        /// <param name="cameraBehavior">A <see cref="CameraBehavior"/> object that will determine how the <see cref="Camera"/> behaves.</param>
        /// <param name="lerpSpeed">The inverse of the distance between the current value and the desired value to travel. Must be greater than 1.</param>
        /// <param name="frameTime">Indicates the desired frame time (in milliseconds), which will adjust the <see cref="Camera"/> moevement accordingly.</param>
        public void UpdateCamera(SKSize canvasSize, CameraBehavior cameraBehavior, Vector2 focus, float lerpSpeed = 10f, float frameTime = 30)
        {
            SetView(new Vector2(canvasSize.Width, canvasSize.Height), 1);

            float frames = 1;
            if (stopwatch.ElapsedMilliseconds <= lerpSpeed * frameTime && stopwatch.ElapsedMilliseconds > 0)
            {
                frames = stopwatch.ElapsedMilliseconds / frameTime;
            }

            MoveCamera(cameraBehavior, focus, lerpSpeed / frames);
            stopwatch.Restart();
        }

        #endregion
    }

    /// <summary>
    /// Provides information about how the <see cref="Camera"/> should behave.
    /// </summary>
    public class CameraBehavior
    {
        /// <summary>
        /// A bounding region that the <see cref="Camera"/> must restrict movement in.
        /// </summary>
        public Bounds Bounds { get; }

        /// <summary>
        /// A <see cref="CameraBehaviorType"/> specifying how the <see cref="Bounds"/> and <see cref="DesiredZoom"/> affect camera movement.
        /// </summary>
        public CameraBehaviorType BehaviorType { get; }

        /// <summary>
        /// A nullable <see cref="float"/> indicating the desired zoom; if not set, a value is selected based on the <see cref="BehaviorType"/>.
        /// </summary>
        public float? DesiredZoom { get; }

        /// <summary>
        /// Creates a new <see cref="CameraBehavior"/>.
        /// </summary>
        /// <param name="bounds">A bounding region that the <see cref="Camera"/> must restrict movement in.</param>
        /// <param name="regionType">A <see cref="CameraBehaviorType"/> specifying how the <see cref="Bounds"/> and <see cref="DesiredZoom"/> affect camera movement.</param>
        /// <param name="desiredZoom">A nullable <see cref="float"/> indicating the desired zoom; if not set, a value is selected based on the <see cref="BehaviorType"/>.</param>
        public CameraBehavior(Bounds bounds, CameraBehaviorType regionType = CameraBehaviorType.Free, float? desiredZoom = null)
        {
            BehaviorType = regionType;
            Bounds = bounds;
            if (Bounds == null && BehaviorType != CameraBehaviorType.Free)
            {
                throw new ArgumentException("The only time no bounds may be specified is if the BehaviorType is set to CameraBehaviorType.Free.");
            }
            DesiredZoom = desiredZoom;
        }
    }

    /// <summary>
    /// An enum indicating how a <see cref="Camera"/> will move and zoom when given a <see cref="CameraBehavior"/>.
    /// </summary>
    public enum CameraBehaviorType
    {
        /// <summary>
        /// The <see cref="Camera"/> will follow the focus point around the screen, zooming to the value indicated by <see cref="CameraBehavior.DesiredZoom"/> or to a value that fills the screen with the <see cref="CameraBehavior.Bounds"/>.
        /// </summary>
        Free = 0,

        /// <summary>
        /// The <see cref="Camera"/> will follow the focus point within the specified <see cref="CameraBehavior.Bounds"/>, zooming to the value indicated by <see cref="CameraBehavior.DesiredZoom"/> or to a value that fills the screen with the <see cref="CameraBehavior.Bounds"/>.
        /// </summary>
        Fill = 1,

        /// <summary>
        /// The <see cref="Camera"/> will stay put in the center of the region, zooming to the value indicated by <see cref="CameraBehavior.DesiredZoom"/> or to a value that fits the entire <see cref="CameraBehavior.Bounds"/> on the screen at one time.
        /// </summary>
        Fit = 2
    }

    /// <summary>
    /// Represents minimum and maximum X and Y values for creating <see cref="Camera"/> bounds.
    /// </summary>
    public class Bounds
    {
        /// <summary>
        /// The minimum X value.
        /// </summary>
        public float MinX { get; set; }

        /// <summary>
        /// The minimum Y value.
        /// </summary>
        public float MinY { get; set; }

        /// <summary>
        /// The maximum X value.
        /// </summary>
        public float MaxX { get; set; }

        /// <summary>
        /// The maximum Y value.
        /// </summary>
        public float MaxY { get; set; }

        /// <summary>
        /// Returns a <see cref="Vector2"/> point at the center of the <see cref="Bounds"/>.
        /// </summary>
        public Vector2 Center()
        {
            return new Vector2((MinX + MaxX) / 2, (MinY + MaxY) / 2);
        }

        /// <summary>
        /// Returns a <see cref="Vector2"/> (width, height) indicating the size of the <see cref="Bounds"/>.
        /// </summary>
        public Vector2 Size()
        {
            return new Vector2(MaxX - MinX, MaxY - MinY);
        }

        /// <summary>
        /// Creates a new <see cref="Bounds"/> from two <see cref="Vector2"/>s, ensuring the min and max values are set correctly.
        /// </summary>
        /// <param name="a">The first <see cref="Vector2"/> point.</param>
        /// <param name="b">The second <see cref="Vector2"/> point.</param>
        public Bounds(Vector2 a, Vector2 b)
        {
            var xs = new float[] { a.X, b.X };
            var ys = new float[] { a.Y, b.Y };

            MinX = xs.Min();
            MaxX = xs.Max();
            MinY = ys.Min();
            MaxY = ys.Max();
        }
    }
}
