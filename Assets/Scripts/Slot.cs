using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IDropHandler, IPointerClickHandler
{

	public delegate void TileClicked (GameObject clickedTile);

	public static event TileClicked OnTileClicked;

	public GameObject item {
		get {
			if (transform.childCount > 0) {
				return transform.GetChild (0).gameObject;
			}
			return null;
		}
	}

	#region IDropHandler implementation

	public void OnDrop (PointerEventData eventData)
	{
		if (!item) {
			DragHandler.itemBeingDragged.transform.SetParent (transform);
		}
	}

	#endregion


	public void OnPointerClick (PointerEventData eventData)
	{
		if (OnTileClicked != null) {
			OnTileClicked (transform.gameObject);
		}
	}
}
