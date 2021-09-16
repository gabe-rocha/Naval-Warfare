using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TabsController : MonoBehaviour {

#region Public Fields

#endregion

#region Private Serializable Fields

#endregion

#region Private Fields
    List<TabButtonToggle> listTabButtons;

#endregion

#region MonoBehaviour CallBacks
#endregion

#region Private Methods
    void ResetAllTabs() {
        foreach (var tabButton in listTabButtons) {
            tabButton.SetIdle();
        }
    }
#endregion

#region Public Methods

    public void Subscribe(TabButtonToggle tabButton) {
        if(listTabButtons == null) {
            listTabButtons = new List<TabButtonToggle>();
        }
        listTabButtons.Add(tabButton);
    }

    public void OnTabClicked(TabButtonToggle tabButton) {
        ResetAllTabs();
        tabButton.SetActive();
    }
    public void OnTabEnter(TabButtonToggle tabButton) {
        tabButton.SetHovering(true);
    }
    public void OnTabExit(TabButtonToggle tabButton) {
        tabButton.SetHovering(false);
    }
#endregion
}