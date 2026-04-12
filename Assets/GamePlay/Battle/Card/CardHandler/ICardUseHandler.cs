using Cysharp.Threading.Tasks;
using GamePlay.Battle.Field;
using GameSystem.Enums;

namespace GamePlay.Battle.Card.CardHandler
{
    public interface ICardUseHandler
    {
        CardType CardType { get; }

        void BeginSelection(CardUseManager manager, CardObject card);
        void UpdateSelection(CardUseManager manager, CardObject card);
        bool CanResolve(CardUseManager manager, CardObject card, FieldSlot slot);
        UniTask Resolve(CardUseManager manager, CardObject card, FieldSlot slot, int selectionVersion);
        UniTask ReturnToOrigin(CardUseManager manager, CardObject card);
        void EndSelection(CardUseManager manager, CardObject card);
    }
}