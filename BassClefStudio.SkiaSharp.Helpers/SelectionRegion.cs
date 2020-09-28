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
        /// A <see cref="SKPath"/> representing the bounds of the selection region.
        /// </summary>
        public SKPoint SelectionCenter { get => selectionCenter; set => Set(ref selectionCenter, value); }

        private float radius;
        /// <summary>
        /// The maximum distance away from the <see cref="SelectionCenter"/> a point can be to still be considered selecting this <see cref="SelectionRegion"/>.
        /// </summary>
        public float Radius { get => radius; set => Set(ref radius, value); }

        private object attachedObject;
        /// <summary>
        /// The model <see cref="object"/> that is rendered in this <see cref="SelectionRegion"/>.
        /// </summary>
        public object AttachedObject { get => attachedObject; set => Set(ref attachedObject, value); }
    }
}
