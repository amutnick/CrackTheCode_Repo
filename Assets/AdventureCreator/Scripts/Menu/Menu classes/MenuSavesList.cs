/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuSavesList.cs"
 * 
 *	This MenuElement handles the display of any saved games recorded.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;	
#endif

namespace AC
{

	public class MenuSavesList : MenuElement
	{

		public enum SaveDisplayType { LabelOnly, ScreenshotOnly, LabelAndScreenshot };

		public string newSaveText = "New save";
		public TextEffects textEffects;
		public TextAnchor anchor;
		public AC_SaveListType saveListType;
		public int maxSaves = 5;
		public ActionListAsset actionListOnSave;
		public SaveDisplayType displayType = SaveDisplayType.LabelOnly;
		public Texture2D blankSlotTexture;

		public bool fixedOption;
		public int optionToShow;

		private bool newSaveSlot = false;

		
		public override void Declare ()
		{
			newSaveText = "New save";
			isVisible = true;
			isClickable = true;
			numSlots = 1;
			maxSaves = 5;

			SetSize (new Vector2 (20f, 5f));
			anchor = TextAnchor.MiddleCenter;
			saveListType = AC_SaveListType.Save;

			actionListOnSave = null;
			newSaveSlot = false;
			textEffects = TextEffects.None;
			displayType = SaveDisplayType.LabelOnly;
			blankSlotTexture = null;

			fixedOption = false;
			optionToShow = 1;

			base.Declare ();
		}


		public override MenuElement DuplicateSelf ()
		{
			MenuSavesList newElement = CreateInstance <MenuSavesList>();
			newElement.Declare ();
			newElement.CopySavesList (this);
			return newElement;
		}
		
		
		public void CopySavesList (MenuSavesList _element)
		{
			newSaveText = _element.newSaveText;
			textEffects = _element.textEffects;
			anchor = _element.anchor;
			saveListType = _element.saveListType;
			maxSaves = _element.maxSaves;
			actionListOnSave = _element.actionListOnSave;
			displayType = _element.displayType;
			blankSlotTexture = _element.blankSlotTexture;
			fixedOption = _element.fixedOption;
			optionToShow = _element.optionToShow;
			
			base.Copy (_element);
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI ()
		{
			EditorGUILayout.BeginVertical ("Button");

			fixedOption = EditorGUILayout.Toggle ("Fixed option number?", fixedOption);
			if (fixedOption)
			{
				numSlots = 1;
				slotSpacing = 0f;
				optionToShow = EditorGUILayout.IntField ("Option to display:", optionToShow);
			}
			else
			{
				numSlots = EditorGUILayout.IntSlider ("Test slots:", numSlots, 1, 10);
				maxSaves = EditorGUILayout.IntField ("Max saves:", maxSaves);
				slotSpacing = EditorGUILayout.Slider ("Slot spacing:", slotSpacing, 0f, 20f);
				orientation = (ElementOrientation) EditorGUILayout.EnumPopup ("Slot orientation:", orientation);
				if (orientation == ElementOrientation.Grid)
				{
					gridWidth = EditorGUILayout.IntSlider ("Grid size:", gridWidth, 1, 10);
				}
			}

			anchor = (TextAnchor) EditorGUILayout.EnumPopup ("Text alignment:", anchor);
			textEffects = (TextEffects) EditorGUILayout.EnumPopup ("Text effect:", textEffects);
			displayType = (SaveDisplayType) EditorGUILayout.EnumPopup ("Display:", displayType);
			if (displayType != SaveDisplayType.LabelOnly)
			{
				EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Empty slot texture:", GUILayout.Width (145f));
					blankSlotTexture = (Texture2D) EditorGUILayout.ObjectField (blankSlotTexture, typeof (Texture2D), false, GUILayout.Width (70f), GUILayout.Height (30f));
				EditorGUILayout.EndHorizontal ();
			}
			saveListType = (AC_SaveListType) EditorGUILayout.EnumPopup ("Click action:", saveListType);
			if (saveListType == AC_SaveListType.Save)
			{
				newSaveText = EditorGUILayout.TextField ("'New save' text:", newSaveText);
				actionListOnSave = (ActionListAsset) EditorGUILayout.ObjectField ("ActionList after saving:", actionListOnSave, typeof (ActionListAsset), false);
			}
				
			EditorGUILayout.EndVertical ();
			
			base.ShowGUI ();
		}
		
		#endif


		public override string GetLabel (int slot)
		{
			return SaveSystem.GetSaveSlotLabel (slot, optionToShow, fixedOption);
		}


		public bool ProcessClick (int _slot)
		{
			if (saveListType == AC_SaveListType.Save)
			{
				if (newSaveSlot && _slot == (numSlots - 1))
				{
					SaveSystem.SaveNewGame ();
				}
				else
				{
					SaveSystem.SaveGame (_slot, optionToShow, fixedOption);
				}

				if (actionListOnSave)
				{
					AdvGame.RunActionListAsset (actionListOnSave);
				}
			}
			else if (saveListType == AC_SaveListType.Load)
			{
				if (fixedOption && newSaveSlot)
				{
					return false;
				}

				return SaveSystem.LoadGame (_slot, optionToShow, fixedOption);
			}

			return true;
		}


		public override void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display (_style, _slot, zoom, isActive);

			if (displayType != SaveDisplayType.LabelOnly)
			{
				Texture2D tex = SaveSystem.GetSaveSlotScreenshot (_slot, optionToShow, fixedOption);
				if (tex != null)
				{
					GUI.DrawTexture (ZoomRect (GetSlotRectRelative (_slot), zoom), tex, ScaleMode.StretchToFill, true, 0f);
				}
				else if (blankSlotTexture != null)
				{
					GUI.DrawTexture (ZoomRect (GetSlotRectRelative (_slot), zoom), blankSlotTexture, ScaleMode.StretchToFill, true, 0f);
				}
			}

			if (displayType != SaveDisplayType.ScreenshotOnly)
			{
				_style.alignment = anchor;
				if (zoom < 1f)
				{
					_style.fontSize = (int) ((float) _style.fontSize * zoom);
				}
				
				string slotLabel = "";
				if (newSaveSlot)
				{
					if (!fixedOption && _slot == (numSlots -1))
					{
						slotLabel = TranslateLabel (newSaveText);
					}
					else if (fixedOption && saveListType == AC_SaveListType.Save)
					{
						slotLabel = TranslateLabel (newSaveText);
					}
					else
					{
						slotLabel = SaveSystem.GetSaveSlotLabel (_slot, optionToShow, fixedOption);
					}
				}
				else
				{
					slotLabel = SaveSystem.GetSaveSlotLabel (_slot, optionToShow, fixedOption);
				}

				if (textEffects != TextEffects.None)
				{
					AdvGame.DrawTextEffect (ZoomRect (GetSlotRectRelative (_slot), zoom), slotLabel, _style, Color.black, _style.normal.textColor, 2, textEffects);
				}
				else
				{
					GUI.Label (ZoomRect (GetSlotRectRelative (_slot), zoom), slotLabel, _style);
				}
			}
		}
		
		
		public override void RecalculateSize ()
		{
			newSaveSlot = false;

			if (Application.isPlaying)
			{
				if (fixedOption)
				{
					numSlots = 1;

					if (!SaveSystem.DoesSaveExist (optionToShow))
					{
						newSaveSlot = true;
					}
				}
				else
				{
					numSlots = SaveSystem.GetNumSlots ();

					if (saveListType == AC_SaveListType.Save && numSlots < maxSaves)
					{
						newSaveSlot = true;
						numSlots ++;
					}
				}
			}

			base.RecalculateSize ();
		}
		
		
		protected override void AutoSize ()
		{
			if (displayType == SaveDisplayType.ScreenshotOnly)
			{
				if (blankSlotTexture != null)
				{
					AutoSize (new GUIContent (blankSlotTexture));
				}
				else
				{
					AutoSize (GUIContent.none);
				}
			}
			else if (displayType == SaveDisplayType.LabelAndScreenshot)
			{
				if (blankSlotTexture != null)
				{
					AutoSize (new GUIContent (blankSlotTexture));
				}
				else
				{
					AutoSize (new GUIContent (SaveSystem.GetSaveSlotLabel (0, optionToShow, fixedOption)));
				}
			}
			else
			{
				AutoSize (new GUIContent (SaveSystem.GetSaveSlotLabel (0, optionToShow, fixedOption)));
			}
		}
		
	}

}