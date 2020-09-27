using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace BassClefStudio.SkiaSharp.Helpers
{
    /// <summary>
    /// Represents a service capable of rendering the graphics representing an object of type <typeparamref name="T"/> onto a <see cref="SKCanvas"/>.
    /// </summary>
    public abstract class RenderService<T> : IDisposable
    {
        protected Dictionary<string, SKPaint> Paints { get; } = new Dictionary<string, SKPaint>();

        /// <summary>
        /// Renders the <typeparamref name="T"/> object onto the given <see cref="SKCanvas"/>.
        /// </summary>
        /// <param name="context">The <typeparamref name="T"/> representing the context of the game/app.</param>
        /// <param name="canvas">The canvas to render content onto.</param>
        public abstract void Render(T context, SKCanvas canvas);

        /// <summary>
        /// Gets the <see cref="ISelectable"/> object found at the given SkiaSharp coordinates (for use in selection and click events, etc.)
        /// </summary>
        /// <param name="point">The point, in SkiaSharp pixel coordinates, to check for selection.</param>
        public abstract ISelectable GetSelected(SKPoint point);

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
