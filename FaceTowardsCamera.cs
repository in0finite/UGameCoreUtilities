using UnityEngine;

namespace UGameCore.Utilities
{
    /// <summary>
    /// Rotates the specified transforms so that they face toward camera.
    /// </summary>
    public class FaceTowardsCamera : MonoBehaviour
    {
        public Transform[] transformsToFace = System.Array.Empty<Transform>();
        public Vector3 eulerOffset = Vector3.zero;
        public bool editMode = true;


        FaceTowardsCamera()
        {
            EditorApplicationEvents.Register(this);
        }

        void Update()
        {
            if (!this.editMode && !Application.isPlaying)
                return;

            var cam = Camera.current;
            if (cam == null)
                return;
            
            Quaternion camRotation = cam.transform.rotation;
            Quaternion offsetRotation = Quaternion.Euler(this.eulerOffset);
            Quaternion rotation = Quaternion.LookRotation(-camRotation.GetForward(), camRotation.GetUp()) * offsetRotation;
            for (int i = 0; i < this.transformsToFace.Length; i++)
            {
                this.transformsToFace[i].rotation = rotation;
            }
        }
    }
}
