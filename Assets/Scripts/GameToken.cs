using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameToken : MonoBehaviour
{
	[SerializeField] public Image image = null;

	public RectTransform Rt { get; set; }
	public int TokenColor { get; set; }
	public Vector2 PrevAnchorPos { get; set; }

	// ----------------------------------------------------------------------------------------------------------------

	private void Awake()
	{
		Rt = GetComponent<RectTransform>();	
	}

	// ----------------------------------------------------------------------------------------------------------------
}
