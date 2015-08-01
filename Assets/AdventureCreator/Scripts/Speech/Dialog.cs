/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Dialog.cs"
 * 
 *	This script handles the running of dialogue lines, speech or otherwise.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace AC
{

	public class Dialog : MonoBehaviour
	{
		
		public bool isMessageAlive { get; set; }
		public bool foundAudio { get; set; }
		[HideInInspector] public bool isBackground = false;

		private bool currentLineHasAudio = false;
		
		private AC.Char speakerChar;
		private string speakerName;
		
		private bool isSkippable = false;
		private string displayText = "";
		private string fullText = "";
		private float displayDuration;
		private float endTime;

		[HideInInspector] public bool pauseGap = false;
		private int gapIndex = -1;
		private List<SpeechGap> speechGaps = new List<SpeechGap>();
		private string allGapText;

		private Options options;
		private AudioSource defaultAudioSource;
		private PlayerInput playerInput;
		private SpeechManager speechManager;
		private StateHandler stateHandler;
		
		
		private void Awake ()
		{
			playerInput = this.GetComponent <PlayerInput>();
			
			if (AdvGame.GetReferences () == null)
			{
				Debug.LogError ("A References file is required - please use the Adventure Creator window to create one.");
			}
			else
			{
				speechManager = AdvGame.GetReferences ().speechManager;
				
				if (speechManager.textScrollSpeed == 0f)
				{
					Debug.LogError ("Cannot have a Text Scroll Speed of zero - please amend your Speech Manager");
				}
			}
			
			if (this.GetComponent <SceneSettings>() && this.GetComponent <SceneSettings>().defaultSound && this.GetComponent <SceneSettings>().defaultSound.GetComponent <AudioSource>())
			{
				defaultAudioSource = this.GetComponent <SceneSettings>().defaultSound.GetComponent <AudioSource>();
			}
		}
		
		
		private void Start ()
		{
			stateHandler = GameObject.FindWithTag  (Tags.persistentEngine).GetComponent <StateHandler>();
			options = stateHandler.GetComponent <Options>();
		}
		
		
		public void UpdateSkipDialogue ()
		{
			if (isSkippable && isMessageAlive && playerInput != null)
			{
				if (pauseGap && !isBackground)
				{
					if ((playerInput.mouseState == MouseState.SingleClick || playerInput.mouseState == MouseState.RightClick))
					{
						if (speechGaps[gapIndex].waitTime < 0f)
						{
							playerInput.mouseState = MouseState.Normal;
							pauseGap = false;
							if (speechManager.scrollSubtitles)
							{
								StartCoroutine ("EndMessage");
							}
						}
						else if (speechManager.allowSpeechSkipping)
						{
							playerInput.mouseState = MouseState.Normal;
							pauseGap = false;
						}
					}
				}

				else if (speechManager.displayForever)
				{
					if ((playerInput.mouseState == MouseState.SingleClick || playerInput.mouseState == MouseState.RightClick))
					{
						playerInput.mouseState = MouseState.Normal;
						
						if (stateHandler.gameState == GameState.Cutscene)
						{
							if (speechManager.endScrollBeforeSkip && speechManager.scrollSubtitles && displayText != fullText)
							{
								// Stop scrolling
								StopCoroutine ("StartMessage");
								displayText = fullText;
							}
							else
							{
								// Stop message
								isMessageAlive = false;
								StartCoroutine ("EndMessage");
							}
						}
					}

					else if (Time.time > endTime && isBackground)
					{
						// Stop message due to timeout
						StartCoroutine ("EndMessage");
					}
				}

				else if ((playerInput.mouseState == MouseState.SingleClick || playerInput.mouseState == MouseState.RightClick) && speechManager && speechManager.allowSpeechSkipping)
				{
					playerInput.mouseState = MouseState.Normal;
					
					if (stateHandler.gameState == GameState.Cutscene || (speechManager.allowGameplaySpeechSkipping && stateHandler.gameState == GameState.Normal))
					{
						if (speechManager.endScrollBeforeSkip && speechManager.scrollSubtitles && displayText != fullText)
						{
							// Stop scrolling
							StopCoroutine ("StartMessage");
							displayText = fullText;
						}
						else
						{
							// Stop message
							isMessageAlive = false;
							StartCoroutine ("EndMessage");
						}
					}
				}

				else if (Time.time > endTime)
				{
					// Stop message due to timeout
					StartCoroutine ("EndMessage");
				}
			}
		}
		
		
		public string GetSpeaker ()
		{
			if (speakerChar)
			{
				if (speakerChar.speechLabel != "")
				{
					return speakerChar.speechLabel;
				}
				return speakerChar.name;
			}
			
			return "";
		}
		
		
		public AC.Char GetSpeakingCharacter ()
		{
			return speakerChar;
		}
		
		
		public Texture2D GetPortrait ()
		{
			if (speakerChar && speakerChar.portraitIcon.texture)
			{
				return speakerChar.portraitIcon.texture;
			}
			return null;
		}
		
		
		public bool IsAnimating ()
		{
			if (speakerChar && speakerChar.portraitIcon.isAnimated)
			{
				return true;
			}
			return false;
		}


		public Rect GetAnimatedRect ()
		{
			if (speakerChar != null && speakerChar.portraitIcon != null)
			{
				if (speakerChar.isLipSyncing)
				{
					return speakerChar.portraitIcon.GetAnimatedRect (speakerChar.GetLipSyncFrame ());
				}
				return speakerChar.portraitIcon.GetAnimatedRect ();
			}
			return new Rect (0,0,0,0);
		}
		
		
		public Color GetColour ()
		{
			if (speakerChar)
			{
				return speakerChar.speechColor;
			}
			
			return Color.white;
		}
		
		
		public string GetLine ()
		{
			if (speechManager.keepTextInBuffer)
			{
				return displayText;
			}
			if (isMessageAlive && isSkippable)
			{
				return displayText;
			}
			return "";
		}
		
		
		public string GetFullLine ()
		{
			if (isMessageAlive && isSkippable)
			{
				return fullText;
			}
			return "";
		}
		
		
		private IEnumerator EndMessage ()
		{
			StopCoroutine ("StartMessage");
			isSkippable = false;

			if (speakerChar)
			{
				speakerChar.StopSpeaking ();
			}

			if (gapIndex >= 0 && gapIndex < speechGaps.Count)
			{
				gapIndex ++;
				StartCoroutine ("StartMessage", allGapText);
			}
			else
			{
				// Wait a short moment for fade-out
				yield return new WaitForSeconds (0.1f);
				isMessageAlive = false;
			}
		}
		
		
		private IEnumerator StartMessage (string message)
		{
			isMessageAlive = true;
			isSkippable = true;
			
			displayText = "";
			message = AdvGame.ConvertTokens (message);
			fullText = message;
			pauseGap = false;

			endTime = Time.time + displayDuration;

			if (speechManager.scrollSubtitles)
			{
				// Start scroll the message
				float amount = 0f;

				while (amount < 1f)
				{
					if (!pauseGap)
					{
						amount += speechManager.textScrollSpeed / 100f / message.Length;
						if (amount > 1f)
						{
							amount = 1f;
						}
					}

					int numChars = (int) (amount * message.Length);
					if (gapIndex > 0)
					{
						numChars += speechGaps[gapIndex-1].characterIndex;
						if (numChars > message.Length)
						{
							numChars = message.Length;
						}
					}
					string newText = message.Substring (0, numChars);

					displayText = newText;
					if (gapIndex >= 0 && speechGaps.Count > gapIndex)
					{
						if (numChars == speechGaps [gapIndex].characterIndex)
						{
							float waitTime = (float) speechGaps [gapIndex].waitTime;
							pauseGap = true;
							if (waitTime >= 0f)
							{
								float pauseEndTime = Time.time + waitTime;
								while (pauseGap && Time.time < pauseEndTime)
								{
									yield return new WaitForFixedUpdate ();
								}
							}
							else
							{
								while (pauseGap)
								{
									yield return new WaitForFixedUpdate ();
								}
							}
							pauseGap = false;
							gapIndex ++;
							amount = 0f;
						}
					}

					if (displayText != newText && speechManager.textScrollCLip && !currentLineHasAudio)
					{
						if (defaultAudioSource)
						{
							if (!defaultAudioSource.isPlaying)
							{
								defaultAudioSource.PlayOneShot (speechManager.textScrollCLip);
							}
						}
						else
						{
							Debug.LogWarning ("Cannot play text scroll audio clip as no 'Default' sound prefab has been defined in the Scene Manager");
						}
					}
			
					yield return new WaitForFixedUpdate ();
				}
				displayText = message;
			}
			else
			{
				if (gapIndex >= 0 && speechGaps.Count >= gapIndex)
				{
					if (gapIndex == speechGaps.Count)
					{
						displayText = message;
						foreach (SpeechGap gap in speechGaps)
						{
							endTime -= gap.waitTime;
						}
					}
					else
					{
						float waitTime = (float) speechGaps[gapIndex].waitTime;
						displayText = message.Substring (0, speechGaps[gapIndex].characterIndex);

						if (waitTime >= 0)
						{
							endTime = Time.time + waitTime;
						}
						else
						{
							pauseGap = true;
						}
					}
				}
				else
				{
					displayText = message;
				}

				yield return new WaitForSeconds (message.Length / speechManager.textScrollSpeed);
			}
			
			if (endTime == Time.time)
			{
				endTime += 2f;
			}
		}
		
		
		public float StartDialog (AC.Char _speakerChar, string message, int lineNumber, string language, bool _isBackground)
		{
			isMessageAlive = false;
			currentLineHasAudio = false;
			isBackground = _isBackground;

			if (_speakerChar)
			{
				speakerChar = _speakerChar;
				speakerChar.isTalking = true;

				speakerName = _speakerChar.name;
				if (_speakerChar.GetComponent <Player>())
				{
					speakerName = "Player";
				}
				
				if (_speakerChar.portraitIcon != null)
				{
					_speakerChar.portraitIcon.Reset ();
				}
				
				if (_speakerChar.GetComponent <Hotspot>())
				{
					if (_speakerChar.GetComponent <Hotspot>().hotspotName != "")
					{
						speakerName = _speakerChar.GetComponent <Hotspot>().hotspotName;
					}
				}

				if (speechManager.lipSyncMode == LipSyncMode.Off)
				{
					speakerChar.isLipSyncing = false;
				}
				else if (speechManager.lipSyncMode == LipSyncMode.FromSpeechText || speechManager.lipSyncMode == LipSyncMode.ReadPamelaFile || speechManager.lipSyncMode == LipSyncMode.ReadSapiFile)
				{
					speakerChar.StartLipSync (GenerateLipSyncShapes (speechManager.lipSyncMode, lineNumber, speakerName, language, message));
				}
			}
			else
			{
				if (speakerChar)
				{
					speakerChar.isTalking = false;
				}
				speakerChar = null;			
				speakerName = "Narrator";
			}
			
			// Play sound and time displayDuration to it
			if (lineNumber > -1 && speakerName != "" && speechManager.searchAudioFiles)
			{
				string filename = "Speech/";
				if (language != "" && speechManager.translateAudio)
				{
					// Not in original language
					filename += language + "/";
				}
				filename += speakerName + lineNumber;
				
				foundAudio = false;
				AudioClip clipObj = Resources.Load(filename) as AudioClip;
				if (clipObj)
				{
					AudioSource audioSource = null;
					currentLineHasAudio = true;

					if (_speakerChar != null)
					{
						if (speechManager.lipSyncMode == LipSyncMode.FaceFX)
						{
							FaceFXIntegration.Play (speakerChar, speakerName + lineNumber, clipObj);
						}

						if (_speakerChar.GetComponent <AudioSource>())
						{
							_speakerChar.GetComponent <AudioSource>().volume = options.optionsData.speechVolume;
							audioSource = _speakerChar.GetComponent <AudioSource>();
						}
						else
						{
							Debug.LogWarning (_speakerChar.name + " has no audio source component!");
						}
					}
					else if (KickStarter.player && KickStarter.player.GetComponent <AudioSource>())
					{
						KickStarter.player.GetComponent <AudioSource>().volume = options.optionsData.speechVolume;
						audioSource = KickStarter.player.GetComponent <AudioSource>();
					}
					else if (defaultAudioSource != null)
					{
						audioSource = defaultAudioSource;
					}
					
					if (audioSource != null)
					{
						audioSource.clip = clipObj;
						audioSource.loop = false;
						audioSource.Play();
						
						foundAudio = true;
					}
					
					displayDuration = clipObj.length;
				}
				else
				{
					displayDuration = speechManager.screenTimeFactor * (float) message.Length;
					if (displayDuration < 0.5f)
					{
						displayDuration = 0.5f;
					}
					
					Debug.Log ("Cannot find audio file: " + filename);
				}
			}
			else
			{
				displayDuration = speechManager.screenTimeFactor * (float) message.Length;
				if (displayDuration < 0.5f)
				{
					displayDuration = 0.5f;
				}
			}

			message = DetermineGaps (message);
			if (speechGaps.Count > 0)
			{
				gapIndex = 0;
				allGapText = message;

				foreach (SpeechGap gap in speechGaps)
				{
					displayDuration += (float) gap.waitTime;
				}
			}
			else
			{
				gapIndex = -1;
			}

			StopCoroutine ("StartMessage");
			StartCoroutine ("StartMessage", message);

			return displayDuration;
		}
		
		
		public void KillDialog (bool forceMenusOff)
		{
			isSkippable = false;
			isMessageAlive = false;
			
			StopCoroutine ("StartMessage");
			StopCoroutine ("EndMessage");

			if (speakerChar)
			{
				speakerChar.StopSpeaking ();
			}

			if (forceMenusOff)
			{
				GameObject.FindWithTag (Tags.persistentEngine).GetComponent <PlayerMenus>().ForceOffSubtitles ();
			}
		}


		private void OnDestroy ()
		{
			playerInput = null;
			speakerChar = null;
			speechManager = null;
			defaultAudioSource = null;
		}


		private string DetermineGaps (string _text)
		{
			speechGaps.Clear ();

			if (_text != null && _text.Contains ("[wait"))
			{
				while (_text.Contains ("[wait:"))
				{
					int startIndex = _text.IndexOf ("[wait:");
					int endIndex = startIndex + 7;
					if (_text.Substring (endIndex + 1, 1) == "]")
					{
						// Double digits
						endIndex += 1;
					}
					string waitTimeText = _text.Substring (startIndex + 6, endIndex - startIndex - 6);
					speechGaps.Add (new SpeechGap (startIndex, IntParseFast (waitTimeText)));
					_text = _text.Substring (0, startIndex) + _text.Substring (endIndex + 1);
				}
				while (_text.Contains ("[wait]"))
				{
					int startIndex = _text.IndexOf ("[wait]");
					speechGaps.Add (new SpeechGap (startIndex, -1));
					_text = _text.Substring (0, startIndex) + _text.Substring (startIndex + 6);
				}
			}

			return _text;
		}


		private int IntParseFast (string value)
		{
			int result = 0;
			for (int i=0; i<value.Length; i++)
			{
				char letter = value[i];
				result = 10 * result + (int) char.GetNumericValue (letter);
			}
			return result;
		}


		public bool HasPausing ()
		{
			if (speechGaps.Count > 0)
			{
				return true;
			}
			return false;
		}


		public List<LipSyncShape> GenerateLipSyncShapes (LipSyncMode _lipSyncMode, int lineNumber, string speakerName, string language, string _message)
		{
			List<LipSyncShape> lipSyncShapes = new List<LipSyncShape>();
			lipSyncShapes.Add (new LipSyncShape (0, 0f, speechManager.lipSyncSpeed));
			TextAsset textFile = null;

			if (lineNumber > -1 && speakerName != "" && speechManager.searchAudioFiles)
			{
				string filename = "Lipsync/";
				if (language != "" && speechManager.translateAudio)
				{
					// Not in original language
					filename += language + "/";
				}
				filename += speakerName + lineNumber;
				textFile = Resources.Load (filename) as TextAsset;
			}

			if (_lipSyncMode == LipSyncMode.ReadPamelaFile && textFile != null)
			{
				string[] pamLines = textFile.text.Split('\n');
				bool foundSpeech = false;
				float fps = 24f;
				foreach (string pamLine in pamLines)
				{
					if (!foundSpeech)
					{
						if (pamLine.Contains ("framespersecond:"))
						{
							string[] pamLineArray = pamLine.Split(':');
							float.TryParse (pamLineArray[1], out fps);
						}
						else if (pamLine.Contains ("[Speech]"))
						{
							foundSpeech = true;
						}
					}
					else if (pamLine.Contains (":"))
					{
						string[] pamLineArray = pamLine.Split(':');

						float timeIndex = 0f;
						float.TryParse (pamLineArray[0], out timeIndex);
						string searchText = pamLineArray[1].ToLower ().Substring (0, pamLineArray[1].Length-1);

						bool found = false;
						foreach (string phoneme in speechManager.phonemes)
						{
							string[] shapesArray = phoneme.ToLower ().Split ("/"[0]);
							if (!found)
							{
								foreach (string shape in shapesArray)
								{
									if (shape == searchText)
									{
										int frame = speechManager.phonemes.IndexOf (phoneme);
										lipSyncShapes.Add (new LipSyncShape (frame, timeIndex, speechManager.lipSyncSpeed, fps));
										found = true;
									}
								}
							}
						}
						if (!found)
						{
							lipSyncShapes.Add (new LipSyncShape (0, timeIndex, speechManager.lipSyncSpeed, fps));
						}
					}
				}
			}
			else if (_lipSyncMode == LipSyncMode.ReadSapiFile && textFile != null)
			{
				string[] sapiLines = textFile.text.Split('\n');
				foreach (string sapiLine in sapiLines)
				{
					if (sapiLine.StartsWith ("phn "))
					{
						string[] sapiLineArray = sapiLine.Split(' ');
						
						float timeIndex = 0f;
						float.TryParse (sapiLineArray[1], out timeIndex);
						string searchText = sapiLineArray[4].ToLower ().Substring (0, sapiLineArray[4].Length-1);
						bool found = false;
						foreach (string phoneme in speechManager.phonemes)
						{
							string[] shapesArray = phoneme.ToLower ().Split ("/"[0]);
							if (!found)
							{
								foreach (string shape in shapesArray)
								{
									if (shape == searchText)
									{
										int frame = speechManager.phonemes.IndexOf (phoneme);
										lipSyncShapes.Add (new LipSyncShape (frame, timeIndex, speechManager.lipSyncSpeed, 60f));

										found = true;
									}
								}
							}
						}
						if (!found)
						{
							lipSyncShapes.Add (new LipSyncShape (0, timeIndex, speechManager.lipSyncSpeed, 60f));
						}
					}
				}
			}
			else if (_lipSyncMode == LipSyncMode.FromSpeechText)
			{
				for (int i=0; i<_message.Length; i++)
				{
					int maxSearch = Mathf.Min (5, _message.Length - i);
					for (int n=maxSearch; n>0; n--)
					{
						string searchText = _message.Substring (i, n);
						searchText = searchText.ToLower ();

						foreach (string phoneme in speechManager.phonemes)
						{
							string[] shapesArray = phoneme.ToLower ().Split ("/"[0]);
							foreach (string shape in shapesArray)
							{
								if (shape == searchText)
								{
									int frame = speechManager.phonemes.IndexOf (phoneme);
									lipSyncShapes.Add (new LipSyncShape (frame, (float) i, speechManager.lipSyncSpeed));
									i += n;
									n = Mathf.Min (5, _message.Length - i);
									break;
								}
							}
						}

					}
					lipSyncShapes.Add (new LipSyncShape (0, (float) i, speechManager.lipSyncSpeed));
				}
			}

			if (lipSyncShapes.Count > 1)
			{
				lipSyncShapes.Sort (delegate (LipSyncShape a, LipSyncShape b) {return a.timeIndex.CompareTo (b.timeIndex);});
			}

			return lipSyncShapes;
		}

	}


	public struct SpeechGap
	{

		public int characterIndex;
		public int waitTime;
	
		public SpeechGap (int _characterIndex, int _waitTime)
		{
			characterIndex = _characterIndex;
			waitTime = _waitTime;
		}

	}


	public struct LipSyncShape
	{

		public int frame;
		public float timeIndex;


		public LipSyncShape (int _frame, float _timeIndex, float speed, float fps)
		{
			// Pamela / Sapi
			frame = _frame;
			timeIndex = (_timeIndex / 15f / speed / fps) + Time.time;
		}
		
		
		public LipSyncShape (int _frame, float _timeIndex, float speed)
		{
			// Automatic
			frame = _frame;
			timeIndex = (_timeIndex / 15f / speed) + Time.time;
		}

	}

}