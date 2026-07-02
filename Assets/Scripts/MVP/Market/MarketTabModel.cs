using System;
using System.Collections.Generic;
using Domain.MVP.Tab;

namespace Domain.MVP.Market
{
    /// <summary>
    /// Model for the Market tab. Manages shop listings, sellable items, and favorites.
    /// </summary>
    public class MarketTabModel : TabModel
    {
        /// <summary>Fired when shop listings refresh.</summary>
        public event Action OnShopListChanged;

        /// <summary>Fired when sellable items refresh.</summary>
        public event Action OnSellableItemsChanged;

        /// <summary>Fired when a purchase is made.</summary>
        public event Action<string> OnItemPurchased;

        /// <summary>Fired when an item is sold.</summary>
        public event Action<string> OnItemSold;

        /// <summary>Fired when a favorite is toggled.</summary>
        public event Action<string, bool> OnFavoriteToggled;

        /// <summary>Current player currency / gold amount.</summary>
        public int CurrentGold { get; private set; } = 0;

        /// <summary>Get all shop listings. Returns (itemId, name, price, iconPath, vendorName).</summary>
        public List<(string id, string name, int price, string iconPath, string vendorName, bool isFavorited)> GetShopListings()
        {
            // TODO: Query market/shop service for available listings
            return new List<(string, string, int, string, string, bool)>();
        }

        /// <summary>Get player items available for selling. Returns (itemId, name, sellPrice, iconPath, count).</summary>
        public List<(string id, string name, int sellPrice, string iconPath, int count)> GetSellableItems()
        {
            // TODO: Query inventory service for sellable items
            return new List<(string, string, int, string, int)>();
        }

        /// <summary>Get the list of favorited item IDs.</summary>
        public List<string> GetFavoriteItemIds()
        {
            // TODO: Query favorites storage
            return new List<string>();
        }

        /// <summary>Get the favorited item details.</summary>
        public List<(string id, string name, int price, string iconPath, string vendorName, bool isFavorited)> GetFavoriteItems()
        {
            // TODO: Merge favorites list with shop data
            return new List<(string, string, int, string, string, bool)>();
        }

        /// <summary>Check if an item is favorited.</summary>
        public bool IsFavorited(string itemId)
        {
            return GetFavoriteItemIds().IndexOf(itemId) >= 0;
        }

        /// <summary>Toggle an item's favorite status.</summary>
        public void ToggleFavorite(string itemId)
        {
            var favorites = GetFavoriteItemIds();
            var index = favorites.IndexOf(itemId);
            if (index >= 0)
            {
                favorites.RemoveAt(index);
                OnFavoriteToggled?.Invoke(itemId, false);
            }
            else
            {
                favorites.Add(itemId);
                OnFavoriteToggled?.Invoke(itemId, true);
            }
        }

        /// <summary>Purchase an item from a shop listing. Returns true if successful.</summary>
        public bool PurchaseItem(string itemId, int price)
        {
            if (CurrentGold < price) return false;

            CurrentGold -= price;
            OnItemPurchased?.Invoke(itemId);
            return true;
        }

        /// <summary>Sell an item to a shop. Returns the gold gained.</summary>
        public int SellItem(string itemId, int sellPrice)
        {
            CurrentGold += sellPrice;
            OnItemSold?.Invoke(itemId);
            return sellPrice;
        }

        /// <summary>Get the formatted gold string (e.g. '1,234 G').</summary>
        public string GetGoldString()
        {
            return CurrentGold.ToString("N0") + " G";
        }

        public MarketTabModel()
            : base("market", "Market", new[] { "buy", "sell", "favorites" })
        {
        }

        public override void LoadFromService()
        {
            // TODO: Load current gold, shop listings, and favorites from services
            // CurrentGold = CurrencyService.CurrentGold;
            // CurrencyService.OnGoldChanged += OnGoldChanged;
        }

        private void OnGoldChanged(int newGold)
        {
            CurrentGold = newGold;
            OnShopListChanged?.Invoke();
        }
    }
}
