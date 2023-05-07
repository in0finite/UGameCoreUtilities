using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UGameCore.Utilities
{
    /// <summary>
    /// General purpose functions.
    /// </summary>
    public static class F
    {
        private static readonly Vector3[] s_fourCornersArray = new Vector3[4];


        public static Mesh GetSharedMesh(this Collider col)
        {
            if (col is MeshCollider)
            {
                return ((MeshCollider)col).sharedMesh;
            }
            else
            {
                MeshFilter f = col.gameObject.GetComponent<MeshFilter>();
                return f != null ? f.sharedMesh : null;
            }
        }

        public static Vector3 ClosestPointOrBoundsCenter(this Collider collider, Vector3 position)
        {
            if (collider is MeshCollider && !((MeshCollider)collider).convex)
            {
                // ClosestPoint() function is not supported on non-convex mesh colliders
                return collider.bounds.center;
            }

            return collider.ClosestPoint(position);
        }

        public static string CurrentDateForLogging
        {
            get
            {
                try {
                    return System.DateTime.Now.ToString("dd MMM HH:mm:ss");
                } catch {
                    return "";
                }
            }
        }

        /// <summary>
        /// Formats the elapsed time (in seconds) in format [d].[hh]:mm:ss.[fff].
        /// For example, 40 seconds will return 00:40, 70 will return 01:10, 3700 will return 01:01:40.
        /// </summary>
        public static string FormatElapsedTime(double elapsedTimeSeconds, bool useMilliseconds = false)
        {
            if (double.IsNaN(elapsedTimeSeconds))
                return "NaN";

            if (TimeSpan.MaxValue.TotalSeconds < elapsedTimeSeconds)
                return double.PositiveInfinity.ToString(CultureInfo.InvariantCulture);

            if (TimeSpan.MinValue.TotalSeconds > elapsedTimeSeconds)
                return double.NegativeInfinity.ToString(CultureInfo.InvariantCulture);

            TimeSpan timeSpan = TimeSpan.FromSeconds(elapsedTimeSeconds);
            string format = (timeSpan.Days > 0 ? "d\\." : "") + (timeSpan.Hours > 0 || timeSpan.Days > 0 ? "hh\\:" : "") + "mm\\:ss" + (useMilliseconds ? "\\.fff" : "");
            return timeSpan.ToString(format, CultureInfo.InvariantCulture);
        }


        public static void EnableRigidBody(Rigidbody rb)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }

        public static void DisableRigidBody(Rigidbody rb)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }


        public static void DestroyEvenInEditMode(this Object obj)
        {
            if (F.IsAppInEditMode)
                Object.DestroyImmediate(obj, false);
            else
                Object.Destroy(obj);
        }


        public static IEnumerable<AnimationState> GetAnimationStates(this Animation animation)
        {
            foreach (AnimationState animationState in animation)
                yield return animationState;
        }

        public static float GetTimePerc (this AnimationState state)
		{
			return state.time / state.length;
		}

		public static void SetTimePerc (this AnimationState state, float perc)
		{
			state.time = state.length * perc;
		}


		public static bool RunExceptionSafe (System.Action function, string errorMessagePrefix)
		{
			try {
				function();
				return true;
			} catch(System.Exception ex) {
				try {
					Debug.LogError (errorMessagePrefix + ex);
				} catch {}
			}

			return false;
		}

		public static void RunExceptionSafe (System.Action function)
		{
			try {
				function();
			} catch(System.Exception ex) {
				try {
					Debug.LogException (ex);
				} catch {}
			}
		}

		public	static	void	Invoke( this System.Object obj, string methodName, params object[] args ) {

			var method = obj.GetType().GetMethod( methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
			if(method != null) {
				method.Invoke( obj, args );
			}

		}

		public	static	void	InvokeExceptionSafe( this System.Object obj, string methodName, params object[] args ) {

			try {
				obj.Invoke( methodName, args );
			} catch (System.Exception ex) {
				Debug.LogException (ex);
			}

		}

        /// <summary>
		/// Invokes all subscribed delegates, and makes sure any exception is caught and logged, so that all
		/// subscribers will get notified.
		/// </summary>
		public static void InvokeEventExceptionSafe( MulticastDelegate eventDelegate, params object[] parameters )
        {
            RunExceptionSafe( () => {
                var delegates = eventDelegate.GetInvocationList ();

                foreach (var del in delegates) {
                    if (del.Method != null) {
                        try {
                            del.Method.Invoke (del.Target, parameters);
                        } catch(System.Exception ex) {
                            UnityEngine.Debug.LogException (ex);
                        }
                    }
                }
            });
        }

		public static void SendMessageToObjectsOfType<T> (string msg, params object[] args) where T : UnityEngine.Object
		{
			var objects = UnityEngine.Object.FindObjectsOfType<T> ();

			foreach (var obj in objects) {
				obj.InvokeExceptionSafe (msg, args);
			}

		}


        public static IEnumerable<T> WhereAlive<T> (this IEnumerable<T> enumerable)
	        where T : UnityEngine.Object
        {
	        return enumerable.Where(obj => obj != null);
        }

        public static int RemoveDeadObjects<T> (this List<T> list)
	        where T : UnityEngine.Object
        {
            return list.RemoveAll(item => null == item);
        }

        public static int RemoveDeadObjectsIfNotEmpty<T>(this List<T> list)
	        where T : UnityEngine.Object
        {
	        if (list.Count > 0)
		        return list.RemoveDeadObjects();
	        return 0;
        }


		public static Vector2 GetSize (this Texture2D tex)
		{
			return new Vector2 (tex.width, tex.height);
		}

		public static Color OrangeColor { get { return Color.Lerp (Color.yellow, Color.red, 0.5f); } }

        public static Rect GetRect (this RectTransform rectTransform)
        {

            Vector3[] localCorners = s_fourCornersArray;
            rectTransform.GetLocalCorners (localCorners);

            float xMin = float.PositiveInfinity, yMin = float.PositiveInfinity;
            float xMax = float.NegativeInfinity, yMax = float.NegativeInfinity;

            for (int i = 0; i < localCorners.Length; i++) {
                Vector3 corner = localCorners [i];

                if (corner.x < xMin)
                    xMin = corner.x;
                else if (corner.x > xMax)
                    xMax = corner.x;

                if (corner.y < yMin)
                    yMin = corner.y;
                else if (corner.y > yMax)
                    yMax = corner.y;
            }

            return new Rect (xMin, yMin, xMax - xMin, yMax - yMin);
        }

		public	static	Texture2D	CreateTexture (int width, int height, Color color) {

			Color[] pixels = new Color[width * height];

			for (int i = 0; i < pixels.Length; i++)
				pixels [i] = color;

			Texture2D texture = new Texture2D (width, height);
			texture.SetPixels (pixels);
			texture.Apply ();

			return texture;
		}


		public static Ray GetRayFromCenter (this Camera cam)
		{
			Vector3 viewportPos = new Vector3 (0.5f, 0.5f, 0f);
			return cam.ViewportPointToRay (viewportPos);
		}

        public static Camera FindMainCameraEvenIfDisabled()
        {
            GameObject camObj = GameObject.FindGameObjectWithTag("MainCamera");
            if (camObj != null)
                return camObj.GetComponentInChildren<Camera>(true);
            return null;
        }


		public static void GizmosDrawLineFromCamera ()
		{
			if (null == Camera.main)
				return;

			Ray ray = Camera.main.GetRayFromCenter ();

			Gizmos.DrawLine (ray.origin, ray.origin + ray.direction * Camera.main.farClipPlane);
		}

        public static void HandlesDrawText(Vector3 pos, string text, Color color)
        {
            #if UNITY_EDITOR
            UnityEditor.Handles.color = color;
            UnityEditor.Handles.Label(pos, text);
            #endif
        }


        private static bool? _isInHeadlessModeCached;
	    public static bool IsInHeadlessMode
	    {
		    get
		    {
			    if (_isInHeadlessModeCached.HasValue)
				    return _isInHeadlessModeCached.Value;
			    _isInHeadlessModeCached = SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null;
			    return _isInHeadlessModeCached.Value;
		    }
	    }

        public static bool IsAppInEditMode
        {
            get
            {
#if !UNITY_EDITOR
                return false;
#else
                return !Application.isPlaying;
#endif
            }
        }

        public static bool ScreenHasHighDensity => Application.isMobilePlatform;


        public static int GetAudioClipSizeInBytes(AudioClip clip)
        {
            return clip.samples * sizeof(float);
        }

    }
}