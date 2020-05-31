using UnityEngine;
using UnityEngine.UI;

namespace ClientPacman
{
	public class ConnectWindow : MonoBehaviour
	{
		[SerializeField] private GameController gameController;
		[SerializeField] private InputField inputField;
		[SerializeField] private ToggleGroup toggleGroup;
		[SerializeField] private Button connectButton;

		private string color;

		private void Start()
		{
			connectButton.onClick.AddListener(ConnectClient);
			toggleGroup.SetAllTogglesOff();
		}

		private void ConnectClient()
		{
			if (inputField.text == string.Empty)
			{
				Debug.Log("Введите имя");
				return;
			}

			if (!toggleGroup.AnyTogglesOn())
			{
				Debug.Log("Выберите цвет");
				return;
			}

			gameController.ConnectToServer(inputField.text, color);
			gameObject.SetActive(false);
		}

		public void SetColor(string color)
		{
			this.color = color;
		}

		private void OnDestroy()
		{
			connectButton.onClick.RemoveAllListeners();
		}
	}
}
