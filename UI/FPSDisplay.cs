using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Diagnostics;

namespace UGameCore.Utilities
{
    public class FPSDisplay : MonoBehaviour
	{
		private static readonly int s_fpsTextureWidth = 75;
		private static readonly int s_fpsTextureHeight = 25;
		private float m_fpsMaximum = 60.0f;
		private Texture2D m_fpsTexture = null;
		private Color[] m_colors = null;
		private float[] m_fpsHistory = null;
		private int m_fpsIndex = 0;

		public RawImage fpsImage;
		public Text fpsText;

		public bool updateFPS = true;

        readonly Stopwatch m_stopwatch = new();

		[Range(0, 2)] public float timeToUpdateFpsText = 0.5f;
		double m_timeSinceUpdatedFpsText = 0;
		double m_textFpsSum = 0;
		int m_textFpsCount = 0;


		void InitTexure()
		{
			if (m_fpsTexture != null)
				return;

			m_fpsTexture = new Texture2D(s_fpsTextureWidth, s_fpsTextureHeight, TextureFormat.RGBA32, false, true);

			m_fpsHistory = new float[s_fpsTextureWidth];

			m_colors = new Color[m_fpsTexture.width * m_fpsTexture.height];

			if (this.fpsImage != null)
				this.fpsImage.texture = this.m_fpsTexture;
		}

        void OnDestroy()
        {
			if (m_fpsTexture != null)
				Destroy(m_fpsTexture);
			m_fpsTexture = null;
        }

        void Update()
		{
			double deltaTime = m_stopwatch.Elapsed.TotalSeconds;
			m_stopwatch.Restart();

            if (this.updateFPS)
			{
				this.UpdateTexture(deltaTime);
				this.UpdateText(deltaTime);
			}
		}

		void UpdateTexture(double deltaTime)
		{
			if (null == this.fpsImage)
				return;

			if (0.0 == deltaTime)
				return;

			this.InitTexure();

			float fps = (float)(1.0f / deltaTime);

			UnityEngine.Profiling.Profiler.BeginSample("Reset texture pixels");
			int numPixels = m_fpsTexture.width * m_fpsTexture.height;
			Color backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.66f); // Half-transparent background for FPS graph
			for (int i = 0; i < numPixels; i++)
				m_colors[i] = backgroundColor;
			UnityEngine.Profiling.Profiler.EndSample();

			UnityEngine.Profiling.Profiler.BeginSample("Set pixels");
			m_fpsTexture.SetPixels(m_colors);
			UnityEngine.Profiling.Profiler.EndSample();

			// Append to history storage
			m_fpsHistory[m_fpsIndex] = fps;

			int f = m_fpsIndex;

			if (fps > m_fpsHistory.Average())
				m_fpsMaximum = fps;

			// Draw graph into texture
			UnityEngine.Profiling.Profiler.BeginSample("Set fps history pixels");
			for (int i = m_fpsTexture.width - 1; i >= 0; i--)
			{
				float graphVal = (m_fpsHistory[f] > m_fpsMaximum) ? m_fpsMaximum : m_fpsHistory[f]; //Clamps
				int height = (int)(graphVal * m_fpsTexture.height / (m_fpsMaximum + 0.1f)); //Returns the height of the desired point with a padding of 0.1f units

				float p = m_fpsHistory[f] / m_fpsMaximum,
				r = Mathf.Lerp(1, 1 - p, p),
				g = Mathf.Lerp(p * 2, p, p);

				m_fpsTexture.SetPixel(i, height, new Color(r, g, 0));
				f--;

				if (f < 0)
					f = m_fpsHistory.Length - 1;
			}
			UnityEngine.Profiling.Profiler.EndSample();

			// Next entry in rolling history buffer
			m_fpsIndex++;
			if (m_fpsIndex >= m_fpsHistory.Length)
				m_fpsIndex = 0;

			UnityEngine.Profiling.Profiler.BeginSample("Apply texture");
			m_fpsTexture.Apply(false, false);
			UnityEngine.Profiling.Profiler.EndSample();

            if (this.fpsImage.texture != this.m_fpsTexture)
                this.fpsImage.texture = this.m_fpsTexture;

        }

		void UpdateText(double deltaTime)
		{
			if (null == this.fpsText)
				return;

			if (0.0 == deltaTime)
				return;

			double fps = 1.0 / deltaTime;

			m_textFpsSum += fps;
            m_textFpsCount++;
            m_timeSinceUpdatedFpsText += deltaTime;

			if (m_timeSinceUpdatedFpsText < this.timeToUpdateFpsText)
				return;

			double averageFps = m_textFpsSum / (double)m_textFpsCount;

			m_textFpsSum = 0;
			m_textFpsCount = 0;
			m_timeSinceUpdatedFpsText = 0;

            string text = string.Format("{0:0.0} fps", averageFps);
			if (this.fpsText.text != text)
				this.fpsText.text = text;
		}
	}
}
