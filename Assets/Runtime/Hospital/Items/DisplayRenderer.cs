using UnityEngine;

namespace LiverDie.Hospital.Items
{
    public class DisplayRenderer : MonoBehaviour
    {
        [SerializeField]
        private int _square;

        [SerializeField]
        private float _delay = 0.01f;

        [SerializeField]
        private Renderer _target = null!;

        [SerializeField]
        private Texture2D _sheet = null!;

        private float _time;
        private Vector2 _current;
        private Material? _material;

        private void Start()
        {
            _material = _target.material;
            _material.mainTexture = _sheet;
            _material.mainTextureScale = new Vector2(1f / _square, 1f / _square);
            _current = Vector2.zero;
        }

        private void Update()
        {
            _time += Time.deltaTime;
            if (_delay > _time || !_material)
                return;

            _time = 0f;

            var diff = 1f / _square;
            var (x, y) = _current;

            x += diff;
            if (x >= 1f)
            {
                x = 0;
                y -= diff;
            }

            if (y < 0f - 0.05f)
                y = diff * (_square - 1);

            _current = new Vector2(x, y);
            _material!.mainTextureOffset = _current;
        }

        private void OnDestroy()
        {
            Destroy(_material);
        }
    }
}
