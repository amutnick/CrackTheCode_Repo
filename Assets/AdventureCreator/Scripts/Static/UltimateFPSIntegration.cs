/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"UltimateFPSIntegration.cs"
 * 
 *	This script contains a number of static functions for use
 *	in integrating AC with the Ultimate FPS asset
 *
 *	To allow for UFPS integration, the 'UltimateFPSIsPresent'
 *	preprocessor must be defined.  This can be done from
 *	Edit -> Project Settings -> Player, and entering
 *	'UltimateFPSIsPresent' into the Scripting Define Symbols text box
 *	for your game's build platform.
 *
 *	NOTE: AC is designed for UFPS v1.4.8 - to integrate with an earlier
 *	version, follow the instructions below on line 87
 * 
 */


using UnityEngine;
using System.Collections;


namespace AC
{
	
	public class UltimateFPSIntegration : ScriptableObject
	{
		
		public static bool IsDefinePresent ()
		{
			#if UltimateFPSIsPresent
			return true;
			#else
			return false;
			#endif
		}


		public static void _Update (GameState gameState)
		{
			if (gameState == GameState.Normal)
			{
				UltimateFPSIntegration.SetMovementState (true);
				UltimateFPSIntegration.SetCameraState (true);
			}
			else
			{
				UltimateFPSIntegration.SetMovementState (false);
				UltimateFPSIntegration.SetCameraState (false);
			}
		}


		public static void SetMovementState (bool state)
		{
			#if UltimateFPSIsPresent
			if (KickStarter.player && KickStarter.player.GetComponent <vp_FPController>())
			{
				vp_FPController Controller = KickStarter.player.GetComponent <vp_FPController>();

				if (state && GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>().isUpLocked)
				{
					Controller.SetState ("Freeze", true);
				}
				else
				{
					Controller.SetState ("Freeze", !state);
				}

				if (!state)
				{
					Controller.Stop();
				}
			}
			else
			{
				Debug.LogWarning ("Cannot find 'vp_FPController' script on Player.");
			}
			#else
			Debug.LogWarning ("The 'UltimateFPSIsPresent' preprocessor is not defined - check your Player Settings.");
			#endif
		}


		public static void SetCameraState (bool state)
		{
			#if UltimateFPSIsPresent

			/* IMPORTANT NOTE
			 * 
			 * For Ultimate FPS integration to work, one of lines 102 and 103 must be commented out.
			 * Each line provides support for a different version of UFPS.
			 * 
			 * To support v1.4.8 or later, uncomment line 102, and comment line 103
			 * To support v1.4.7c or earlier, comment line 102, and uncomment line 103
			 * 
			 * To comment a line, simply insert to slashes ("//") before it.
			 * 
			 * The need for this change is that v1.4.8 requires that the vp_FPInput script stores the "Freeze" input state,
			 * and no longer the vp_FPCamera script.  As such, your UFPS Player prefab will also need to be updated.
			 * 
			 */

			vp_FPInput _camera = KickStarter.player.GetComponent <vp_FPInput>(); // For UFPS v1.4.8 or later
			//vp_FPCamera _camera = GameObject.FindWithTag (Tags.firstPersonCamera).GetComponent <vp_FPCamera>(); // For UFPS v1.4.7c or earlier

			if (state && GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>().IsCursorLocked () == false)
			{
				_camera.SetState ("Freeze", true);
			}
			else
			{
				_camera.SetState ("Freeze", !state);
			}
			#else
			Debug.Log ("The 'UltimateFPSIsPresent' preprocessor is not defined - check your Player Settings.");
			#endif
		}


		public static void Teleport (Vector3 position)
		{
			#if UltimateFPSIsPresent
			KickStarter.player.GetComponent <vp_FPController>().SetPosition (position);
			#endif
		}


		public static void SetRotation (Vector3 rotation)
		{
			#if UltimateFPSIsPresent
			GameObject.FindWithTag (Tags.firstPersonCamera).GetComponent <vp_FPCamera>().SetRotation (new Vector2 (rotation.x, rotation.y), true, true);
			#endif
		}


		public static void SetPitch (float pitch)
		{
			#if UltimateFPSIsPresent
			Transform camTransform = GameObject.FindWithTag (Tags.firstPersonCamera).transform;
			camTransform.GetComponent <vp_FPCamera>().Angle = new Vector2 (camTransform.eulerAngles.x, pitch);
			#endif
		}


		public static void SetTilt (float tilt)
		{
			#if UltimateFPSIsPresent
			Transform camTransform = GameObject.FindWithTag (Tags.firstPersonCamera).transform;
			camTransform.GetComponent <vp_FPCamera>().Angle = new Vector2 (tilt, camTransform.eulerAngles.y);
			#endif
		}

	}
	
}