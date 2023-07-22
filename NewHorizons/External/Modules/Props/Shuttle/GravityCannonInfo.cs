using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External.Modules.Props.Shuttle
{
    [JsonObject]
    public class GravityCannonInfo : GeneralPropInfo
    {
        /// <summary>
        /// Unique ID for the shuttle that pairs with this gravity cannon
        /// </summary>
        public string shuttleID;

        /// <summary>
        /// Ship log fact revealed when retrieving the shuttle to this pad. Optional.
        /// </summary>
        public string retrieveReveal;

        /// <summary>
        /// Ship log fact revealed when launching from this pad. Optional.
        /// </summary>
        public string launchReveal;

        /// <summary>
        /// Will create a modern Nomai computer linked to this gravity cannon.
        /// </summary>
        public NomaiGravityCannonComputerInfo computer;
    }
}
