using LogiPack.Gameplay;
using LogiPack.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace LogiPack.UI
{
    /// <summary>
    /// Estación de botones interactivos para alternar los materiales de embalaje
    /// (Relleno de Espuma y Film Térmico) sobre la caja activa en la mesa de armado.
    /// </summary>
    public class PackagingStationUI : MonoBehaviour
    {
        [Header("Botones de Embalaje")]
        public Button FoamButton;
        public Image FoamButtonImage;
        public Text FoamButtonText;

        public Button ThermalFilmButton;
        public Image ThermalFilmButtonImage;
        public Text ThermalFilmButtonText;

        [Header("Colores de Estado")]
        public Color InactiveColor = new Color(0.22f, 0.26f, 0.32f, 0.95f);
        public Color FoamActiveColor = new Color(0.95f, 0.82f, 0.20f, 1f);
        public Color ThermalActiveColor = new Color(0.20f, 0.75f, 0.95f, 1f);

        private PackageBox _subscribedBox;

        private void Start()
        {
            if (FoamButton != null)
            {
                FoamButton.onClick.AddListener(OnClickFoam);
            }

            if (ThermalFilmButton != null)
            {
                ThermalFilmButton.onClick.AddListener(OnClickThermalFilm);
            }

            if (BoxSpawner.Instance != null)
            {
                BoxSpawner.Instance.OnBoxSpawned += HandleBoxSpawned;
                if (BoxSpawner.Instance.CurrentActiveBox != null)
                {
                    HandleBoxSpawned(BoxSpawner.Instance.CurrentActiveBox);
                }
            }

            RefreshUI();
        }

        private void OnDestroy()
        {
            if (FoamButton != null) FoamButton.onClick.RemoveListener(OnClickFoam);
            if (ThermalFilmButton != null) ThermalFilmButton.onClick.RemoveListener(OnClickThermalFilm);

            if (BoxSpawner.Instance != null)
            {
                BoxSpawner.Instance.OnBoxSpawned -= HandleBoxSpawned;
            }

            if (_subscribedBox != null)
            {
                _subscribedBox.OnPackagingChanged -= RefreshUI;
                _subscribedBox.OnStateChanged -= HandleBoxStateChanged;
            }
        }

        private void HandleBoxSpawned(PackageBox newBox)
        {
            if (_subscribedBox != null)
            {
                _subscribedBox.OnPackagingChanged -= RefreshUI;
                _subscribedBox.OnStateChanged -= HandleBoxStateChanged;
            }

            _subscribedBox = newBox;

            if (_subscribedBox != null)
            {
                _subscribedBox.OnPackagingChanged += RefreshUI;
                _subscribedBox.OnStateChanged += HandleBoxStateChanged;
            }

            RefreshUI();
        }

        private void HandleBoxStateChanged(BoxState state)
        {
            RefreshUI();
        }

        public void OnClickFoam()
        {
            PackageBox box = GetActiveAssemblyBox();
            if (box != null)
            {
                box.ToggleFoam();
                RefreshUI();
            }
        }

        public void OnClickThermalFilm()
        {
            PackageBox box = GetActiveAssemblyBox();
            if (box != null)
            {
                box.ToggleThermalFilm();
                RefreshUI();
            }
        }

        private PackageBox GetActiveAssemblyBox()
        {
            if (BoxSpawner.Instance != null)
            {
                PackageBox current = BoxSpawner.Instance.CurrentActiveBox;
                if (current != null && current.State == BoxState.Assembling)
                {
                    return current;
                }
            }
            return null;
        }

        public void RefreshUI()
        {
            PackageBox box = GetActiveAssemblyBox();

            bool hasFoam = box != null && box.HasFoamPadding;
            bool hasThermal = box != null && box.HasThermalFilm;

            if (FoamButtonImage != null)
            {
                FoamButtonImage.color = hasFoam ? FoamActiveColor : InactiveColor;
            }

            if (FoamButtonText != null)
            {
                FoamButtonText.color = hasFoam ? Color.black : Color.white;
            }

            if (ThermalFilmButtonImage != null)
            {
                ThermalFilmButtonImage.color = hasThermal ? ThermalActiveColor : InactiveColor;
            }

            if (ThermalFilmButtonText != null)
            {
                ThermalFilmButtonText.color = hasThermal ? Color.black : Color.white;
            }
        }
    }
}
