using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameData.Scripts;
using GamePlay.Battle.Card;
using GamePlay.Battle.Event.EventType;
using GameSystem.Enums;
using GameSystem.Managers;
using UnityEngine;

namespace GamePlay.Battle.Event
{
    public class CardAbilitySystem : MonoBehaviour
    {
        private readonly Queue<CardAbilityBase> _abilityQueue = new Queue<CardAbilityBase>();
        public void Init()
        {
            _abilityQueue.Clear();
        }
        
        private void OnEnable()
        {
            EventBus.Subscribe<CardAbilityRequestEvent>(HandleCardAbilityRequest);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CardAbilityRequestEvent>(HandleCardAbilityRequest);
        }
        
        
        private void HandleCardAbilityRequest(CardAbilityRequestEvent evt)
        {
            ExecuteCardAbilityAsync(evt);
        }

        private async void ExecuteCardAbilityAsync(CardAbilityRequestEvent evt)
        {
            var agent = evt.Card;
            var abilities = agent.Data.cardAbilityIDs;
            
            // 카드의 효과 리스트 처리
            foreach (var ability in abilities.Select(abilityString => GameManager.Inst.Data.GetAbility(abilityString)))
            {
                    await UseAbility(ability, agent, evt.TargetSlot?.CardInstance);
            }
        }
        
        /// <summary>
        /// abilityOwner는 카드 사용 주체
        /// target은 일반카드에서만 설정
        /// </summary>
        /// <param name="ability"></param>
        /// <param name="abilityOwner"></param>
        /// <param name="target"></param>
        private async UniTask UseAbility(CardAbilityBase ability, CardInstance abilityOwner, CardInstance target = null)
        {
            var targets = FindTarget(ability, abilityOwner, target);
            var actionA = ability.actionListA;
            
            // 카드 효과 내부의 액션들 처리
            foreach (var action in actionA)
            {
                var actionValue = action.ActionValue == -1 ? 
                    GetActionValue(abilityOwner, target, ability.actionAValueType)
                    : action.ActionValue;
                
                await ExecuteAction(action.ActionType, actionValue, targets, ability.actionAString);
            }
            
            
            // TODO : condition이랑 actionB는 나중에 만들거임
            var condition = ability.condition;
        }

        private int GetActionValue(CardInstance abilityOwner, CardInstance target, ActionValueType actionValueType)
        {
            var result = -1;
            if (abilityOwner == null) return result;
            
            switch (actionValueType)
            {
                case ActionValueType.ATK:
                    result = abilityOwner.CurrentATK;
                    break;
                case ActionValueType.TargetATK:
                    if (target == null) return result;
                    result = target.CurrentATK;
                    break;
                case ActionValueType.BeforeDamage:
                    result = BattleManager.Instance.BeforeDamage;
                    break;

                case ActionValueType.SelfHP:
                    result = abilityOwner.CurrentHp;
                    break;
                case ActionValueType.TargetHP:
                    result = target.CurrentHp;
                    break;
                case ActionValueType.SelfShield:
                    result = abilityOwner.CurrentShield;
                    break;
                case ActionValueType.TargetShield:
                    result = target.CurrentShield;
                    break;
                
                case ActionValueType.SelfBurn:
                    break;
                case ActionValueType.TargetBurn:
                    break;
                
                case ActionValueType.None:
                case ActionValueType.ActionString:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(actionValueType), actionValueType, null);
            }
                

            return result;
        }
        
        #region Target

        private List<CardInstance> FindTarget(CardAbilityBase ability, CardInstance agent, CardInstance target = null)
        {
            var result = new List<CardInstance>();
            switch (ability.actionTarget)
            {
                case ActionTarget.Self:
                    result.Add(agent);
                    break;
                case ActionTarget.FrontEnemy:
                    result = BattleManager.Instance.GetFieldCard(ActualActionTarget.Front, GetOpposite(agent.CardOwner));
                    break;
                case ActionTarget.RandomEnemy:
                    result = BattleManager.Instance.GetFieldCard(ActualActionTarget.Random, GetOpposite(agent.CardOwner));
                    break;
                case ActionTarget.BackEnemy:
                    result = BattleManager.Instance.GetFieldCard(ActualActionTarget.Back, GetOpposite(agent.CardOwner));
                    break;
                case ActionTarget.AllAlly:
                    result = BattleManager.Instance.GetFieldCard(ActualActionTarget.All, agent.CardOwner);
                    break;
                case ActionTarget.RandomAlly:
                    result = BattleManager.Instance.GetFieldCard(ActualActionTarget.Random, agent.CardOwner);
                    break;
                case ActionTarget.FrontAllAlly:
                    result = BattleManager.Instance.GetFrontCards(agent);
                    break;
                case ActionTarget.NearAlly:
                    result = BattleManager.Instance.GetNearCards(agent);
                    break;
                case ActionTarget.FrontSingleAlly:
                    result.Add(BattleManager.Instance.GetFrontCard(agent));
                    break;
                case ActionTarget.AllEnemy:
                    result = BattleManager.Instance.GetFieldCard(ActualActionTarget.All, GetOpposite(agent.CardOwner));
                    break;
                case ActionTarget.Target:
                    if (target != null)
                        result.Add(target);
                    break;
                case ActionTarget.AllEnemyAlly:
                    result.AddRange(BattleManager.Instance.GetAllFieldCards().Select(card => card.CardInstance));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return result;
        }
        private CardOwner GetOpposite(CardOwner owner)
        {
            return owner switch
            {
                CardOwner.Player => CardOwner.Enemy,
                CardOwner.Enemy => CardOwner.Player,
                _ => throw new ArgumentOutOfRangeException(nameof(owner), owner, null)
            };
        }

        #endregion

        /// <summary>
        /// 여기서 연출도 다뤄줘야함
        /// </summary>
        /// <param name="actionValue"></param>
        /// <param name="targets"></param>
        /// <param name="actionType"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private async UniTask ExecuteAction(ActionType actionType, int actionValue, List<CardInstance> targets, string actionString)
        {
            if (targets.Count <= 0)
            {
                Debug.Log("타겟이 없음");
                return;
            }
            Debug.Log(targets.Count);
            switch (actionType)
            {
                case ActionType.Damage:
                    foreach (var target in targets)
                    {
                        target?.TakeDamage(actionValue);
                    }
                    break;
                case ActionType.Heal:
                    foreach (var target in targets)
                    {
                        target?.ChangeCardInstanceValue(CardInstanceValueType.C_HP, actionValue);
                    }
                    break;
                case ActionType.IncreaseShield:
                    foreach (var target in targets)
                    {
                        target?.ChangeCardInstanceValue(CardInstanceValueType.C_SHD, actionValue);
                    }
                    break;
                case ActionType.IncreaseATK:
                    foreach (var target in targets)
                    {
                        target?.ChangeCardInstanceValue(CardInstanceValueType.I_ATK, actionValue);
                    }
                    break;
                case ActionType.DecreaseATK:
                    foreach (var target in targets)
                    {
                        target?.ChangeCardInstanceValue(CardInstanceValueType.I_ATK, -actionValue);
                    }
                    break;
                case ActionType.DecreaseActionCount:
                    foreach (var target in targets)
                    {
                        target?.ChangeCardInstanceValue(CardInstanceValueType.C_AC, -actionValue);
                    }
                    break;
                case ActionType.IncreaseActionCount:
                    foreach (var target in targets)
                    {
                        target?.ChangeCardInstanceValue(CardInstanceValueType.C_AC, actionValue);
                    }
                    break;
                case ActionType.CreateCardToHand:
                    var creationToHand = GameManager.Inst.Data.GetCard(actionString);
                    if (creationToHand == null) return;
                    await BattleManager.Instance.CreateToHandCard(creationToHand);
                    break;
                case ActionType.CreateCardToField:
                    var creationToField = GameManager.Inst.Data.GetCard(actionString);
                    if (creationToField == null) return;
                    
                    break;
                case ActionType.BurnATK:
                    break;
                case ActionType.BurnBySelfBurn:
                    break;
                case ActionType.Bloodrage:
                    break;
                case ActionType.IncreaseNum:
                case ActionType.DecreaseNum:
                case ActionType.Burn:
                    break;
                case ActionType.BurnDouble:
                case ActionType.BurnDMG:
                    break;
                case ActionType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            BattleManager.Instance.RefreshAllFieldCards();
        }

    }
}
