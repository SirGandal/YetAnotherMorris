using UnityEngine;
using System.Collections;

public class ShowPanels : MonoBehaviour {

	public GameObject optionsPanel;							//Store a reference to the Game Object OptionsPanel 
	public GameObject optionsTint;							//Store a reference to the Game Object OptionsTint 
	public GameObject menuPanel;							//Store a reference to the Game Object MenuPanel 
	public Transform startTransf;
	public Transform endTransf;


	//Call this function to activate and display the Options panel during the main menu
	public void ShowOptionsPanel()
	{
		StartCoroutine (MoveObject.use.TranslateTo(optionsPanel.transform, startTransf.position, 0.75f, MoveObject.MoveType.Time));
	}

	//Call this function to deactivate and hide the Options panel during the main menu
	public void HideOptionsPanel()
	{
		StartCoroutine (MoveObject.use.TranslateTo(optionsPanel.transform, endTransf.position, 0.75f, MoveObject.MoveType.Time));
	}

	//Call this function to activate and display the main menu panel during the main menu
	public void ShowMenu()
	{
		menuPanel.SetActive (true);
	}

	//Call this function to deactivate and hide the main menu panel during the main menu
	public void HideMenu()
	{
		menuPanel.SetActive (false);
	}
}
