using UnityEngine;

namespace App.Scenes
{
    public class DemoScene : MonoBehaviour
    {
        [SerializeField] Camera _camera;

        void Start()
        {
            DebugOutputOtherProjectionModeSettings();
        }

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                SwitchCameraMode();
            }
        }

        /// <summary>
        /// カメラの投影方式を切替える
        /// </summary>
        void SwitchCameraMode()
        {
            _camera.orthographic = !_camera.orthographic;

            DebugOutputOtherProjectionModeSettings();
        }

        // 現在の投影モードの表示と同じになるような、もう一方の投影モードの表示設定をログ出力する
        // （Perspective に合わせて Orthograpihc の表示を調整したいなら、Perspective 時に表示される OrthographicSize を設定する等)
        void DebugOutputOtherProjectionModeSettings()
        {
            if (_camera.orthographic)
            {
                // カメラの距離と OrthographicSize から FOV を算出
                var fieldOfView = 2.0f * Mathf.Atan(_camera.orthographicSize / _camera.transform.position.z) * Mathf.Rad2Deg;
                Debug.Log($"FieldOfView = {fieldOfView * -1}");
            }
            else
            {
                // カメラの距離と FOV から OrthographicSize を算出
                var orthographicSize = _camera.transform.position.z * Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
                Debug.Log($"OrthographicSize = {orthographicSize * -1}");
            }
        }
    }
}