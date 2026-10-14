using System;
using UnityEngine;

namespace LastHaven.Player
{
    /// <summary>Bir kamera preset'inin açısı, uzaklığı ve görüş alanı (GDD §4).</summary>
    [Serializable]
    public struct CameraPresetSettings
    {
        [SerializeField, Range(1f, 89f), Tooltip("Aşağı bakış açısı (derece). 90 olamaz; yaw tanımsız kalır.")]
        private float _pitch;

        [SerializeField, Range(-180f, 180f), Tooltip("Yatay açı (derece). 0 = kameranın yukarısı kuzey (+Z).")]
        private float _yaw;

        [SerializeField, Min(1f), Tooltip("Takip hedefine uzaklık (m).")]
        private float _distance;

        [SerializeField, Range(10f, 90f), Tooltip("Dikey görüş alanı (derece).")]
        private float _fieldOfView;

        public CameraPresetSettings(float pitch, float yaw, float distance, float fieldOfView)
        {
            _pitch = pitch;
            _yaw = yaw;
            _distance = distance;
            _fieldOfView = fieldOfView;
        }

        public float Pitch => _pitch;

        public float Yaw => _yaw;

        public float Distance => _distance;

        public float FieldOfView => _fieldOfView;

        /// <summary>Kameranın dünya rotasyonu (roll yok).</summary>
        public Quaternion Rotation => Quaternion.Euler(_pitch, _yaw, 0f);

        /// <summary>Takip hedefinden kameraya dünya ofseti: bakış yönünün tersine <see cref="Distance"/> kadar.</summary>
        public Vector3 FollowOffset => Rotation * Vector3.back * _distance;
    }
}
