using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BranchFactory))]
public class GameManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private bool configureCameraForPrototype = true;

    public static GameManager Instance { get; private set; }
    public InventorySystem Inventory { get; private set; }
    public OrderSystem Orders { get; private set; }
    public BouquetSystem Bouquet { get; private set; }
    public BouquetOrderManager BouquetOrders { get; private set; }
    public ChargeHarvestSystem ChargeHarvest { get; private set; }
    public ChargeHarvestConfig ChargeHarvestConfig { get; private set; }

    private Transform orderRoot;
    private readonly List<OrderView> orderViews = new List<OrderView>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ConfigureCamera();
        EnsureBackground();
        EnsureInventorySystem();
        EnsureOrderSystem();
        EnsureBouquetSystem();
        EnsureBouquetOrderSystem();
        EnsureInventorySystem();
        EnsureChargeHarvestSystem();
    }

    private void ConfigureCamera()
    {
        if (!configureCameraForPrototype)
        {
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            return;
        }

        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 5f;
        mainCamera.transform.position = new Vector3(0f, 0f, -10f);
    }

    private void EnsureBackground()
    {
        BackgroundView backgroundView = FindFirstObjectByType<BackgroundView>();
        if (backgroundView == null)
        {
            GameObject backgroundObject = new GameObject("BackgroundView");
            backgroundView = backgroundObject.AddComponent<BackgroundView>();
        }

        backgroundView.Initialize(mainCamera);
    }

    private void EnsureInventorySystem()
    {
        Inventory = GetComponent<InventorySystem>();
        if (Inventory == null)
        {
            Inventory = gameObject.AddComponent<InventorySystem>();
        }

        InventoryView inventoryView = FindFirstObjectByType<InventoryView>();
        if (inventoryView == null)
        {
            GameObject inventoryViewObject = new GameObject("InventoryView");
            inventoryViewObject.transform.position = new Vector3(4.25f, 2.8f, 0f);
            inventoryView = inventoryViewObject.AddComponent<InventoryView>();
        }

        inventoryView.Initialize(Inventory, Bouquet, BouquetOrders);
    }

    private void EnsureOrderSystem()
    {
        Orders = GetComponent<OrderSystem>();
        if (Orders == null)
        {
            Orders = gameObject.AddComponent<OrderSystem>();
        }

        Orders.InitializeDefaultOrders();

        EnsureOrderRoot();
        Orders.OrdersChanged -= RefreshOrderViews;
        Orders.OrdersChanged += RefreshOrderViews;
        RefreshOrderViews();
    }

    private void EnsureBouquetSystem()
    {
        Bouquet = GetComponent<BouquetSystem>();
        if (Bouquet == null)
        {
            Bouquet = gameObject.AddComponent<BouquetSystem>();
        }

        Bouquet.Initialize(Inventory, Orders);

        BouquetDropZone bouquetDropZone = FindFirstObjectByType<BouquetDropZone>();
        if (bouquetDropZone == null)
        {
            GameObject bouquetDropZoneObject = new GameObject("BouquetDropZone");
            bouquetDropZoneObject.transform.position = new Vector3(4.25f, -3.25f, 0f);
            bouquetDropZone = bouquetDropZoneObject.AddComponent<BouquetDropZone>();
        }

        bouquetDropZone.Initialize(Bouquet, Orders);
    }

    private void EnsureBouquetOrderSystem()
    {
        BouquetOrders = GetComponent<BouquetOrderManager>();
        if (BouquetOrders == null)
        {
            BouquetOrders = gameObject.AddComponent<BouquetOrderManager>();
        }

        BouquetOrders.Initialize(Inventory, Orders);

        BouquetLayoutView bouquetLayoutView = FindFirstObjectByType<BouquetLayoutView>();
        if (bouquetLayoutView == null)
        {
            GameObject bouquetLayoutObject = new GameObject("BouquetLayoutView");
            bouquetLayoutObject.transform.position = new Vector3(-4.25f, -2.85f, 0f);
            bouquetLayoutView = bouquetLayoutObject.AddComponent<BouquetLayoutView>();
        }

        bouquetLayoutView.Initialize(BouquetOrders);
    }

    private void EnsureChargeHarvestSystem()
    {
        ChargeHarvestConfig = GetComponent<ChargeHarvestConfig>();
        if (ChargeHarvestConfig == null)
        {
            ChargeHarvestConfig = gameObject.AddComponent<ChargeHarvestConfig>();
        }

        ChargeHarvest = GetComponent<ChargeHarvestSystem>();
        if (ChargeHarvest == null)
        {
            ChargeHarvest = gameObject.AddComponent<ChargeHarvestSystem>();
        }

        ChargeHarvest.Initialize(ChargeHarvestConfig);
    }

    private void EnsureOrderRoot()
    {
        if (orderRoot != null)
        {
            return;
        }

        GameObject orderRootObject = GameObject.Find("OrderRoot");
        if (orderRootObject == null)
        {
            orderRootObject = new GameObject("OrderRoot");
            orderRootObject.transform.position = new Vector3(4.25f, 0f, 0f);
        }

        orderRoot = orderRootObject.transform;
    }

    private void RefreshOrderViews()
    {
        EnsureOrderRoot();
        ClearOrderViews();

        for (int i = 0; i < Orders.ActiveOrders.Count; i++)
        {
            OrderData order = Orders.ActiveOrders[i];
            GameObject orderViewObject = new GameObject($"OrderView_{order.Id}");
            orderViewObject.transform.SetParent(orderRoot, false);
            orderViewObject.transform.localPosition = new Vector3(0f, -i * 1.45f, 0f);

            OrderView orderView = orderViewObject.AddComponent<OrderView>();
            orderView.Initialize(Orders, order);
            orderViews.Add(orderView);
        }
    }

    private void ClearOrderViews()
    {
        for (int i = 0; i < orderViews.Count; i++)
        {
            if (orderViews[i] != null)
            {
                Destroy(orderViews[i].gameObject);
            }
        }

        orderViews.Clear();
    }
}
