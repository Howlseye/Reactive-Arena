using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IDropHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private CombineSlot combineSlotRef;
    
    private Vector2 startPosition;
    private Transform originalParent;
    private int originalSiblingIndex;

    public string cardType;
    public bool isInCombine = false;
    public bool isResultCard = false; // Penanda apakah ini kartu gabungan
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
            combineSlotRef = FindObjectOfType<CombineSlot>();
        }
        
        startPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isInCombine) return;

        if (combineSlotRef != null && combineSlotRef.IsFull())
        {
            return;
        }

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

        UpdateState(); // Pastikan tampilan kembali normal
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

    // Fungsi baru untuk memperbarui efek visual/status disable
    public void UpdateState()
    {
        if (combineSlotRef == null) return;

        if (isResultCard)
        {
            // Kartu result biarkan nyala terang, tapi matikan sensor sentuhnya
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 1f;
            return;
        }

        if (!isInCombine && (combineSlotRef.isLockedByResult || combineSlotRef.IsFull()))
        {
            // Jika kartu ada di Deck dan (Combine Penuh ATAU Terkunci karena ada Result):
            // Matikan total fitur drag & sentuh, ubah jadi transparan
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.4f; 
        }
        else
        {
            // Nyalakan kembali
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
        }
    }
}