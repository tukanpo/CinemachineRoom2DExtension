using UnityEngine;

namespace App.Player
{
    public class PlayerController : MonoBehaviour
    {
        Rigidbody _rb;

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        void Update()
        {
            var moveX= Input.GetAxis("Horizontal");
            var moveY= Input.GetAxis("Vertical");
            var pos = new Vector3(moveX, moveY, 0);
            _rb.velocity = pos.normalized * 7.0f;
        }
    }
}
