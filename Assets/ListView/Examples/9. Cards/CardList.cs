using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Random = System.Random;

//Images sourced from http://web.stanford.edu/~jlewis8/cs148/pokerscene/

namespace ListView
{
    public class CardList : ListViewController<CardData, Card>
    {
        public string defaultTemplate = "Card";
        public float interpolate = 15f;
        public float recycleDuration = 0.3f;
        public bool autoScroll;
        public float scrollSpeed = 1;
        public Transform leftDeck, rightDeck;

        float m_ScrollReturn = float.MaxValue;
        float m_LastScrollOffset;

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
            Random rnd = new Random();
            data = dataList.OrderBy(x => rnd.Next()).ToArray();
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(range, itemSize.y, itemSize.z));
        }

        public void OnStopScrolling()
        {
            if (scrollOffset > itemSize.x)
            { //Let us over-scroll one whole card
                scrollOffset = itemSize.x * 0.5f;
            }
            if (m_ScrollReturn < float.MaxValue)
            {
                scrollOffset = m_ScrollReturn;
                m_ScrollReturn = float.MaxValue;
            }
        }

        protected override void UpdateItems()
        {
            if (autoScroll)
            {
                scrollOffset -= scrollSpeed * Time.deltaTime;
                if (-scrollOffset > (data.Length - m_NumItems) * itemSize.x || scrollOffset >= 0)
                    scrollSpeed *= -1;
            }
            for (int i = 0; i < data.Length; i++)
            {
                if (i + m_DataOffset < 0)
                {
                    ExtremeLeft(data[i]);
                } else if (i + m_DataOffset > m_NumItems - 1)
                { //End the m_List one item early
                    ExtremeRight(data[i]);
                } else
                {
                    ListMiddle(data[i], i);
                }
            }
            m_LastScrollOffset = scrollOffset;
        }

        protected override void ExtremeLeft(CardData data)
        {
            RecycleItemAnimated(data, leftDeck);
        }

        protected override void ExtremeRight(CardData data)
        {
            RecycleItemAnimated(data, rightDeck);
        }

        protected override void ListMiddle(CardData data, int offset)
        {
            if (data.item == null)
            {
                data.item = GetItem(data);
                if (scrollOffset - m_LastScrollOffset < 0)
                {
                    data.item.transform.position = rightDeck.transform.position;
                    data.item.transform.rotation = rightDeck.transform.rotation;
                } else
                {
                    data.item.transform.position = leftDeck.transform.position;
                    data.item.transform.rotation = leftDeck.transform.rotation;
                }
            }
            Positioning(data.item.transform, offset);
        }

        void RecycleItemAnimated(CardData data, Transform destination)
        {
            if (data.item == null)
                return;
            MonoBehaviour item = data.item;
            data.item = null;
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
            t.position = Vector3.Lerp(t.position, m_LeftSide + (offset * m_ItemSize.x + scrollOffset) * Vector3.right, interpolate * Time.deltaTime);
            t.rotation = Quaternion.Lerp(t.rotation, Quaternion.identity, interpolate * Time.deltaTime);
        }

    }
}