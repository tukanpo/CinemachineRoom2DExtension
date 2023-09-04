using UnityEngine;

namespace App.Cameras
{
    /// <summary>
    /// 矩形の移動制限領域
    /// </summary>
    public class Room2D : MonoBehaviour
    {
        [SerializeField] Rect _bounds = new(Vector2.one * -0.5f, Vector2.one);
        [SerializeField] bool _alwaysShowBounds = true;

        public Rect Bounds
        {
            get => _bounds;
            set => _bounds = value;
        }

        public bool AlwaysShowBounds
        {
            get => _alwaysShowBounds;
            set => _alwaysShowBounds = value;
        }

        public Vector3[] GetCorners()
        {
            return new Vector3[]
            {
                new(_bounds.min.x, _bounds.min.y, 0),
                new(_bounds.max.x, _bounds.min.y, 0),
                new(_bounds.max.x, _bounds.max.y, 0),
                new(_bounds.min.x, _bounds.max.y, 0)
            };
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (AlwaysShowBounds)
                DrawGizmos();
        }

        void OnDrawGizmosSelected()
        {
            if (!AlwaysShowBounds)
                DrawGizmos();
        }

        void DrawGizmos()
        {
            Gizmos.color = Color.green;

            var corners = GetCorners();
            Gizmos.DrawLineList(new []
            {
                corners[0], corners[1],
                corners[1], corners[2],
                corners[2], corners[3],
                corners[3], corners[0],
            });

            var namePos = new Vector3(_bounds.min.x, _bounds.max.y, 0);
            UnityEditor.Handles.Label(namePos, gameObject.name);
        }
#endif // UNITY_EDITOR
    }
}
