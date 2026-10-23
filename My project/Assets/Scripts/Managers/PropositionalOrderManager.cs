using System;
using System.Collections.Generic;
using LogiPack.Core;
using LogiPack.Gameplay;
using LogiPack.Managers;
using LogiPack.UI;
using UnityEngine;
using UnityEngine.UI;

namespace LogiPack.Managers
{
    public class PropositionalOrderManager : MonoBehaviour
    {
        public static PropositionalOrderManager Instance { get; private set; }

        [Header("Datos de Ítems Disponibles")]
        public List<ItemData> AllItems = new List<ItemData>();

        [Header("Referencias Visuales del Pedido (Post-it)")]
        public GameObject OrderPostitContainer;
        public Text OrderHeaderTitleText;
        public Text OrderFBFFormulaText;
        public Text OrderNaturalLanguageText;

        [Header("Audio")]
        public AudioSource SfxSource;
        public AudioClip SuccessClip;
        public AudioClip FailureClip;

        [Header("Economía")]
        public EconomyManager Economy;

        public Orders.PropositionalOrder CurrentOrder { get; private set; }
        private int _orderCounter = 1;

        public event Action<Orders.PropositionalOrder> OnOrderChanged;

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        private void Start()
        {
            if (AllItems == null || AllItems.Count == 0)
            {
                // Cargar ítems desde Resources si están disponibles
                var loaded = Resources.LoadAll<ItemData>("");
                if (loaded != null && loaded.Length > 0)
                {
                    AllItems.AddRange(loaded);
                }
            }

            SpawnNextOrder();
        }

        public void SpawnNextOrder()
        {
            CurrentOrder = Orders.PropositionalOrder.GenerateRandomOrder(_orderCounter++, AllItems);
            UpdateOrderUI();
            OnOrderChanged?.Invoke(CurrentOrder);
            Debug.Log($"<color=cyan>[PropositionalOrder]</color> Nueva orden #{CurrentOrder.OrderId}: {CurrentOrder.FBFString} ({CurrentOrder.NaturalLanguage})");
        }

        public void UpdateOrderUI()
        {
            if (CurrentOrder == null)
            {
                if (OrderPostitContainer != null) OrderPostitContainer.SetActive(false);
                return;
            }

            if (OrderPostitContainer != null) OrderPostitContainer.SetActive(true);

            if (OrderHeaderTitleText != null)
            {
                OrderHeaderTitleText.text = $"PEDIDO #{CurrentOrder.OrderId}   <color=#0E6251>+${CurrentOrder.Reward}</color>";
            }

            if (OrderFBFFormulaText != null)
            {
                OrderFBFFormulaText.text = CurrentOrder.FBFString;
            }

            if (OrderNaturalLanguageText != null)
            {
                OrderNaturalLanguageText.text = CurrentOrder.NaturalLanguage;
            }
        }

        public bool ValidateAndDispatch(PropositionalBox box)
        {
            if (box == null || box.FormulaNode == null || CurrentOrder == null)
            {
                FloatingFeedbackFX.Show(box != null ? box.transform.position : transform.position, "Falta algo...", new Color(0.9f, 0.29f, 0.23f));
                PlaySound(FailureClip);
                return false;
            }

            bool isMatch = box.FormulaNode.IsEquivalentTo(CurrentOrder.FormulaNode);

            if (isMatch)
            {
                FloatingFeedbackFX.Show(box.transform.position, "Todo en orden!", new Color(0.18f, 0.8f, 0.44f));
                PlaySound(SuccessClip);

                // Acreditar dinero en EconomyManager si existe
                var economy = Economy != null ? Economy : FindAnyObjectByType<EconomyManager>();
                if (economy != null)
                {
                    economy.AddEarnings(CurrentOrder.Reward, 0);
                }

                // Generar siguiente pedido
                SpawnNextOrder();
                return true;
            }
            else
            {
                FloatingFeedbackFX.Show(box.transform.position, "Falta algo...", new Color(0.9f, 0.29f, 0.23f));
                PlaySound(FailureClip);
                return false;
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null && SfxSource != null)
            {
                SfxSource.PlayOneShot(clip);
            }
        }
    }
}
