using UnityEngine;

namespace LiverDie.Hospital.Generation
{
    public class EntranceDefinition : MonoBehaviour
    {
        private static readonly Vector3 _doorSize = new(2f, 3f, 0.2f);

        public Pose Location => new(transform.position, transform.rotation);

        private void OnDrawGizmos()
        {
            var localToWorldMatrix = transform.localToWorldMatrix;
            var offset = Vector3.zero.WithY(_doorSize.y / 2f);

            Gizmos.color = Color.green.WithA(0.5f);
            Gizmos.matrix = localToWorldMatrix;
            Gizmos.DrawCube(Vector3.zero + offset, _doorSize);

            Gizmos.color = Color.blue;
            Gizmos.matrix = localToWorldMatrix;
            Gizmos.DrawLine(Vector3.zero + offset, Vector3.forward * 2 + offset);
        }
    }
}
