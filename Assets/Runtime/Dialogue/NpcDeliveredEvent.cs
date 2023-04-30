using LiverDie.Dialogue.Data;
using LiverDie.NPC;

namespace LiverDie.Runtime.Dialogue
{
    public struct NpcDeliveredEvent
    {
        public NpcDefinition Npc { get; private set; }


        public NpcDeliveredEvent(NpcDefinition npc)
        {
            Npc = npc;
        }
    }
}
