using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace LiverDie.Hospital.Generation
{
    [ExecuteAlways]
    public class WallDefinition : MonoBehaviour
    {
        private WallState? _lastSeenState;

        [SerializeField]
        private WallState _wallState = WallState.Solid;

        [SerializeField]
        private GameObject _solidWall = null!;

        [SerializeField]
        private EntranceDefinition _doorWall = null!;

        public WallState State
        {
            get => _wallState;
            set
            {
                _wallState = value;
                ChangeWallState(_wallState);
            }
        }

        private void Update() => EnsureSynchronizedWallState();

#if UNITY_EDITOR
        private void OnValidate()
        {
            // For some reason, the syncronization code for the editor gives a SendMessage warning if called in
            // OnValidate, and Update causes it to have a slight visual delay, which I don't like. This delays
            // the synchronization until after validation occurs.
            void ValidationSync()
            {
                EditorApplication.delayCall -= EnsureSynchronizedWallState;
                EnsureSynchronizedWallState();
            }
            EditorApplication.delayCall += ValidationSync;
        }
#endif

        private void ChangeWallState(WallState state)
        {
            _doorWall.gameObject.SetActive(state is WallState.Door);
            _solidWall.gameObject.SetActive(state is WallState.Solid);
        }

        private void EnsureSynchronizedWallState()
        {
            if (_lastSeenState == _wallState)
                return;

            _lastSeenState = _wallState;
            ChangeWallState(_wallState);
        }

        public enum WallState
        {
            Door,
            Solid,
            [UsedImplicitly] Invisible
        }
    }
}
