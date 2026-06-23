using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Coroutine returnCoroutine;
    private Coroutine scaleCoroutine;
    private Vector3 originalScale;
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
        originalScale = transform.localScale;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isInCombine) return;

        if (returnCoroutine != null) StopCoroutine(returnCoroutine);

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

        if (gameObject.activeInHierarchy)
        {
            if (returnCoroutine != null) StopCoroutine(returnCoroutine);
            returnCoroutine = StartCoroutine(SmoothReturnRoutine());
        }
        else
        {
            rectTransform.anchoredPosition = startPosition;
        }

        UpdateState(); // Pastikan tampilan kembali normal
    }

    private IEnumerator SmoothReturnRoutine()
    {
        Vector2 startPos = rectTransform.anchoredPosition;
        float elapsed = 0f;
        float duration = 0.25f; // Waktu animasi kembali (detik)

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Ease out smoothstep manual: t * t * (3f - 2f * t)
            float smoothT = t * t * (3f - 2f * t);
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, startPosition, smoothT);
            yield return null;
        }

        rectTransform.anchoredPosition = startPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDragging || (combineSlotRef != null && (combineSlotRef.isLockedByResult || combineSlotRef.IsFull()) && !isInCombine)) return;
        
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(SmoothScaleRoutine(originalScale * 1.1f));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(SmoothScaleRoutine(originalScale));
    }

    private IEnumerator SmoothScaleRoutine(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;
        float duration = 0.15f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale;
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