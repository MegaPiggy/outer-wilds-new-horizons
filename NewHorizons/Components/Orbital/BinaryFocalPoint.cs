using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components.Orbital
{
    public class BinaryFocalPoint : UnaryFocalPoint
    {
        public string SecondaryName = null;

        public AstroObject Secondary = null;
        
        protected override void Update()
        {
            // Secondary and primary must have been engulfed by a star
            if((Primary == null || !Primary.isActiveAndEnabled) && (Secondary == null || !Secondary.isActiveAndEnabled))
            {
                Disable();
            }
        }
    }
}
