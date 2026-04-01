#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;


public enum ConversionType
{
    Items,
    Dialogs
}

[Serializable]
public class DialogRowData
{
    public int? id;
    public string characterName;
    public string text;
    public int? nextId;
    public string portraitPath;
    public string choiceText;
    public int? choiceNextID;
}

public class JsonToScriptableConverter : EditorWindow
{
    private string jsonFilePath = "";                                           //JSON 파일 경로 문자열 값
    private string outputFolder = "Assets/ScriptableObjects/Items";             //출력 SO 파일 경로 값
    private bool createDatabase = true;                                         //데이터 베이스 활용 여부 체크 값
    private ConversionType conversionType = ConversionType.Items;

    [MenuItem("Tools/JSON to Scriptable Objects")]

    public static void ShowWindow()
    {
        GetWindow<JsonToScriptableConverter>("JSON to Scriptable Objects");
    }

    private void OnGUI()
    {
        GUILayout.Label("JSON to Scriptable object Converter", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (GUILayout.Button("Select JSON File"))
        {
            jsonFilePath = EditorUtility.OpenFilePanel("Select JSON File", "", "json");
        }

        EditorGUILayout.LabelField("Selected File : ", jsonFilePath);
        EditorGUILayout.Space();
        outputFolder = EditorGUILayout.TextField("Output Foloder :", outputFolder);
        createDatabase = EditorGUILayout.Toggle("Create Database Asset", createDatabase);
        EditorGUILayout.Space();

        conversionType = (ConversionType)EditorGUILayout.EnumPopup("Conversion Type: ", conversionType);

        if (conversionType == ConversionType.Items && outputFolder == "Assets/ScriptableObjects")
        {
            outputFolder = "Assest/ScriptableObjects/Items";
        }
        else if(conversionType == ConversionType.Dialogs && outputFolder == "Assest/ScriptableObjects")
        {
            outputFolder = "Assest/ScriptableObjects/Dialogs";
        }

        if (GUILayout.Button("Convert to Scriptable Objects"))
        {
            if (string.IsNullOrEmpty(jsonFilePath))
            {
                EditorUtility.DisplayDialog("Error", "Pease Select a JSON file first", "OK");
                return;
            }

            switch (conversionType)
            {
                case ConversionType.Items:
                    ConvertJsonToItemScriptableObjects();
                    break;
                
                case ConversionType.Dialogs:
                    ConvertJsonToDialogScriptableObjects();
                    break;
            }
        }
    }
    private void ConvertJsonToItemScriptableObjects()                               //JSON 파일을 ScriptableObjcet 파일로 변환 시켜주는 함수
    {
        //폴더 생성
        if (!Directory.Exists(outputFolder))                                    //폴더 위치를 확인하고 없으면 생성한다
        {
            Directory.CreateDirectory(outputFolder);
        }

        //JSON 파일 읽기
        string jsonText = File.ReadAllText(jsonFilePath);                       //JSON 파일을 읽는다

        try
        {
            //JSON 파싱
            List<ItemData> itemDataList = JsonConvert.DeserializeObject<List<ItemData>>(jsonText);

            List<ItemSO> createdItems = new List<ItemSO>();                     //Item List 생성

            //각 아이템을 데이터 스크립터블 오브젝트로 변환
            foreach (ItemData itemData in itemDataList)
            {
                ItemSO itemSO = ScriptableObject.CreateInstance<ItemSO>();      //ItemSO 파일을 생성

                //데이터 복사
                itemSO.id = itemData.id;
                itemSO.itemName = itemData.itemName;
                itemSO.nameENG = itemData.nameEng;
                itemSO.description = itemData.description;

                //열거형 변환
                if (System.Enum.TryParse(itemData.itemTypeString, out ItemType parsedType))
                {
                    itemSO.itemType = parsedType;
                }
                else
                {
                    Debug.LogWarning($"아이템 {itemData.itemName}의 유효하지 않은 타입 : {itemData.itemTypeString}");
                }

                itemSO.price  = itemData.price;
                itemSO.power = itemData.power;
                itemSO.level = itemData.level;
                itemSO.isStackable = itemData.isStackable;

                //아이콘 로드 (경로가 있는 경우)

                if (!string.IsNullOrEmpty(itemData.iconPath))                   //아이콘 경로가 있는지 확인한다.
                {
                    itemSO.icon = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Resources/{itemData.iconPath}.png");

                    if (itemSO.icon == null)
                    {
                        Debug.LogWarning($"아이템 {itemData.nameEng} 의 아이콘을 찾을 수 없습니다. : {itemData.iconPath}");
                    }
                }

                //스크립터블 오브젝트 저장 - ID를 4자리 숫자로 포맷팅
                string assetPath = $"{outputFolder}/Item_{itemData.id.ToString("D4")}_{itemData.nameEng}.asset";
                AssetDatabase.CreateAsset(itemSO, assetPath);

                //에셋 이름 지정
                itemSO.name = $"Item_{itemData.id.ToString("D4")} + {itemData.nameEng}";
                createdItems.Add(itemSO);

                EditorUtility.SetDirty(itemSO);
            }
            //데이터 베이스
            if (createDatabase && createdItems.Count > 0)
            {
                ItemDataBaseSO database = ScriptableObject.CreateInstance<ItemDataBaseSO>();                //생성
                database.items = createdItems;

                AssetDatabase.CreateAsset(database, $"{outputFolder}/ItemDatabase.asset");
                EditorUtility.SetDirty(database);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Sucess", $"Created {createdItems.Count} scriptable objects!", "OK");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to Convert JSON : {e.Message}", "OK");
            Debug.LogError($"JSON 변환 오류 : {e}");
        }
    }

    private void ConvertJsonToDialogScriptableObjects()
    {
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        //JSON 파일 읽기
        string JsonText = File.ReadAllText(jsonFilePath);

        try
        {
            // JSON 파싱
            List<DialogRowData> rowDataList = JsonConvert.DeserializeObject<List<DialogRowData>>(JsonText);

            // 대화 데이터 재구성
            Dictionary<int, DialogSO> dialogMap = new Dictionary<int, DialogSO>();
            List<DialogSO> createDialogs = new List<DialogSO>();

            // 1단계 : 대화 항목 생성
            foreach (var rowData in rowDataList)
            {
                if (!rowData.id.HasValue)       // id 없는 row는 스킵
                    continue;

                // id 있는 행을 대화로 처리
                DialogSO dialogSO = ScriptableObject.CreateInstance<DialogSO>();

                // 데이터 복사
                dialogSO.id = rowData.id.Value;
                dialogSO.characterName = rowData.characterName;
                dialogSO.text = rowData.text;
                dialogSO.nextId = rowData.nextId.HasValue ? rowData.nextId.Value : -1;
                dialogSO.portaitPath = rowData.portraitPath;
                dialogSO.choises = new List<DialogChoiceSO>();

                // 초상화 로드 (경로가 있을 경우)
                if (!string.IsNullOrEmpty(rowData.portraitPath))
                {
                    dialogSO.portrait = Resources.Load<Sprite>(rowData.portraitPath);
                    
                    if (dialogSO.portrait == null)
                    {
                        Debug.LogWarning($"대화 {rowData.id}의 초상화를 찾을 수 없습니다.");
                    }

                }
                dialogMap[dialogSO.id] = dialogSO;
                createDialogs.Add(dialogSO);
            }

            // 2단계 : 선택지 항목 처리 및 연결
            foreach (var rowData in rowDataList)
            {
                // id가 없고 choiceText가 있는 행은 선택지로 처리
                if (!rowData.id.HasValue && !string.IsNullOrEmpty(rowData.choiceText) && rowData.choiceNextID.HasValue)
                {
                    // 이전 행의 ID를 부모 ID로 사용(연속되는 선택지의 경우
                    int parentID = -1;

                    // 이 선택지 바로 위에 있는 대화 (id가 있는 항목)을 찾음
                    int currentIndex = rowDataList.IndexOf(rowData);
                    for (int i = currentIndex - 1; i >= 0; i--)
                    {
                        if (rowDataList[i].id.HasValue)
                        {
                            parentID = rowDataList[i].id.Value;
                            break;
                        }
                    }

                    // 부모 ID를 찾지 못했거나 부모 ID가 -1인 경우 (첫 번째 항목)
                    if (parentID == -1)
                    {
                        Debug.LogWarning($"선택지 {rowData.choiceText}의 부모 대화를 찾을 수 없습니다.");
                    }

                    if (dialogMap.TryGetValue(parentID, out DialogSO parentDialog))
                    {
                        DialogChoiceSO choiceSO = ScriptableObject.CreateInstance<DialogChoiceSO>();

                        choiceSO.text = rowData.choiceText;
                        choiceSO.nextId = rowData.choiceNextID.Value;

                        // 선택지 에셋 저장
                        string choiceAssetPath = $"{outputFolder}/Choice_{parentDialog}_{parentDialog.choises.Count + 1}.asset";
                        AssetDatabase.CreateAsset(choiceSO, choiceAssetPath);
                        EditorUtility.SetDirty(choiceSO);
                        parentDialog.choises.Add(choiceSO);
                    }
                    else
                    {
                        Debug.LogWarning($"선택지 {rowData.choiceText}를 연결할 대화 (ID : {parentID}를 찾을 수 없습니다.");
                    }
                }
            }

            // 3단계 : 대화 스크립터블 오브젝트 저장
            foreach (var dialog in createDialogs)
            {
                // 스크립터블 오브젝트 저장 - ID 4자리 숫자
                string assetPath = $"{outputFolder}/Dialog_{dialog.id.ToString("D4")}.asset";
                AssetDatabase.CreateAsset(dialog, assetPath);

                // 에셋 이름 지정
                dialog.name = $"Dialog_{dialog.id.ToString("D4")}";

                EditorUtility.SetDirty(dialog);
            }

            if (createDatabase && createDialogs.Count > 0)
            {
                // 데이터 베이스 생성
                DialogDatabaseSO database = ScriptableObject.CreateInstance<DialogDatabaseSO>();
                database.dialogs = createDialogs;

                AssetDatabase.CreateAsset(database, $"{outputFolder}/DialogDatabase.asset");
                EditorUtility.SetDirty(database);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Succes", $"created {createDialogs.Count} dialog Scriptable Objects!", "OK");

        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Faild to convert JSON : {e.Message}", "OK");
            Debug.LogError($"JSON 변환 오류 :{e}");
        }
    }

}

#endif