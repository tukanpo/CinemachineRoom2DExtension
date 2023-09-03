using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace App.Cameras
{
    /// <summary>
    /// Room2D 内にカメラの移動を制限する為の Cinemachine Extension<br/>
    /// </summary>
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    public class CinemachineRoom2D : CinemachineExtension
    {
        [SerializeField] List<Room2D> _rooms;

        CinemachineVirtualCamera _vcam;
        float _cameraDistance;
        float _cameraHeight;
        float _cameraWidth;

        // 再計算判定用に保持するカメラのサイズ情報
        bool _prevOrthographic;
        float _prevOrthographicSize;
        float _prevFieldOfView;
        float _prevAspectRatio;
        float _prevCameraDistance;

        Room2D _prevRoom;
        Vector3 _prevPosition;

        protected override void ConnectToVcam(bool connect)
        {
            base.ConnectToVcam(connect);
            if (connect == false) return;

            _vcam = VirtualCamera as CinemachineVirtualCamera;
            if (_vcam == null) return;

            _cameraDistance = GetCameraDistance();

            RememberCurrentCameraSize(_vcam.m_Lens);
            CalculateCameraSize(_vcam.m_Lens);
        }

        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage,
            ref CameraState state,
            float deltaTime)
        {
            if (stage != CinemachineCore.Stage.Body)
            {
                return;
            }

            var lens = state.Lens;

            _cameraDistance = GetCameraDistance();

            // カメラのサイズが変更されていたら再計算する
            if (IsCameraSizeChanged(lens))
            {
                CalculateCameraSize(lens);
            }

            // カメラのサイズを前回の情報として保持する
            RememberCurrentCameraSize(lens);

            Room2D room = null;
            if (_vcam.Follow != null)
            {
                room = FindRoomByPosition(_vcam.Follow.position);
                if (room != null)
                {
                    // Room と Follow ターゲットの位置関係からカメラの位置を補正する
                    // NOTE: Room 切り替わり時のカメラ移動に遷移アニメーションを加えるならここを改良する
                    state.RawPosition = ClampCameraToRoomBounds(state.RawPosition, room);
                }
                else
                {
                    // ターゲット位置を範囲内に含む Room が見つからない場合、前回のカメラ位置のままにする
                    state.RawPosition = _prevPosition;
                }
            }

            _prevRoom = room;
            _prevPosition = state.RawPosition;
        }

        float GetCameraDistance()
        {
            var componentBase = _vcam.GetCinemachineComponent(CinemachineCore.Stage.Body);
            if (componentBase is CinemachineFramingTransposer transposer)
            {
                return transposer.m_CameraDistance;
            }

            return 0;
        }

        void RememberCurrentCameraSize(LensSettings lens)
        {
            _prevOrthographic = lens.Orthographic;
            _prevOrthographicSize = lens.OrthographicSize;
            _prevAspectRatio = lens.Aspect;
            _prevFieldOfView = lens.FieldOfView;

            _prevCameraDistance = _cameraDistance;
        }

        /// <summary>
        /// カメラの状態を前回と比較して内容に変更があるかどうかを返す
        /// </summary>
        /// <returns>変更があるか？</returns>
        bool IsCameraSizeChanged(LensSettings lens)
        {
            // 投影方式
            if (lens.Orthographic != _prevOrthographic)
            {
                return true;
            }

            if (lens.Orthographic)
            {
                if (Math.Abs(lens.OrthographicSize - _prevOrthographicSize) > Epsilon ||
                    Math.Abs(lens.Aspect - _prevAspectRatio) > Epsilon)
                {
                    return true;
                }
            }
            else
            {
                if (Math.Abs(lens.FieldOfView - _prevFieldOfView) > Epsilon ||
                    Math.Abs(lens.Aspect - _prevAspectRatio) > Epsilon ||
                    Math.Abs(_cameraDistance - _prevCameraDistance) > Epsilon)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// カメラが見ている範囲（高さと幅）を算出する
        /// </summary>
        /// <seealso href="https://docs.unity3d.com/ja/2022.3/Manual/FrustumSizeAtDistance.html">参考</seealso>
        void CalculateCameraSize(LensSettings lens)
        {
            if (lens.Orthographic)
            {
                // 正射影カメラの場合、視錐台の近接平面と遠方平面の間の距離は無関係で、
                // その高さと幅は直接的に OrthographicSize プロパティとアスペクト比から計算する。

                // カメラの高さを計算（カメラの正射影サイズの2倍がカメラの高さになる）
                _cameraHeight = 2f * lens.OrthographicSize;

                // カメラの幅を計算（カメラの高さとアスペクト比から算出）
                _cameraWidth = _cameraHeight * lens.Aspect;
            }
            else
            {
                // 透視投影カメラの場合、視錐台はカメラから遠くに行くほど広がり、
                // その高さと幅は視野角（FOV）とカメラからの距離に基づいて計算する。
                // これはカメラの視錐台の近接平面（ビューポートに対応する3D空間内の領域）の高さと幅となる。

                // カメラの高さを計算（カメラの視野角と距離から算出）
                _cameraHeight = 2f * _cameraDistance * Mathf.Tan(lens.FieldOfView * 0.5f * Mathf.Deg2Rad);

                // カメラの幅を計算（カメラの高さとアスペクト比から算出）
                _cameraWidth = _cameraHeight * lens.Aspect;
            }
        }

        /// <summary>
        /// ターゲット位置を範囲に含む Room を返す.
        /// Room が複数ある場合は直前の Room を優先で返す
        /// </summary>
        /// <param name="position">ターゲット位置</param>
        /// <returns>Room</returns>
        Room2D FindRoomByPosition(Vector3 position)
        {
            // 直前の Room 範囲内かどうかを優先チェック
            // （Room の範囲が重なっている場合は前回 Room を優先させる）
            if (_prevRoom != null)
            {
                if (Contains(_prevRoom, position))
                {
                    return _prevRoom;
                }
            }

            // 他の Room 範囲内かチェック
            for (var i = _rooms.Count - 1; i >= 0; i--)
            {
                if (Contains(_rooms[i], position))
                {
                    return _rooms[i];
                }
            }

            return null;

            bool Contains(Room2D room, Vector3 pos)
            {
                return room.Bounds.Contains(new Vector2(pos.x, pos.y));
            }
        }

        /// <summary>
        /// カメラの位置を Room の境界に合わせて調整する
        /// </summary>
        /// <param name="rawPosition">調整前の位置</param>
        /// <param name="room">Room</param>
        /// <returns>調整後の位置</returns>
        Vector3 ClampCameraToRoomBounds(Vector3 rawPosition, Room2D room)
        {
            // カメラのy座標を補正する
            if (_cameraHeight < room.Bounds.height)
            {
                // カメラの高さが Room の高さよりも小さい場合、カメラのy座標を Bounds の範囲内に制約する
                // カメラの中心が Room の端に接触しないようにするため、カメラの半分の高さを余白として考慮
                rawPosition.y = Mathf.Clamp(rawPosition.y, room.Bounds.yMin + _cameraHeight * 0.5f, room.Bounds.yMax - _cameraHeight * 0.5f);
            }
            else
            {
                // カメラの高さが Room の高さよりも大きい場合、Room の中心のy座標をカメラのy座標とする
                rawPosition.y = room.Bounds.center.y;
            }

            // カメラのx座標を補正する （y座標と同じ）
            if (_cameraWidth < room.Bounds.width)
            {
                rawPosition.x = Mathf.Clamp(rawPosition.x, room.Bounds.xMin + _cameraWidth * 0.5f, room.Bounds.xMax - _cameraWidth * 0.5f);
            }
            else
            {
                rawPosition.x = room.Bounds.center.x;
            }

            return rawPosition;
        }
    }
}
