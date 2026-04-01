using System;
using Cysharp.Threading.Tasks;
using Data;
using GameData.SO.Scripts;
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
    
        #if UNITY_EDITOR
        [MenuItem("Data/Create Data SO")]
        public static async void CreateDataSO()
        {
            try
            {
                // SO 에셋 생성
                await CreateCardData();
        
                // SO 에셋 저장
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        
            Debug.Log("CSV 파싱 완료");
        }
    
    
        private static async UniTask CreateCardData()
        {
            const string sheetAddress = "Assets/GameData/SheetData/CardData.asset";
            const string dataBaseAddress = "Assets/GameData/SO/CardDataBase.asset";
            
            
            var handle = Addressables.LoadAssetAsync<TextAsset>(sheetAddress);
            await handle;
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"{sheetAddress} 로드 실패");
                return;
            }
            var csvData = handle.Result.text;
            var dataSet = CSVParser.ParseCSV(csvData);
            
            var dataBase = AssetDatabase.LoadAssetAtPath<CardDataBase>(dataBaseAddress);
            if (dataBase == null)
            {
                dataBase = ScriptableObject.CreateInstance<CardDataBase>();
                AssetDatabase.CreateAsset(dataBase, dataBaseAddress);
            }
            
            
            
            
            
            EditorUtility.SetDirty(dataBase);
            Addressables.Release(handle);
        }
    
    #endif
    }
}
