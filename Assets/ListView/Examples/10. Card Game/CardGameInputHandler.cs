using UnityEngine;

namespace ListView
{
    public class CardGameInputHandler : ListViewInputHandler
    {
        public CardGameHand hand;

        protected override void HandleInput()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction);
            if (Input.GetMouseButtonUp(0))
            {
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.name.Equals("Deck"))
                    {
                        ((CardGameList) listView).Deal();
                    } else
                    {
                        Card item = hit.collider.GetComponent<Card>();
                        if (item)
                        {
                            if (item.transform.parent == listView.transform)
                                hand.DrawCard(item);
                            else if (item.transform.parent == hand.transform)
                                hand.ReturnCard(item);
                        }
                    }
                }
            }
        }
    }
}
