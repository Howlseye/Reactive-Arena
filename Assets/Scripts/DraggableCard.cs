using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IDropHandler
{
    // TAMBAHAN BARU: Identitas Kartu (Isi di Inspector dengan: "Air", "Garam", "Asam", atau "Logam")
    public string cardType; 

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private CombineSlot combineSlotRef;
    
    private Vector2 startPosition;
    private Transform originalParent;
    private int originalSiblingIndex;

    public bool isInCombine = false;
    private bool isDragging = false;

private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        
        GameObject combineObj = GameObject.Find("Combine");
        if (combineObj != null)
        {
            combineSlotRef = combineObj.GetComponent<CombineSlot>();
        }
        else
        {
            combineSlotRef = FindFirstObjectByType<CombineSlot>(); 
        }
        // ---------------------------------
        
        startPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isInCombine) return;
        if (combineSlotRef != null && combineSlotRef.IsFull()) return;

        isDragging = true;
        transform.SetAsLastSibling();
        
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.8f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        if (canvas != null)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        isDragging = false;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (!isInCombine)
        {
            ReturnToDeck();
        }
    }

    public void ReturnToDeck()
    {
        if (isInCombine && combineSlotRef != null)
        {
            combineSlotRef.RemoveCard(this);
        }
        
        isInCombine = false;
        transform.SetParent(originalParent);
        transform.SetSiblingIndex(originalSiblingIndex);
        rectTransform.anchoredPosition = startPosition;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isInCombine && !isDragging)
        {
            ReturnToDeck();
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (combineSlotRef != null)
        {
            combineSlotRef.OnDrop(eventData);
        }
    }
}