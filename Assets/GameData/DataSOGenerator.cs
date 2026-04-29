using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameData.Scripts;
using GameSystem.Enums;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GameData
{
    public static class DataSOGenerator
    {
        private static string _cardData;
        private static string _cardAbilityData;
        
        private const string CardDataAddress = "CardData";
        private const string CardAbilityDataAddress = "CardAbilityData";
        private const string RoomDataAddress = "RoomData";
        private const string WaveDataAddress = "WaveData";
        private const string FateCardDataAddress = "FateCardData";
        private const string StringDataAddress = "StringData";
        private const string TooltipDataAddress = "TooltipData";
        
        private const string DataBaseAssetPath = "Assets/GameData/SO/CardDataBase.asset";
        private const string DataBaseAddressKey = "CardDataBase";
    
        #if UNITY_EDITOR
        [MenuItem("Data/Create Data SO")]
        public static async void CreateDataSO()
        {
            try
            {
                // SO 에셋 생성
                await CreateData();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                        
                Debug.Log("CSV 파싱 완료");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

        }
    
    
        private static async UniTask CreateData()
        {
            // 데이터베이스 SO 생성
            var dataBase = ScriptableObject.CreateInstance<DataBase>();
            dataBase.ClearAll();
            if (AssetDatabase.LoadAssetAtPath<DataBase>(DataBaseAssetPath) != null)
            {
                AssetDatabase.DeleteAsset(DataBaseAssetPath);
            }
            AssetDatabase.CreateAsset(dataBase, DataBaseAssetPath);
            EditorUtility.SetDirty(dataBase);

            // 생성 직후 저장/갱신
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 바로 어드레서블 등록
            RegisterToAddressables(DataBaseAssetPath, DataBaseAddressKey);
            
            await CreateCardData(dataBase);
            Debug.Log("Data Generator : 카드 데이터 파싱 완료");
            await CreateCardAbilityData(dataBase);
            Debug.Log("Data Generator : 카드 효과 파싱 완료");
            await CreateRoomData(dataBase);
            Debug.Log("Data Generator : 방 데이터 파싱 완료");
            await CreateWaveData(dataBase);
            Debug.Log("Data Generator : 웨이브 데이터 파싱 완료");
            await CreateFateCardData(dataBase);
            Debug.Log("Data Generator : 운명 카드 데이터 파싱 완료");
            await CreateStringData(dataBase);
            Debug.Log("Data Generator : 스트링 데이터 파싱 완료");
            // await CreateTooltipData(dataBase);
            // Debug.Log("Data Generator : 툴팁 데이터 파싱 완료");
            
            EditorUtility.SetDirty(dataBase);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        private static void RegisterToAddressables(string assetPath, string addressKey)
        {
            if (!AddressableAssetSettingsDefaultObject.SettingsExists)
            {
                Debug.LogError("Addressables Settings가 없습니다. 먼저 Addressables를 세팅해주세요.");
                return;
            }

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("Addressables Settings를 가져오지 못했습니다.");
                return;
            }

            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogError($"GUID를 찾을 수 없습니다: {assetPath}");
                return;
            }

            var group = settings.DefaultGroup;
            if (group == null)
            {
                Debug.LogError("Default Addressables Group이 없습니다.");
                return;
            }

            var entry = settings.CreateOrMoveEntry(guid, group, false, true);
            entry.SetAddress(addressKey, true);

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();

            Debug.Log($"Addressables 등록 완료: {assetPath} -> {addressKey}");
        }

        #region CheckChanged

        private static bool SetIfChanged<T>(ref T field, T newValue)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
                return false;

            field = newValue;
            return true;
        }
        private static bool SetListIfChanged<T>(List<T> target, List<T> source)
        {
            if (target.SequenceEqual(source))
                return false;

            target.Clear();
            target.AddRange(source);
            return true;
        }
        private static bool SetEquatableListIfChanged<T>(ref List<T> target, List<T> source) where T : IEquatable<T>
        {
            if (target != null && source != null && target.Count == source.Count &&
                target.SequenceEqual(source)) return false;
            if (target == null)
            {
                if (source != null) target = new List<T>(source);
            }
            else
            {
                target.Clear();
                if (source != null) target.AddRange(source);
            }
            return true;
        }

        #endregion

        #region CreateData
        private static async UniTask CreateCardData(DataBase dataBase)
        {
            // 카드 데이터 csv파일 로드
            var cardDataHandle = Addressables.LoadAssetAsync<TextAsset>(CardDataAddress);
            await cardDataHandle;
            if (cardDataHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"{CardDataAddress} 로드 실패");
                return;
            }
            
            var csvCardData = cardDataHandle.Result.text;
            Addressables.Release(cardDataHandle);
            var cardDataSet = CsvParser.ParseCSV(csvCardData);
            
            
            for (var i = 1; i < cardDataSet.Length; i++)
            {
                try
                {
                    var dataRow = cardDataSet[i].Split(',');
                    // 경로 체크
                    var cardSOPath = $"Assets/GameData/SO/CardData/{dataRow[2]}";
                    if (!AssetDatabase.IsValidFolder(cardSOPath))
                    {
                        AssetDatabase.CreateFolder("Assets/GameData/SO/CardData", dataRow[2]);
                    }
                    cardSOPath += $"/{dataRow[0]}.asset";
                    // 기존에 동일한 에셋이 있는지 체크
                    var existing = AssetDatabase.LoadAssetAtPath<CardBase>(cardSOPath);
                    CardBase data;
                    // 동일한 이름의 에셋이 이미 존재함
                    if (existing != null)
                    {
                        data = existing;

                        data.keywords ??= new List<KeywordType>();
                        data.cardAbilityIDs ??= new List<string>();
                        data.nextPromotionIDs ??= new List<string>();
                        data.tooltipIDs ??= new List<string>();
                        
                        var isDirty = false;
                        
                        // 1. ID
                        isDirty |= SetIfChanged(ref data.cardID, dataRow[0]);
                        // 2. 등급
                        if (Enum.TryParse<CardTier>(dataRow[1], true, out var tier))
                            isDirty |= SetIfChanged(ref data.cardTier, tier);
                        else
                        {
                            Debug.Log("Data Generator : 카드 등급 파싱 실패");
                            isDirty |= SetIfChanged(ref data.cardTier, CardTier.None);
                        }
                        // 3. 종류
                        if (Enum.TryParse<CardType>(dataRow[2], true, out var cardType))
                            isDirty |= SetIfChanged(ref data.cardType, cardType);
                        else
                        {
                            Debug.Log("Data Generator : 카드 종류 파싱 실패");
                            isDirty |= SetIfChanged(ref data.cardType, CardType.None);
                        }
                        // 4. 키워드
                        var keywordStringArray = CsvParser.ParseArray(dataRow[3]);
                        var newKeywordList = new List<KeywordType>();
                        foreach (var keyword in keywordStringArray)
                        {
                            if (string.IsNullOrWhiteSpace(keyword) || keyword == "none")
                                break;
                            if (Enum.TryParse<KeywordType>(keyword, true, out var keywordType))
                                newKeywordList.Add(keywordType);
                            else
                            {
                                Debug.Log("Data Generator : 카드 키워드 설정 오류");
                                break;
                            }
                        }
                        isDirty |= SetListIfChanged(data.keywords, newKeywordList);
                        // 5. 카드 번호
                        int.TryParse(dataRow[4], out var cardNum);
                        isDirty |= SetIfChanged(ref data.cardNum, cardNum);
                        // 6. 체력
                        int.TryParse(dataRow[5], out var hp);
                        isDirty |= SetIfChanged(ref data.HP, hp);
                        // 7. 공격력
                        int.TryParse(dataRow[6], out var atk);
                        isDirty |= SetIfChanged(ref data.ATK, atk);
                        // 8. 행동카운트
                        int.TryParse(dataRow[7], out var ac);
                        isDirty |= SetIfChanged(ref data.actionCount, ac);
                        // 9. Card Ability
                        var cardAbilityArray = CsvParser.ParseArray(dataRow[8]);
                        var newAbilityList = new List<string>(cardAbilityArray);
                        isDirty |= SetListIfChanged(data.cardAbilityIDs, newAbilityList);
                        // 10. 전직
                        var nextPromotionArray = CsvParser.ParseArray(dataRow[9]);
                        var newPromotionList = new List<string>(nextPromotionArray);
                        isDirty |= SetListIfChanged(data.nextPromotionIDs, newPromotionList);
                        // 11. 카드 이름
                        isDirty |= SetIfChanged(ref data.nameString, dataRow[10]);
                        // 12. 카드 설명
                        isDirty |= SetIfChanged(ref data.descString, dataRow[11]);
                        // 13. 일러스트 경로
                        isDirty |= SetIfChanged(ref data.imgPath, dataRow[12]);
                        // 14. 툴팁
                        var tooltips = CsvParser.ParseArray(dataRow[13]);
                        var newTooltipList = new List<string>(tooltips);
                        isDirty |= SetListIfChanged(data.tooltipIDs, newTooltipList);
                        // 15 배경 경로 
                        isDirty |= SetIfChanged(ref data.backgroundPath, dataRow[14]);
                        
                        
                        if (isDirty)
                        {
                            Debug.Log($"{data.cardID} SO 변경");
                            EditorUtility.SetDirty(data);
                        }
                    }
                    else
                    {
                        data = ScriptableObject.CreateInstance<CardBase>();
                        
                        // 1. ID
                        data.cardID = dataRow[0];
                        // 2. 등급
                        if (Enum.TryParse<CardTier>(dataRow[1], true, out var tier))
                        {
                            data.cardTier = tier;
                        }
                        else
                        {
                            Debug.Log("Data Generator : 카드 등급 파싱 실패");
                            data.cardTier = 0;
                        }
                        // 3. 종류
                        if (Enum.TryParse<CardType>(dataRow[2], true, out var cardType))
                        {
                            data.cardType = cardType;
                        }
                        else
                        {
                            Debug.Log("Data Generator : 카드 종류 파싱 실패");
                            data.cardType = 0;
                        }
                        // 4. 키워드
                        var keywordStringArray = CsvParser.ParseArray(dataRow[3]);
                        foreach (var keyword in keywordStringArray)
                        {
                            if (string.IsNullOrWhiteSpace(keyword) || keyword == "none")
                                break;
                            
                            if (Enum.TryParse<KeywordType>(keyword, true, out var keywordType))
                            {
                                data.keywords.Add(keywordType);
                            }
                            else
                            {
                                Debug.Log("Data Generator : 카드 키워드 설정 오류");
                                break;
                            }
                        }
                        // 5. 카드 번호
                        int.TryParse(dataRow[4], out data.cardNum);
                        // 6. 체력
                        int.TryParse(dataRow[5], out data.HP);
                        // 7. 공격력
                        int.TryParse(dataRow[6], out data.ATK);
                        // 8. 행동카운트
                        int.TryParse(dataRow[7], out data.actionCount);
                        // 9. Card Ability
                        var cardAbilityArray = CsvParser.ParseArray(dataRow[8]);
                        data.cardAbilityIDs = new List<string>(cardAbilityArray);
                        // 10. 전직
                        var nextPromotionArray = CsvParser.ParseArray(dataRow[9]);
                        data.nextPromotionIDs = new List<string>(nextPromotionArray);
                        // 11. 카드 이름
                        data.nameString = dataRow[10];
                        // 12. 카드 설명
                        data.descString = dataRow[11];
                        // 13. 일러스트 경로
                        data.imgPath = dataRow[12];
                        // 14. 툴팁
                        var tooltips = CsvParser.ParseArray(dataRow[13]);
                        data.tooltipIDs = new List<string>(tooltips);
                        // 15 배경 경로
                        data.backgroundPath = dataRow[14];
                        
                        AssetDatabase.CreateAsset(data, cardSOPath);
                        EditorUtility.SetDirty(data);
                    }
                    if (!dataBase.allCards.Contains(data))
                        dataBase.allCards.Add(data);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    throw;
                }
            }
        }
        private static async UniTask CreateCardAbilityData(DataBase dataBase)
        {
            // 카드 능력 데이터 csv파일 로드
            var cardAbilityDataHandle =  Addressables.LoadAssetAsync<TextAsset>(CardAbilityDataAddress);
            await cardAbilityDataHandle;
            if (cardAbilityDataHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"{CardAbilityDataAddress} 로드 실패");
                return;
            }
            var csvAbilityData = cardAbilityDataHandle.Result.text;
            Addressables.Release(cardAbilityDataHandle);
            var abilityDataSet = CsvParser.ParseCSV(csvAbilityData);

            for (var i = 1; i < abilityDataSet.Length; i++)
            {
                try
                {
                    var dataRow = abilityDataSet[i].Split(',');
                    var cardAbilitySOPath = $"Assets/GameData/SO/CardData/CardAbilityData/{dataRow[0]}.asset";
                    var existing = AssetDatabase.LoadAssetAtPath<CardAbilityBase>(cardAbilitySOPath);
                    CardAbilityBase data;

                    if (existing != null)
                    {
                        data = existing;
                        
                        // 리스트 널 체크
                        data.actionListA ??= new List<AbilityActionSet>();
                        data.condition ??= new ConditionSet();
                        data.actionListB ??= new List<AbilityActionSet>();
                        
                        var isDirty = false;
                        
                        // 1. ID
                        isDirty |= SetIfChanged(ref data.abilityID, dataRow[0]);
                        // 2. ActionTarget
                        if (Enum.TryParse<ActionTarget>(dataRow[1], true, out var actionTarget))
                        {
                            isDirty |= SetIfChanged(ref data.actionTarget, actionTarget);
                        }
                        else
                        {
                            Debug.Log($"Data Generator : 카드 {dataRow[0]} 타겟 파싱 실패 : {dataRow[1]} 존재하지 않음");
                            isDirty |= SetIfChanged(ref data.actionTarget, ActionTarget.None);
                        }
                        // 3. ActionList_A
                        var actionArrayA = CsvParser.ParseArray(dataRow[2]);
                        var actionValueArrayA = CsvParser.ParseArray(dataRow[3]);
                        
                        var newActionListA = actionArrayA.Select((t, j) =>
                        {
                            if (!Enum.TryParse<ActionType>(t, true, out var actionType))
                                return new AbilityActionSet();
                            var value = (j < actionValueArrayA.Length && int.TryParse(actionValueArrayA[j], out var v))
                                ? v
                                : 0;
                            return new AbilityActionSet(actionType, value);
                        }).ToList();
                        
                        isDirty |= SetEquatableListIfChanged(ref data.actionListA, newActionListA);
                        
                        
                        // 4. ConditionSet
                        if (Enum.TryParse<ConditionType>(dataRow[4], true, out var conditionType))
                        {
                            var conditionSet = new ConditionSet(conditionType,  int.Parse(dataRow[5]));
                            if (data.condition == null || !data.condition.Equals(conditionSet))
                            {
                                data.condition = conditionSet;
                                isDirty = true;
                            }
                        }
                        else
                        {
                            Debug.Log($"Data Generator : 효과 {data.abilityID} 조건 파싱 실패 : {dataRow[4]} 없음");
                            var defaultCondition = new ConditionSet();
                            isDirty = SetIfChanged(ref data.condition, defaultCondition);
                        }
                        
                        // 5. ActionList_B
                        var actionArrayB = CsvParser.ParseArray(dataRow[6]);
                        var actionValueArrayB = CsvParser.ParseArray(dataRow[7]);

                        var newActionListB = actionArrayB.Select((t, j) =>
                            {
                                if (!Enum.TryParse<ActionType>(t, true, out var actionType))
                                    return new AbilityActionSet();
                                var value = (j < actionValueArrayB.Length &&
                                             int.TryParse(actionValueArrayB[j], out var v))
                                    ? v
                                    : 0;
                                return new AbilityActionSet(actionType, value);
                            }).ToList();
                        isDirty |= SetEquatableListIfChanged(ref data.actionListB, newActionListB);
                        
                        // 6. Action A String
                        isDirty |= SetIfChanged(ref data.actionAString, dataRow[8]);
                        // 7. Action B String
                        isDirty |= SetIfChanged(ref data.actionBString, dataRow[9]);
                        
                        // 8. ActionA Value Type
                        if (Enum.TryParse<ActionValueType>(dataRow[10], true, out var actionAValueType))
                        {
                            isDirty |= SetIfChanged(ref data.actionAValueType, actionAValueType);
                        }
                        else
                        {
                            Debug.Log($"Data Generator : 카드 {dataRow[0]} 행동 값 타입 파싱 실패 : {dataRow[10]} 존재하지 않음");
                            isDirty |= SetIfChanged(ref data.actionAValueType, ActionValueType.None);
                        }
                        
                        // 9. ActionB Value Type
                        if (Enum.TryParse<ActionValueType>(dataRow[11], true, out var actionBValueType))
                        {
                            isDirty |= SetIfChanged(ref data.actionBValueType, actionBValueType);
                        }
                        else
                        {
                            Debug.Log($"Data Generator : 카드 {dataRow[0]} 행동 값 타입 파싱 실패 : {dataRow[11]} 존재하지 않음");
                            isDirty |= SetIfChanged(ref data.actionBValueType, ActionValueType.None);
                        }
                        
                        if (isDirty)
                        {
                            Debug.Log($"{data.abilityID} SO 변경");
                            EditorUtility.SetDirty(data);
                        }
                    }
                    else
                    {
                        data = ScriptableObject.CreateInstance<CardAbilityBase>();
                        // 1. ID
                        data.abilityID = dataRow[0];
                        // 2. ActionTarget
                        if (Enum.TryParse<ActionTarget>(dataRow[1], true, out var actionTarget))
                        {
                            data.actionTarget = actionTarget;
                        }
                        else
                        {
                            Debug.Log($"Data Generator : 카드 {dataRow[0]} 타겟 설정 오류 : {dataRow[1]} 존재하지 않음");
                            data.actionTarget = 0;
                        }
                        // 3. ActionList_A
                        var actionArrayA = CsvParser.ParseArray(dataRow[2]);
                        var actionValueArrayA = CsvParser.ParseArray(dataRow[3]);
                        
                        for (var j = 0; j < actionArrayA.Length; j++)
                        {
                            if (Enum.TryParse<ActionType>(actionArrayA[j], true, out var actionType))
                            {
                                var value = (j < actionValueArrayA.Length &&
                                             int.TryParse(actionValueArrayA[j], out var v))
                                    ? v
                                    : 0;
                                data.actionListA.Add((new AbilityActionSet(actionType, value)));
                            }
                            else
                            {
                                data.actionListA.Add(new AbilityActionSet());
                            }

                        }
                        // 4. ConditionSet
                        if (Enum.TryParse<ConditionType>(dataRow[4], true, out var conditionType))
                        {
                            int.TryParse(dataRow[5], out var value);
                            data.condition = new ConditionSet(conditionType, value);
                        }
                        else
                        {
                            data.condition = new ConditionSet();
                        }
                        // 5. ActionList_B
                        var actionArrayB = CsvParser.ParseArray(dataRow[6]);
                        var actionValueArrayB = CsvParser.ParseArray(dataRow[7]);
                        
                        for (var j = 0; j < actionArrayB.Length; j++)
                        {
                            if (Enum.TryParse<ActionType>(actionArrayB[j], true, out var actionType))
                            {
                                var value = (j < actionValueArrayB.Length && int.TryParse(actionValueArrayB[j], out var v))
                                    ? v
                                    : 0;

                                data.actionListB.Add(new AbilityActionSet(actionType, value));
                            }
                            else
                            {
                                data.actionListB.Add(new AbilityActionSet());
                            }

                        }
                        data.actionAString = dataRow[8];
                        data.actionBString = dataRow[9];
                        
                        // 6. ActionA Value Type
                        if (Enum.TryParse<ActionValueType>(dataRow[10], true, out var actionAValueType))
                        {
                            data.actionAValueType = actionAValueType;
                        }
                        else
                        {
                            Debug.Log($"Data Generator : 카드 {dataRow[0]} 행동A 값 타입 설정 오류 : {dataRow[10]} 존재하지 않음");
                            data.actionAValueType = 0;
                        }
                        
                        // 7. ActionB Value Type
                        if (Enum.TryParse<ActionValueType>(dataRow[10], true, out var actionBValueType))
                        {
                            data.actionBValueType = actionBValueType;
                        }
                        else
                        {
                            Debug.Log($"Data Generator : 카드 {dataRow[0]} 행동B 값 타입 설정 오류 : {dataRow[11]} 존재하지 않음");
                            data.actionBValueType = 0;
                        }
                        
                        AssetDatabase.CreateAsset(data, cardAbilitySOPath);
                        EditorUtility.SetDirty(data);
                    }
                    if (!dataBase.allAbilities.Contains(data))
                        dataBase.allAbilities.Add(data);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    throw;
                }
            }
        }
        
        private static async UniTask CreateRoomData(DataBase dataBase)
        {
            // csv파일 로드
            var roomDataHandle = Addressables.LoadAssetAsync<TextAsset>(RoomDataAddress);
            await roomDataHandle;
            if (roomDataHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"{RoomDataAddress} 로드 실패");
                return;
            }
            
            var csvRoomData = roomDataHandle.Result.text;
            Addressables.Release(roomDataHandle);
            var roomDataSet = CsvParser.ParseCSV(csvRoomData);
            
            for (var i = 1; i < roomDataSet.Length; i++)
            {
                try
                {
                    var dataRow = roomDataSet[i].Split(',');
                    var roomSOPath = $"Assets/GameData/SO/RoomData/{dataRow[0]}.asset";
                    var existing = AssetDatabase.LoadAssetAtPath<RoomBase>(roomSOPath);
                    RoomBase data;
                    if (existing != null)
                    {
                        data = existing;
                        var isDirty = false;
                        
                        // 1. ID
                        isDirty |= SetIfChanged(ref data.id, dataRow[0]);
                        // 2. floor
                        int.TryParse(dataRow[1], out var floor);
                        isDirty |= SetIfChanged(ref data.floor, floor);
                        // 3. room
                        int.TryParse(dataRow[2], out var room);
                        isDirty |= SetIfChanged(ref data.room, room);
                        // 4. level
                        int.TryParse(dataRow[3], out var level);
                        isDirty |= SetIfChanged(ref data.level, level);
                        // 5. tier
                        if (Enum.TryParse<EncounterTier>(dataRow[4], true, out var tier))
                        {
                            isDirty |= SetIfChanged(ref data.tier, tier);
                        }
                        else
                        {
                            Debug.Log($"Data Generator : 방 {data.id} 티어 파싱 실패");
                            isDirty |= SetIfChanged(ref data.tier, EncounterTier.Normal);
                        }
                        // 6. ClearGold
                        int.TryParse(dataRow[5], out var clearGoldMin);
                        isDirty |= SetIfChanged(ref data.clearGoldMin, clearGoldMin);
                        int.TryParse(dataRow[6], out var clearGoldMax);
                        isDirty |= SetIfChanged(ref data.clearGoldMax, clearGoldMax);
                        
                        if (isDirty)
                        {
                            Debug.Log($"{data.id} SO 변경");
                            EditorUtility.SetDirty(data);
                        }
                    }
                    else
                    {
                        data = ScriptableObject.CreateInstance<RoomBase>();
                        // 1. ID
                        data.id = dataRow[0];
                    
                        // 2. floor
                        int.TryParse(dataRow[1], out data.floor);
                        // 3. room
                        int.TryParse(dataRow[2], out data.room);
                        // 4. level
                        int.TryParse(dataRow[3], out data.level);
                        // 5. tier
                        if (Enum.TryParse<EncounterTier>(dataRow[4], true, out var tier))
                        {
                            data.tier = tier;
                        }
                        else
                        {
                            Debug.Log("Data Generator : 카드 종류 설정 오류");
                            data.tier = EncounterTier.Normal;
                        }
                        // 6. ClearGold
                        int.TryParse(dataRow[5], out data.clearGoldMin);
                        int.TryParse(dataRow[6], out data.clearGoldMax);
                    
                    
                        AssetDatabase.CreateAsset(data, roomSOPath);
                        EditorUtility.SetDirty(data);
                    }
                    if (!dataBase.rooms.Contains(data))
                        dataBase.rooms.Add(data);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    throw;
                }
            }
        }

        private static async UniTask CreateWaveData(DataBase dataBase)
        {
            // csv파일 로드
            var waveDataHandle = Addressables.LoadAssetAsync<TextAsset>(WaveDataAddress);
            await waveDataHandle;
            if (waveDataHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"{WaveDataAddress} 로드 실패");
                return;
            }
            
            var csvWaveData = waveDataHandle.Result.text;
            Addressables.Release(waveDataHandle);
            var waveDataSet = CsvParser.ParseCSV(csvWaveData);
            
            
            for (var i = 1; i < waveDataSet.Length; i++)
            {
                try
                {
                    var dataRow = waveDataSet[i].Split(',');
                    var waveSOPath = $"Assets/GameData/SO/WaveData/{dataRow[0]}.asset";
                    var existing = AssetDatabase.LoadAssetAtPath<WaveBase>(waveSOPath);
                    WaveBase data;

                    if (existing != null)
                    {
                        data = existing;

                        data.enemyID ??= new List<string>();
                        data.waveEnemyCount ??= new List<int>();
                        data.turnsToNextWave ??= new List<int>();

                        var isDirty = false;
                        // 1. ID
                        isDirty |= SetIfChanged(ref data.id, dataRow[0]);
                        // 2. level
                        int.TryParse(dataRow[1], out var level);
                        isDirty |= SetIfChanged(ref data.level, level);
                        // 3. enemyIDs
                        var enemyIDs = CsvParser.ParseArray(dataRow[2]);
                        var newEnemyIDs = new List<string>(enemyIDs);
                        isDirty |= SetListIfChanged(data.enemyID, newEnemyIDs);
                        // 4. waveEnemyCount
                        var waveEnemyCount = CsvParser.ParseIntArray(dataRow[3]);
                        var newEnemyCountList = new List<int>(waveEnemyCount);
                        isDirty |= SetListIfChanged(data.waveEnemyCount, newEnemyCountList);
                        // 5. turnsToNextWave
                        var turnsToNextWave = CsvParser.ParseIntArray(dataRow[4]);
                        var newTurnsToNextWaveList = new List<int>(turnsToNextWave);
                        isDirty |= SetListIfChanged(data.turnsToNextWave, newTurnsToNextWaveList);

                        if (isDirty)
                        {
                            Debug.Log($"{data.id} SO 변경");
                            EditorUtility.SetDirty(data);
                        }
                    }
                    else
                    {
                        data = ScriptableObject.CreateInstance<WaveBase>();
                        
                        // 1. ID
                        data.id = dataRow[0];
                        // 2. level
                        int.TryParse(dataRow[1], out data.level);
                        // 3. enemyIDs
                        var enemyIDs = CsvParser.ParseArray(dataRow[2]);
                        data.enemyID = new List<string>(enemyIDs);
                        // 4. waveEnemyCount
                        var waveEnemyCount = CsvParser.ParseIntArray(dataRow[3]);
                        data.waveEnemyCount = new List<int>(waveEnemyCount);
                        // 5. turnsToNextWave
                        var turnsToNextWave = CsvParser.ParseIntArray(dataRow[4]);
                        data.turnsToNextWave = new List<int>(turnsToNextWave);
                        
                        AssetDatabase.CreateAsset(data, waveSOPath);
                        EditorUtility.SetDirty(data);
                    }
                    
                    if (!dataBase.waves.Contains(data)) 
                        dataBase.waves.Add(data);

                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    throw;
                }
            }
        }

        private static async UniTask CreateFateCardData(DataBase dataBase)
        {
            // 카드 데이터 csv파일 로드
            var cardDataHandle = Addressables.LoadAssetAsync<TextAsset>(FateCardDataAddress);
            await cardDataHandle;
            if (cardDataHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"{CardDataAddress} 로드 실패");
                return;
            }
            
            var csvCardData = cardDataHandle.Result.text;
            Addressables.Release(cardDataHandle);
            var cardDataSet = CsvParser.ParseCSV(csvCardData);
            
            
            for (var i = 1; i < cardDataSet.Length; i++)
            {
                try
                {
                    var dataRow = cardDataSet[i].Split(',');
                    var cardSOPath = $"Assets/GameData/SO/CardData/FateCardData/{dataRow[0]}.asset";

                    var existing = AssetDatabase.LoadAssetAtPath<CardBase>(cardSOPath);
                    CardBase data;
                    if (existing != null)
                    {
                        data = existing;
                        
                        data.keywords ??= new List<KeywordType>();
                        data.cardAbilityIDs ??= new List<string>();
                        data.nextPromotionIDs ??= new List<string>();
                        data.tooltipIDs ??= new List<string>();
                        
                        var isDirty = false;
                        
                        // 1. ID
                        isDirty |= SetIfChanged(ref data.cardID, dataRow[0]);
                        // 2. 등급
                        if (Enum.TryParse<CardTier>(dataRow[1], true, out var tier))
                            isDirty |= SetIfChanged(ref data.cardTier, tier);
                        else
                        {
                            Debug.Log("Data Generator : 카드 등급 파싱 실패");
                            isDirty |= SetIfChanged(ref data.cardTier, CardTier.None);
                        }
                        // 3. 종류
                        if (Enum.TryParse<CardType>(dataRow[2], true, out var cardType))
                            isDirty |= SetIfChanged(ref data.cardType, cardType);
                        else
                        {
                            Debug.Log("Data Generator : 카드 종류 파싱 실패");
                            isDirty |= SetIfChanged(ref data.cardType, CardType.None);
                        }
                        // 4. 키워드
                        var keywordStringArray = CsvParser.ParseArray(dataRow[3]);
                        var newKeywordList = new List<KeywordType>();
                        foreach (var keyword in keywordStringArray)
                        {
                            if (string.IsNullOrWhiteSpace(keyword) || keyword == "none")
                                break;
                            if (Enum.TryParse<KeywordType>(keyword, true, out var keywordType))
                                newKeywordList.Add(keywordType);
                            else
                            {
                                Debug.Log("Data Generator : 카드 키워드 설정 오류");
                                break;
                            }
                        }
                        isDirty |= SetListIfChanged(data.keywords, newKeywordList);
                        // 5. 카드 번호
                        int.TryParse(dataRow[4], out var cardNum);
                        isDirty |= SetIfChanged(ref data.cardNum, cardNum);
                        // 6. 체력
                        int.TryParse(dataRow[5], out var hp);
                        isDirty |= SetIfChanged(ref data.HP, hp);
                        // 7. 공격력
                        int.TryParse(dataRow[6], out var atk);
                        isDirty |= SetIfChanged(ref data.ATK, atk);
                        // 8. 행동카운트
                        int.TryParse(dataRow[7], out var ac);
                        isDirty |= SetIfChanged(ref data.actionCount, ac);
                        // 9. Card Ability
                        var cardAbilityArray = CsvParser.ParseArray(dataRow[8]);
                        var newAbilityList = new List<string>(cardAbilityArray);
                        isDirty |= SetListIfChanged(data.cardAbilityIDs, newAbilityList);
                        // 10. 전직
                        var nextPromotionArray = CsvParser.ParseArray(dataRow[9]);
                        var newPromotionList = new List<string>(nextPromotionArray);
                        isDirty |= SetListIfChanged(data.nextPromotionIDs, newPromotionList);
                        // 11. 카드 이름
                        isDirty |= SetIfChanged(ref data.nameString, dataRow[10]);
                        // 12. 카드 설명
                        isDirty |= SetIfChanged(ref data.descString, dataRow[11]);
                        // 13. 일러스트 경로
                        isDirty |= SetIfChanged(ref data.imgPath, dataRow[12]);
                        // 14. 툴팁
                        var tooltips = CsvParser.ParseArray(dataRow[13]);
                        var newTooltipList = new List<string>(tooltips);
                        isDirty |= SetListIfChanged(data.tooltipIDs, newTooltipList);
                        // 15 배경 경로 
                        isDirty |= SetIfChanged(ref data.backgroundPath, dataRow[14]);
                        
                        
                        if (isDirty)
                        {
                            Debug.Log($"{data.cardID} SO 변경");
                            EditorUtility.SetDirty(data);
                        }
                    }
                    else
                    {
                        data = ScriptableObject.CreateInstance<CardBase>();
                        // 1. ID
                        data.cardID = dataRow[0];
                        // 2. 등급
                        if (Enum.TryParse<CardTier>(dataRow[1], true, out var tier))
                        {
                            data.cardTier = tier;
                        }
                        else
                        {
                            Debug.Log("Data Generator : 카드 등급 파싱 실패");
                            data.cardTier = 0;
                        }
                        // 3. 종류
                        if (Enum.TryParse<CardType>(dataRow[2], true, out var cardType))
                        {
                            data.cardType = cardType;
                        }
                        else
                        {
                            Debug.Log("Data Generator : 카드 종류 파싱 실패");
                            data.cardType = 0;
                        }
                        // 4. 키워드
                        var keywordStringArray = CsvParser.ParseArray(dataRow[3]);
                        foreach (var keyword in keywordStringArray)
                        {
                            if (string.IsNullOrWhiteSpace(keyword) || keyword == "none")
                                break;
                            
                            if (Enum.TryParse<KeywordType>(keyword, true, out var keywordType))
                            {
                                data.keywords.Add(keywordType);
                            }
                            else
                            {
                                Debug.Log("Data Generator : 카드 키워드 설정 오류");
                                break;
                            }
                        }
                        // 5. 카드 번호
                        int.TryParse(dataRow[4], out data.cardNum);
                        // 6. 체력
                        int.TryParse(dataRow[5], out data.HP);
                        // 7. 공격력
                        int.TryParse(dataRow[6], out data.ATK);
                        // 8. 행동카운트
                        int.TryParse(dataRow[7], out data.actionCount);
                        // 9. Card Ability
                        var cardAbilityArray = CsvParser.ParseArray(dataRow[8]);
                        data.cardAbilityIDs = new List<string>(cardAbilityArray);
                        // 10. 전직
                        var nextPromotionArray = CsvParser.ParseArray(dataRow[9]);
                        data.nextPromotionIDs = new List<string>(nextPromotionArray);
                        // 11. 카드 이름
                        data.nameString = dataRow[10];
                        // 12. 카드 설명
                        data.descString = dataRow[11];
                        // 13. 일러스트 경로
                        data.imgPath = dataRow[12];
                        // 14. 툴팁
                        var tooltips = CsvParser.ParseArray(dataRow[13]);
                        data.tooltipIDs = new List<string>(tooltips);
                        // 15 배경 경로
                        data.backgroundPath = dataRow[14];
                        
                        AssetDatabase.CreateAsset(data, cardSOPath);
                        EditorUtility.SetDirty(data);
                    }
                    if (!dataBase.fateCards.Contains(data))
                        dataBase.fateCards.Add(data);

                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    throw;
                }
            }
        }
        private static async UniTask CreateStringData(DataBase dataBase)
        {
            // csv파일 로드
            var stringDataHandle = Addressables.LoadAssetAsync<TextAsset>(StringDataAddress);
            await stringDataHandle;
            if (stringDataHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"{StringDataAddress} 로드 실패");
                return;
            }
            
            var csvStringData = stringDataHandle.Result.text;
            Addressables.Release(stringDataHandle);
            var stringDataSet = CsvParser.ParseCSV(csvStringData);
            
            
            for (var i = 1; i < stringDataSet.Length; i++)
            {
                try
                {
                    var dataRow = stringDataSet[i].Split(',');
                    var stringSOPath = $"Assets/GameData/SO/StringData/{dataRow[0]}.asset";
                    
                    var existing = AssetDatabase.LoadAssetAtPath<StringBase>(stringSOPath);
                    StringBase data;
                    
                    if (existing != null)
                    {
                        data = existing;
                        var isDirty = false;

                        isDirty |= SetIfChanged(ref data.id, dataRow[0]);
                        isDirty |= SetIfChanged(ref data.kr, dataRow[1]);
                        isDirty |= SetIfChanged(ref data.en, dataRow[2]);

                        if (isDirty)
                        {
                            Debug.Log($"{data.id} SO 변경");
                            EditorUtility.SetDirty(data);
                        }
                    }
                    else
                    {
                        data = ScriptableObject.CreateInstance<StringBase>();
                        
                        data.id = dataRow[0];
                        data.kr = dataRow[1];
                        data.en = dataRow[2];
                        
                        AssetDatabase.CreateAsset(data, stringSOPath);
                        EditorUtility.SetDirty(data);
                    }
                    
                    if (!dataBase.strings.Contains(data))
                        dataBase.strings.Add(data);

                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    throw;
                }
            }
        }

        #endregion


        
        private static async UniTask CreateTooltipData(DataBase dataBase)
        {
            // csv파일 로드
            var tooltipDataHandle = Addressables.LoadAssetAsync<TextAsset>(TooltipDataAddress);
            await tooltipDataHandle;
            if (tooltipDataHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"{TooltipDataAddress} 로드 실패");
                return;
            }
            
            var csvTooltipData = tooltipDataHandle.Result.text;
            Addressables.Release(tooltipDataHandle);
            var tooltipDataSet = CsvParser.ParseCSV(csvTooltipData);
            
            for (var i = 1; i < tooltipDataSet.Length; i++)
            {
                try
                {
                    var dataRow = tooltipDataSet[i].Split(',');
                    var tooltipSOPath = $"Assets/GameData/SO/TooltipData/{dataRow[0]}.asset";
                    var existing = AssetDatabase.LoadAssetAtPath<TooltipBase>(tooltipSOPath);

                    TooltipBase data;
                    if (existing != null)
                    {
                        data = existing;
                        var isDirty = false;
                        
                        isDirty |= SetIfChanged(ref data.id, dataRow[0]);
                        int.TryParse(dataRow[1], out var priority);
                        isDirty |= SetIfChanged(ref data.priority, priority);
                        isDirty |= SetIfChanged(ref data.nameString, dataRow[2]);
                        isDirty |= SetIfChanged(ref data.descString, dataRow[3]);

                        if (isDirty)
                        {
                            Debug.Log($"{data.id} SO 변경");
                            EditorUtility.SetDirty(data);
                        }
                    }
                    else
                    {
                        data = ScriptableObject.CreateInstance<TooltipBase>();
                        
                        // 1. ID
                        data.id = dataRow[0];
                        // 2. Priority
                        data.priority = int.Parse(dataRow[1]);
                        // 3. nameString
                        data.nameString =  dataRow[2];
                        // 4. descString
                        data.descString = dataRow[3];
                    
                        AssetDatabase.CreateAsset(data, tooltipSOPath);
                        EditorUtility.SetDirty(data);
                    }
                    
                    if (!dataBase.tooltips.Contains(data))
                        dataBase.tooltips.Add(data);

                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    throw;
                }
            }
        }
        
        #endif
    }
}
