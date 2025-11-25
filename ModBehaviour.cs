using Duckov.UI;
using Duckov.Utilities;
using ItemStatsSystem;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace WhereAreMyItems
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private static TextMeshProUGUI s_textMeshPro = null;
        private bool m_showDetails = true;
        private Item m_currentItem = null;
        private float m_lastShiftPressedTime = Time.time;
        private ItemHoveringUI m_currentItemHoveringUI = null;

        private void Awake()
        {
            m_tryCreateUI();
            Debug.Log("[WhereAreMyItems] Mod Loaded");
        }

        private void OnEnable()
        {
            ItemHoveringUI.onSetupItem += m_onSetupItemHoveringUI;
            ItemHoveringUI.onSetupMeta += m_onSetupMeta;
            Utils.s_Prepare();
            Debug.Log("[WhereAreMyItems] Mod Enabled");
        }

        private void Update()
        {
            m_tickShiftButton();
        }

        private void OnDisable()
        {
            ItemHoveringUI.onSetupItem -= m_onSetupItemHoveringUI;
            ItemHoveringUI.onSetupMeta -= m_onSetupMeta;
            Debug.Log("[WhereAreMyItems] Mod Disabled");
        }

        private void OnDestroy()
        {
            if (s_textMeshPro.gameObject != null)
                Destroy(s_textMeshPro.gameObject);
            Debug.Log("[WhereAreMyItems] Mod Destroied");
        }

        private void m_tickShiftButton()
        {
            if (Time.time - m_lastShiftPressedTime < 0.5f)
                return;

            if (m_currentItem == null || !s_textMeshPro.gameObject.activeInHierarchy)
                return;

            var isShiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            if (!isShiftHeld)
                return;

            m_lastShiftPressedTime = Time.time;
            m_showDetails = !m_showDetails;
            m_refreshItemInformation();
            Debug.Log($"[WhereAreMyItems] Show Detail Toggled: {m_showDetails}");
        }

        private void m_tryCreateUI()
        {
            if (s_textMeshPro != null)
                return;

            s_textMeshPro = Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI);
            var fitter = gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var layoutElement = gameObject.AddComponent<LayoutElement>();
            layoutElement.minWidth = 20f;
            layoutElement.flexibleWidth = 0f;
            Debug.Log("[WhereAreMyItems] UI Recreated");
        }

        private void m_onSetupMeta(ItemHoveringUI ui, ItemMetaData data)
        {
            s_textMeshPro?.gameObject?.SetActive(false);
        }


        private void m_onSetupItemHoveringUI(ItemHoveringUI ui, Item item)
        {
            m_tryCreateUI();

            if (ui == null || item == null)
                return;

            m_currentItemHoveringUI = ui;
            m_currentItem = item;

            if (s_textMeshPro.transform.parent != ui.LayoutParent)
            {
                s_textMeshPro.transform.SetParent(ui.LayoutParent, false);
                s_textMeshPro.transform.localScale = Vector3.one;
                s_textMeshPro.transform.SetAsLastSibling();
                s_textMeshPro.fontSize = 16;
            }

            m_refreshItemInformation();
            s_textMeshPro.gameObject.SetActive(true);
            Debug.Log($"[WhereAreMyItems] Showing Item: {item.TypeID}");
        }

        private void m_refreshItemInformation()
        {
            Logic.s_RefreshItemInformation(m_currentItem.TypeID, m_showDetails);
            s_textMeshPro.SetText(Logic.s_GetText());
            //layoutrebuilder.forcerebuildlayoutimmediate(m_currentitemhoveringui.layoutparent);
            //s_textleft.forcemeshupdate(true, true);
            Debug.Log($"[WhereAreMyItems] Refreshed Item: {m_currentItem.TypeID}");
        }
    }
}