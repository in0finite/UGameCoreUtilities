using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace UGameCore.Utilities
{
    /// <summary>
    /// General purpose functions.
    /// </summary>
    public static class F
    {
        public const string kEditorMenuName = nameof(UGameCore);

        private static readonly Vector3[] s_fourCornersArray = new Vector3[4];


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

        public static Span<T> CopyFromOther<T>(this Span<T> destinationSpan, ReadOnlySpan<T> other)
        {
            if (destinationSpan.Length < other.Length)
                throw new ArgumentOutOfRangeException($"Destination span is shorter ({destinationSpan.Length}) than source span ({other.Length})");
            other.CopyTo(destinationSpan);
            return destinationSpan.Slice(other.Length);
        }

        public static void CopyFromOther<T>(this Span<T> destinationSpan, ReadOnlySpan<T> other, out int otherLength)
        {
            destinationSpan.CopyFromOther(other);
            otherLength = other.Length;
        }

        /// <summary>
        /// Formats the elapsed time (in seconds) in format [d].[hh]:mm:ss.[fff].
        /// For example, 40 seconds will return 00:40, 70 will return 01:10, 3700 will return 01:01:40.
        /// </summary>
        public static string FormatElapsedTime(double elapsedTimeSeconds, bool useMilliseconds = false)
        {
            Span<char> chars = stackalloc char[32];
            FormatElapsedTime(chars, out int charsWritten, elapsedTimeSeconds, useMilliseconds);
            return new string(chars[..charsWritten]);
        }

        /// <summary>
        /// Formats the elapsed time (in seconds) in format [d].[hh]:mm:ss.[fff].
        /// For example, 40 seconds will return 00:40, 70 will return 01:10, 3700 will return 01:01:40.
        /// </summary>
        public static void FormatElapsedTime(
            Span<char> resultSpan, out int charsWritten, double elapsedTimeSeconds, bool useMilliseconds = false)
        {
            if (double.IsNaN(elapsedTimeSeconds))
            {
                resultSpan.CopyFromOther("NaN", out charsWritten);
                return;
            }

            if (TimeSpan.MaxValue.TotalSeconds < elapsedTimeSeconds)
            {
                resultSpan.CopyFromOther("Infinity", out charsWritten);
                return;
            }

            if (TimeSpan.MinValue.TotalSeconds > elapsedTimeSeconds)
            {
                resultSpan.CopyFromOther("-Infinity", out charsWritten);
                return;
            }

            TimeSpan timeSpan = TimeSpan.FromSeconds(elapsedTimeSeconds);

            Span<char> formatSpan = stackalloc char[20];
            Span<char> tempSpan = formatSpan;

            if (timeSpan.Days > 0)
                tempSpan = tempSpan.CopyFromOther("d\\.");

            if (timeSpan.Hours > 0 || timeSpan.Days > 0)
                tempSpan = tempSpan.CopyFromOther("hh\\:");

            tempSpan = tempSpan.CopyFromOther("mm\\:ss");

            if (useMilliseconds)
                tempSpan = tempSpan.CopyFromOther("\\.fff");

            formatSpan = formatSpan[..(formatSpan.Length - tempSpan.Length)];

            if (!timeSpan.TryFormat(resultSpan, out charsWritten, formatSpan, CultureInfo.InvariantCulture))
                throw new ArgumentException("Failed to format time span");
        }


        public static T FindObjectByInstanceId<T>(int id)
            where T : Object
        {
            Object obj = Resources.InstanceIDToObject(id);
            if (obj == null)
                throw new ArgumentException($"Failed to find {typeof(T).Name} by instance id: {id}");

            if (obj is not T specificTypeObj)
                throw new ArgumentException($"Object with id {id} is of type {obj.GetType().Name}, so it can not be casted to {typeof(T).Name}");

            return specificTypeObj;
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

        public static void DestroyImmediate(this Object obj)
        {
            Object.DestroyImmediate(obj, false);
        }


        public static void HandleRunSafeException(System.Exception ex, Object contextObject)
        {
            // no exception should leave this function
            try
            {
                Debug.LogException(ex, contextObject);
            }
            catch { }
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

		public static bool RunExceptionSafe (System.Action function, Object contextObject = null)
		{
			try
            {
				function();
                return true;
			}
            catch (System.Exception ex)
            {
				HandleRunSafeException(ex, contextObject);
                return false;
            }
		}

        public static bool RunExceptionSafeArg<T1>(T1 arg1, System.Action<T1> function, Object contextObject = null)
        {
            try
            {
                function(arg1);
                return true;
            }
            catch (System.Exception ex)
            {
                HandleRunSafeException(ex, contextObject);
                return false;
            }
        }

        public static bool RunExceptionSafeArg2<T1, T2>(T1 arg1, T2 arg2, System.Action<T1, T2> function, Object contextObject = null)
        {
            return RunExceptionSafeArg<(T1, T2, System.Action<T1, T2>)>(
                (arg1, arg2, function),
                static (a) => a.Item3(a.Item1, a.Item2),
                contextObject);
        }

        public static bool RunExceptionSafeArg3<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3, System.Action<T1, T2, T3> function, Object contextObject = null)
        {
            return RunExceptionSafeArg<(T1, T2, T3, System.Action<T1, T2, T3>)>(
                (arg1, arg2, arg3, function),
                static (a) => a.Item4(a.Item1, a.Item2, a.Item3),
                contextObject);
        }

        public static bool RunExceptionSafe(System.Action function, out System.Exception exception)
        {
            try
            {
                function();
                exception = null;
                return true;
            }
            catch (System.Exception ex)
            {
                exception = ex;
                return false;
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
                HandleRunSafeException(ex, null);
			}

		}

        /// <summary>
		/// Invokes all subscribed delegates, and makes sure any exception is caught and logged, so that all
		/// subscribers will get notified.
		/// </summary>
		public static void InvokeEventExceptionSafe( this MulticastDelegate eventDelegate, params object[] parameters )
        {
            RunExceptionSafe( () => {
                var delegates = eventDelegate.GetInvocationList ();

                foreach (var del in delegates) {
                    if (del.Method != null) {
                        try {
                            del.Method.Invoke (del.Target, parameters);
                        } catch(System.Exception ex) {
                            HandleRunSafeException(ex, null);
                        }
                    }
                }
            });
        }

		public static void SendMessageToObjectsOfType<T> (string msg, params object[] args)
            where T : UnityEngine.Component
		{
			var objects = UnityEngine.Object.FindObjectsByType<T>(FindObjectsSortMode.InstanceID);

			foreach (var obj in objects) {
				obj.InvokeExceptionSafe (msg, args);
			}

		}


        public static IEnumerable<T> WhereAlive<T> (this IEnumerable<T> enumerable)
	        where T : UnityEngine.Object
        {
	        return enumerable.Where(static obj => obj != null);
        }

        public static int RemoveDeadObjects<T> (this List<T> list)
	        where T : UnityEngine.Object
        {
            return list.RemoveAll(static item => null == item);
        }

        public static int RemoveDeadObjectsIfNotEmpty<T>(this List<T> list)
	        where T : UnityEngine.Object
        {
	        if (list.Count > 0)
		        return list.RemoveDeadObjects();
	        return 0;
        }

        public static T RemoveFromEndUntilAliveObject<T>(this List<T> list)
            where T : UnityEngine.Object
        {
            while (list.Count > 0)
            {
                T obj = list.RemoveLast();
                if (obj != null)
                    return obj;
            }

            return null;
        }

        public static T RemoveFromEndUntilAliveObject<T>(this HashSetAndList<T> hashSetAndList)
            where T : UnityEngine.Object
        {
            while (hashSetAndList.Count > 0)
            {
                T obj = hashSetAndList.RemoveLast();
                if (obj != null)
                    return obj;
            }

            return null;
        }

        public static int RemoveDeadObjects<T>(this HashSet<T> hashSet)
            where T : UnityEngine.Object
        {
            return hashSet.RemoveWhere(static obj => null == obj);
        }


        public static Vector2 GetSize (this Texture2D tex)
		{
			return new Vector2 (tex.width, tex.height);
		}

		public static Color OrangeColor { get { return Color.Lerp (Color.yellow, Color.red, 0.5f); } }

        /// <summary>
        /// Does User Interface have focus currently ?
        /// </summary>
        public static bool UIHasFocus()
        {
            if (GUIUtility.hotControl != 0)
                return true;

            EventSystem evSys = EventSystem.current;
            if (evSys == null)
                return false;

            if (evSys.currentSelectedGameObject != null)
                return true;

            return false;
        }

        /// <summary>
        /// Get currently focused UI object.
        /// </summary>
        public static GameObject UIFocusedObject()
        {
            if (GUIUtility.hotControl != 0)
                return null;

            EventSystem evSys = EventSystem.current;
            if (evSys == null)
                return null;

            return evSys.currentSelectedGameObject;
        }

        /// <summary>
        /// Does User Interface have keyboard focus currently ?
        /// </summary>
        public static bool UIHasKeyboardFocus()
        {
            if (GUIUtility.keyboardControl != 0)
                return true;

            EventSystem evSys = EventSystem.current;
            if (evSys == null)
                return false;

            if (evSys.currentSelectedGameObject == null)
                return false;

            if (!evSys.currentSelectedGameObject.TryGetComponent<UnityEngine.UI.InputField>(out var inputField))
                return false;

            if (!inputField.isFocused)
                return false;

            return true;
        }

        public static void RemoveThenAddListener(this UnityEvent unityEvent, UnityAction action)
        {
            unityEvent.RemoveListener(action);
            unityEvent.AddListener(action);
        }

        public static void RemoveThenAddListener<T>(this UnityEvent<T> unityEvent, UnityAction<T> action)
        {
            unityEvent.RemoveListener(action);
            unityEvent.AddListener(action);
        }

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

        public static Camera GetSceneViewCamera()
        {
#if UNITY_EDITOR
            var sceneView = UnityEditor.SceneView.lastActiveSceneView;
            if (null == sceneView)
                return null;

            return sceneView.camera;
#else
            return null;
#endif
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

        public static bool IsIL2CPPBuild
        {
            get
            {
#if ENABLE_IL2CPP
                return true;
#else
                return false;
#endif
            }
        }

        public static bool IsOnDesktopPlatform => 
            !Application.isMobilePlatform && !Application.isConsolePlatform && SystemInfo.deviceType == DeviceType.Desktop;

        public static readonly bool IsWebGLBuildTarget =
#if UNITY_WEBGL
            true;
#else
            false;
#endif

        public static bool PlatformSupportsCreatingProcesses => 
            !Application.isMobilePlatform && !Application.isConsolePlatform && Application.platform != RuntimePlatform.WebGLPlayer;


        public static int GetTotalPCMSamplesSizeInBytes(this AudioClip clip)
        {
            return clip.samples * sizeof(float) * clip.channels;
        }
    }
}