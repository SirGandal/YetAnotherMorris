using UnityEngine;
using System.Collections;

public class MoveObject : MonoBehaviour
{
	public enum MoveType { Time, Speed }
	public static MoveObject use = null;
	public static bool isMoving = false;

	void Awake()
	{
		if (use)
		{
			Debug.LogWarning("Only one instance of the MoveObject script in a scene is allowed");
			return;
		}
		use = this;
	}

	public IEnumerator TranslateTo(Transform thisTransform, Vector3 endPos, float value, MoveType moveType, Transform endTransform = null)
	{
		yield return Translation(thisTransform, thisTransform.position, endPos, value, moveType);
	}

	public IEnumerator TranslateToTransform(Transform thisTransform, Transform endTransform, float value, MoveType moveType, bool destroy = false)
	{
		yield return TranslationWithReset(thisTransform, endTransform, value, moveType, destroy);
	}

	public IEnumerator Translation(Transform thisTransform, Vector3 endPos, float value, MoveType moveType)
	{
		yield return Translation(thisTransform, thisTransform.position, thisTransform.position + endPos, value, moveType);
	}

	public IEnumerator TranslationWithReset(Transform thisTransform, Transform endTransform, float value, MoveType moveType, bool destroy = false)
	{
		isMoving = true;
		var startPos = thisTransform.position;
		var endPos = endTransform.position;

		float rate = (moveType == MoveType.Time) ? 1.0f / value : 1.0f / Vector3.Distance(startPos, endPos) * value;
		float t = 0.0f;
		while (t < 1.0)
		{
			t += Time.deltaTime * rate;
			thisTransform.position = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0.0f, 1.0f, t));
			yield return null;
		}

		if(destroy && thisTransform.childCount != 0)
		{
			var child = thisTransform.GetChild(0).gameObject;
			GameObject.Destroy(child);
		}else{
			if(thisTransform.childCount > 0){
				thisTransform.GetChild(0).transform.SetParent(endTransform);
			}else{
				thisTransform.SetParent(endTransform);
			}
		}
		thisTransform.position = startPos;
		isMoving = false;
	}

	public IEnumerator Translation(Transform thisTransform, Vector3 startPos, Vector3 endPos, float value, MoveType moveType)
	{
		float rate = (moveType == MoveType.Time) ? 1.0f / value : 1.0f / Vector3.Distance(startPos, endPos) * value;
		float t = 0.0f;
		while (t < 1.0)
		{
			t += Time.deltaTime * rate;
			thisTransform.position = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0.0f, 1.0f, t));
			yield return null;
		}
	}

	public IEnumerator Rotation(Transform thisTransform, Vector3 degrees, float time)
	{
		Quaternion startRotation = thisTransform.rotation;
		Quaternion endRotation = thisTransform.rotation * Quaternion.Euler(degrees);
		float rate = 1.0f / time;
		float t = 0.0f;
		while (t < 1.0f)
		{
			t += Time.deltaTime * rate;
			thisTransform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
			yield return null;
		}
	}
}
