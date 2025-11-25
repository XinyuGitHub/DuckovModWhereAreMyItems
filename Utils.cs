using ItemStatsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WhereAreMyItems
{
    public static class Utils
    {
        private static readonly Dictionary<int, int> s_cachedStoroedItemIDToNumberDict = new Dictionary<int, int>();

        private static bool s_isPrepared = false;

        public static bool s_IsStorageAvailable
        {
            get
            {
                return s_storageInventory != null;
            }
        }
        private static Item s_characterItem
        {
            get
            {
                return CharacterMainControl.Main?.CharacterItem;
            }
        }

        private static Inventory s_petInventory
        {
            get
            {
                return PetProxy.PetInventory;
            }
        }

        private static Inventory s_storageInventory
        {
            get
            {
                return PlayerStorage.Inventory;
            }
        }
        public static List<string> GetPath(this Item item)
        {
            var path = new List<string>();
            var currentNode = item;
            int counter = 0;
            while (counter++ <= 5)
            {
                if (currentNode.InInventory == s_petInventory)
                {
                    path.Add(ShortMessage.PetBackpack.GetTranslation());
                    break;
                }
                else if (s_IsStorageAvailable && currentNode.InInventory == s_storageInventory)
                {
                    path.Add(ShortMessage.Warehouse.GetTranslation());
                    break;
                }
                else if (currentNode.PluggedIntoSlot?.Master == s_characterItem)
                {
                    path.Add(ShortMessage.Equipment.GetTranslation());
                    break;
                }
                else if (currentNode.InInventory?.AttachedToItem == s_characterItem)
                {
                    path.Add(ShortMessage.Backpack.GetTranslation());
                    break;
                }
                else if (currentNode.InInventory?.AttachedToItem != null)
                {
                    currentNode = currentNode.InInventory.AttachedToItem;
                    path.Add(currentNode.DisplayName);
                }
                else if (currentNode.PluggedIntoSlot?.Master != null)
                {
                    currentNode = currentNode.PluggedIntoSlot.Master;
                    path.Add(currentNode.DisplayName);
                }
                else
                {
                    Debug.LogError($"[WhereAreMyItems] Unexpected Orphan Item");
                    break;
                }
            }
            path.Reverse();
            return path;
        }

        public static int s_GetItemCountInStorage(int itemID)
        {
            if (s_IsStorageAvailable)
                return s_GetItemsInStorageByID(itemID).Select(j => j.StackCount).Sum();
            else
                if (s_cachedStoroedItemIDToNumberDict.TryGetValue(itemID, out var count))
                return count;
            return 0;
        }

        public static int s_GetItemCountOnDuckAndDog(int itemID)
        {
            return s_GetItemsOnDuckAndDogByID(itemID).Select(j => j.StackCount).Sum();
        }

        public static IEnumerable<Item> s_GetItemsInStorageByID(int itemID)
        {
            if (s_IsStorageAvailable)
                return s_getAllItemsInStorage().Where(i => i.TypeID == itemID);
            return Enumerable.Empty<Item>();
        }

        public static IEnumerable<Item> s_GetItemsOnDuckAndDogByID(int itemID)
        {
            return s_getAllItemsOnDuckAndDog().Where(i => i.TypeID == itemID);
        }

        public static void s_Prepare()
        {
            if (s_isPrepared)
                return;

            HarmonyUtils.s_Prepare();
            HarmonyUtils.s_OnPlayerStorageDestroy += s_cacheStoredItemCount;
            s_isPrepared = true;
            Debug.Log($"[WhereAreMyItems] Utils is Prepared");
        }

        private static void s_cacheStoredItemCount()
        {
            if (s_storageInventory == null)
                return;

            s_cachedStoroedItemIDToNumberDict.Clear();
            foreach (var item in s_getAllItemsInStorage())
                if (s_cachedStoroedItemIDToNumberDict.Keys.Contains(item.TypeID))
                    s_cachedStoroedItemIDToNumberDict[item.TypeID] += item.StackCount;
                else
                    s_cachedStoroedItemIDToNumberDict[item.TypeID] = item.StackCount;
            Debug.Log($"[WhereAreMyItems] Cached Dict Refreshed. Count: {s_cachedStoroedItemIDToNumberDict.Count}");
        }

        private static IEnumerable<Item> s_getAllItemsInInventory(Inventory inventory)
        {
            if (inventory == null)
                yield break;

            foreach (var item in inventory.FindAll(i => i != null))
            {
                yield return item;
                foreach (var innerItemOnInnerItem in s_getAllItemsOnItem(item))
                    yield return innerItemOnInnerItem;
            }
        }

        private static IEnumerable<Item> s_getAllItemsInStorage()
        {
            if (s_storageInventory == null)
                yield break;

            foreach (var innerItem in s_getAllItemsInInventory(s_storageInventory))
                yield return innerItem;
        }

        private static IEnumerable<Item> s_getAllItemsOnDuckAndDog()
        {
            foreach (var innerItem in s_getAllItemsOnItem(s_characterItem))
                yield return innerItem;

            foreach (var innerItem in s_getAllItemsInInventory(s_petInventory))
                yield return innerItem;
        }

        private static IEnumerable<Item> s_getAllItemsOnItem(Item item)
        {
            if (item == null)
                yield break;

            foreach (var innerItem in s_getItemsOnItemSlotAndInventory(item))
            {
                yield return innerItem;
                foreach (var innerItemOnInnerItem in s_getAllItemsOnItem(innerItem))
                    yield return innerItemOnInnerItem;
            }
        }

        private static IEnumerable<Item> s_getItemsOnItemSlotAndInventory(Item item)
        {
            if (item == null)
                yield break;

            if (item.Slots != null && item.Slots.Count > 0)
                for (int i = 0; i < item.Slots.Count; i++)
                {
                    var currentSlot = item.Slots[i];
                    if (currentSlot?.Content == null)
                        continue;
                    yield return currentSlot.Content;
                }

            if (item.Inventory?.Content != null)
                foreach (var innerItem in item.Inventory.Content)
                {
                    if (innerItem == null)
                        continue;
                    yield return innerItem;
                }
        }
    }
}