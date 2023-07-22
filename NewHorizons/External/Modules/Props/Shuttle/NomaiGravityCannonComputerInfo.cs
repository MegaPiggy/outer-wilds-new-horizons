using Newtonsoft.Json;

namespace NewHorizons.External.Modules.Props.Shuttle
{
    [JsonObject]
    public class NomaiGravityCannonComputerInfo : NomaiComputerInfo
    {
        /// <summary>
        /// The relative path to the xml file for this computer.
        /// </summary>
        public string xmlFile;
    }
}
