using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameData.SO.Scripts;
using GamePlay.Card;
using GameSystem.Enums;
using UnityEditor;
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
        private const string DataBaseAddress = "Assets/GameData/SO/CardDataBase.asset";
    
        #if UNITY_EDITOR
        [MenuItem("Data/Create Data SO")]
        public static async void CreateDataSO()
        {
            try
            {
                // SO 에셋 생성
                await CreateCardData();


                
                        
                Debug.Log("CSV 파싱 완료");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

        }
    
    
        private static async UniTask CreateCardData()
        {
            // csv파일 로드
            var handle = Addressables.LoadAssetAsync<TextAsset>(CardDataAddress);
            await handle;
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"{CardDataAddress} 로드 실패");
                return;
            }
            
            var csvData = handle.Result.text;
            var dataSet = CsvParser.ParseCSV(csvData);
            
            
            var dataBase = ScriptableObject.CreateInstance<CardDataBase>();
            if (AssetDatabase.LoadAssetAtPath<CardDataBase>(DataBaseAddress) != null)
            {
                AssetDatabase.DeleteAsset(DataBaseAddress);
            }
            AssetDatabase.CreateAsset(dataBase, DataBaseAddress);
            
            for (var i = 1; i < dataSet.Length; i++)
            {
                try
                {
                    var dataRow = dataSet[i].Split(',');
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
                        if (string.IsNullOrWhiteSpace(keyword) || keyword == "None")
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
            
            EditorUtility.SetDirty(dataBase);
            // SO 에셋 저장
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Addressables.Release(handle);
        }
    
    #endif
    }
}
