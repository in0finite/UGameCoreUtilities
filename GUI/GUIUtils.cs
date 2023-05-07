using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UGameCore.Utilities
{
    public enum ScreenCorner { TopRight, TopLeft, BottomRight, BottomLeft }

    public static class GUIUtils
    {

		private	static	GUIStyle	styleWithBackground = new GUIStyle ();

		private static GUIStyle s_centeredLabelStyle = null;
		public static GUIStyle CenteredLabelStyle {
			get {
				if (null == s_centeredLabelStyle)
					s_centeredLabelStyle = new GUIStyle (GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
				return s_centeredLabelStyle;
			}
		}

		public static Rect ScreenRect { get { return new Rect (0, 0, Screen.width, Screen.height); } }



        public static Rect GetCornerRect(ScreenCorner corner, Vector2 size, Vector2? padding = null)
        {
            return GetCornerRect(corner, size.x, size.y, padding);
        }

        public static Rect GetCornerRect(ScreenCorner corner, float width, float height, Vector2? padding = null)
        {
            float padX = 0,
                  padY = 0;

            if (padding != null)
            {
                padX = padding.Value.x;
                padY = padding.Value.y;
            }

            switch (corner)
            {
                case ScreenCorner.TopLeft:
                    return new Rect(padX, padY, width, height);

                case ScreenCorner.TopRight:
                    return new Rect(Screen.width - (width + padX), padY, width, height);

                case ScreenCorner.BottomLeft:
                    return new Rect(padX, Screen.height - (height + padY), width, height);

                case ScreenCorner.BottomRight:
                    return new Rect(Screen.width - (width + padX), Screen.height - (height + padY), width, height);
            }

            return default(Rect);
        }

		public static Rect GetCenteredRect( Vector2 size ) {

			Vector2 pos = new Vector2 (Screen.width * 0.5f, Screen.height * 0.5f);
			pos -= size * 0.5f;

			return new Rect (pos, size);
		}

		public static Rect GetCenteredRectPerc( Vector2 sizeInScreenPercentage ) {

			return GetCenteredRect (new Vector2 (Screen.width * sizeInScreenPercentage.x, Screen.height * sizeInScreenPercentage.y));

		}

		public	static	Vector2	CalcScreenSizeForContent( GUIContent content, GUIStyle style ) {

			return style.CalcScreenSize (style.CalcSize (content));
		}

		public	static	Vector2	CalcScreenSizeForText( string text, GUIStyle style ) {

			return CalcScreenSizeForContent (new GUIContent (text), style);
		}

		public	static	bool	ButtonWithCalculatedSize( string text ) {
			return ButtonWithCalculatedSize(new GUIContent(text));
		}

		public static bool ButtonWithCalculatedSize(string text, float minWidth, float minHeight, GUIStyle style)
		{
			return ButtonWithCalculatedSize(new GUIContent(text), minWidth, minHeight, style);
		}

		public static bool ButtonWithCalculatedSize(string text, float minWidth, float minHeight)
		{
			return ButtonWithCalculatedSize(text, minWidth, minHeight, GUI.skin.button);
		}

		public static bool ButtonWithCalculatedSize(GUIContent content)
		{
			return ButtonWithCalculatedSize(content, 0f, 0f);
		}

		public static bool ButtonWithCalculatedSize(GUIContent content, float minWidth, float minHeight)
		{
			return ButtonWithCalculatedSize(content, minWidth, minHeight, GUI.skin.button);
		}

		public static bool ButtonWithCalculatedSize(GUIContent content, float minWidth, float minHeight, GUIStyle style)
		{
			Vector2 size = CalcScreenSizeForContent (content, style);
			
			if (size.x < minWidth)
				size.x = minWidth;
			if (size.y < minHeight)
				size.y = minHeight;

			return GUILayout.Button (content, style, GUILayout.Width (size.x), GUILayout.Height (size.y));
		}

		public	static	bool	ButtonWithColor( Rect rect, string text, Color color) {

			var oldColor = GUI.backgroundColor;
			GUI.backgroundColor = color;

			bool result = GUI.Button (rect, text);

			GUI.backgroundColor = oldColor;

			return result;
		}

		public static void DrawRect (Rect position, Color color, GUIContent content = null)
		{
			var backgroundColor = GUI.backgroundColor;
			GUI.backgroundColor = color;
			styleWithBackground.normal.background = Texture2D.whiteTexture;
			GUI.Box (position, content ?? GUIContent.none, styleWithBackground);
			GUI.backgroundColor = backgroundColor;
		}

		public static void DrawBar (Rect rect, float fillPerc, Color fillColor, Color backgroundColor, float borderWidth)
		{
			fillPerc = Mathf.Clamp01 (fillPerc);

			Rect fillRect = rect;
			fillRect.position += Vector2.one * borderWidth;
			fillRect.size -= Vector2.one * borderWidth * 2;

			// first fill with black - that will be the border
			GUIUtils.DrawRect( rect, Color.black );

			// fill with background
			GUIUtils.DrawRect( fillRect, backgroundColor );

			// draw filled part
			fillRect.width *= fillPerc;
			GUIUtils.DrawRect( fillRect, fillColor );

		}

		public static int TabsControl (int currentTabIndex, params string[] tabNames)
		{
			return GUILayout.Toolbar (currentTabIndex, tabNames);
		}

		public static Rect GetRectForBarAsBillboard (Vector3 worldPos, float worldWidth, float worldHeight, Camera cam)
		{

			Vector3 camRight = cam.transform.right;
		//	Vector3 camUp = cam.transform.up;

//			Vector3 upperLeft = worldPos - camRight * worldWidth * 0.5f + camUp * worldHeight * 0.5f;
//			Vector3 upperRight = upperLeft + camRight * worldWidth;
//			Vector3 lowerLeft = upperLeft - camUp * worldHeight;
//			Vector3 lowerRight = lowerLeft + camRight * worldWidth;

			Vector3 leftWorld = worldPos - camRight * worldWidth * 0.5f;
			Vector3 rightWorld = worldPos + camRight * worldWidth * 0.5f;

			Vector3 leftScreen = cam.WorldToScreenPoint (leftWorld);
			Vector3 rightScreen = cam.WorldToScreenPoint (rightWorld);

			if (leftScreen.z < 0 || rightScreen.z < 0)
				return Rect.zero;

			// transform to gui coordinates
			leftScreen.y = Screen.height - leftScreen.y;
			rightScreen.y = Screen.height - rightScreen.y;

			float screenWidth = rightScreen.x - leftScreen.x;
			float screenHeight = screenWidth * worldHeight / worldWidth;

			return new Rect (new Vector2(leftScreen.x, leftScreen.y - screenHeight * 0.5f), new Vector2(screenWidth, screenHeight) );
		}

		public	static	void	CenteredLabel(Vector2 pos, string text) {

			Vector2 size = CalcScreenSizeForText (text, GUI.skin.label);

			GUI.Label (new Rect (pos - size * 0.5f, size), text);
		}

		public static void DrawHorizontalLine(float height, float spaceBetween, Color color)
		{
			GUILayout.Space(spaceBetween);
			float width = GUILayoutUtility.GetLastRect().width;
			Rect rect = GUILayoutUtility.GetRect(width, height);
			GUIUtils.DrawRect(rect, color);
			GUILayout.Space(spaceBetween);
		}

		/// <summary> Draws the texture flipped around Y axis. </summary>
		public	static	void	DrawTextureWithYFlipped(Rect rect, Texture2D tex) {

			var savedMatrix = GUI.matrix;

			GUIUtility.ScaleAroundPivot (new Vector2 (1, -1), rect.center);

			GUI.DrawTexture (rect, tex);

			GUI.matrix = savedMatrix;
		}

		public static Rect DrawItemsInARowPerc (Rect rect, System.Action<Rect, string> drawItem, string[] items, float[] widthPercs ) {

			Rect itemRect = rect;
			float x = rect.position.x;

			for (int i = 0; i < items.Length; i++) {
				float width = widthPercs [i] * rect.width;

				itemRect.position = new Vector2 (x, itemRect.position.y);
				itemRect.width = width;

				drawItem (itemRect, items [i]);

				x += width;
			}

			rect.position += new Vector2 (x, 0f);
			rect.width -= x;
			return rect;
		}

		public static Rect DrawItemsInARow (Rect rect, System.Action<Rect, string> drawItem, string[] items, float[] widths ) {

			float[] widthPercs = new float[widths.Length];
			for (int i = 0; i < widths.Length; i++) {
				widthPercs [i] = widths [i] / rect.width;
			}

			return DrawItemsInARowPerc (rect, drawItem, items, widthPercs);
		}

		public static Rect GetNextRectInARowPerc (Rect rowRect, ref int currentRectIndex, float spacing, params float[] widthPercs) {

			float x = rowRect.position.x;

			for (int i = 0; i < currentRectIndex; i++) {
				x += widthPercs [i] * rowRect.width;
				x += spacing;
			}

			float width = widthPercs [currentRectIndex] * rowRect.width;
			currentRectIndex++;

			return new Rect( x, rowRect.position.y, width, rowRect.height );
		}

		public static Rect GetNextRectInARow (Rect rowRect, ref int currentRectIndex, float spacing, params float[] widths) {

			float[] widthPercs = new float[widths.Length];
			for (int i = 0; i < widths.Length; i++) {
				widthPercs [i] = widths [i] / rowRect.width;
			}

			return GetNextRectInARowPerc (rowRect, ref currentRectIndex, spacing, widthPercs);
		}

		[System.Serializable]
		public class PagedViewParams
        {
			public int currentPage; // page number, not an index
			public int totalNumItems;
			public int pageSize = 30;
			public int jumpButtonPageCount = 10;

			public float? width; // for layout draw
			public float height = 25; // for layout draw
			public float buttonWidth = 25;
			public float spacingBetweenButtons = 1;

			public int NumPages => Mathf.CeilToInt(this.totalNumItems / (float)this.pageSize);

			public void FixValues()
            {
				if (currentPage < 1)
					currentPage = 1;
				if (width.HasValue && width.Value <= 0)
					width = null;
				if (height <= 0)
					height = 0;
				if (buttonWidth <= 0)
					buttonWidth = 0;
				if (spacingBetweenButtons <= 0)
					spacingBetweenButtons = 0;
				if (totalNumItems < 0)
					totalNumItems = 0;
				if (pageSize < 1)
					pageSize = 1;
				if (jumpButtonPageCount < 1)
					jumpButtonPageCount = 1;
            }

            public IEnumerable<T> ApplyPaging<T>(IEnumerable<T> enumerable)
            {
				this.FixValues();
                return enumerable.Skip((this.currentPage - 1) * this.pageSize).Take(this.pageSize);
            }
        }

		public static float FindOutLayoutWidth()
        {
			GUILayout.Space(1);
			return GUILayoutUtility.GetLastRect().width;
        }

		public static int DrawPagedViewNumbers(Rect rect, int currentPage, int numPages)
        {
			var pagedViewParams = new PagedViewParams();
			pagedViewParams.currentPage = currentPage;
			// page size doesn't matter here - it can be any value
			pagedViewParams.totalNumItems = numPages * pagedViewParams.pageSize;
			DrawPagedViewNumbers(rect, pagedViewParams);
			return pagedViewParams.currentPage;
		}

		public static void DrawPagedViewNumbers(Rect rect, PagedViewParams pagedViewParams)
		{
			pagedViewParams.FixValues();

			int currentPage = pagedViewParams.currentPage;
			int numPages = pagedViewParams.NumPages;

			int resultingPage = currentPage;
			float spacing = pagedViewParams.spacingBetweenButtons;
			float buttonWidth = pagedViewParams.buttonWidth;

			Rect btnRect = rect;
			btnRect.width = buttonWidth;

			// << < 1 2 3 4 > >>

			// always display 2 buttons on ends - they move to first/last page
			// always display 2 buttons for fast scrolling - they move the view for 10 pages
			// display as many page number buttons as possible in the middle

			float minWidth = spacing + buttonWidth + spacing + buttonWidth + spacing + buttonWidth + spacing + buttonWidth;
			float widthLeftForMiddleButtons = rect.width - minWidth;
			int numMiddleButtonsToShow = Mathf.FloorToInt(widthLeftForMiddleButtons / (buttonWidth + spacing));
			if (numMiddleButtonsToShow > numPages)
				numMiddleButtonsToShow = numPages;

			// 2 buttons on left side

			btnRect.position += new Vector2(spacing, 0f);
			if (GUI.Button(btnRect, "<<"))
            {
				resultingPage = 1;
			}

			btnRect.position += new Vector2(buttonWidth + spacing, 0f);
			if (GUI.Button(btnRect, "<"))
			{
				resultingPage -= pagedViewParams.jumpButtonPageCount;
			}

			// middle buttons
			
			int firstMiddleButtonPageNumber = currentPage - (numMiddleButtonsToShow - 1) / 2;
			if (firstMiddleButtonPageNumber < 1)
				firstMiddleButtonPageNumber = 1;
			if (firstMiddleButtonPageNumber + numMiddleButtonsToShow > numPages)
				firstMiddleButtonPageNumber = numPages - numMiddleButtonsToShow + 1;

            for (int i = 0; i < numMiddleButtonsToShow; i++)
            {
				int page = firstMiddleButtonPageNumber + i;
				GUIStyle style = currentPage == page ? GUI.skin.box : GUI.skin.button;
				btnRect.position += new Vector2(buttonWidth + spacing, 0f);
				if (GUI.Button(btnRect, $"{page}", style))
				{
					resultingPage = page;
				}
			}

			// 2 buttons on right side

			btnRect.position += new Vector2(buttonWidth + spacing, 0f);
			if (GUI.Button(btnRect, ">"))
			{
				resultingPage += pagedViewParams.jumpButtonPageCount;
			}

			btnRect.position += new Vector2(buttonWidth + spacing, 0f);
			if (GUI.Button(btnRect, ">>"))
			{
				resultingPage = numPages;
			}


			resultingPage = Mathf.Clamp(resultingPage, 1, numPages);
			pagedViewParams.currentPage = resultingPage;
		}

		public static void LayoutDrawPagedViewNumbers(PagedViewParams pagedViewParams)
		{
			pagedViewParams.FixValues();
			float width = pagedViewParams.width ?? FindOutLayoutWidth();
			Rect rect = GUILayoutUtility.GetRect(width, pagedViewParams.height);
			DrawPagedViewNumbers(rect, pagedViewParams);
		}

		public static int DrawPagedViewNumbers (Rect rect, int currentPage, int totalNumItems, int numItemsPerPage)
		{
			int numPages = Mathf.CeilToInt (totalNumItems / (float) numItemsPerPage);
			return DrawPagedViewNumbers (rect, currentPage, numPages);
		}

    }
}