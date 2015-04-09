using UnityEngine;
using System.Collections;

public class ColorMixer : MonoBehaviour {

	public Color currentColor;
	public Color lastClickedColor;

	public Material mixerMat;

	public bool sendColorToWall = false;

	public int itemsMixed = 0;
	bool lastClickedSaved;

	bool lastClickOnMixer;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		mixerMat.color = currentColor;

		if( sendColorToWall )
		{
			SendMessageUpwards("UpdateColor", currentColor);
			sendColorToWall = false;
		}

		if (Input.GetMouseButtonDown (0)) {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit)) {
				GameObject hitObj = hit.collider.gameObject;
				ColorButton colorButton = hitObj.GetComponent<ColorButton>();
				lastClickedSaved = false;

				lastClickOnMixer = false;
				if( colorButton )
				{
					lastClickedColor = colorButton.buttonColor;
				}
				else if( hitObj.name == "MixColor" )
				{
					lastClickedColor = currentColor;
					lastClickOnMixer = true;
				}
				else if( hitObj.name == "SavedColor" )
				{
					lastClickedColor = hitObj.GetComponent<SavedColor>().currentColor;
					lastClickedSaved = true;
				}
			}
		}

		if (Input.GetMouseButtonUp (0)) {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit)) {
				GameObject hitObj = hit.collider.gameObject;
				if( hitObj.name == "MixColor" )
				{
					if( itemsMixed == 0 )
					{
						currentColor = lastClickedColor;
						currentColor.a = 1;
						itemsMixed++;
					}
					else
					{
						currentColor.r = (currentColor.r*itemsMixed + lastClickedColor.r) / (itemsMixed+1);
						currentColor.g = (currentColor.g*itemsMixed + lastClickedColor.g) / (itemsMixed+1);
						currentColor.b = (currentColor.b*itemsMixed + lastClickedColor.b) / (itemsMixed+1);
						currentColor.a = (currentColor.a*itemsMixed + lastClickedColor.a) / (itemsMixed+1);

						itemsMixed++;
					}
					SendMessageUpwards("UpdateColor", currentColor);
				}
				else if( hitObj.name == "SavedColor" )
				{
					hitObj.SendMessageUpwards("UpdateSavedColor", currentColor);
				}
				else if( lastClickOnMixer ) // Clear Pallete
				{
					currentColor = new Color(0,0,0,0);
					itemsMixed = 0;
					
					SendMessageUpwards("UpdateColor", currentColor);
					lastClickOnMixer = false;
				}
			}
		}
	}
}
