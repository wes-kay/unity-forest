using System;
using System.Collections.Generic;
using Domain.MVP.Tab;

namespace Domain.MVP.Settlement
{
    /// <summary>Represents a settlement building with its current state.</summary>
    public struct BuildingInfo
    {
        public string Id;
        public string Name;
        public int Level;
        public int MaxLevel;
        public string IconPath;
        public string Description;
        public bool IsCompleted;
        public bool IsUpgradable => Level < MaxLevel;

        public BuildingInfo(string id, string name, int level, int maxLevel, string iconPath, string description)
        {
            Id = id;
            Name = name;
            Level = level;
            MaxLevel = maxLevel;
            IconPath = iconPath;
            Description = description;
            IsCompleted = level >= maxLevel;
        }
    }

    /// <summary>Represents a settlement resource (gold, food, materials, etc.).</summary>
    public struct ResourceInfo
    {
        public string Name;
        public int Current;
        public int Max;
        public int Production; // per tick

        public ResourceInfo(string name, int current, int max, int production = 0)
        {
            Name = name;
            Current = current;
            Max = max;
            Production = production;
        }

        /// <summary>Percentage of capacity filled (0-1).</summary>
        public float CapacityPercent => Max > 0 ? (float)Current / Max : 0f;
    }

    /// <summary>Represents a news/event item in the settlement feed.</summary>
    public struct NewsItem
    {
        public string Id;
        public string Title;
        public string Description;
        public DateTime Timestamp;
        public string IconPath;
        public bool IsRead;

        public NewsItem(string id, string title, string description, DateTime timestamp, string iconPath = "", bool isRead = false)
        {
            Id = id;
            Title = title;
            Description = description;
            Timestamp = timestamp;
            IconPath = iconPath;
            IsRead = isRead;
        }
    }

    /// <summary>Represents an active construction project.</summary>
    public struct ProjectInfo
    {
        public string Id;
        public string Name;
        public string Description;
        public int Progress;
        public int Total;
        public int Priority;
        public string AssignedBuilder;

        public ProjectInfo(string id, string name, string description, int progress, int total, int priority = 0, string assignedBuilder = "")
        {
            Id = id;
            Name = name;
            Description = description;
            Progress = progress;
            Total = total;
            Priority = priority;
            AssignedBuilder = assignedBuilder;
        }

        /// <summary>Completion percentage (0-1).</summary>
        public float CompletionPercent => Total > 0 ? (float)Progress / Total : 0f;
    }

    /// <summary>Represents a faction and its relationship level with the settlement.</summary>
    public struct FactionInfo
    {
        public string Id;
        public string Name;
        public int Reputation; // -100 to 100
        public string Status; // "Hostile", "Neutral", "Friendly", "Allied"
        public string IconPath;

        public FactionInfo(string id, string name, int reputation, string status, string iconPath = "")
        {
            Id = id;
            Name = name;
            Reputation = reputation;
            Status = status;
            IconPath = iconPath;
        }
    }

    /// <summary>Represents a visitor currently in the settlement.</summary>
    public struct VisitorInfo
    {
        public string Id;
        public string Name;
        public string Role;
        public string Description;
        public int Morale; // 0-100
        public bool IsTrusted;
        public string IconPath;

        public VisitorInfo(string id, string name, string role, string description, int morale, bool isTrusted, string iconPath = "")
        {
            Id = id;
            Name = name;
            Role = role;
            Description = description;
            Morale = morale;
            IsTrusted = isTrusted;
            IconPath = iconPath;
        }
    }

    /// <summary>
    /// Model for the Settlement tab. Manages settlement state including buildings,
    /// resources, projects, factions, and visitors. Uses mock data for MVP.
    /// </summary>
    public class SettlementTabModel : TabModel
    {
        /// <summary>All settlement buildings.</summary>
        public BuildingInfo[] Buildings { get; private set; }

        /// <summary>All settlement resources (gold, food, materials, etc.).</summary>
        public ResourceInfo[] Resources { get; private set; }

        /// <summary>Active construction projects.</summary>
        public ProjectInfo[] ActiveProjects { get; private set; }

        /// <summary>Factions and their relationship levels.</summary>
        public FactionInfo[] Factions { get; private set; }

        /// <summary>Current visitors in the settlement.</summary>
        public VisitorInfo[] Visitors { get; private set; }

        /// <summary>Recent news/events feed.</summary>
        public NewsItem[] NewsItems { get; private set; }

        /// <summary>Total population count.</summary>
        public int Population { get; private set; }

        /// <summary>Maximum population capacity.</summary>
        public int MaxPopulation { get; private set; }

        /// <summary>Settlement reputation level (1-5).</summary>
        public int ReputationLevel { get; private set; }

        /// <summary>Fired when a building upgrade is requested.</summary>
        public event Action<string> OnUpgradeRequested;

        /// <summary>Fired when resources change.</summary>
        public event Action OnResourcesChanged;

        /// <summary>Fired when population changes.</summary>
        public event Action<int> OnPopulationChanged;

        /// <summary>Fired when a project is completed.</summary>
        public event Action<string> OnProjectCompleted;

        /// <summary>Fired when a news item is marked as read.</summary>
        public event Action<string> OnNewsItemRead;

        public SettlementTabModel()
            : base("settlement", "Settlement", new[] { "overview", "buildings", "resources", "projects", "factions", "visitors" })
        {
        }

        /// <summary>Load mock data for MVP.</summary>
        public override void LoadFromService()
        {
            LoadMockData();
        }

        /// <summary>Populate with placeholder data.</summary>
        public void LoadMockData()
        {
            Buildings = new[]
            {
                new BuildingInfo("townhall", "Town Hall", 2, 5, "icons/townhall", "The administrative center of the settlement. Increases governance capacity."),
                new BuildingInfo("inn", "The Wounded Bear Inn", 1, 3, "icons/inn", "A tavern that boosts morale and provides healing services."),
                new BuildingInfo("blacksmith", "Blacksmith Forge", 1, 4, "icons/blacksmith", "Crafts and repairs weapons and armor for the community."),
                new BuildingInfo("market", "Market Stall", 1, 3, "icons/market", "Facilitates trade with visiting merchants."),
                new BuildingInfo("watchtower", "Watchtower", 2, 2, "icons/watchtower", "Provides early warning of approaching threats."),
                new BuildingInfo("infirmary", "Field Infirmary", 1, 3, "icons/infirmary", "Treats the wounded and manages disease outbreaks."),
            };

            Resources = new[]
            {
                new ResourceInfo("Gold", 1250, 5000, 50),
                new ResourceInfo("Food", 800, 1000, 25),
                new ResourceInfo("Materials", 450, 800, 15),
                new ResourceInfo("Munitions", 200, 500, 10),
                new ResourceInfo("Medicine", 120, 300, 5),
            };

            ActiveProjects = new[]
            {
                new ProjectInfo("project1", "Expand Town Hall", "Upgrade the Town Hall to increase governance capacity.", 3, 10, 1, "Miller"),
                new ProjectInfo("project2", "Build Barracks", "Construct a new barracks to house additional settlers.", 0, 15, 2, ""),
                new ProjectInfo("project3", "Repair Watchtower", "Reinforce the watchtower with stone reinforcements.", 7, 8, 0, "Hank"),
            };

            Factions = new[]
            {
                new FactionInfo("order", "The Order", -30, "Hostile", "icons/order"),
                new FactionInfo("hunters", "Hunters' Guild", 45, "Friendly", "icons/hunters"),
                new FactionInfo("church", "The Church", 20, "Neutral", "icons/church"),
                new FactionInfo("merchants", "Merchant Consortium", 60, "Friendly", "icons/merchants"),
                new FactionInfo("outcasts", "The Outcasts", -50, "Hostile", "icons/outcasts"),
            };

            Visitors = new[]
            {
                new VisitorInfo("visitor1", "Dr. Aris Thorne", "Physician", "A renowned doctor seeking shelter from the Plutonian cult.", 70, true, "icons/doctor"),
                new VisitorInfo("visitor2", "Miller", "Carpenter", "A skilled carpenter looking for work and a place to settle.", 50, false, "icons/carpenter"),
                new VisitorInfo("visitor3", "Hank", "Retired Soldier", "A gruff veteran with combat experience, willing to train guards.", 80, true, "icons/soldier"),
            };

            NewsItems = new[]
            {
                new NewsItem("news_settler", "New Settler Arrived", "A wanderer named Miller has joined the settlement as a carpenter.", DateTime.Now.AddHours(-2), "icons/settler"),
                new NewsItem("news_food", "Food Shortage Warning", "Food stores are running low. Consider assigning workers to foraging.", DateTime.Now.AddHours(-6), "icons/warning"),
                new NewsItem("news_scout", "Order Scout Spotted", "A scout from the Order was seen near the northern ridge.", DateTime.Now.AddHours(-12), "icons/danger"),
                new NewsItem("news_injury", "Injury Report", "Two settlers were injured in a logging accident. Assign to infirmary.", DateTime.Now.AddHours(-18), "icons/injury"),
                new NewsItem("news_merchant", "Merchant Caravan", "A merchant caravan has arrived at the market with supplies.", DateTime.Now.AddDays(-1), "icons/trade"),
            };

            Population = 12;
            MaxPopulation = 50;
            ReputationLevel = 2;
        }

        /// <summary>Request an upgrade for a building.</summary>
        public void RequestUpgrade(string buildingId)
        {
            OnUpgradeRequested?.Invoke(buildingId);
        }

        /// <summary>Mark a news item as read.</summary>
        public void MarkNewsRead(string itemId)
        {
            for (int i = 0; i < NewsItems.Length; i++)
            {
                if (NewsItems[i].Id == itemId)
                {
                    // Note: NewsItem is a struct, so we can't mutate in place.
                    // The presenter handles this by refreshing the view.
                    OnNewsItemRead?.Invoke(itemId);
                    return;
                }
            }
        }

        /// <summary>Update a resource value.</summary>
        public void UpdateResource(string resourceName, int newValue)
        {
            for (int i = 0; i < Resources.Length; i++)
            {
                if (Resources[i].Name == resourceName)
                {
                    // ResourceInfo is a struct; notify presenter to refresh.
                    Resources = (ResourceInfo[])Resources.Clone();
                    Resources[i] = new ResourceInfo(resourceName, newValue, Resources[i].Max, Resources[i].Production);
                    OnResourcesChanged?.Invoke();
                    return;
                }
            }
        }

        /// <summary>Update population count.</summary>
        public void UpdatePopulation(int newPopulation)
        {
            Population = newPopulation;
            OnPopulationChanged?.Invoke(newPopulation);
        }

        /// <summary>Get building by ID.</summary>
        public BuildingInfo GetBuilding(string buildingId)
        {
            foreach (var b in Buildings)
            {
                if (b.Id == buildingId) return b;
            }
            return default;
        }

        /// <summary>Get upgrade cost for a building.</summary>
        public (int gold, int materials) GetUpgradeCost(BuildingInfo building)
        {
            int costPerLevel = 200;
            int matCostPerLevel = 100;
            return (costPerLevel * building.Level, matCostPerLevel * building.Level);
        }

        /// <summary>Check if resources are sufficient for an upgrade.</summary>
        public bool CanAffordUpgrade(BuildingInfo building, int goldCost, int matCost)
        {
            foreach (var r in Resources)
            {
                if (r.Name == "Gold" && r.Current < goldCost) return false;
                if (r.Name == "Materials" && r.Current < matCost) return false;
            }
            return true;
        }

        /// <summary>Perform an upgrade (deducts resources).</summary>
        public bool PerformUpgrade(BuildingInfo building, int goldCost, int matCost)
        {
            if (!CanAffordUpgrade(building, goldCost, matCost)) return false;

            UpdateResource("Gold", Resources[0].Current - goldCost);
            UpdateResource("Materials", Resources[1].Current - matCost);

            var updated = building;
            updated.Level++;
            if (updated.Level >= updated.MaxLevel)
            {
                updated.IsCompleted = true;
            }

            // Replace in array (struct, so clone needed)
            var newBuildings = (BuildingInfo[])Buildings.Clone();
            for (int i = 0; i < newBuildings.Length; i++)
            {
                if (newBuildings[i].Id == building.Id)
                {
                    newBuildings[i] = updated;
                    break;
                }
            }
            Buildings = newBuildings;

            return true;
        }

        public override void Activate()
        {
            base.Activate();
            if (!IsLoaded)
            {
                LoadFromService();
                IsLoaded = true;
            }
        }
    }
}
