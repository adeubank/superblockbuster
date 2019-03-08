#if UNITY_PURCHASING

using System.Collections.Generic;

namespace UnityEngine.Purchasing
{
    /// <summary>
    ///     Automatically initializes Unity IAP with the products defined in the IAP Catalog (if enabled in the UI).
    ///     Manages IAPButtons and IAPListeners.
    /// </summary>
    public class CodelessIAPStoreListener : IStoreListener
    {
        private static CodelessIAPStoreListener instance;
        private static bool unityPurchasingInitialized;

        // Allows outside sources to know whether the full initialization has taken place.
        public static bool initializationComplete;
        private readonly List<IAPButton> activeButtons = new List<IAPButton>();
        private readonly List<IAPListener> activeListeners = new List<IAPListener>();
        protected ProductCatalog catalog;

        protected IStoreController controller;
        protected IExtensionProvider extensions;

        private CodelessIAPStoreListener()
        {
            catalog = ProductCatalog.LoadDefaultCatalog();
        }

        public static CodelessIAPStoreListener Instance
        {
            get
            {
                if (instance == null) CreateCodelessIAPStoreListenerInstance();
                return instance;
            }
        }

        public IStoreController StoreController => controller;

        public IExtensionProvider ExtensionProvider => extensions;

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            initializationComplete = true;
            this.controller = controller;
            this.extensions = extensions;

            foreach (var button in activeButtons) button.UpdateText();
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError(string.Format("Purchasing failed to initialize. Reason: {0}", error.ToString()));
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
        {
            PurchaseProcessingResult result;

            // if any receiver consumed this purchase we return the status
            var consumePurchase = false;
            var resultProcessed = false;

            foreach (var button in activeButtons)
                if (button.productId == e.purchasedProduct.definition.id)
                {
                    result = button.ProcessPurchase(e);

                    if (result == PurchaseProcessingResult.Complete) consumePurchase = true;

                    resultProcessed = true;
                }

            foreach (var listener in activeListeners)
            {
                result = listener.ProcessPurchase(e);

                if (result == PurchaseProcessingResult.Complete) consumePurchase = true;

                resultProcessed = true;
            }

            // we expect at least one receiver to get this message
            if (!resultProcessed)
                Debug.LogError("Purchase not correctly processed for product \"" +
                               e.purchasedProduct.definition.id +
                               "\". Add an active IAPButton to process this purchase, or add an IAPListener to receive any unhandled purchase events.");

            return consumePurchase ? PurchaseProcessingResult.Complete : PurchaseProcessingResult.Pending;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
        {
            var resultProcessed = false;

            foreach (var button in activeButtons)
                if (button.productId == product.definition.id)
                {
                    button.OnPurchaseFailed(product, reason);

                    resultProcessed = true;
                }

            foreach (var listener in activeListeners)
            {
                listener.OnPurchaseFailed(product, reason);

                resultProcessed = true;
            }

            // we expect at least one receiver to get this message
            if (!resultProcessed)
                Debug.LogError("Failed purchase not correctly handled for product \"" + product.definition.id +
                               "\". Add an active IAPButton to handle this failure, or add an IAPListener to receive any unhandled purchase failures.");
        }

        [RuntimeInitializeOnLoadMethod]
        private static void InitializeCodelessPurchasingOnLoad()
        {
            var catalog = ProductCatalog.LoadDefaultCatalog();
            if (catalog.enableCodelessAutoInitialization && !catalog.IsEmpty() && instance == null)
                CreateCodelessIAPStoreListenerInstance();
        }

        private static void InitializePurchasing()
        {
            var module = StandardPurchasingModule.Instance();
            module.useFakeStoreUIMode = FakeStoreUIMode.StandardUser;

            var builder = ConfigurationBuilder.Instance(module);

            IAPConfigurationHelper.PopulateConfigurationBuilder(ref builder, instance.catalog);

            UnityPurchasing.Initialize(instance, builder);

            unityPurchasingInitialized = true;
        }

        /// <summary>
        ///     Creates the static instance of CodelessIAPStoreListener and initializes purchasing
        /// </summary>
        private static void CreateCodelessIAPStoreListenerInstance()
        {
            instance = new CodelessIAPStoreListener();
            if (!unityPurchasingInitialized)
            {
                Debug.Log("Initializing UnityPurchasing via Codeless IAP");
                InitializePurchasing();
            }
        }

        public bool HasProductInCatalog(string productID)
        {
            foreach (var product in catalog.allProducts)
                if (product.id == productID)
                    return true;
            return false;
        }

        public Product GetProduct(string productID)
        {
            if (controller != null && controller.products != null && !string.IsNullOrEmpty(productID))
                return controller.products.WithID(productID);
            Debug.LogError("CodelessIAPStoreListener attempted to get unknown product " + productID);
            return null;
        }

        public void AddButton(IAPButton button)
        {
            activeButtons.Add(button);
        }

        public void RemoveButton(IAPButton button)
        {
            activeButtons.Remove(button);
        }

        public void AddListener(IAPListener listener)
        {
            activeListeners.Add(listener);
        }

        public void RemoveListener(IAPListener listener)
        {
            activeListeners.Remove(listener);
        }

        public void InitiatePurchase(string productID)
        {
            if (controller == null)
            {
                Debug.LogError("Purchase failed because Purchasing was not initialized correctly");

                foreach (var button in activeButtons)
                    if (button.productId == productID)
                        button.OnPurchaseFailed(null, PurchaseFailureReason.PurchasingUnavailable);
                return;
            }

            controller.InitiatePurchase(productID);
        }
    }
}

#endif