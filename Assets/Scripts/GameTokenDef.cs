using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class GameTokenDef
{
	[SerializeField] public int tokenColor;          // this relates to the index of colours defined in GameBoard.tokenColours
	[SerializeField] public Vector2Int startPos;	// relative to the GameBoard grid

	// ----------------------------------------------------------------------------------------------------------------
}
