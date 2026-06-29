using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CombineSlot : MonoBehaviour, IDropHandler
{
    private List<DraggableCard> cardsInSlot = new List<DraggableCard>();
    public float spacing = 15f; 
    public int maxCards = 3; 

    public bool isLockedByResult = false;

    void Start()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName == "MainScene")
        {
            maxCards = 3;
        }
        else if (sceneName == "Level 2")
        {
            maxCards = 3;
        }
        else if (sceneName == "Level 3")
        {
            maxCards = 3;
        }
    }

    public void SetLockState(bool locked)
    {
        isLockedByResult = locked;
        NotifyAllCards();
    }

    public List<DraggableCard> GetCards()
    {
        return cardsInSlot;
    }

    public void ClearCardsAfterThrow()
    {
        // Hancurkan objek kartu agar bersih
        foreach (var card in cardsInSlot)
        {
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }
        
        cardsInSlot.Clear();
        UpdateLayout(); // Ini akan otomatis menyalakan kembali kartu-kartu di Deck!
    }

    public bool IsFull()
    {
        return cardsInSlot.Count >= maxCards;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            DraggableCard droppedCard = eventData.pointerDrag.GetComponent<DraggableCard>();
            
            if (droppedCard != null && !droppedCard.isInCombine)
            {
                if (IsFull()) return;

                droppedCard.isInCombine = true;
                droppedCard.transform.SetParent(this.transform);
                
                cardsInSlot.Add(droppedCard);
                UpdateLayout();
            }
        }
    }

    public void RemoveCard(DraggableCard card)
    {
        if (cardsInSlot.Contains(card))
        {
            cardsInSlot.Remove(card);
            UpdateLayout();
        }
    }

    private void UpdateLayout()
    {
        int count = cardsInSlot.Count;
        
        // Posisikan kartu-kartu
        if (count > 0)
        {
            float cardWidth = cardsInSlot[0].GetComponent<RectTransform>().rect.width;
            float containerWidth = GetComponent<RectTransform>().rect.width;
            
            float totalWidth = (count * cardWidth) + ((count - 1) * spacing);
            float currentSpacing = spacing;
            
            if (totalWidth > containerWidth)
            {
                currentSpacing = (containerWidth - (count * cardWidth)) / (count - 1);
                totalWidth = containerWidth;
            }
            
            float startX = -totalWidth / 2f + cardWidth / 2f;

            for (int i = 0; i < count; i++)
            {
                RectTransform cardRect = cardsInSlot[i].GetComponent<RectTransform>();
                float posX = startX + (i * (cardWidth + currentSpacing));
                
                cardRect.anchoredPosition = new Vector2(posX, 0);
            }
        }

        // SETELAH posisi diperbarui, beri tahu SEMUA kartu untuk memperbarui status visualnya (Enable/Disable)
        NotifyAllCards();
    }

    // Memberitahu kartu di Deck untuk mati/nyala tergantung penuh tidaknya Combine
    private void NotifyAllCards()
    {
        DraggableCard[] allCards = FindObjectsOfType<DraggableCard>();
        foreach (var card in allCards)
        {
            card.UpdateState();
        }
    }
}