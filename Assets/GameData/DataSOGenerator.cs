using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameData.Scripts;
using GameSystem.Enums;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;
using UnityEngine.AddressableAssets;
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
            await CreateCardAbilityData(dataBase);
            await CreateRoomData(dataBase);
            await CreateWaveData(dataBase);
            await CreateFateCardData(dataBase);
            await CreateStringData(dataBase);
            await CreateTooltipData(dataBase);
            
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
                    var data = ScriptableObject.CreateInstance<CardBase>();
                    
                    // 1. ID
                    data.cardID = dataRow[0];
                    
                    // 2. 등급
                    if (Enum.TryParse<CardTier>(dataRow[1], true, out var tier))
                    {
                        data.cardTier = tier;
                    }
                    else
                    {
                        Debug.Log("Data Generator : 카드 등급 설정 오류");
                        data.cardTier = 0;
                    }
                    
                    // 3. 종류
                    if (Enum.TryParse<CardType>(dataRow[2], true, out var cardType))
                    {
                        data.cardType = cardType;
                    }
                    else
                    {
                        Debug.Log("Data Generator : 카드 종류 설정 오류");
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

                    var cardSOPath = $"Assets/GameData/SO/CardData/{dataRow[2]}";
                    if (!AssetDatabase.IsValidFolder(cardSOPath))
                    {
                        AssetDatabase.CreateFolder("Assets/GameData/SO/CardData", dataRow[2]);
                    }

                    cardSOPath += $"/{dataRow[0]}.asset";
                    if (AssetDatabase.LoadAssetAtPath<CardBase>(cardSOPath) != null)
                    {
                        AssetDatabase.DeleteAsset(cardSOPath);
                    }
                    
                    AssetDatabase.CreateAsset(data, cardSOPath);
                    EditorUtility.SetDirty(data);
                    
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
                    var data = ScriptableObject.CreateInstance<CardAbilityBase>();
                    
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
                            var actionSet = new AbilityActionSet(actionType, int.Parse(actionValueArrayA[j]));
                            data.actionListA.Add(actionSet);
                        }
                        else
                        {
                            var actionSet = new AbilityActionSet();
                            data.actionListA.Add(actionSet);
                        }

                    }
                    // 4. ConditionSet
                    if (Enum.TryParse<ConditionType>(dataRow[4], true, out var conditionType))
                    {
                        data.condition = new ConditionSet(conditionType,  int.Parse(dataRow[5]));
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
                        if (!Enum.TryParse<ActionType>(actionArrayB[j], true, out var actionType))
                        {
                            var actionSet = new AbilityActionSet(actionType, int.Parse(actionValueArrayB[j]));
                            data.actionListB.Add(actionSet);
                        }
                        else
                        {
                            var actionSet = new AbilityActionSet();
                            data.actionListB.Add(actionSet);
                        }

                    }
                    
                    data.actionAString = dataRow[8];
                    data.actionBString = dataRow[9];
                    
                    var cardAbilitySOPath = $"Assets/GameData/SO/CardData/CardAbilityData/{dataRow[0]}.asset";
                    if (AssetDatabase.LoadAssetAtPath<CardAbilityBase>(cardAbilitySOPath) != null)
                    {
                        AssetDatabase.DeleteAsset(cardAbilitySOPath);
                    }
                    AssetDatabase.CreateAsset(data, cardAbilitySOPath);
                    EditorUtility.SetDirty(data);
                    
                    dataBase.allAbilities.Add(data);
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
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
                    var data = ScriptableObject.CreateInstance<RoomBase>();
                    
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
                        data.tier = 0;
                    }
                    // 6. ClearGold
                    int.TryParse(dataRow[5], out data.clearGoldMin);
                    int.TryParse(dataRow[6], out data.clearGoldMax);
                    

                    var roomSOPath = $"Assets/GameData/SO/RoomData/{dataRow[0]}.asset";
                    if (AssetDatabase.LoadAssetAtPath<CardAbilityBase>(roomSOPath) != null)
                    {
                        AssetDatabase.DeleteAsset(roomSOPath);
                    }
                    AssetDatabase.CreateAsset(data, roomSOPath);
                    EditorUtility.SetDirty(data);
                    
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
                    var data = ScriptableObject.CreateInstance<WaveBase>();
                    
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
                    

                    var waveSOPath = $"Assets/GameData/SO/WaveData/{dataRow[0]}.asset";
                    if (AssetDatabase.LoadAssetAtPath<CardAbilityBase>(waveSOPath) != null)
                    {
                        AssetDatabase.DeleteAsset(waveSOPath);
                    }
                    AssetDatabase.CreateAsset(data, waveSOPath);
                    EditorUtility.SetDirty(data);
                    
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
                    var data = ScriptableObject.CreateInstance<CardBase>();
                    
                    // 1. ID
                    data.cardID = dataRow[0];
                    
                    // 2. 등급
                    if (Enum.TryParse<CardTier>(dataRow[1], true, out var tier))
                    {
                        data.cardTier = tier;
                    }
                    else
                    {
                        Debug.Log("Data Generator : 카드 등급 설정 오류");
                        data.cardTier = 0;
                    }
                    
                    // 3. 종류
                    if (Enum.TryParse<CardType>(dataRow[2], true, out var cardType))
                    {
                        data.cardType = cardType;
                    }
                    else
                    {
                        Debug.Log("Data Generator : 카드 종류 설정 오류");
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

                    var cardSOPath = $"Assets/GameData/SO/CardData/FateCardData/{dataRow[0]}.asset";
                    if (AssetDatabase.LoadAssetAtPath<CardBase>(cardSOPath) != null)
                    {
                        AssetDatabase.DeleteAsset(cardSOPath);
                    }
                    
                    AssetDatabase.CreateAsset(data, cardSOPath);
                    EditorUtility.SetDirty(data);
                    
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
                    var data = ScriptableObject.CreateInstance<StringBase>();
                    
                    // 1. ID
                    data.id = dataRow[0];
                    data.kr = dataRow[1];
                    data.en = dataRow[2];
                    

                    var stringSOPath = $"Assets/GameData/SO/StringData/{dataRow[0]}.asset";
                    if (AssetDatabase.LoadAssetAtPath<CardAbilityBase>(stringSOPath) != null)
                    {
                        AssetDatabase.DeleteAsset(stringSOPath);
                    }
                    AssetDatabase.CreateAsset(data, stringSOPath);
                    EditorUtility.SetDirty(data);
                    
                    dataBase.strings.Add(data);

                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    throw;
                }
            }
        }
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
                    var data = ScriptableObject.CreateInstance<TooltipBase>();
                    
                    // 1. ID
                    data.id = dataRow[0];
                    // 2. Priority
                    data.priority = int.Parse(dataRow[1]);
                    // 3. nameString
                    data.nameString =  dataRow[2];
                    // 4. descString
                    data.descString = dataRow[3];
                    

                    var tooltipSOPath = $"Assets/GameData/SO/TooltipData/{dataRow[0]}.asset";
                    if (AssetDatabase.LoadAssetAtPath<CardAbilityBase>(tooltipSOPath) != null)
                    {
                        AssetDatabase.DeleteAsset(tooltipSOPath);
                    }
                    AssetDatabase.CreateAsset(data, tooltipSOPath);
                    EditorUtility.SetDirty(data);
                    
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
