using UnityEngine;
using System.Collections;

public class SavedColor : MonoBehaviour {

	public Color currentColor;
	public Material savedMat;
	new MeshRenderer renderer;

	void Start()
	{
		MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer> ();
		if( renderers[0] )
			renderer = renderers[0];

		savedMat = new Material (renderer.material);

		currentColor = new Color (0, 0, 0, 0);
		savedMat.color = currentColor;
	}

	void Update()
	{
		renderer.material.color = currentColor;
	}

	void UpdateSavedColor(Color c)
	{
		currentColor = c;
	}
}
