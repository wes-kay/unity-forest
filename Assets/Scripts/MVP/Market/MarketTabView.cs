using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Domain.MVP.Tab;

namespace Domain.MVP.Market
{
    /// <summary>
    /// View for the Market tab. Manages shop listings, sellable items, and favorites grids.
    /// </summary>
    public class MarketTabView : TabView
    {
        [Header("Item Grid")]
        [Tooltip("Grid layout for shop/sell/favorite items.")]
        public GridLayoutGroup itemGrid;

        [Tooltip("Prefab for a market item slot.")]
        public GameObject itemSlotPrefab;

        [Header("Gold Display")]
        [Tooltip("Text showing current player gold/currency.")]
        public TextMeshProUGUI goldText;

        [Header("Item Detail Panel")]
        [Tooltip("Container for the item detail/info panel.")]
        public RectTransform detailPanel;

        [Tooltip("Item name in the detail panel.")]
        public TextMeshProUGUI detailNameText;

        [Tooltip("Item price in the detail panel.")]
        public TextMeshProUGUI detailPriceText;

        [Tooltip("Item description in the detail panel.")]
        public TextMeshProUGUI detailDescText;

        [Tooltip("Item icon in the detail panel.")]
        public Image detailIconImage;

        [Tooltip("Buy button in the detail panel.")]
        public Button buyButton;

        [Tooltip("Sell button in the detail panel.")]
        public Button sellButton;

        [Tooltip("Favorite button in the detail panel.")]
        public Button favoriteButton;

        [Header("Vendor Info")]
        [Tooltip("Current vendor/NPC name display.")]
        public TextMeshProUGUI vendorNameText;

        [Header("Search")]
        [Tooltip("Search input field for filtering items.")]
        public TMP_InputField searchInput;

        /// <summary>Fired when an item slot is clicked.</summary>
        public event Action<string> OnItemClick;

        /// <summary>Fired when the buy button is pressed.</summary>
        public event Action OnBuyPressed;

        /// <summary>Fired when the sell button is pressed.</summary>
        public event Action OnSellPressed;

        /// <summary>Fired when the favorite button is pressed.</summary>
        public event Action<string> OnFavoritePressed;

        /// <summary>Fired when a vendor is selected.</summary>
        public event Action<string> OnVendorSelected;

        /// <summary>Fired when the search input is submitted.</summary>
        public event Action<string> OnSearchSubmitted;

        private Dictionary<string, GameObject> _itemSlots;

        public override void Initialize(TabModel model)
        {
            base.Initialize(model);
            _itemSlots = new Dictionary<string, GameObject>();
        }

        /// <summary>Update the gold display text.</summary>
        public void SetGoldText(string goldString)
        {
            if (goldText != null) goldText.text = goldString;
        }

        /// <summary>Update the vendor name display.</summary>
        public void SetVendorName(string name)
        {
            if (vendorNameText != null) vendorNameText.text = name;
        }

        /// <summary>Refresh the item grid for the current subtab.</summary>
        public void RefreshItemGrid(List<(string id, string name, int price, string iconPath, string vendorName, bool isFavorited)> items)
        {
            if (itemGrid == null || itemSlotPrefab == null) return;

            // Clear existing slots
            foreach (var kvp in _itemSlots)
            {
                if (kvp.Value != null) Destroy(kvp.Value);
            }
            _itemSlots.Clear();

            foreach (var item in items)
            {
                var slot = Instantiate(itemSlotPrefab, itemGrid.transform);
                slot.SetActive(true);

                var nameText = slot.GetComponentInChildren<TextMeshProUGUI>();
                if (nameText != null) nameText.text = item.name;

                // Show price if non-zero
                if (item.price > 0)
                {
                    var priceText = slot.transform.Find("PriceText")?.GetComponent<TextMeshProUGUI>();
                    if (priceText != null)
                        priceText.text = item.price.ToString("N0") + " G";
                }

                // Show favorite indicator
                var favIcon = slot.transform.Find("FavoriteIcon");
                if (favIcon != null)
                    favIcon.gameObject.SetActive(item.isFavorited);

                var btn = slot.GetComponent<Button>();
                if (btn != null)
                {
                    var itemId = item.id;
                    btn.onClick.AddListener(() => OnItemClick?.Invoke(itemId));
                }

                _itemSlots[item.id] = slot;
            }
        }

        /// <summary>Refresh the item grid for sellable items (with sell price).</summary>
        public void RefreshSellableGrid(List<(string id, string name, int sellPrice, string iconPath, int count)> items)
        {
            if (itemGrid == null || itemSlotPrefab == null) return;

            foreach (var kvp in _itemSlots)
            {
                if (kvp.Value != null) Destroy(kvp.Value);
            }
            _itemSlots.Clear();

            foreach (var item in items)
            {
                var slot = Instantiate(itemSlotPrefab, itemGrid.transform);
                slot.SetActive(true);

                var nameText = slot.GetComponentInChildren<TextMeshProUGUI>();
                if (nameText != null) nameText.text = item.name;

                // Show sell price
                var priceText = slot.transform.Find("PriceText")?.GetComponent<TextMeshProUGUI>();
                if (priceText != null)
                    priceText.text = item.sellPrice.ToString("N0") + " G";

                // Show quantity
                var countText = slot.transform.Find("CountText")?.GetComponent<TextMeshProUGUI>();
                if (countText != null)
                    countText.text = "x" + item.count;

                var btn = slot.GetComponent<Button>();
                if (btn != null)
                {
                    var itemId = item.id;
                    btn.onClick.AddListener(() => OnItemClick?.Invoke(itemId));
                }

                _itemSlots[item.id] = slot;
            }
        }

        /// <summary>Update the detail panel with the selected item's data.</summary>
        public void UpdateDetailPanel(string itemName, int itemPrice, string itemDesc, string vendorName)
        {
            if (detailPanel != null) detailPanel.gameObject.SetActive(true);
            if (detailNameText != null) detailNameText.text = itemName;
            if (detailPriceText != null) detailPriceText.text = itemPrice.ToString("N0") + " G";
            if (detailDescText != null) detailDescText.text = itemDesc;
            if (vendorNameText != null) vendorNameText.text = vendorName;

            // Show/hide action buttons based on subtab
            if (buyButton != null) buyButton.gameObject.SetActive(ActiveSubtab == "buy");
            if (sellButton != null) sellButton.gameObject.SetActive(ActiveSubtab == "sell");
        }

        /// <summary>Update the favorite button's heart icon.</summary>
        public void SetFavoriteIcon(bool isFavorited)
        {
            if (favoriteButton != null)
            {
                var heartIcon = favoriteButton.transform.Find("HeartIcon");
                if (heartIcon != null)
                    heartIcon.gameObject.SetActive(isFavorited);
            }
        }

        /// <summary>Clear the detail panel.</summary>
        public void ClearDetailPanel()
        {
            if (detailPanel != null) detailPanel.gameObject.SetActive(false);
            if (detailNameText != null) detailNameText.text = string.Empty;
            if (detailPriceText != null) detailPriceText.text = string.Empty;
            if (detailDescText != null) detailDescText.text = string.Empty;
            if (detailIconImage != null) detailIconImage.sprite = null;
        }

        /// <summary>Enable or disable the buy button and show tooltip if insufficient gold.</summary>
        public void SetBuyButtonEnabled(bool enabled, string insufficientGoldText)
        {
            if (buyButton != null)
            {
                buyButton.interactable = enabled;
                // TODO: Set tooltip text when disabled due to insufficient gold
            }
        }

        public override void ShowSubtab(string subtabId)
        {
            base.ShowSubtab(subtabId);
            // Hide detail panel when switching subtabs
            if (detailPanel != null)
                detailPanel.gameObject.SetActive(false);
        }
    }
}
