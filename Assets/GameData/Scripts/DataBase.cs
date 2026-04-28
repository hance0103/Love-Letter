using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameData.Scripts
{
    [Serializable]
    [CreateAssetMenu(fileName = "DataBase", menuName = "Card/DataBase")]
    public class DataBase : ScriptableObject
    {
        public List<CardBase> allCards = new();
        public List<CardAbilityBase> allAbilities = new();
        public List<RoomBase> rooms = new();
        public List<WaveBase> waves = new();
        public List<CardBase> fateCards = new();
        public List<StringBase> strings = new();
        public List<TooltipBase> tooltips = new();
        
        private Dictionary<string, CardBase> _cardDataDict;
        private Dictionary<string, CardAbilityBase> _abilityDataDict;
        private Dictionary<string, RoomBase> _roomDataDict;
        private Dictionary<string, WaveBase> _waveDataDict;
        private Dictionary<string, CardBase> _fateDataDict;
        private Dictionary<string, StringBase> _stringDataDict;
        private Dictionary<string, TooltipBase> _tooltipDataDict;
        
        public void Init()
        {
            // 딕셔너리 구축
            _cardDataDict = new Dictionary<string, CardBase>();
            _abilityDataDict = new Dictionary<string, CardAbilityBase>();
            
            foreach (var card in allCards)
            {
                _cardDataDict[card.cardID] = card;
            }
            foreach (var ability in allAbilities)
            {
                _abilityDataDict[ability.abilityID] = ability;
            }
            foreach (var room in rooms)
            {
                _roomDataDict[room.name] = room;
            }
            foreach (var wave in waves)
            {
                _waveDataDict[wave.name] = wave;
            }
            foreach (var fate in fateCards)
            {
                _fateDataDict[fate.name] = fate;
            }
            foreach (var stringData in strings)
            {
                _stringDataDict[stringData.name] = stringData;
            }

            foreach (var tooltipData in tooltips)
            {
                _tooltipDataDict[tooltipData.name] = tooltipData;
            }
        }

        public CardBase GetCard(string cardID)
        {
            return _cardDataDict.GetValueOrDefault(cardID);
        }

        public CardAbilityBase GetAbility(string abilityID)
        {
            return _abilityDataDict.GetValueOrDefault(abilityID);
        }

        public RoomBase GetRoom(string roomID)
        {
            return _roomDataDict.GetValueOrDefault(roomID);
        }

        public WaveBase GetWave(string waveID)
        {
            return _waveDataDict.GetValueOrDefault(waveID);
        }

        public CardBase GetFate(string fateID)
        {
            return _fateDataDict.GetValueOrDefault(fateID);
        }

        public StringBase GetString(string stringID)
        {
            return _stringDataDict.GetValueOrDefault(stringID);
        }

        public TooltipBase GetTooltip(string tooltipID)
        {
            return _tooltipDataDict.GetValueOrDefault(tooltipID);
        }
    }
}
