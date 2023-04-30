using System;
using LiverDie.NPC;
using LiverDie.Runtime.Dialogue;
using UnityEngine;

namespace LiverDie.Runtime.Intermediate
{
    public class DialogueEventIntermediate : MonoBehaviour
    {
        public event Action<NpcSelectedEvent>? OnNpcSelected;
        public event Action<NpcDeselectedEvent>? OnNpcDeselected;
        public event Action<NpcDeliveredEvent>? OnNpcDelivered;

        public void SelectNpc(NpcDefinition npcDefinition)
        {
            OnNpcSelected?.Invoke(new NpcSelectedEvent(npcDefinition));
        }

        public void DeselectNpc(NpcDefinition npcDefinition)
        {
            OnNpcDeselected?.Invoke(new NpcDeselectedEvent(npcDefinition));
        }

        public void DeliverNpc(NpcDefinition npcDefinition)
        {
            OnNpcDelivered?.Invoke(new NpcDeliveredEvent(npcDefinition));
        }
    }
}
