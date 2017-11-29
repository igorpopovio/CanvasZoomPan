using System;

namespace CanvasZoomPan {
    public static class ExtensionMethods {
        public static double EnsureIsBetween(this double value, double lowerBound, double upperBound) {
            return Math.Max(lowerBound, Math.Min(upperBound, value));
        }
    }
}
