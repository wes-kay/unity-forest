using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Domain.MVP.Tab;

namespace Domain.MVP.Market
{
    /// <summary>
    /// Presenter for the Market tab. Handles shop browsing, selling, favorites, and currency display.
    /// </summary>
    public class MarketTabPresenter : TabPresenter<MarketTabModel, MarketTabView>
    {
        [Inject] private MarketTabModel _marketModel;
        [Inject] private MarketTabView _marketView;

        /// <summary>Currently selected item ID (null = none).</summary>
        private string _selectedItemId;

        public override void OnTabActivated()
        {
            if (!_marketModel.IsLoaded)
            {
                _marketModel.LoadFromService();
            }

            // Sync item grid for the active subtab
            RefreshItemGrid();

            // Sync gold display
            _marketView.SetGoldText(_marketModel.GetGoldString());

            // Show the correct subtab
            _marketView.ShowSubtab(_marketModel.ActiveSubtab);
        }

        public override void OnTabDeactivated()
        {
            // No cleanup needed — data stays cached
        }

        public override void OnSubtabChanged(string subtabId)
        {
            RefreshItemGrid();
            _marketView.ClearDetailPanel();
        }

        /// <summary>Handle item slot click from the view.</summary>
        public void OnItemClick(string itemId)
        {
            _selectedItemId = itemId;

            // Get item data from the current subtab's source
            string itemName = itemId;
            int itemPrice = 0;
            string itemDesc = string.Empty;
            string vendorName = string.Empty;
            bool isFavorited = _marketModel.IsFavorited(itemId);

            if (_marketModel.ActiveSubtab == "buy" || _marketModel.ActiveSubtab == "favorites")
            {
                var listings = _marketModel.GetShopListings();
                foreach (var item in listings)
                {
                    if (item.id == itemId)
                    {
                        itemName = item.name;
                        itemPrice = item.price;
                        vendorName = item.vendorName;
                        break;
                    }
                }
            }
            else if (_marketModel.ActiveSubtab == "sell")
            {
                var sellable = _marketModel.GetSellableItems();
                foreach (var item in sellable)
                {
                    if (item.id == itemId)
                    {
                        itemName = item.name;
                        itemPrice = item.sellPrice;
                        itemDesc = "Sell this item for " + item.sellPrice.ToString("N0") + " G";
                        break;
                    }
                }
            }

            _marketView.UpdateDetailPanel(itemName, itemPrice, itemDesc, vendorName);
            _marketView.SetFavoriteIcon(isFavorited);

            // Enable/disable buy button based on gold
            if (itemPrice > 0)
            {
                _marketView.SetBuyButtonEnabled(_marketModel.CurrentGold >= itemPrice, "Insufficient Gold");
            }
        }

        /// <summary>Handle buy button press.</summary>
        public void OnBuyPressed()
        {
            if (_selectedItemId == null) return;

            var listings = _marketModel.GetShopListings();
            foreach (var item in listings)
            {
                if (item.id == _selectedItemId)
                {
                    if (_marketModel.PurchaseItem(item.id, item.price))
                    {
                        _marketView.SetGoldText(_marketModel.GetGoldString());
                        RefreshItemGrid();
                    }
                    return;
                }
            }
        }

        /// <summary>Handle sell button press.</summary>
        public void OnSellPressed()
        {
            if (_selectedItemId == null) return;

            var sellable = _marketModel.GetSellableItems();
            foreach (var item in sellable)
            {
                if (item.id == _selectedItemId)
                {
                    _marketModel.SellItem(item.id, item.sellPrice);
                    _marketView.SetGoldText(_marketModel.GetGoldString());
                    RefreshItemGrid();
                    _marketView.ClearDetailPanel();
                    _selectedItemId = null;
                    return;
                }
            }
        }

        /// <summary>Handle favorite button press.</summary>
        public void OnFavoritePressed(string itemId)
        {
            _marketModel.ToggleFavorite(itemId);
            // Refresh if on favorites subtab to update heart icons
            if (_marketModel.ActiveSubtab == "favorites")
            {
                RefreshItemGrid();
            }
        }

        /// <summary>Handle vendor selection.</summary>
        public void OnVendorSelected(string vendorId)
        {
            // TODO: Load vendor's shop listings
            // RefreshItemGrid();
            // _marketView.SetVendorName(vendorName);
        }

        /// <summary>Handle search input submission.</summary>
        public void OnSearchSubmitted(string query)
        {
            // TODO: Filter items by search query
            // var filtered = FilterByQuery(query);
            // _marketView.RefreshItemGrid(filtered);
        }

        // ==================== Helpers ====================

        private void RefreshItemGrid()
        {
            List<(string id, string name, int price, string iconPath, string vendorName, bool isFavorited)> items;

            if (_marketModel.ActiveSubtab == "sell")
            {
                // Convert sellable items to the unified format
                var sellable = _marketModel.GetSellableItems();
                items = new List<(string, string, int, string, string, bool)>();
                foreach (var item in sellable)
                {
                    items.Add((item.id, item.name, item.sellPrice, item.iconPath, "Shop", false));
                }
            }
            else if (_marketModel.ActiveSubtab == "favorites")
            {
                items = _marketModel.GetFavoriteItems();
            }
            else
            {
                // Default: buy/shop subtab — model already includes isFavorited
                items = _marketModel.GetShopListings();
            }

            if (_marketModel.ActiveSubtab == "sell")
            {
                // Use the sellable grid for the sell subtab
                var sellable = _marketModel.GetSellableItems();
                _marketView.RefreshSellableGrid(sellable);
            }
            else
            {
                _marketView.RefreshItemGrid(items);
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            // Subscribe to view events
            _marketView.OnItemClick += OnItemClick;
            _marketView.OnBuyPressed += OnBuyPressed;
            _marketView.OnSellPressed += OnSellPressed;
            _marketView.OnFavoritePressed += OnFavoritePressed;
            _marketView.OnVendorSelected += OnVendorSelected;
            _marketView.OnSearchSubmitted += OnSearchSubmitted;

            // Subscribe to model events
            _marketModel.OnShopListChanged += OnModelShopListChanged;
            _marketModel.OnSellableItemsChanged += OnModelSellableItemsChanged;
            _marketModel.OnItemPurchased += OnModelItemPurchased;
            _marketModel.OnItemSold += OnModelItemSold;
        }

        private void OnModelShopListChanged()
        {
            if (_marketModel.ActiveSubtab == "buy" || _marketModel.ActiveSubtab == "favorites")
            {
                RefreshItemGrid();
            }
            _marketView.SetGoldText(_marketModel.GetGoldString());
        }

        private void OnModelSellableItemsChanged()
        {
            if (_marketModel.ActiveSubtab == "sell")
            {
                RefreshItemGrid();
            }
            _marketView.SetGoldText(_marketModel.GetGoldString());
        }

        private void OnModelItemPurchased(string itemId)
        {
            _marketView.SetGoldText(_marketModel.GetGoldString());
            RefreshItemGrid();
        }

        private void OnModelItemSold(string itemId)
        {
            _marketView.SetGoldText(_marketModel.GetGoldString());
            RefreshItemGrid();
        }

        public override void Destroy()
        {
            _marketView.OnItemClick -= OnItemClick;
            _marketView.OnBuyPressed -= OnBuyPressed;
            _marketView.OnSellPressed -= OnSellPressed;
            _marketView.OnFavoritePressed -= OnFavoritePressed;
            _marketView.OnVendorSelected -= OnVendorSelected;
            _marketView.OnSearchSubmitted -= OnSearchSubmitted;

            _marketModel.OnShopListChanged -= OnModelShopListChanged;
            _marketModel.OnSellableItemsChanged -= OnModelSellableItemsChanged;
            _marketModel.OnItemPurchased -= OnModelItemPurchased;
            _marketModel.OnItemSold -= OnModelItemSold;

            base.Destroy();
        }
    }
}
