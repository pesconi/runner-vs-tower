using UnityEngine;

namespace Claybreak
{
    /// <summary>
    /// RT-6t.17: the menu colors, the toon pastel direction of the prototype (docs/reference/crowd-proto.html) and the
    /// world palettes, brightened so white outlined text stays readable on every button. One source for the scene builder
    /// and the screens that recolor buttons at run time.
    /// </summary>
    public static class UiColors
    {
        /// <summary>The main action (PLAY, NEXT, buy): the prototype's orange.</summary>
        public static readonly Color Primary = Hex(0xff9f43);
        /// <summary>The prototype's coral: titles and the selected tab.</summary>
        public static readonly Color Coral = Hex(0xf26a5a);
        public static readonly Color Mint = Hex(0x3ccf91);
        public static readonly Color Sky = Hex(0x4fb6f5);
        public static readonly Color Lilac = Hex(0xa77be0);
        public static readonly Color Pink = Hex(0xff6f91);
        public static readonly Color Gold = Hex(0xffcf3a);
        /// <summary>Light cards and the cancel / back buttons.</summary>
        public static readonly Color Cream = Hex(0xfff4ea);
        public static readonly Color CardBorder = Hex(0xf4b4c8);
        /// <summary>Dark text on light faces and the text outline.</summary>
        public static readonly Color Ink = Hex(0x3b2f4a);
        /// <summary>A button that cannot be pressed now (the price stays readable on it).</summary>
        public static readonly Color Unavailable = Hex(0xc9bfd3);
        /// <summary>The veil over the level behind a menu (ink, so the level shows through tinted, not black).</summary>
        public static readonly Color Veil = Hex(0x3b2f4a);

        public static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        private static Color Hex(int rgb)
        {
            return new Color(((rgb >> 16) & 0xff) / 255f, ((rgb >> 8) & 0xff) / 255f, (rgb & 0xff) / 255f, 1f);
        }
    }
}
