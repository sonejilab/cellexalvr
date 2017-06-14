using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;      
using Random = System.Random;

namespace ListView
{
    public class CardGameList : ListViewController<CardData, Card>
    {
        public string defaultTemplate = "Card";
        public float interpolate = 15f;
        public float recycleDuration = 0.3f;
        public int dealMax = 5;
        public Transform deck;

        Vector3 m_StartPos;


        protected override void Setup()
        {
            base.Setup();

            List<CardData> dataList = new List<CardData>(52);
            for (int i = 0; i < 4; i++)
            {
                for (int j = 1; j < 14; j++)
                {
                    CardData card = new CardData();
                    switch (j)
                    {
                        case 1:
                            card.value = "A";
                            break;
                        case 11:
                            card.value = "J";
                            break;
                        case 12:
                            card.value = "Q";
                            break;
                        case 13:
                            card.value = "K";
                            break;
                        default:
                            card.value = j + "";
                            break;
                    }
                    card.suit = (Card.Suit) i;
                    card.template = defaultTemplate;
                    dataList.Add(card);
                }
            }
            Shuffle(dataList);

            range = 0;
        }

        void Shuffle(List<CardData> dataList)
        {
            Random rnd = new Random();
            data = dataList.OrderBy(x => rnd.Next()).ToArray();
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position + Vector3.left * (itemSize.x * dealMax - range) * 0.5f, new Vector3(range, itemSize.y, itemSize.z));
        }

        protected override void UpdateItems()
        {
            m_StartPos = transform.position + Vector3.left * itemSize.x * dealMax * 0.5f;
            for (int i = 0; i < data.Length; i++)
            {
                if (i + m_DataOffset < 0)
                {
                    ExtremeLeft(data[i]);
                } else if (i + m_DataOffset > m_NumItems - 1)
                {
                    ExtremeRight(data[i]);
                } else
                {
                    ListMiddle(data[i], i);
                }
            }
        }

        protected override void ExtremeLeft(CardData data)
        {
            RecycleItemAnimated(data, deck);
        }

        protected override void ExtremeRight(CardData data)
        {
            RecycleItemAnimated(data, deck);
        }

        protected override void ListMiddle(CardData data, int offset)
        {
            if (data.item == null)
            {
                data.item = GetItem(data);
                data.item.transform.position = deck.transform.position;
                data.item.transform.rotation = deck.transform.rotation;
            }
            Positioning(data.item.transform, offset);
        }

        protected override Card GetItem(CardData data)
        {
            if (data == null)
            {
                Debug.LogWarning("Tried to get item with null data");
                return null;
            }
            if (!m_Templates.ContainsKey(data.template))
            {
                Debug.LogWarning("Cannot get item, template " + data.template + " doesn't exist");
                return null;
            }
            Card item = null;
            if (m_Templates[data.template].pool.Count > 0)
            {
                item = (Card) m_Templates[data.template].pool[0];
                m_Templates[data.template].pool.RemoveAt(0);

                item.gameObject.SetActive(true);
                item.GetComponent<BoxCollider>().enabled = true;
                item.Setup(data);
            } else
            {
                item = Instantiate(m_Templates[data.template].prefab).GetComponent<Card>();
                item.transform.parent = transform;
                item.Setup(data);
            }
            return item;
        }

        void RecycleItemAnimated(CardData data, Transform destination)
        {
            if (data.item == null)
                return;
            MonoBehaviour item = data.item;
            data.item = null;
            item.GetComponent<BoxCollider>().enabled = false;       //Disable collider so we can't click the card during the animation
            StartCoroutine(RecycleAnimation(item, data.template, destination, recycleDuration));
        }

        IEnumerator RecycleAnimation(MonoBehaviour card, string template, Transform destination, float speed)
        {
            float start = Time.time;
            Quaternion startRot = card.transform.rotation;
            Vector3 startPos = card.transform.position;
            while (Time.time - start < speed)
            {
                card.transform.rotation = Quaternion.Lerp(startRot, destination.rotation, (Time.time - start) / speed);
                card.transform.position = Vector3.Lerp(startPos, destination.position, (Time.time - start) / speed);
                yield return null;
            }
            card.transform.rotation = destination.rotation;
            card.transform.position = destination.position;
            RecycleItem(template, card);
        }

        protected override void Positioning(Transform t, int offset)
        {
            t.position = Vector3.Lerp(t.position, m_StartPos + (offset * m_ItemSize.x + scrollOffset) * Vector3.right, interpolate * Time.deltaTime);
            t.rotation = Quaternion.Lerp(t.rotation, Quaternion.identity, interpolate * Time.deltaTime);
        }

        void RecycleCard(CardData data)
        {
            RecycleItemAnimated(data, deck);
        }

        public CardData DrawCard()
        {
            if (data.Length == 0)
            {
                Debug.Log("Out of Cards");
                return null;
            }
            List<CardData> newData = new List<CardData>(data);
            CardData result = newData[newData.Count - 1];
            newData.RemoveAt(newData.Count - 1);
            if (result.item == null)
            {
                GetItem(result);
            }
            data = newData.ToArray();
            return result;
        }

        public void RemoveCardFromDeck(CardData cardData)
        {
            List<CardData> newData = new List<CardData>(data);
            newData.Remove(cardData);
            data = newData.ToArray();
            if (range > 0)
                range -= itemSize.x;
        }

        public void AddCardToDeck(CardData cardData)
        {
            data = new List<CardData>(data) {cardData}.ToArray();
            cardData.item.transform.parent = transform;
            RecycleCard(cardData);
        }

        public void Deal()
        {
            range += itemSize.x;
            if (range >= itemSize.x * (dealMax + 1))
            {
                scrollOffset -= itemSize.x * dealMax;
                range = 0;
            }
            if (-scrollOffset >= (data.Length - dealMax) * itemSize.x)
            { //reshuffle
                Shuffle(new List<CardData>(data));
                scrollOffset = itemSize.x * 0.5f;
            }
        }
    }
}