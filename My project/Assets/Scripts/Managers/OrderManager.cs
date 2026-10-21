using System;
using System.Collections.Generic;
using LogiPack.Core;
using LogiPack.Logic;
using LogiPack.Orders;
using UnityEngine;

namespace LogiPack.Managers
{
    /// <summary>
    /// Gestiona la llegada, actualización de tiempo, caducidad
    /// y deducción monetaria de los pedidos activos (estilo Overcooked).
    /// </summary>
    public class OrderManager : MonoBehaviour
    {
        [Header("Configuración de Pedidos")]
        public int MaxConcurrentOrders = 4;
        public float OrderSpawnInterval = 12f;
        public List<ItemData> AvailableItems = new List<ItemData>();

        [Header("Referencias a otros Managers")]
        public EnvironmentManager Environment;
        public EconomyManager Economy;
        public PackagingZoneManager PackagingZone;

        public readonly List<Order> ActiveOrders = new List<Order>();

        public event Action<Order> OnOrderExpired;

        private float _spawnTimer;

        private void Start()
        {
            if (PackagingZone != null)
            {
                PackagingZone.OnOrderDispatched += HandleOrderDispatched;
            }

            // Generar el primer pedido inmediatamente
            SpawnNewOrder();
        }

        private void Update()
        {
            if (Environment != null && Environment.IsDayOver) return;

            // 1. Spawner de pedidos
            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= OrderSpawnInterval && ActiveOrders.Count < MaxConcurrentOrders)
            {
                _spawnTimer = 0f;
                SpawnNewOrder();
            }

            // 2. Actualizar temporizadores individuales de pedidos
            for (int i = ActiveOrders.Count - 1; i >= 0; i--)
            {
                Order order = ActiveOrders[i];
                order.RemainingTime -= Time.deltaTime;

                // Si caduca: penalización monetaria (usando CustomPenalty para órdenes especiales)
                if (order.IsExpired)
                {
                    int penalty = (order.CustomPenalty > 0) ? order.CustomPenalty : order.MonetaryReward;
                    Debug.Log($"[OrderManager] ¡PEDIDO CADUCADO! Orden '{order.OrderId}' expiró. Restando penalización -${penalty}");
                    if (Economy != null)
                    {
                        Economy.DeductPenalty(penalty);
                    }
                    ActiveOrders.RemoveAt(i);
                    OnOrderExpired?.Invoke(order);
                }
            }

            // 3. Evaluar despacho automático en la zona de embalaje
            if (PackagingZone != null && Environment != null)
            {
                PackagingZone.EvaluateAutoDispatch(Environment.CurrentState, ActiveOrders);
            }
        }

        private void SpawnNewOrder()
        {
            EnvironmentState envState = Environment != null ? Environment.CurrentState : default;
            Order newOrder = OrderGenerator.GenerateRandomOrder(envState, AvailableItems);
            ActiveOrders.Add(newOrder);
            Debug.Log($"[OrderManager] Nuevo pedido entrante: {newOrder.OrderId} ({newOrder.Condition.NaturalLanguageText}) - Recompensa: ${newOrder.MonetaryReward}");
        }

        private void HandleOrderDispatched(Gameplay.PackageBox box, Order order, int tip)
        {
            ActiveOrders.Remove(order);
            if (Economy != null)
            {
                Economy.AddEarnings(order.MonetaryReward, tip);
            }
        }

        /// <summary>
        /// Limpia los pedidos activos al reiniciar o comenzar una nueva jornada laboral.
        /// </summary>
        public void ClearAllOrders()
        {
            ActiveOrders.Clear();
            _spawnTimer = 0f;
            SpawnNewOrder();
            Debug.Log("[OrderManager] Pedidos reiniciados para la nueva jornada.");
        }
    }
}
