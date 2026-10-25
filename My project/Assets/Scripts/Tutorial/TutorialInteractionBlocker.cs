using System;
using LogiPack.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LogiPack.Tutorial
{
    /// <summary>
    /// Bloqueador físico integral (Decisiones 7 y 8).
    /// Actúa como receptor de clics en Canvas (raycastTarget = true) y a la vez
    /// conmuta IsInteractionsBlocked en la mesa de armado para neutralizar
    /// la física 2D de arrastres de ítems mientras Mochi esté hablando.
    /// Al hacer clic sobre el bloqueador, notifica al DialogueUI para avanzar el texto.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class TutorialInteractionBlocker : MonoBehaviour, IPointerClickHandler
    {
        public static TutorialInteractionBlocker Instance { get; private set; }

        public event Action OnBlockerClicked;

        private Image _image;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            _image = GetComponent<Image>();
            if (_image != null)
            {
                _image.color = new Color(0f, 0f, 0f, 0.001f); // Prácticamente invisible pero captura raycasts
                _image.raycastTarget = false;
            }
        }

        public void SetBlocked(bool blocked)
        {
            if (_image != null)
            {
                _image.raycastTarget = blocked;
                gameObject.SetActive(blocked);
            }

            if (AssemblyWorkbenchManager.Instance != null)
            {
                AssemblyWorkbenchManager.Instance.IsInteractionsBlocked = blocked;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnBlockerClicked?.Invoke();
        }
    }
}
