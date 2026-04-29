using TMPro;
using UnityEngine;

public class CardDisplay : MonoBehaviour
{
    public CardData cardData;
    public int cardIndex;

    public MeshRenderer cardRenderer;
    public TextMeshPro nameText;
    public TextMeshPro costText;
    public TextMeshPro attackText;
    public TextMeshPro descriptionText;

    public bool isDragging = false;
    private Vector3 originalPosition;

    public LayerMask enemyLayer;
    public LayerMask playerMask;

    private void Start()
    {
        playerMask = LayerMask.GetMask("Player");
        enemyLayer = LayerMask.GetMask("Enemy");

        

        SetupCard(cardData);
    }

    public void SetupCard(CardData data)
    {
        cardData = data;

        if (nameText != null) nameText.text = data.cardName;
        if (costText != null) costText.text = data.manaCost.ToString();
        if (attackText != null) attackText.text = data.effectAmount.ToString();
        if (descriptionText != null) descriptionText.text = data.description;

        if (cardRenderer != null && data.artwork != null)
        {
            Material cardMaterial = cardRenderer.material;
            cardMaterial.mainTexture = data.artwork.texture;
        }

        if (descriptionText != null)
        {
            descriptionText.text = data.description + data.GetAdditionalEffectDescription();
        }

    }

    private void OnMouseDown()
    {
        originalPosition = transform.position;
        isDragging = true;
    }

    private void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Camera.main.WorldToScreenPoint(transform.position).z;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            transform.position = new Vector3(worldPos.x, worldPos.y, transform.position.z);
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;
        
        if (CardManager.Instance != null)
        {
            float disToDiscard = Vector3.Distance(transform.position, CardManager.Instance.discardPosition.position);

            if (disToDiscard < 2f)
            {
                CardManager.Instance.DiscardCard(cardIndex);
                return;
            }
        }

        if (CardManager.Instance.playerStats != null && CardManager.Instance.playerStats.currentMana < cardData.manaCost)
        {
            Debug.Log($"마나가 부족합니다! (필요 : {cardData.manaCost}, 현재 : {CardManager.Instance.playerStats?.currentMana ?? 0}");
            transform.position = originalPosition;
            return;
        }

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        bool cardUsed = false;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, enemyLayer))
        {
            CharacterStats enemyStats = hit.collider.GetComponent<CharacterStats>();

            if (enemyStats != null)
            {
                if (cardData.cardType == CardData.CardType.Attack)
                {
                    enemyStats.TakeDamage(cardData.effectAmount);
                    Debug.Log($"{cardData.cardName} 카드로 적에게 {cardData.effectAmount} 데미지를 입혔습니다");
                    cardUsed = true;
                }
            }
            else
            {
                Debug.Log("이 카드는 적에게 사용 할 수 없습니다.");
            }
        }
        else if (Physics.Raycast(ray, out hit, Mathf.Infinity, playerMask))
        {
            if (CardManager.Instance.playerStats != null)
            {
                if (cardData.cardType == CardData.CardType.Heal)
                {
                    CardManager.Instance.playerStats.Heal(cardData.effectAmount);
                    Debug.Log($"{cardData.cardName} 카드로 플레이어의 체력을 {cardData.effectAmount} 회복 하였습니다.");
                    cardUsed = true;
                }
            }
            else
            {
                Debug.Log("이 카드는 플레이어에게 사용 할 수 없습니다.");
            }
        }

        if (!cardUsed)
        {
            transform.position = originalPosition;
            if (CardManager.Instance != null)
            {
                CardManager.Instance.ArrangeHand();
            }

            return;
        }

        CardManager.Instance.playerStats.UseMana(cardData.manaCost);
        Debug.Log($"마나를 {cardData.manaCost} 사용 했습니다. (남은 마나 : {CardManager.Instance.playerStats.currentMana}");

        if (cardData.addtionalEffects != null && cardData.addtionalEffects.Count > 0)
        {
            ProgressAditionalEffectAndDiscard();
        }
        else
        {
            if (CardManager.Instance != null)
            {
                CardManager.Instance.DiscardCard(cardIndex);
            }
        }

        /*  변경 전 기존 코드
        if (CardManager.Instance.playerStats == null || CardManager.Instance.playerStats.currentMana < cardData.manaCost)
        {
            Debug.Log($"마나가 부족합니다! (필요 : {cardData.manaCost}, 현재 : {CardManager.Instance.playerStats.currentMana}");
            transform.position = originalPosition;
            return;
        }


        isDragging = false;

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        bool cardUsed = false;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, enemyLayer))
        {
            CharacterStats enemyStats = hit.collider.GetComponent<CharacterStats>();

            if (enemyStats != null)
            {
                if (cardData.cardType == CardData.CardType.Attack)
                {
                    enemyStats.TakeDamage(cardData.effectAmount);
                    Debug.Log($"{cardData.cardName} 카드로 적에게 {cardData.effectAmount} 데미지를 입혔습니다.");
                    cardUsed = true;
                }
                else
                {
                    Debug.Log($"이 카드는 적에게 사용할 수 없습니다.");
                }
            }
        }
        else if (Physics.Raycast(ray, out hit, Mathf.Infinity, playerMask))
        {
            CharacterStats playerStats = hit.collider.GetComponent<CharacterStats>();

            if (playerStats != null)
            {
                if (cardData.cardType == CardData.CardType.Heal)
                {
                    playerStats.Heal(cardData.effectAmount);
                    Debug.Log($"{cardData.cardName} 카드로 플레이어의 체력을 {cardData.effectAmount} 회복했습니다.");
                    cardUsed = true;
                }
                else
                {
                    Debug.Log("이 카드는 플레이어에게 사용할 수 없습니다.");
                }
            }
        }
        else if (CardManager.Instance != null)
        { 
            float distTODiscard = Vector3.Distance(transform.position, CardManager.Instance.discardPosition.position);

            if (distTODiscard < 2.0f)
            {
                CardManager.Instance.DiscardCard(cardIndex);
                return;
            }
        }

        if (!cardUsed)
        {
            transform.position = originalPosition;

            CardManager.Instance.ArrangeHand();
        }
        else
        {
            if (CardManager.Instance != null)
            {
                CardManager.Instance.DiscardCard(cardIndex);
            }

            CardManager.Instance.playerStats.UseMana(cardData.manaCost);
            Debug.Log($"마나를 {cardData.manaCost} 만큼 사용했습니다.");
        } */
    }

    public void ProgressAditionalEffectAndDiscard()
    {
        CardData cardDataCopy = cardData;
        int cardIndexCopy = cardIndex;

        foreach (var effect in cardDataCopy.addtionalEffects)
        {
            switch (effect.effectType)
            {
                case CardData.AdditionalEffectType.DrawCard:
                    for (int i = 0; i < effect.effectAmount; i++)
                    {
                        if (CardManager.Instance != null)
                        {
                            CardManager.Instance.DrawCard();
                        }
                    }
                    Debug.Log($"{effect.effectAmount} 장의 카드를 드로우 했습니다");
                    break;

                case CardData.AdditionalEffectType.DiscardCard:
                    for (int i = 0; i < effect.effectAmount; i++)
                    {
                        if (CardManager.Instance != null && CardManager.Instance.handCards.Count > 0)
                        {
                            int randomIndex = Random.Range(0, CardManager.Instance.handCards.Count);

                            Debug.Log($"랜덤 카드 버리기 : 선택된 인덱스 {randomIndex}, 현재 손패 크기 : {CardManager.Instance.handCards.Count}");

                            if (cardIndexCopy < CardManager.Instance.handCards.Count)
                            {
                                if (randomIndex != cardIndexCopy)
                                {
                                    CardManager.Instance.DiscardCard(randomIndex);

                                    if (randomIndex < cardIndexCopy)
                                    {
                                        cardIndexCopy--;
                                    }
                                }
                                else if (CardManager.Instance.handCards.Count > 1)
                                {
                                    int newIndex = (randomIndex + 1)% CardManager.Instance.handCards.Count;
                                    CardManager.Instance.DiscardCard(newIndex);

                                    if (randomIndex < cardIndexCopy)
                                    {
                                        cardIndexCopy--;
                                    }
                                }
                            }
                            else
                            {
                                CardManager.Instance.DiscardCard(randomIndex);
                            }
                        }
                    }
                    break;

                case CardData.AdditionalEffectType.GainMana:
                    if (CardManager.Instance.playerStats != null)
                    {
                        CardManager.Instance.playerStats.GainMana(effect.effectAmount);
                        Debug.Log($"마나를 {effect.effectAmount} 획득 했습니다.");
                    }

                    break;

                case CardData.AdditionalEffectType.ReduceEnemyMana:
                    if (CardManager.Instance.enemyStats != null)
                    {
                        CardManager.Instance.enemyStats.UseMana(effect.effectAmount);
                        Debug.Log($"적이 마나를 {effect.effectAmount} 잃었습니다.");
                    }

                    break;
            }            
        }

        if (CardManager.Instance != null)
        {
            CardManager.Instance.DiscardCard(cardIndexCopy);
        }
    }
}
