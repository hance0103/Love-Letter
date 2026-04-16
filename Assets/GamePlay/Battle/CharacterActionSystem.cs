using System.Collections.Generic;
using GameData.SO.Scripts;
using GamePlay.Battle.Event;
using GamePlay.Battle.Event.EventType;
using GameSystem.Managers;
using UnityEngine;

namespace GamePlay.Battle
{
    public class CharacterActionSystem : MonoBehaviour
    {
        public void Init()
        {
            
        }
        
        private void OnEnable()
        {
            EventBus.Subscribe<CharacterActionEvent>(CharacterAction);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CharacterActionEvent>(CharacterAction);
        }

        private void CharacterAction(CharacterActionEvent characterActionEvent)
        {
            Debug.Log("캐릭터 행동");
            
            var abilities = characterActionEvent.ActionObject.Data.cardAbilityIDs;
            var abilitiesList = new List<CardAbilityBase>();
            
            foreach (var ability in abilities)
            {
                abilitiesList.Add(GameManager.Inst.Data.GetAbility(ability));
            }
        }
    }
}
