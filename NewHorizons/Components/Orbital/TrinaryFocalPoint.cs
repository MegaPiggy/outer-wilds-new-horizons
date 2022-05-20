using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components.Orbital
{
    public class TrinaryFocalPoint : BinaryFocalPoint
    {
        public string TertiaryName { get; set; }

        public AstroObject Tertiary { get; set; }

        public GameObject FakeMassBody2 { get; set; }

        protected override void Start()
        {
            base.Start();
            // Make sure its active but maybe it hasn't been set yet
            if (FakeMassBody2) FakeMassBody2.SetActive(true);
        }

        protected override void Disable()
        {
            base.Disable();
            FakeMassBody2.SetActive(false);
        }

        protected override void Update()
        {
            // Trinary, secondary, and primary must have been engulfed by a star
            if ((Primary == null || !Primary.isActiveAndEnabled) && (Secondary == null || !Secondary.isActiveAndEnabled) && (Tertiary == null || !Tertiary.isActiveAndEnabled))
            {
                Disable();
            }
        }
    }
}
