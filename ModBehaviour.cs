using Duckov.UI;
using Duckov.Utilities;
using ItemStatsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WhereAreMyItems
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private static GameObject s_uiParent = null;
        private static TextMeshProUGUI s_textLeft = null;
        private static TextMeshProUGUI s_spaceMiddle = null;
        private static TextMeshProUGUI s_textRight = null;
        private bool m_showDetailInformation = true;
        private Item m_currentItem = null;
        private float m_lastShiftPressedTime = Time.time;

        private void Awake()
        {
            Debug.Log("[WhereAreMyItems] Mod Loaded");
            m_recreateUI();
        }

        private void OnEnable()
        {
            Debug.Log("[WhereAreMyItems] Mod Enabled");
            ItemHoveringUI.onSetupItem += m_onSetupItemHoveringUI;
            ItemHoveringUI.onSetupMeta += m_onSetupMeta;
            Utils.s_Prepare();
        }

        private void Update()
        {
            m_tickShiftButton();
        }

        private void OnDisable()
        {
            Debug.Log("[WhereAreMyItems] Mod Disabled");
            ItemHoveringUI.onSetupItem -= m_onSetupItemHoveringUI;
            ItemHoveringUI.onSetupMeta -= m_onSetupMeta;
        }

        private void OnDestroy()
        {
            Debug.Log("[WhereAreMyItems] Mod Destroied");
            if (s_uiParent != null)
                Destroy(s_uiParent);
        }

        private void m_tickShiftButton()
        {
            if (Time.time - m_lastShiftPressedTime < 0.5f)
                return;

            if (m_currentItem == null || !s_uiParent.activeInHierarchy)
                return;

            var isShiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            if (!isShiftHeld)
                return;

            m_lastShiftPressedTime = Time.time;
            m_showDetailInformation = !m_showDetailInformation;
            m_refreshItemInformation();
            Debug.Log($"[WhereAreMyItems] Show Detail Toggled: {m_showDetailInformation}");
        }

        private void m_recreateUI()
        {
            if (s_uiParent != null)
                Destroy(s_uiParent);

            s_uiParent = new GameObject("UIParent", typeof(RectTransform));
            s_uiParent.SetActive(false);

            var hlg = s_uiParent.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.UpperLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            s_textLeft = Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI, s_uiParent.transform);
            s_textLeft.name = "TextLeft";
            s_textLeft.alignment = TextAlignmentOptions.Left;
            s_textLeft.fontSize = 18;
            m_addFitterAndElement(s_textLeft.gameObject);

            s_spaceMiddle = Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI, s_uiParent.transform);
            s_spaceMiddle.name = "SpaceMiddle";
            var middleLayoutElement = s_spaceMiddle.gameObject.AddComponent<LayoutElement>();
            middleLayoutElement.minWidth = 1;
            middleLayoutElement.flexibleWidth = 1;

            s_textRight = Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI, s_uiParent.transform);
            s_textRight.name = "TextRight";
            s_textRight.alignment = TextAlignmentOptions.Right;
            s_textRight.fontSize = 18;
            m_addFitterAndElement(s_textRight.gameObject);
            Debug.Log("[WhereAreMyItems] UI Recreated.");
        }

        private void m_addFitterAndElement(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            var fitter = gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var layoutElement = gameObject.AddComponent<LayoutElement>();
            layoutElement.minWidth = 20f;
            layoutElement.flexibleWidth = 0f;
        }

        private void m_onSetupMeta(ItemHoveringUI ui, ItemMetaData data)
        {
            if (s_uiParent == null)
                m_recreateUI();

            s_uiParent?.SetActive(false);
        }

        private ItemHoveringUI m_currentItemHoveringUI = null;
        private void m_onSetupItemHoveringUI(ItemHoveringUI ui, Item item)
        {
            Debug.Log($"[WhereAreMyItems] Trying to Show Item: {item.TypeID}");
            if (s_uiParent == null)
                m_recreateUI();

            if (s_uiParent == null)
                return;

            if (ui == null || item == null)
            {
                s_uiParent.SetActive(false);
                return;
            }

            if (s_uiParent.transform.parent != ui.LayoutParent)
            {
                s_uiParent.transform.SetParent(ui.LayoutParent, false);
                s_uiParent.transform.localScale = Vector3.one;
                s_uiParent.transform.SetAsLastSibling();
            }

            m_currentItemHoveringUI = ui;
            m_currentItem = item;
            m_refreshItemInformation();
        }

        private void m_refreshItemInformation()
        {
            s_uiParent?.SetActive(true);
            Utils.s_RefreshItemInformation(m_currentItem, m_showDetailInformation);
            s_textLeft.SetText(Utils.s_GetLeftText());
            s_textRight.SetText(Utils.s_GetRightText());
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_currentItemHoveringUI.LayoutParent);
            s_textLeft.ForceMeshUpdate(true, true);
            s_textRight.ForceMeshUpdate(true, true);
        }
    }
}