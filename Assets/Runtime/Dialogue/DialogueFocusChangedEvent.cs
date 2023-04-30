using LiverDie.Dialogue.Data;
using LiverDie.NPC;

namespace LiverDie.Runtime.Dialogue
{
    public struct DialogueFocusChangedEvent
    {
        public bool Focused { get; private set; }

        public DialogueFocusChangedEvent(bool focused)
        {
            Focused = focused;
        }
    }
}
