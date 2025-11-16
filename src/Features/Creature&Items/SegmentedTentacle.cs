using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deadlands.Features
{
    internal class SegmentedTentacle : Tentacle
    {
        public SegmentedTentacle(PhysicalObject owner, BodyChunk connectedChunk, float length) : base(owner, connectedChunk, length)
        {
        }
    }
}
