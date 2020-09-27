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
        private SKRect selectionBounds;
        /// <summary>
        /// A <see cref="SKRect"/> representing the bounds of the selection region.
        /// </summary>
        public SKRect SelectionBounds { get => selectionBounds; set => Set(ref selectionBounds, value); }

        private object attachedObject;
        /// <summary>
        /// The model <see cref="object"/> that is rendered in this <see cref="SelectionRegion"/>.
        /// </summary>
        public object AttachedObject { get => attachedObject; set => Set(ref attachedObject, value); }
    }
}
