using LiverDie.Dialogue.Data;
using LiverDie.NPC;

namespace LiverDie.Runtime.Dialogue
{
    public struct NpcDeselectedEvent
    {
        public NpcDefinition Npc { get; private set; }


        public NpcDeselectedEvent(NpcDefinition npc)
        {
            Npc = npc;
        }
    }
}
