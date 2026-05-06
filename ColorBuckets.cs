using UnityEngine;

namespace RealisticVehicleColors
{
    public enum ColorBucket
    {
        White,
        Black,
        Grey,
        Silver,
        Red,
        Blue,
        Brown,
        Green,
        Yellow,
        Other,
    }

    public static class ColorClassifier
    {
        public static ColorBucket Classify(Color rgb)
        {
            Color.RGBToHSV(rgb, out float hNorm, out float s, out float v);
            float h = hNorm * 360f;

            if (v < 0.15f) return ColorBucket.Black;

            if (s < 0.12f)
            {
                if (v > 0.85f) return ColorBucket.White;
                if (v > 0.5f) return ColorBucket.Silver;
                return ColorBucket.Grey;
            }

            // Brown / beige: warm hues with reduced value or saturation.
            if (h < 50f && v < 0.45f) return ColorBucket.Brown;
            if (h >= 15f && h < 45f && s < 0.5f) return ColorBucket.Brown;

            if (h >= 345f || h < 15f) return ColorBucket.Red;
            if (h < 45f) return ColorBucket.Other;          // pure orange — no bucket assigned yet
            if (h < 70f) return ColorBucket.Yellow;
            if (h < 165f) return ColorBucket.Green;
            if (h < 260f) return ColorBucket.Blue;
            return ColorBucket.Other;                        // purple, magenta, pink-leaning
        }

        public static bool TryParseHex(string hex, out Color color)
        {
            color = Color.white;
            if (string.IsNullOrWhiteSpace(hex)) return false;
            string s = hex.Trim();
            if (s.Length > 0 && s[0] == '#') s = s.Substring(1);
            if (s.Length != 6 && s.Length != 8) return false;
            if (!ColorUtility.TryParseHtmlString("#" + s, out color)) return false;
            return true;
        }
    }
}
