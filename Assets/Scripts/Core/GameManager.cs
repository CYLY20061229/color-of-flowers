using UnityEngine;

[RequireComponent(typeof(BranchFactory))]
public class GameManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private bool configureCameraForPrototype = true;

    public static GameManager Instance { get; private set; }
    public InventorySystem Inventory { get; private set; }
    public BasketDisplay BasketDisplay { get; private set; }
    public OrderSystem Orders { get; private set; }
    public BouquetSystem Bouquet { get; private set; }
    public BouquetOrderManager BouquetOrders { get; private set; }
    public ChargeHarvestSystem ChargeHarvest { get; private set; }
    public ChargeHarvestConfig ChargeHarvestConfig { get; private set; }
    public HarvestFlyToBasketSystem HarvestFlyToBasket { get; private set; }
    public CurrencyGateway Currency { get; private set; }
    public FlowerSpeciesMapController FlowerSpeciesMap { get; private set; }
    public Transform WorkspaceRoot { get; private set; }
    public Transform MapRoot { get; private set; }

    private CurrentOrderDetailView currentOrderDetailView;
    private BouquetSubmitView bouquetSubmitView;
    private OrderCarouselView orderCarouselView;
    private OrderMenuButton orderMenuButton;
    private BranchFactory branchFactory;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        branchFactory = GetComponent<BranchFactory>();
        ConfigureCamera();
        EnsureInterfaceRoots();
        EnsureBackground();
        EnsureInventorySystem();
        EnsureBasketDisplay();
        EnsureOrderSystem();
        EnsureBouquetSystem();
        EnsureBouquetOrderSystem();
        EnsureChargeHarvestSystem();
        EnsureHarvestFlyToBasketSystem();
        EnsureCurrencyGateway();
        EnsureFlowerSpeciesMap();
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
        mainCamera.orthographicSize = 6.8f;
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
        if (inventoryView != null)
        {
            inventoryView.gameObject.SetActive(false);
        }
    }

    private void EnsureInterfaceRoots()
    {
        WorkspaceRoot = EnsureRoot("WorkspaceRoot");
        MapRoot = EnsureRoot("FlowerSpeciesMapRoot");
    }

    private void EnsureBasketDisplay()
    {
        BasketDisplay = FindFirstObjectByType<BasketDisplay>();
        if (BasketDisplay == null)
        {
            GameObject basketDisplayObject = new GameObject("BasketDisplay");
            basketDisplayObject.transform.SetParent(WorkspaceRoot, false);
            basketDisplayObject.transform.position = new Vector3(-0.85f, -4.2f, 0f);
            BasketDisplay = basketDisplayObject.AddComponent<BasketDisplay>();
        }
        else
        {
            ParentToWorkspace(BasketDisplay.transform);
        }

        BasketDisplay.Initialize(Inventory);
    }

    private void EnsureOrderSystem()
    {
        Orders = GetComponent<OrderSystem>();
        if (Orders == null)
        {
            Orders = gameObject.AddComponent<OrderSystem>();
        }

        Orders.InitializeDefaultOrders();

        EnsureCurrentOrderDetailView();
        EnsureOrderCarousel();
        EnsureOrderMenuButton();
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
            bouquetDropZoneObject.transform.SetParent(WorkspaceRoot, false);
            bouquetDropZoneObject.transform.position = new Vector3(2.75f, -4.2f, 0f);
            bouquetDropZone = bouquetDropZoneObject.AddComponent<BouquetDropZone>();
        }
        else
        {
            ParentToWorkspace(bouquetDropZone.transform);
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
            bouquetLayoutObject.transform.SetParent(WorkspaceRoot, false);
            bouquetLayoutObject.transform.position = new Vector3(-2.65f, -3.45f, 0f);
            bouquetLayoutView = bouquetLayoutObject.AddComponent<BouquetLayoutView>();
        }
        else
        {
            ParentToWorkspace(bouquetLayoutView.transform);
        }

        bouquetLayoutView.Initialize(BouquetOrders);
        EnsureBouquetSubmitView();
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

        GameObject chargeHarvestUi = GameObject.Find("ChargeHarvestUI");
        if (chargeHarvestUi != null)
        {
            ParentToWorkspace(chargeHarvestUi.transform);
        }
    }

    private void EnsureOrderCarousel()
    {
        if (orderCarouselView == null)
        {
            orderCarouselView = FindFirstObjectByType<OrderCarouselView>();
        }

        if (orderCarouselView == null)
        {
            GameObject carouselObject = new GameObject("OrderCarouselView");
            carouselObject.transform.SetParent(WorkspaceRoot, false);
            carouselObject.transform.position = new Vector3(0f, 0.35f, 0f);
            orderCarouselView = carouselObject.AddComponent<OrderCarouselView>();
        }
        else
        {
            ParentToWorkspace(orderCarouselView.transform);
        }

        orderCarouselView.Initialize(Orders);
    }

    private void EnsureCurrentOrderDetailView()
    {
        if (currentOrderDetailView == null)
        {
            currentOrderDetailView = FindFirstObjectByType<CurrentOrderDetailView>();
        }

        if (currentOrderDetailView == null)
        {
            GameObject detailObject = new GameObject("CurrentOrderDetailView");
            detailObject.transform.SetParent(WorkspaceRoot, false);
            detailObject.transform.position = new Vector3(0f, 4.35f, 0f);
            currentOrderDetailView = detailObject.AddComponent<CurrentOrderDetailView>();
        }
        else
        {
            ParentToWorkspace(currentOrderDetailView.transform);
        }

        currentOrderDetailView.Initialize(Orders);
    }

    private void EnsureOrderMenuButton()
    {
        if (orderMenuButton == null)
        {
            orderMenuButton = FindFirstObjectByType<OrderMenuButton>();
        }

        if (orderMenuButton == null)
        {
            GameObject buttonObject = new GameObject("OrderMenuButton");
            buttonObject.transform.SetParent(WorkspaceRoot, false);
            buttonObject.transform.position = new Vector3(2.95f, 4.25f, 0f);
            orderMenuButton = buttonObject.AddComponent<OrderMenuButton>();
        }
        else
        {
            ParentToWorkspace(orderMenuButton.transform);
        }

        orderMenuButton.Initialize(orderCarouselView);
    }

    private void EnsureBouquetSubmitView()
    {
        if (bouquetSubmitView == null)
        {
            bouquetSubmitView = FindFirstObjectByType<BouquetSubmitView>();
        }

        if (bouquetSubmitView == null)
        {
            GameObject submitObject = new GameObject("BouquetSubmitView");
            submitObject.transform.SetParent(WorkspaceRoot, false);
            submitObject.transform.position = new Vector3(-2.65f, -4.55f, 0f);
            bouquetSubmitView = submitObject.AddComponent<BouquetSubmitView>();
        }
        else
        {
            ParentToWorkspace(bouquetSubmitView.transform);
        }

        bouquetSubmitView.Initialize(BouquetOrders);
    }

    private void EnsureCurrencyGateway()
    {
        Currency = GetComponent<CurrencyGateway>();
        if (Currency == null)
        {
            Currency = gameObject.AddComponent<CurrencyGateway>();
        }

        Currency.Initialize();
    }

    private void EnsureHarvestFlyToBasketSystem()
    {
        HarvestFlyToBasket = GetComponent<HarvestFlyToBasketSystem>();
        if (HarvestFlyToBasket == null)
        {
            HarvestFlyToBasket = gameObject.AddComponent<HarvestFlyToBasketSystem>();
        }
    }

    private void EnsureFlowerSpeciesMap()
    {
        FlowerSpeciesMap = GetComponent<FlowerSpeciesMapController>();
        if (FlowerSpeciesMap == null)
        {
            FlowerSpeciesMap = gameObject.AddComponent<FlowerSpeciesMapController>();
        }

        FlowerSpeciesMap.Initialize(Currency, branchFactory, MapRoot, WorkspaceRoot);
    }

    private Transform EnsureRoot(string rootName)
    {
        Transform existing = transform.Find(rootName);
        if (existing != null)
        {
            return existing;
        }

        GameObject rootObject = new GameObject(rootName);
        rootObject.transform.SetParent(transform, false);
        return rootObject.transform;
    }

    private void ParentToWorkspace(Transform target)
    {
        if (target != null && WorkspaceRoot != null && target.parent != WorkspaceRoot)
        {
            target.SetParent(WorkspaceRoot, true);
        }
    }

}
