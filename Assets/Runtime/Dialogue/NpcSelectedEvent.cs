using LiverDie.NPC;

namespace LiverDie.Runtime.Dialogue
{
    public struct NpcSelectedEvent
    {
        public NpcDefinition Npc { get; private set; }

        public NpcSelectedEvent(NpcDefinition npc)
        {
            Npc = npc;
        }
    }
}
