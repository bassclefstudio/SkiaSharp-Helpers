using BassClefStudio.NET.Core;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace BassClefStudio.SkiaSharp.Helpers
{
    /// <summary>
    /// Represents a selectable region as displayed by a <see cref="RenderService{T}"/>.
    /// </summary>
    public class SelectionRegion : Observable
    {
        private SKPoint selectionCenter;
        /// <summary>
        /// A <see cref="SKPoint"/> representing the center of the selection region.
        /// </summary>
        public SKPoint SelectionCenter { get => selectionCenter; set => Set(ref selectionCenter, value); }

        private float radiusSquared;
        /// <summary>
        /// The maximum distance away from the <see cref="SelectionCenter"/> a point can be to still be considered selecting this <see cref="SelectionRegion"/>, squared.
        /// </summary>
        public float RadiusSquared { get => radiusSquared; set => Set(ref radiusSquared, value); }

        private object attachedObject;
        /// <summary>
        /// The model <see cref="object"/> that is rendered in this <see cref="SelectionRegion"/>.
        /// </summary>
        public object AttachedObject { get => attachedObject; set => Set(ref attachedObject, value); }

        /// <summary>
        /// Creates a new <see cref="SelectionRegion"/>.
        /// </summary>
        public SelectionRegion() { }

        /// <summary>
        /// Creates a new <see cref="SelectionRegion"/> for the given <see cref="object"/> at the given position.
        /// </summary>
        /// <param name="attached">The model <see cref="object"/> that is rendered in this <see cref="SelectionRegion"/>.</param>
        /// <param name="point">A <see cref="SKPoint"/> representing the center of the selection region.</param>
        /// <param name="radiusSquared">The maximum distance away from the <see cref="SelectionCenter"/> a point can be to still be considered selecting this <see cref="SelectionRegion"/>, squared.</param>
        public SelectionRegion(object attached, SKPoint point, float radiusSquared)
        {
            AttachedObject = attached;
            SelectionCenter = point;
            RadiusSquared = radiusSquared;
        }
    }
}
