using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class TabButtonToggle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

#region Public Fields
#endregion

#region Private Serializable Fields
    [SerializeField] TabsController tabsController;
    [SerializeField] GameObject tabParent;
    [SerializeField] Sprite spriteBackgroundActive, spriteBackgroundHovering, spriteBackgroundIdle;
    [SerializeField] Color overrideColorActive = Color.white, overrideColorHovering = Color.white, overrideColorIdle = Color.white;
    [SerializeField] bool startActive = false;
    [SerializeField] GameObject alsoDeativateThis;
#endregion

#region Private Fields
    Image imgBackground;
    bool isActive = false;
#endregion

#region MonoBehaviour CallBacks

    private void Awake() {
        imgBackground = GetComponent<Image>();
        if(imgBackground == null) {
            Debug.LogError($"{name} is missing a component");
        }
    }

    void Start() {
        imgBackground.sprite = spriteBackgroundIdle;
        imgBackground.color = overrideColorIdle;
        tabsController.Subscribe(this);

        if(startActive) {
            SetActive();
        } else {
            SetIdle();
        }
    }
#endregion

#region Private Methods

#endregion

#region Public Methods
    public void SetIdle() {
        imgBackground.sprite = spriteBackgroundIdle;
        imgBackground.color = overrideColorIdle;
        isActive = false;
        tabParent.SetActive(false);
        if(alsoDeativateThis != null) {
            alsoDeativateThis.SetActive(false);
        }
    }
    public void SetActive() {
        imgBackground.sprite = spriteBackgroundActive;
        imgBackground.color = overrideColorActive;
        isActive = true;
        tabParent.SetActive(true);
        if(alsoDeativateThis != null) {
            alsoDeativateThis.SetActive(true);
        }
    }
    public void SetHovering(bool isHovering) {
        if(isHovering) {
            imgBackground.sprite = spriteBackgroundHovering;
            imgBackground.color = overrideColorHovering;
        } else {
            imgBackground.sprite = spriteBackgroundIdle;
            imgBackground.color = overrideColorIdle;
        }
    }
#endregion

#region Events
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
        if(isActive) {
            return;
        }
        tabsController.OnTabEnter(this);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
        if(isActive) {
            return;
        }
        tabsController.OnTabExit(this);
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
        if(isActive) {
            return;
        }
        tabsController.OnTabClicked(this);
    }
#endregion
}