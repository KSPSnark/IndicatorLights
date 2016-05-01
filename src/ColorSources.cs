using UnityEngine;

namespace IndicatorLights
{
    internal static class ColorSources
    {
        public static readonly IColorSource BLACK = Constant(Color.black);

        /// <summary>
        /// Gets a constant-clor source.
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static IColorSource Constant(Color color)
        {
            return new ConstantColorSource(color);
        }


        /// <summary>
        /// Given a color source ID (which might be a literal color, or might be the ID
        /// of a controller), try to find the appropriate source. If there's no match,
        /// returns a constant black source.
        /// </summary>
        /// <param name="sourceID"></param>
        /// <returns></returns>
        public static IColorSource Find(Part part, string sourceID)
        {
            if (!string.IsNullOrEmpty(sourceID))
            {
                // Maybe it's a color string.
                if (Colors.IsColorString(sourceID))
                {
                    return Constant(Colors.Parse(sourceID, Color.black));
                }

                // Not a color string, maybe it's a module on the part?
                for (int i = 0; i < part.Modules.Count; ++i)
                {
                    IColorSource candidate = part.Modules[i] as IColorSource;
                    if (candidate == null) continue;
                    if (sourceID.Equals(candidate.ColorSourceID))
                    {
                        return candidate;
                    }
                }
            }

            // not found
            Logging.Warn("Can't find a color source named '" + sourceID + "' on " + part.GetTitle());
            return BLACK;
        }

        /// <summary>
        /// Puts an IColorSource wrapper around a constant color.
        /// </summary>
        private class ConstantColorSource : IColorSource
        {
            private readonly Color color;
            private readonly string id;

            public ConstantColorSource(Color color)
            {
                this.color = color;
                this.id = Colors.ToString(color);
            }

            public bool HasColor
            {
                get { return true; }
            }

            public Color OutputColor
            {
                get { return color; }
            }

            public string ColorSourceID
            {
                get { return id; }
            }

        }
    }
}
