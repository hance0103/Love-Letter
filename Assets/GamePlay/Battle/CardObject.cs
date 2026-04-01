using UnityEngine;

namespace GamePlay.Battle
{
    public class CardObject : MonoBehaviour
    {
        public string CardIndex { get; private set; }
        public string CardName { get; private set; }
        public int CardCost { get; private set; }
        public string CardText { get; private set; }
    }
}
