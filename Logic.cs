using ItemStatsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WhereAreMyItems
{
    public static class Logic
    {
        private static readonly List<ItemPathAndCount> s_pathsAndCountsForHeldItem = new List<ItemPathAndCount>();

        private static readonly List<ItemPathAndCount> s_pathsAndCountsForStoredItem = new List<ItemPathAndCount>();

        private static string s_cachedInformation = string.Empty;

        public static string s_GetText()
        {
            return s_cachedInformation;
        }

        public static void s_RefreshItemInformation(int itemID, bool isDetail)
        {
            try
            {
                int itemHeldCount = Utils.s_GetItemCountOnDuckAndDog(itemID);
                int itemStoredCount = Utils.s_GetItemCountInStorage(itemID);

                var showDetailHeldInfo = isDetail && itemHeldCount > 0;
                var showDetailStoredInfo = isDetail && itemStoredCount > 0 && Utils.s_IsStorageAvailable;

                List<ItemPathAndCount> itemHeldPathAndCount;
                if (showDetailHeldInfo)
                {
                    var itemsOnDuckAndDog = Utils.s_GetItemsOnDuckAndDogByID(itemID);
                    s_sortItemOnDuckAndDog(itemsOnDuckAndDog);
                    itemHeldPathAndCount = s_pathsAndCountsForHeldItem;
                }
                else
                    itemHeldPathAndCount = new List<ItemPathAndCount>();

                List<ItemPathAndCount> itemStoredPathAndCount;
                if (showDetailStoredInfo)
                {
                    var itemsInStorage = Utils.s_GetItemsInStorageByID(itemID);
                    s_sortItemInStorage(itemsInStorage);
                    itemStoredPathAndCount = s_pathsAndCountsForStoredItem;
                }
                else
                    itemStoredPathAndCount = new List<ItemPathAndCount>();

                string renderedText = s_renderText(itemHeldCount, itemStoredCount, itemHeldPathAndCount, itemStoredPathAndCount);
                s_cachedInformation = renderedText;
                Debug.Log($"[WhereAreMyItems] Refresh Item {itemID} Information, isDetail = {isDetail}");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static string s_renderText(int itemHeldCount, int itemStoredCount, List<ItemPathAndCount> itemHeldPathAndCount, List<ItemPathAndCount> itemStoredPathAndCount)
        {
            List<string> rows = new List<string>();
            rows.Add($"{ShortMessage.Held.GetTranslation()}: {itemHeldCount} {ShortMessage.Stored.GetTranslation()}: {itemStoredCount}");
            rows.AddRange(itemHeldPathAndCount.Select(i => $"<color=#888888>* {i.m_path}: {i.m_count}</color>"));
            rows.AddRange(itemStoredPathAndCount.Select(i => $"<color=#888888>* {i.m_path}: {i.m_count}</color>"));
            string result = string.Join("\n", rows.ToArray());
            return result;
        }

        private static void s_sortItemInStorage(IEnumerable<Item> items)
        {
            Dictionary<string, int> pathCounter = new Dictionary<string, int>();
            foreach (var item in items)
            {
                string fullPath = string.Join(" -> ", item.GetPath());
                if (pathCounter.ContainsKey(fullPath))
                    pathCounter[fullPath] += item.StackCount;
                else
                    pathCounter[fullPath] = item.StackCount;
            }
            s_pathsAndCountsForStoredItem.Clear();
            s_pathsAndCountsForStoredItem.AddRange(pathCounter.Select(i => new ItemPathAndCount(i.Key, i.Value)));
        }

        private static void s_sortItemOnDuckAndDog(IEnumerable<Item> items)
        {
            Dictionary<string, int> pathCounter = new Dictionary<string, int>();
            foreach (var item in items)
            {
                string fullPath = string.Join(" -> ", item.GetPath());
                if (pathCounter.ContainsKey(fullPath))
                    pathCounter[fullPath] += item.StackCount;
                else
                    pathCounter[fullPath] = item.StackCount;
            }
            s_pathsAndCountsForHeldItem.Clear();
            s_pathsAndCountsForHeldItem.AddRange(pathCounter.Select(i => new ItemPathAndCount(i.Key, i.Value)));
        }

        public class ItemPathAndCount
        {
            public int m_count = 0;

            public string m_path = string.Empty;

            public ItemPathAndCount(string path, int count)
            {
                m_path = path;
                m_count = count;
            }
        }
    }
}