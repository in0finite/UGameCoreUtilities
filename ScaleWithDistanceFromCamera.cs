using UnityEngine;

namespace UGameCore.Utilities
{
    /// <summary>
    /// Scales the object based on his distance from camera.
    /// </summary>
    public class ScaleWithDistanceFromCamera : MonoBehaviour
    {
        [Tooltip("Distance at which object has scale of 1")]
        public float relativeDistance = 10f;
        public float raiseToPower = 0.5f;
        public float minScale = 0.1f;
        public float maxScale = 20f;
        public bool editMode = true;


        ScaleWithDistanceFromCamera()
        {
            EditorApplicationEvents.Register(this);
        }

        void Update()
        {
            if (!this.editMode && !Application.isPlaying)
                return;

            var cam = Application.isPlaying ? Camera.main : Camera.current;
            if (cam == null)
                return;

            Vector3 camPos = cam.transform.position;
            float distance = (camPos - this.transform.position).magnitude;

            float scale = Mathf.Pow(distance / this.relativeDistance, this.raiseToPower).ZeroIfNan().Clamp(this.minScale, this.maxScale);
            this.transform.localScale = Vector3.one * scale;
        }
    }
}
