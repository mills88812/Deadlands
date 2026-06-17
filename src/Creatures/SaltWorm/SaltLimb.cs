using System;

namespace Deadlands.Creatures.SaltWorm
{
    internal class SaltLimb : Limb
    {
        public SaltWormGraphics graphics
        {
            get
            {
                return this.owner as SaltWormGraphics;
            }
        }

      
        public SaltLimb(GraphicsModule owner, BodyChunk connectionChunk, int num, float rad, float sfFric, float aFric, float huntSpeed, float quickness, int limbSide) : base(owner, connectionChunk, num, rad, sfFric, aFric, huntSpeed, quickness)
        {
            this.limbSide = limbSide;
            this.saltLimbNumber = num;
        }

        
        public override void Update()
        {
            base.Update();
            if (this.triggerLegCounter > 0)
            {

                this.triggerLegCounter--;
                if (this.triggerLegCounter == 0)
                {
                    this.graphics.FindGrip(this.legToTrigger.saltLimbNumber, this.legToTrigger.limbSide, 1f);
                }
            }
        }

       
        public override void GrabbedTerrain()
        {
            if (this.triggerLegCounter == 0 && this.legToTrigger != null)
            {
                this.triggerLegCounter = this.timeToTrigger;
            }
        }
        public int saltLimbNumber;
        public int limbSide;

        public SaltLimb legToTrigger;

        public int triggerLegCounter;

        public int timeToTrigger = 5;
    }
}
