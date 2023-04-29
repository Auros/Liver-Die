using UnityEngine;

namespace LiverDie.Hospital.Generation
{
    public class EntranceDefinition : MonoBehaviour
    {
        private static readonly Vector3 _doorSize = new(2f, 3f, 0.2f);

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green.WithA(0.5f);
            Gizmos.matrix = transform.localToWorldMatrix;
            var offset = Vector3.zero.WithY(_doorSize.y / 2f);
            Gizmos.DrawCube(Vector3.zero + offset, _doorSize);
        }
    }
}
