using UnityEngine;

public class RabbitSpeedDebugger : MonoBehaviour
{
	[SerializeField]
	private string targetTag = "Player";

	[SerializeField]
	private Vector2 screenPosition = new Vector2(12f, 12f);

	[SerializeField]
	private int fontSize = 16;

	[SerializeField]
	private Color textColor = Color.white;

	[SerializeField]
	private Color backgroundColor = new Color(0f, 0f, 0f, 0.5f);

	private PlayerController_Rabbit rabbitController;
	private CharacterController characterController;
	private GUIStyle labelStyle;

	private void Awake()
	{
		// Try to find the Player_Rabbit controller in scene
		rabbitController = FindObjectOfType<PlayerController_Rabbit>();
		if (rabbitController != null)
		{
			characterController = rabbitController.GetComponent<CharacterController>();
		}
	}

	private void Start()
	{
		// Prepare GUI style once
		if (labelStyle == null)
		{
			labelStyle = new GUIStyle(GUI.skin.label);
			labelStyle.fontSize = fontSize;
			labelStyle.normal.textColor = textColor;
		}
	}

	private void EnsureGuiStyle()
	{
		if (labelStyle == null)
		{
			labelStyle = new GUIStyle(GUI.skin.label);
			labelStyle.fontSize = fontSize;
			labelStyle.normal.textColor = textColor;
		}
	}

	private void OnGUI()
	{
		if (rabbitController == null || characterController == null)
		{
			return;
		}

		EnsureGuiStyle();

		Vector3 velocity = characterController.velocity;
		float horizontalSpeed = new Vector3(velocity.x, 0f, velocity.z).magnitude;
		float verticalSpeed = velocity.y;
		float totalSpeed = velocity.magnitude;

		string text =
			$"Rabbit Speed\n" +
			$"Horizontal: {horizontalSpeed:F2} m/s\n" +
			$"Vertical: {verticalSpeed:F2} m/s\n" +
			$"Total: {totalSpeed:F2} m/s";

		Vector2 size = labelStyle.CalcSize(new GUIContent(text));
		Rect rect = new Rect(screenPosition.x, screenPosition.y, size.x + 12f, size.y + 12f);

		// Background
		Color prevColor = GUI.color;
		GUI.color = backgroundColor;
		GUI.Box(rect, GUIContent.none);
		GUI.color = prevColor;

		// Text
		Rect textRect = new Rect(rect.x + 6f, rect.y + 6f, rect.width - 12f, rect.height - 12f);
		GUI.Label(textRect, text, labelStyle);
	}
}


