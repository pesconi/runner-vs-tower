namespace Claybreak
{
    /// <summary>
    /// RT-6t.17: the inline icons of the menu texts (TextMeshPro sprite tags of the game's icon sprite asset, built by
    /// <c>GameFonts</c>). The game's font has no star glyph, and the coin reads better as a coin than as a cent sign.
    /// </summary>
    public static class UiIcons
    {
        public const string StarName = "star";
        public const string CoinName = "coin";
        public const string Star = "<sprite name=\"" + StarName + "\">";
        public const string Coin = "<sprite name=\"" + CoinName + "\">";

        /// <summary>A coin amount as the menus write it: the coin icon, then the number.</summary>
        public static string Coins(int amount) => Coin + " " + NumberLabels.Plain(amount);
    }
}
