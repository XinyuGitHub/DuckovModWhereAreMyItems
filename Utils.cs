using ItemStatsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WhereAreMyItems
{
    public static class Utils
    {
        public class ItemPathAndCount
        {
            public string m_path = string.Empty;
            public int m_count = 0;
        }

        private static readonly List<ItemPathAndCount> s_pathsAndCountsForStoredItem = new List<ItemPathAndCount>();
        private static readonly List<ItemPathAndCount> s_pathsAndCountsForHeldItem = new List<ItemPathAndCount>();

        private static readonly List<string> s_nodeNames = new List<string>();
        private static readonly Dictionary<int, int> s_cachedStoroedItemIDToNumberDict = new Dictionary<int, int>();
        private static bool s_isPrepared = false;
        private static string s_cachedLeftInformation = string.Empty;
        private static string s_cachedRightInformation = string.Empty;

        private static Item s_characterItem
        {
            get
            {
                return CharacterMainControl.Main?.CharacterItem;
            }
        }

        private static Inventory s_storageInventory
        {
            get
            {
                return PlayerStorage.Inventory;
            }
        }

        private static Inventory s_petInventory
        {
            get
            {
                return PetProxy.PetInventory;
            }
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
            foreach (var item in s_getAllStoredItem())
                if (s_cachedStoroedItemIDToNumberDict.Keys.Contains(item.TypeID))
                    s_cachedStoroedItemIDToNumberDict[item.TypeID] += item.StackCount;
                else
                    s_cachedStoroedItemIDToNumberDict[item.TypeID] = item.StackCount;
            Debug.Log($"[WhereAreMyItems] Cached Dict Refreshed. Count: {s_cachedStoroedItemIDToNumberDict.Count}");
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

        private static IEnumerable<Item> s_getAllStoredItem()
        {
            foreach (var innerItem in s_getAllItemsInInventory(s_storageInventory))
                yield return innerItem;
        }

        private static IEnumerable<Item> s_getAllIHeldItems()
        {
            foreach (var innerItem in s_getAllItemsOnItem(s_characterItem))
                yield return innerItem;

            foreach (var innerItem in s_getAllItemsInInventory(s_petInventory))
                yield return innerItem;
        }

        private static void s_calcuPathsForItemStored(IEnumerable<Item> items)
        {
            s_pathsAndCountsForStoredItem.Clear();
            foreach (var item in items)
            {
                s_nodeNames.Clear();
                var currentNode = item;
                int counter = 0;
                while (counter++ <= 5)
                {
                    if (currentNode.InInventory == s_storageInventory)
                    {
                        s_nodeNames.Add("  仓库");
                        break;
                    }
                    else if (currentNode.InInventory?.AttachedToItem != null)
                    {
                        currentNode = currentNode.InInventory.AttachedToItem;
                        s_nodeNames.Add(currentNode.DisplayName);
                    }
                    else if (currentNode.PluggedIntoSlot?.Master != null)
                    {
                        currentNode = currentNode.PluggedIntoSlot.Master;
                        s_nodeNames.Add(currentNode.DisplayName);
                    }
                    else
                    {
                        Debug.LogError($"[WhereAreMyItems] Unexpected Orphan Item");
                        break;
                    }
                }
                s_nodeNames.Reverse();
                string fullPath = string.Join('-', s_nodeNames);
                s_saveStoredItemPathAndCount(fullPath, item.StackCount);
            }
        }

        private static void s_calcuPathsForItemHeld(IEnumerable<Item> items)
        {
            s_pathsAndCountsForHeldItem.Clear();
            foreach (var item in items)
            {
                s_nodeNames.Clear();
                int counter = 0;
                var currentNode = item;
                while (counter++ <= 5)
                {
                    if (currentNode.InInventory == s_petInventory)
                    {
                        s_nodeNames.Add("  宠物背包");
                        break;
                    }
                    else if (currentNode.PluggedIntoSlot?.Master == s_characterItem)
                    {
                        s_nodeNames.Add("  角色装备");
                        break;
                    }
                    else if (currentNode.InInventory?.AttachedToItem == s_characterItem)
                    {
                        s_nodeNames.Add("  角色背包");
                        break;
                    }
                    else if (currentNode.InInventory?.AttachedToItem != null)
                    {
                        currentNode = currentNode.InInventory.AttachedToItem;
                        s_nodeNames.Add(currentNode.DisplayName);
                    }
                    else if (currentNode.PluggedIntoSlot?.Master != null)
                    {
                        currentNode = currentNode.PluggedIntoSlot.Master;
                        s_nodeNames.Add(currentNode.DisplayName);
                    }
                    else
                    {
                        Debug.LogError($"[WhereAreMyItems] Unexpected Path");
                        break;
                    }
                }
                s_nodeNames.Reverse();
                string fullPath = string.Join('-', s_nodeNames);
                s_saveHeldItemPathAndCount(fullPath, item.StackCount);
            }
        }

        private static void s_saveHeldItemPathAndCount(string path, int count)
        {
            Debug.Log($"[WhereAreMyItems] HeldItemPathAndCount, path: {path}, count: {count}");
            if (!s_pathsAndCountsForHeldItem.Any(i => i.m_path == path))
            {
                var newItemPathAndCount = new ItemPathAndCount
                {
                    m_path = path
                };
                s_pathsAndCountsForHeldItem.Add(newItemPathAndCount);
            }
            var target = s_pathsAndCountsForHeldItem.First(i => i.m_path == path);
            target.m_count += count;
        }

        private static void s_saveStoredItemPathAndCount(string path, int count)
        {
            Debug.Log($"[WhereAreMyItems] StoredItemPathAndCount, path: {path}, count: {count}");
            if (!s_pathsAndCountsForStoredItem.Any(i => i.m_path == path))
            {
                var newItemPathAndCount = new ItemPathAndCount
                {
                    m_path = path
                };
                s_pathsAndCountsForStoredItem.Add(newItemPathAndCount);
            }
            var target = s_pathsAndCountsForStoredItem.First(i => i.m_path == path);
            target.m_count += count;
        }

        private static int s_getItemStoredCount(int typeID, bool isDetail)
        {
            try
            {
                if (s_storageInventory != null)
                {
                    Debug.Log($"[WhereAreMyItems] getItemStoredCount 实时计算Item: {typeID}, isDetail = {isDetail}");
                    var items = s_getAllStoredItem().Where(i => i.TypeID == typeID).ToArray();
                    if (isDetail)
                        s_calcuPathsForItemStored(items);
                    var itemCount = items.Select(i => i.StackCount).Sum();
                    return itemCount;
                }

                Debug.Log($"[WhereAreMyItems] getItemStoredCount 查询缓存Item: {typeID}, isDetail = {isDetail}");
                if (!s_cachedStoroedItemIDToNumberDict.TryGetValue(typeID, out var cachedItemCount))
                    cachedItemCount = 0;
                return cachedItemCount;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return -1;
            }
        }

        private static int s_getItemHeldCount(int typeID, bool isDetail)
        {
            try
            {
                Debug.Log($"[WhereAreMyItems] getItemHeldCount 实时计算Item: {typeID}, isDetail = {isDetail}");
                var items = s_getAllIHeldItems().Where(i => i.TypeID == typeID).ToArray();
                if (isDetail)
                    s_calcuPathsForItemHeld(items);
                var itemCount = items.Select(i => i.StackCount).Sum();
                return itemCount;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return -1;
            }
        }

        public static void s_RefreshItemInformation(Item item, bool isDetail)
        {
            Debug.Log($"[WhereAreMyItems] Refresh Item {item.TypeID} Information, isDetail = {isDetail}");
            try
            {
                int itemHeldCount = s_getItemHeldCount(item.TypeID, isDetail);
                int itemStoredCount = s_getItemStoredCount(item.TypeID, isDetail);

                var showDetailHeldInfo = isDetail && itemHeldCount > 0;
                var showDetailStoredInfo = isDetail && itemStoredCount > 0;
                var heldSpecificCounts = s_pathsAndCountsForHeldItem.Select(i => i.m_count).ToArray();
                var storedSpecificCounts = s_pathsAndCountsForStoredItem.Select(i => i.m_count).ToArray();

                var heldSpecificCountsString = showDetailHeldInfo ? string.Join("\n", heldSpecificCounts) + "\n" : string.Empty;
                var storedSpecificCountsString = showDetailStoredInfo ? string.Join("\n", storedSpecificCounts) + "\n" : string.Empty;
                s_cachedRightInformation = $"{itemHeldCount}\n{heldSpecificCountsString}{itemStoredCount}\n{storedSpecificCountsString}";

                var heldSpecificPathsString = showDetailHeldInfo ? string.Join("\n", s_pathsAndCountsForHeldItem.Select(i => i.m_path).ToArray()) + "\n" : string.Empty;
                var storedSpecificPathsString = showDetailStoredInfo ? string.Join("\n", s_pathsAndCountsForStoredItem.Select(i => i.m_path).ToArray()) + "\n" : string.Empty;
                s_cachedLeftInformation = $"携带数量\n{heldSpecificPathsString}仓库数量\n{storedSpecificPathsString}";
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static string s_GetLeftText()
        {
            Debug.Log($"[WhereAreMyItems] Get Left Text: {s_cachedLeftInformation}");
            return s_cachedLeftInformation;
        }

        public static string s_GetRightText()
        {
            Debug.Log($"[WhereAreMyItems] Get Right Text: {s_cachedRightInformation}");
            return s_cachedRightInformation;
        }
    }
}