using BassClefStudio.NET.Core;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace BassClefStudio.SkiaSharp.Helpers
{
    /// <summary>
    /// Represents a service capable of rendering the graphics representing an object of type <typeparamref name="T"/> onto a <see cref="SKCanvas"/>.
    /// </summary>
    public abstract class RenderService<T> : Observable, IDisposable
    {
        /// <summary>
        /// A collection of <see cref="SKPaint"/>s keyed with <see cref="string"/>s, which are managed by the <see cref="RenderService{T}"/> (e.g. disposal).
        /// </summary>
        protected Dictionary<string, SKPaint> Paints { get; } = new Dictionary<string, SKPaint>();

        private T attachedContext;
        /// <summary>
        /// The attached <typeparamref name="T"/> context that the <see cref="RenderService{T}"/> renders.
        /// </summary>
        public T AttachedContext { get => attachedContext; set => Set(ref attachedContext, value); }

        /// <summary>
        /// Represents the camera that indicates how the view should be presented.
        /// </summary>
        public Camera ViewCamera { get; } = new Camera();

        /// <summary>
        /// Renders the <see cref="AttachedContext"/> onto the given <see cref="SKCanvas"/>.
        /// </summary>
        /// <param name="canvas">The canvas to render content onto.</param>
        public void Render(SKCanvas canvas)
        {
            canvas.Clear();
            float viewScale = ViewCamera.ViewScale;
            Vector2 canvasSize = ViewCamera.ViewSize;
            canvas.Scale(viewScale, -viewScale);
            float width = canvasSize.X / (2 * viewScale);
            float height = canvasSize.Y / (2 * viewScale);
            canvas.Translate(width, -height);

            SKRect boundingRect = new SKRect(-width, height, width, -height);
            canvas.ClipRect(boundingRect, SKClipOperation.Intersect, true);

            RenderInternal(canvas);
        }

        /// <summary>
        /// An internal method that is called in <see cref="Render(SKCanvas)"/> after the <see cref="Camera"/> has been positioned.
        /// </summary>
        /// <param name="canvas">The canvas to render content onto.</param>
        protected abstract void RenderInternal(SKCanvas canvas);

        /// <summary>
        /// Gets a collection of <see cref="SelectionRegion"/>s which are used to find the selected object in the <see cref="GetSelected(SKPoint)"/> method.
        /// </summary>
        protected abstract IEnumerable<SelectionRegion> GetSelectionRegions();

        /// <summary>
        /// Gets the <see cref="SelectionRegion"/> object found at the given SkiaSharp coordinates for the <see cref="AttachedContext"/> (for use in selection and click events, etc.).
        /// </summary>
        /// <param name="point">The point, in SkiaSharp pixel coordinates, to check for selection.</param>
        public SelectionRegion GetSelected(SKPoint point)
        {
            return GetSelectionRegions().LastOrDefault(r => r.SelectionBounds.Contains(point));
        }

        /// <summary>
        /// Return the <typeparamref name="TItem"/> model object found at the given SkiaSharp coordinates for the <see cref="AttachedContext"/> (for use in selection and click events, etc.).
        /// </summary>
        /// <param name="point">The point, in SkiaSharp pixel coordinates, to check for selection.</param>
        public TItem GetSelected<TItem>(SKPoint point)
        {
            var region = GetSelectionRegions().LastOrDefault(r => r.SelectionBounds.Contains(point) && r.AttachedObject is TItem);
            return region != null ? (TItem)region.AttachedObject : default(TItem);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            foreach (var p in Paints)
            {
                p.Value.Dispose();
            }
        }
    }
}
