using UnityEngine;

namespace ListView
{
    public class Card : ListViewItem<CardData>
    {
        public enum Suit
        {
            DIAMONDS,
            HEARTS,
            SPADES,
            CLUBS
        }

        public TextMesh topNum, botNum;
        public float centerScale = 3f;

        [SerializeField] GameObject m_Diamond;
        [SerializeField] GameObject m_Heart;
        [SerializeField] GameObject m_Spade;
        [SerializeField] GameObject m_Club;

        Vector3 m_Size;
        const float k_QuadOffset = 0.001f; //Local y offset for placing quads

        public override void Setup(CardData data)
        {
            base.Setup(data);
            topNum.text = data.value;
            botNum.text = data.value;

            m_Size = GetComponent<BoxCollider>().size;

            SetupCard();
        }

        void DestroyChildren(Transform trans)
        {
            foreach (Transform child in trans)
            {
                if (child.gameObject != botNum.gameObject && child.gameObject != topNum.gameObject)
                    Destroy(child.gameObject);
            }
        }

        void SetupCard()
        {
            DestroyChildren(transform);
            var prefab = m_Heart;
            var color = Color.red;
            switch (data.suit)
            {
                case Suit.CLUBS:
                    color = Color.black;
                    prefab = m_Club;
                    break;
                case Suit.DIAMONDS:
                    prefab = m_Diamond;
                    break;
                case Suit.SPADES:
                    color = Color.black;
                    prefab = m_Spade;
                    break;
            }
            switch (data.value)
            {
                case "J":
                case "K":
                case "Q":
                case "A":
                {
                    GameObject quad = AddQuad(prefab);
                    quad.transform.localScale *= centerScale;
                    quad.transform.localRotation = Quaternion.AngleAxis(90, Vector3.right);
                    break;
                }
                default:
                {
                    int valNum = System.Convert.ToInt32(data.value);
                    float divisionY = 0;
                    float divisionX = 0;
                    int cols = (valNum < 4 ? 1 : 2);
                    int rows = valNum / cols;
                    if (valNum == 8)
                        rows = 3;
                    divisionY = 1f / (rows + 1);
                    divisionX = 1f / (cols + 1);
                    for (int j = 0; j < cols; j++)
                    {
                        for (int i = 0; i < rows; i++)
                        {
                            GameObject quad = AddQuad(prefab);
                            quad.transform.localPosition += Vector3.forward * m_Size.z * (divisionY * (i + 1) - 0.5f);
                            quad.transform.localPosition += Vector3.right * m_Size.x * (divisionX * (j + 1) - 0.5f);
                            quad.transform.localRotation = Quaternion.AngleAxis(90, Vector3.right);
                        }
                    }
                    int leftover = 0;
                    switch (valNum)
                    {
                        case 5:
                            divisionY = 0f;
                            leftover = 1;
                            break;
                        case 7:
                            divisionY = 0.125f;
                            leftover = 1;
                            break;
                        case 8:
                            divisionY = 0.125f;
                            leftover = 3;
                            break;
                        case 9:
                            divisionY = 0.2f;
                            leftover = 1;
                            break;
                        case 10:
                            divisionY = 0.25f;
                            leftover = 3;
                            break;
                    }
                    for (int i = 0; i < leftover; i += 2)
                    {
                        GameObject quad = AddQuad(prefab);
                        quad.transform.localPosition -= Vector3.forward * m_Size.z * (divisionY * (i - 1));
                        quad.transform.localRotation = Quaternion.AngleAxis(90, Vector3.right);
                    }
                    break;
                }
            }
            topNum.text = data.value;
            topNum.color = color;
            botNum.text = data.value;
            botNum.color = color;
        }

        GameObject AddQuad(GameObject prefab)
        {
            //NOTE: If we were really concerned about performance, we could pool the quads
            GameObject quad = Instantiate(prefab);
            quad.transform.parent = transform;
            quad.transform.localPosition = Vector3.up * k_QuadOffset;
            return quad;
        }
    }

    //[System.Serializable]     //Will cause warnings, but helpful for debugging
    public class CardData : ListViewItemData
    {
        //Ace is 1, King is 13
        public string value;
        public Card.Suit suit;
    }
}