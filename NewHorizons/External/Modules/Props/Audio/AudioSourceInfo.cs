using NewHorizons.External.SerializableEnums;
using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules.Props.Audio
{
    [JsonObject]
    public class AudioSourceInfo : BaseAudioInfo
    {
        /// <summary>
        /// The audio track of this audio source
        /// </summary>
        [DefaultValue("environment")] public NHAudioMixerTrackName track = NHAudioMixerTrackName.Environment;
    }
}
