using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameBoard : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
	[SerializeField] private GameObject pinFab = null;
	[SerializeField] private GameObject tokenFab = null;

	[SerializeField] private RectTransform pinsContainer = null;
	[SerializeField] private RectTransform tokensContainer = null;

	[SerializeField] private float minSnapFactor = 1f; // 1 = full token height, 0.5 = half token height
	[SerializeField] private Vector2Int gridSize = Vector2Int.zero;
	[SerializeField] private Color[] tokenColours = null;

	[SerializeField] private GameTokenDef[] startTokens = null;

	// ----------------------------------------------------------------------------------------------------------------

	private GameBoardPin[] grid = null; // note: grid 0x0 is at bottom-left and maxX x maxY at top-right
	private GameToken draggingToken = null;

	// ----------------------------------------------------------------------------------------------------------------

	private void Start()
	{
		CreateGrid();
	}

	private void CreateGrid()
	{
		// calculate pin spacing based on size of board and number of pins
		var space = new Vector2(pinsContainer.rect.width / gridSize.x, pinsContainer.rect.height / gridSize.y);

		// everything is centered around middle of game board so need an offset from that to start placing pins
		var offs = (pinsContainer.rect.size * -0.5f) + (space * 0.5f);

		// define the game board's grid (pins) and place initial tokens
		grid = new GameBoardPin[gridSize.x * gridSize.y];
		var idx = 0;
		for (int x = 0; x < gridSize.x; x++)
		{
			for (int y = 0; y < gridSize.y; y++)
			{
				var gridPos = new Vector2Int(x, y);

				// create and place the pin object
				var pinRt = Instantiate(pinFab, this.pinsContainer).GetComponent<RectTransform>();
				pinRt.anchoredPosition = offs + (space * gridPos);

				// create a token if there is a token in this position at start
				GameTokenDef def = startTokens.FirstOrDefault(t => t.startPos == gridPos);
				GameToken token = null;
				if (def != null)
				{
					if (def.tokenColor >= 0 && def.tokenColor < tokenColours.Length)
					{
						token = Instantiate(tokenFab, tokensContainer).GetComponent<GameToken>();
						token.TokenColor = def.tokenColor;
						token.image.color = tokenColours[def.tokenColor];
						token.Rt.anchoredPosition = pinRt.anchoredPosition;
						token.PrevAnchorPos = token.Rt.anchoredPosition;
					}
					else
					{
						Debug.LogError($"Invalid token colour: {def.tokenColor}. It must be in range 0..{tokenColours.Length-1}");
					}
				}

				// initialize the grid position
				grid[idx] = new GameBoardPin
				{
					pinRt = pinRt,
					token = token,
				};

				idx++;
			}
		}
	}

	private void SnapTokenToPin(GameToken token)
	{
		// calculated from (token_size * minSnapFactor) to determine how close a token must be to pin before it will snap
		float minDistToSnap = minSnapFactor * token.Rt.rect.height;

		// find nearest open pin to the token
		float dist = 0f;
		GameBoardPin pin = null;
		for (int i = 0; i < gridSize.x * gridSize.y; i++)
		{
			var p = grid[i];
			if (p.token == null || p.token == token) // open pin or the pin this token came from
			{
				var d = Vector2.Distance(p.pinRt.localPosition, token.Rt.localPosition);
				if (d <= minDistToSnap && (pin == null || d < dist))
				{
					pin = p;
					dist = d;
				}
			}
		}

		// update token with new position in grid
		if (pin != null)
		{
			// remove token from old position in grid
			var p = grid.FirstOrDefault(g => g.token == token);
			if (p != null)
			{
				p.token = null;
			}
			else
			{
				Debug.LogError("This should not happen. There is a bug somewhere with tokens not being linked to pins/grid positions.");
			}

			// place it in new position
			pin.token = token;
			token.Rt.anchoredPosition = pin.pinRt.anchoredPosition;
			token.PrevAnchorPos = token.Rt.anchoredPosition;
		}
		else
		{
			// snap back to previous position
			token.Rt.anchoredPosition = token.PrevAnchorPos;
		}
	}

	// ----------------------------------------------------------------------------------------------------------------
	#region input interfaces

	void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
	{
		if (eventData.pointerCurrentRaycast.gameObject == null)
		{
			return;
		}

		// check if drag starts over a token
		var token = eventData.pointerCurrentRaycast.gameObject.GetComponent<GameToken>();
		if (token != null)
		{
			eventData.Use();
			draggingToken = token;
		}
	}

	void IEndDragHandler.OnEndDrag(PointerEventData eventData)
	{
		if (draggingToken != null)
		{
			// find nearest open pin and snap token to it
			SnapTokenToPin(draggingToken);

			// done
			eventData.Use();
			draggingToken = null;
		}
	}

	void IDragHandler.OnDrag(PointerEventData eventData)
	{
		if (draggingToken == null || eventData.pointerCurrentRaycast.gameObject == null)
		{
			return;
		}

		eventData.Use();

		// update the position of the token being dragged
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(tokensContainer, eventData.position, null, out Vector2 localPoint))
		{
			draggingToken.Rt.anchoredPosition = localPoint;
		}
	}

	#endregion
	// ----------------------------------------------------------------------------------------------------------------
}
