/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"CursorIcon.cs"
 * 
 *	This script is a data class for cursor icons.
 * 
 */

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class CursorIcon : CursorIconBase
	{

		public string label;
		public int lineID = -1;
		public int id;

		
		public CursorIcon ()
		{
			texture = null;
			id = 0;
			lineID = -1;
			isAnimated = false;
			numFrames = 1;
			size = 0.04f;

			label = "Icon " + (id + 1).ToString ();
		}

		
		public CursorIcon (int[] idArray)
		{
			texture = null;
			id = 0;
			lineID = -1;
			isAnimated = false;
			numFrames = 1;

			// Update id based on array
			foreach (int _id in idArray)
			{
				if (id == _id)
				{
					id ++;
				}
			}
			
			label = "Icon " + (id + 1).ToString ();
		}


		public void Copy (CursorIcon _cursorIcon)
		{
			label = _cursorIcon.label;
			lineID = _cursorIcon.lineID;
			id = _cursorIcon.id;

			base.Copy (_cursorIcon);
		}

	}


	[System.Serializable]
	public class CursorIconBase
	{
		
		public Texture2D texture;
		public bool isAnimated = false;
		public int numFrames = 1;
		public int numRows = 1;
		public int numCols = 1;
		public float size = 0.015f;
		public float animSpeed = 4f;
		public bool endAnimOnLastFrame = false;
		public Vector2 clickOffset;

		private string uniqueIdentifier;
		private float frameIndex = 0f;
		private float frameWidth = -1f;
		private float frameHeight = -1;


		public CursorIconBase ()
		{
			texture = null;
			isAnimated = false;
			numFrames = numRows = numCols = 1;
			size = 0.015f;
			frameIndex = 0f;
			frameWidth = frameHeight = -1f;
			animSpeed = 4;
			endAnimOnLastFrame = false;
			clickOffset = Vector2.zero;
		}
		

		public void Copy (CursorIconBase _icon)
		{
			texture = _icon.texture;
			isAnimated = _icon.isAnimated;
			numFrames = _icon.numFrames;
			animSpeed = _icon.animSpeed;
			endAnimOnLastFrame = _icon.endAnimOnLastFrame;
			clickOffset = _icon.clickOffset;
			numRows = _icon.numRows;
			numCols = _icon.numCols;
		}


		public void DrawAsInteraction (Rect _rect, bool isActive)
		{
			if (texture == null)
			{
				return;
			}

			if (isAnimated && numFrames > 0 && Application.isPlaying)
			{
				if (isActive)
				{
					GUI.DrawTextureWithTexCoords (_rect, texture, GetAnimatedRect ());
				}
				else
				{
					GUI.DrawTextureWithTexCoords (_rect, texture, new Rect (0f, 0f, frameWidth, frameHeight));
					frameIndex = 0f;
				}
			}
			else
			{
				GUI.DrawTexture (_rect, texture, ScaleMode.StretchToFill, true, 0f);
			}
		}


		public Texture2D GetAnimatedTexture ()
		{
			if (texture == null)
			{
				return null;
			}

			if (isAnimated)
			{
				Rect animatedRect = GetAnimatedRect ();

				int x = Mathf.FloorToInt (animatedRect.x * texture.width);
				int y = Mathf.FloorToInt (animatedRect.y * texture.height);
				int width = Mathf.FloorToInt (animatedRect.width * texture.width);
				int height = Mathf.FloorToInt (animatedRect.height * texture.height);

				Color[] pix = texture.GetPixels (x, y, width, height);
				Texture2D frameTex = new Texture2D ((int) (frameWidth * texture.width), (int) (frameHeight * texture.height));
				frameTex.SetPixels (pix);
				frameTex.Apply();
				return frameTex;
			}
			return texture;
		}


		public string GetName ()
		{
			return uniqueIdentifier;
		}


		public void Draw (Vector2 centre)
		{
			if (texture == null)
			{
				return;
			}
			
			Rect _rect = AdvGame.GUIBox (centre, size);
			_rect.x -= clickOffset.x * _rect.width;
			_rect.y -= clickOffset.y * _rect.height;

			if (isAnimated && numFrames > 0 && Application.isPlaying)
			{
				GUI.DrawTextureWithTexCoords (_rect, texture, GetAnimatedRect ());
			}
			else
			{
				GUI.DrawTexture (_rect, texture, ScaleMode.ScaleToFit, true, 0f);
			}
		}


		public Rect GetAnimatedRect ()
		{
			int currentRow = 1;
			int frameInRow = 1;

			if (frameIndex < 0f)
			{
				frameIndex = 0f;
			}
			else if (frameIndex < numFrames)
			{
				if (Time.deltaTime == 0f)
				{
					frameIndex += 0.02f * animSpeed;
				}
				else
				{
					frameIndex += Time.deltaTime * animSpeed;
				}
			}
			
			frameInRow = Mathf.FloorToInt (frameIndex)+1;
			while (frameInRow > numCols)
			{
				frameInRow -= numCols;
				currentRow ++;
			}

			if (frameIndex >= numFrames)
			{
				if (!endAnimOnLastFrame)
				{
					frameIndex = 0f;
					frameInRow = 1;
					currentRow = 1;
				}
				else
				{
					frameIndex = numFrames;
					frameInRow -= 1;
				}
			}

			if (texture != null)
			{
				uniqueIdentifier = texture.name + frameInRow.ToString () + currentRow.ToString ();
			}

			return new Rect (frameWidth * (frameInRow-1), frameHeight * (numRows - currentRow), frameWidth, frameHeight);
		}


		public Rect GetAnimatedRect (int _frameIndex)
		{
			int frameInRow = _frameIndex + 1;
			int currentRow = 1;
			while (frameInRow > numCols)
			{
				frameInRow -= numCols;
				currentRow ++;
			}
			
			if (_frameIndex >= numFrames)
			{
				frameInRow = 1;
				currentRow = 1;
			}
			
			return new Rect (frameWidth * (frameInRow-1), frameHeight * (numRows - currentRow), frameWidth, frameHeight);
		}


		public void Reset ()
		{
			if (isAnimated)
			{
				if (numFrames > 0)
				{
					frameWidth = 1f / numCols;
					frameHeight = 1f / numRows;
					frameIndex = 0f;
				}
				else
				{
					Debug.LogWarning ("Cannot have an animated cursor with less than one frame!");
				}

				if (animSpeed < 0)
				{
					animSpeed = 0;
				}
			}
		}


		#if UNITY_EDITOR

		public void ShowGUI (bool includeSize)
		{
			ShowGUI (includeSize, CursorRendering.Software);	
		}


		public void ShowGUI (bool includeSize, CursorRendering cursorRendering)
		{
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Texture:", GUILayout.Width (145));
			texture = (Texture2D) EditorGUILayout.ObjectField (texture, typeof (Texture2D), false, GUILayout.Width (70), GUILayout.Height (70));
			EditorGUILayout.EndHorizontal ();

			if (includeSize)
			{
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Size:", GUILayout.Width (30f));
				size = EditorGUILayout.FloatField (size, GUILayout.Width (50f));
				if (cursorRendering == CursorRendering.Hardware)
				{
					EditorGUILayout.LabelField ("Click offset (from top left):", GUILayout.Width (150f));
				}
				else
				{
					EditorGUILayout.LabelField ("Click offset (from centre):", GUILayout.Width (150f));
				}
				clickOffset = EditorGUILayout.Vector2Field ("", clickOffset, GUILayout.Width (130f));
				EditorGUILayout.EndHorizontal ();
			}
			
			isAnimated = EditorGUILayout.Toggle ("Animate?", isAnimated);
			if (isAnimated)
			{
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Frames:", GUILayout.Width (50f));
				numFrames = EditorGUILayout.IntField (numFrames, GUILayout.Width (70f));
				EditorGUILayout.LabelField ("Rows:", GUILayout.Width (50f));
				numRows = EditorGUILayout.IntField (numRows, GUILayout.Width (70f));
				EditorGUILayout.LabelField ("Columns:", GUILayout.Width (50f));
				numCols = EditorGUILayout.IntField (numCols, GUILayout.Width (70f));
				EditorGUILayout.EndHorizontal ();

				animSpeed = EditorGUILayout.FloatField ("Animation speed:", animSpeed);
				endAnimOnLastFrame = EditorGUILayout.Toggle ("End on last frame?", endAnimOnLastFrame);
			}
		}

		#endif

	}


	[System.Serializable]
	public class HotspotPrefix
	{

		public string label;
		public int lineID;


		public HotspotPrefix (string text)
		{
			label = text;
			lineID = -1;
		}

	}

}