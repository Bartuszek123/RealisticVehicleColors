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
        // HSV cutoffs. Named so the boundaries are legible and tunable in one place.
        // Calibrated by eyeballing in-traffic results; the warm-hue region
        // (brown / orange / yellow / red) is the fiddly part — see notes inline.
        private const float BlackMaxV = 0.15f;   // below this value → black regardless of hue
        private const float NeutralMaxS = 0.12f; // below this saturation → white/silver/grey
        private const float WhiteMinV = 0.85f;
        private const float SilverMinV = 0.5f;    // between this and WhiteMinV (low sat) → silver, else grey

        public static ColorBucket Classify(Color rgb)
        {
            Color.RGBToHSV(rgb, out float hNorm, out float s, out float v);
            float h = hNorm * 360f;

            if (v < BlackMaxV) return ColorBucket.Black;

            // Near-greyscale: split white / silver / grey by brightness only.
            if (s < NeutralMaxS)
            {
                if (v > WhiteMinV) return ColorBucket.White;
                if (v > SilverMinV) return ColorBucket.Silver;
                return ColorBucket.Grey;
            }

            // ── Warm hues: brown / beige / tan ─────────────────────────────────────
            // A low-hue warm color that is either dark (chocolate/tan) or muted
            // (beige/khaki) reads as brown. Bright, saturated warm colors fall
            // through to red / orange / yellow below. NOTE: the dark-warm test uses
            // h < 50 only — it must NOT include the red-wrap region (h >= 345), or
            // dark red / maroon would misclassify as brown and the Red slider would
            // no longer control them.
            if (h < 50f && v < 0.45f) return ColorBucket.Brown;              // dark warm
            if (h >= 15f && h < 45f && s < 0.5f) return ColorBucket.Brown;   // muted orange-warm (beige/tan)

            // Red wraps around 0°. Kept narrow so orange doesn't leak into red.
            if (h >= 345f || h < 15f) return ColorBucket.Red;

            // Orange (roughly 15–45°, bright & saturated) has no dedicated bucket,
            // so it lands in Other alongside purple/magenta/pink.
            if (h < 45f) return ColorBucket.Other;

            // Yellow: 45–70°. Below 45 is handled as orange above.
            if (h < 70f) return ColorBucket.Yellow;

            if (h < 165f) return ColorBucket.Green;
            if (h < 260f) return ColorBucket.Blue;
            return ColorBucket.Other;                        // purple, magenta, pink-leaning
        }

        // Friendly bucket name, used for the textual color preview on custom slots.
        public static string BucketName(ColorBucket b) => b switch
        {
            ColorBucket.White  => "white",
            ColorBucket.Black  => "black",
            ColorBucket.Grey   => "grey",
            ColorBucket.Silver => "silver",
            ColorBucket.Red    => "red",
            ColorBucket.Blue   => "blue",
            ColorBucket.Brown  => "brown / beige",
            ColorBucket.Green  => "green",
            ColorBucket.Yellow => "yellow",
            _                  => "other (orange / purple / pink)",
        };

        // A representative RGB for each bucket, used by empty-bucket synthesis to
        // inject a stand-in variation when a prefab has no stock color in a bucket
        // the user weighted above zero. Each value is chosen so Classify() maps it
        // back to the same bucket (round-trip safe). Other has no single
        // representative colour, so it is never synthesized.
        public static bool TryRepresentativeColor(ColorBucket b, out Color color)
        {
            switch (b)
            {
                case ColorBucket.White:  color = new Color(0.92f, 0.92f, 0.92f); return true;
                case ColorBucket.Black:  color = new Color(0.04f, 0.04f, 0.04f); return true;
                case ColorBucket.Grey:   color = new Color(0.35f, 0.35f, 0.35f); return true;
                case ColorBucket.Silver: color = new Color(0.65f, 0.65f, 0.66f); return true;
                case ColorBucket.Red:    color = new Color(0.55f, 0.06f, 0.06f); return true;
                case ColorBucket.Blue:   color = new Color(0.10f, 0.18f, 0.50f); return true;
                case ColorBucket.Brown:  color = new Color(0.36f, 0.22f, 0.10f); return true;
                case ColorBucket.Green:  color = new Color(0.12f, 0.35f, 0.15f); return true;
                case ColorBucket.Yellow: color = new Color(0.85f, 0.72f, 0.12f); return true;
                default:                 color = Color.white; return false;      // Other: no stand-in
            }
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
