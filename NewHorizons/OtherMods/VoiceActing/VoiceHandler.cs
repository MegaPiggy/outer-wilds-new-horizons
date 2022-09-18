using NewHorizons.Utility;
using System;
using System.IO;
using System.Linq;

namespace NewHorizons.OtherMods.VoiceActing
{
    public static class VoiceHandler
    {
        public static bool Enabled { get; private set; }

        private static IVoiceMod API;

        public static void Init()
        {
            try
            {
                API = Main.Instance.ModHelper.Interaction.TryGetModApi<IVoiceMod>("Krevace.VoiceMod");

                if (API == null)
                {
                    Logger.LogVerbose("VoiceMod isn't installed");
                    return;
                }

                Enabled = true;

                SetUp();
            }
            catch (Exception ex)
            {
                Logger.LogError($"VoiceMod handler failed to initialize: {ex}");
                Enabled = false;
            }
        }

        private static void SetUp()
        {
            foreach (var mod in Main.Instance.GetDependants().Append(Main.Instance))
            {
                var folder = Path.Combine(mod.ModHelper.Manifest.ModFolderPath, "voicemod");
                if (!Directory.Exists(folder)) {
                    // Fallback to PascalCase bc it used to be like that
                    folder = Path.Combine(mod.ModHelper.Manifest.ModFolderPath, "VoiceMod");
                }
                if (Directory.Exists(folder))
                {
                    Logger.Log($"Registering VoiceMod audio for {mod.ModHelper.Manifest.Name} from {folder}");
                    API.RegisterAssets(folder);
                }
                else
                {
                    Logger.LogVerbose($"Didn't find VoiceMod audio for {mod.ModHelper.Manifest.Name} at {folder}");
                }
            }
        }
    }
}
