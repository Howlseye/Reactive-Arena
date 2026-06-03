using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CombineSlot : MonoBehaviour, IDropHandler
{
    private List<DraggableCard> cardsInSlot = new List<DraggableCard>();
    public float spacing = 15f; 
    public int maxCards = 3; // Batas maksimal kartu

    // Cek apakah kotak combine sudah penuh
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
                // Pengaman ganda: jika sudah penuh, tolak kartu masuk
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
        if (count == 0) return;

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
}
