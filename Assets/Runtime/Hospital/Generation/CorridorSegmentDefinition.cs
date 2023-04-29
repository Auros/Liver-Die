using UnityEngine;

namespace LiverDie.Hospital.Generation
{
    public class CorridorSegmentDefinition : MonoBehaviour
    {
        private int _generation;

        public int Generation
        {
            get => _generation;
            set
            {
                _generation = value;
                name = $"Corridor Gen [{_generation}] {transform.localPosition}";
            }
        }
    }
}
