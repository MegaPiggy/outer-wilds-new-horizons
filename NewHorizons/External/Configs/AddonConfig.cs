using NewHorizons.OtherMods.AchievementsPlus;
using Newtonsoft.Json;

namespace NewHorizons.External.Configs
{
    /// <summary>
    /// Describes the New Horizons addon itself
    /// </summary>
    [JsonObject]
    public class AddonConfig
    {
        /// <summary>
        /// Achievements for this mod if the user is playing with Achievements+
        /// Achievement icons must be put in a folder named "icons"
        /// The icon for the mod must match the name of the mod (e.g., New Horizons.png)
        /// The icons for achievements must match their unique IDs (e.g., NH_WARP_DRIVE.png)
        /// </summary>
        public AchievementInfo[] achievements;

        /// <summary>
        /// Credits info for this mod. A list of contributors and their roles separated by #. For example: xen#New Horizons dev.
        /// </summary>
        public string[] credits;

        /// <summary>
        /// A pop-up message for the first time a user runs the add-on
        /// </summary>
        public string popupMessage;

        /// <summary>
        /// If popupMessage is set, should it repeat every time the game starts or only once
        /// </summary>
        public bool repeatPopup;
    }
}
