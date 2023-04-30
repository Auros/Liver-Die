using UnityEngine;

namespace LiverDie.Hospital
{
    public class PosterController : MonoBehaviour
    {
        [SerializeField]
        private Sprite[] _posters = null!;

        [SerializeField]
        private float _spawnChance = 0.1f;

        [SerializeField]
        private SpriteRenderer _renderer = null!;

        private void Start()
        {
            if (Random.Range(0f, 1f) > _spawnChance)
            {
                gameObject.SetActive(false);
                return;
            }

            _renderer.sprite = _posters[Random.Range(0, _posters.Length)];
        }
    }
}
