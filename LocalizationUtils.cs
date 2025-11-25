using SodaCraft.Localizations;
using System.Collections.Generic;
using UnityEngine;

namespace WhereAreMyItems
{
    public enum ShortMessage
    {
        Held,
        Stored,
        Equipment,
        Backpack,
        PetBackpack,
        Warehouse,
    }

    public static class LocalizationUtils
    {
        private static readonly Dictionary<SystemLanguage, Dictionary<ShortMessage, string>> Translations =
            new Dictionary<SystemLanguage, Dictionary<ShortMessage, string>>()
        {
            {
                SystemLanguage.ChineseSimplified, new Dictionary<ShortMessage, string>()
                {
                    { ShortMessage.Held,         "携带数量" },
                    { ShortMessage.Stored,       "仓库数量" },
                    { ShortMessage.Equipment,   "装备" },
                    { ShortMessage.Backpack,     "背包" },
                    { ShortMessage.PetBackpack,  "宠物背包" },
                    { ShortMessage.Warehouse,   "仓库" },
                }
            },

            {
                SystemLanguage.ChineseTraditional, new Dictionary<ShortMessage, string>()
                {
                    { ShortMessage.Held,         "攜帶數量" },
                    { ShortMessage.Stored,       "倉庫數量" },
                    { ShortMessage.Equipment,   "裝備" },
                    { ShortMessage.Backpack,     "背包" },
                    { ShortMessage.PetBackpack,  "寵物背包" },
                    { ShortMessage.Warehouse,   "倉庫" },
                }
            },

            {
                SystemLanguage.English, new Dictionary<ShortMessage, string>()
                {
                    { ShortMessage.Held,         "Held" },
                    { ShortMessage.Stored,       "Stored" },
                    { ShortMessage.Equipment,   "Equipment" },
                    { ShortMessage.Backpack,     "Backpack" },
                    { ShortMessage.PetBackpack,  "Pet Backpack" },
                    { ShortMessage.Warehouse,   "Warehouse" },
                }
            },

            {
                SystemLanguage.Japanese, new Dictionary<ShortMessage, string>()
                {
                    { ShortMessage.Held,         "所持数" },
                    { ShortMessage.Stored,       "倉庫数" },
                    { ShortMessage.Equipment,   "装備" },
                    { ShortMessage.Backpack,     "バッグ" },
                    { ShortMessage.PetBackpack,  "ペットバッグ" },
                    { ShortMessage.Warehouse,   "倉庫" },
                }
            },

            {
                SystemLanguage.Korean, new Dictionary<ShortMessage, string>()
                {
                    { ShortMessage.Held,         "소지 수" },
                    { ShortMessage.Stored,       "창고 수" },
                    { ShortMessage.Equipment,   "장비" },
                    { ShortMessage.Backpack,     "가방" },
                    { ShortMessage.PetBackpack,  "펫 가방" },
                    { ShortMessage.Warehouse,   "창고" },
                }
            },

            {
                SystemLanguage.French, new Dictionary<ShortMessage, string>()
                {
                    { ShortMessage.Held,         "Possédé" },
                    { ShortMessage.Stored,       "En dépôt" },
                    { ShortMessage.Equipment,   "Équipement" },
                    { ShortMessage.Backpack,     "Sac à dos" },
                    { ShortMessage.PetBackpack,  "Sac familier" },
                    { ShortMessage.Warehouse,   "Entrepôt" },
                }
            },

            {
                SystemLanguage.Russian, new Dictionary<ShortMessage, string>()
                {
                    { ShortMessage.Held,         "На руках" },
                    { ShortMessage.Stored,       "На складе" },
                    { ShortMessage.Equipment,   "Экипировка" },
                    { ShortMessage.Backpack,     "Рюкзак" },
                    { ShortMessage.PetBackpack,  "Рюкзак питомца" },
                    { ShortMessage.Warehouse,   "Склад" },
                }
            },

            {
                SystemLanguage.German, new Dictionary<ShortMessage, string>()
                {
                    { ShortMessage.Held,         "Getragen" },
                    { ShortMessage.Stored,       "Gelagert" },
                    { ShortMessage.Equipment,   "Ausrüstung" },
                    { ShortMessage.Backpack,     "Rucksack" },
                    { ShortMessage.PetBackpack,  "Begleiter-Rucksack" },
                    { ShortMessage.Warehouse,   "Lager" },
                }
            },

            {
                SystemLanguage.Spanish, new Dictionary<ShortMessage, string>()
                {
                    { ShortMessage.Held,         "En mano" },
                    { ShortMessage.Stored,       "Almacenado" },
                    { ShortMessage.Equipment,   "Equipo" },
                    { ShortMessage.Backpack,     "Mochila" },
                    { ShortMessage.PetBackpack,  "Mochila de mascota" },
                    { ShortMessage.Warehouse,   "Almacén" },
                }
            },
        };

        public static string GetTranslation(this ShortMessage shortMessage)
        {
            var currentLanguage = LocalizationManager.CurrentLanguage;
            if (Translations.TryGetValue(currentLanguage, out var langDict))
            {
                if (langDict.TryGetValue(shortMessage, out var translation))
                {
                    return translation;
                }
            }
            return Translations[SystemLanguage.English][shortMessage];
        }
    }
}