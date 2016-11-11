using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;


public class StartOptions : MonoBehaviour {
	
	public Canvas CanvasToEnable;

	[HideInInspector] public bool inMainMenu = true;					//If true, pause button disabled in main menu (Cancel in input manager, default escape key)
	[HideInInspector] public Animator animColorFade; 					//Reference to animator which will fade to and from black when starting game.
	[HideInInspector] public Animator animMenuAlpha;					//Reference to animator that will fade out alpha of MenuPanel canvas group
	 public AnimationClip fadeColorAnimationClip;		//Animation clip fading to color (black default) when changing scenes
	[HideInInspector] public AnimationClip fadeAlphaAnimationClip;		//Animation clip fading out UI elements alpha

	private float fastFadeIn = .01f;									//Very short fade time (10 milliseconds) to start playing music immediately without a click/glitch
	private ShowPanels showPanels;										//Reference to ShowPanels script on UI GameObject, to show and hide panels

	void Awake()
	{
		//Get a reference to ShowPanels attached to UI object
		showPanels = GetComponent<ShowPanels> ();
	}


	public void StartButtonClicked()
	{
        //Pause button now works if escape is pressed since we are no longer in Main menu.
        inMainMenu = false;

        //Set trigger for animator to start animation fading out Menu UI
        animMenuAlpha.SetTrigger ("fade");
        Invoke("HideDelayed", fadeAlphaAnimationClip.length);
        CanvasToEnable.enabled = true;

        Invoke("TriggerGameStartedEvent", fadeAlphaAnimationClip.length + 0.4f);

    }
        
	public void HideDelayed()
	{
		//Hide the main menu UI element after fading out menu for start game in scene
		showPanels.HideMenu();
	}

	private void TriggerGameStartedEvent()
	{
		EventManager.TriggerEvent("GameStarted");
	}
}
