//------------------------------------------------------------
// UGS Save System
// Copyright © 2023 UGS Team. All rights reserved.
//------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UGS.Save;

namespace UGS.Save.Editor
{
    /// <summary>
    /// 存档管理器窗口
    /// </summary>
    public class SaveManagerWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private string _customSavePath = "";
        private SaveFormat _selectedFormat = SaveFormat.Json;
        private bool _useEncryption = false;
        private string _encryptionKey = "";
        private string _selectedSaveId = "";
        private List<string> _saveList = new List<string>();

        [MenuItem("Window/UGS/Save Manager")]
        public static void ShowWindow()
        {
            SaveManagerWindow window = GetWindow<SaveManagerWindow>("存档管理器");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnEnable()
        {
            // 初始化存档系统
            SaveManager.Initialize();
            RefreshSaveList();
        }

        private void OnGUI()
        {
            GUILayout.Label("UGS 存档管理器", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawSettingsSection();
            EditorGUILayout.Space();

            DrawSaveListSection();
            EditorGUILayout.Space();

            DrawSelectedSaveSection();
        }

        /// <summary>
        /// 绘制设置部分
        /// </summary>
        private void DrawSettingsSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("存档系统设置", EditorStyles.boldLabel);

            // 存档路径设置
            EditorGUILayout.BeginHorizontal();
            _customSavePath = EditorGUILayout.TextField("自定义存档路径", _customSavePath);
            if (GUILayout.Button("浏览", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("选择存档路径", Application.dataPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    _customSavePath = path;
                }
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("应用路径设置"))
            {
                if (!string.IsNullOrEmpty(_customSavePath))
                {
                    SaveManager.SetSavePath(_customSavePath);
                    RefreshSaveList();
                    EditorUtility.DisplayDialog("成功", $"存档路径已设置为: {_customSavePath}", "确定");
                }
                else
                {
                    EditorUtility.DisplayDialog("错误", "请输入有效的存档路径", "确定");
                }
            }

            EditorGUILayout.Space();

            // 存档格式设置
            _selectedFormat = (SaveFormat)EditorGUILayout.EnumPopup("存档格式", _selectedFormat);
            if (GUILayout.Button("应用格式设置"))
            {
                SaveManager.SetSaveFormat(_selectedFormat);
                EditorUtility.DisplayDialog("成功", $"存档格式已设置为: {_selectedFormat}", "确定");
            }

            EditorGUILayout.Space();

            // 加密设置
            _useEncryption = EditorGUILayout.Toggle("启用加密", _useEncryption);
            if (_useEncryption)
            {
                _encryptionKey = EditorGUILayout.PasswordField("加密密钥", _encryptionKey);
            }

            if (GUILayout.Button("应用加密设置"))
            {
                SaveManager.EnableEncryption(_useEncryption, _encryptionKey);
                EditorUtility.DisplayDialog("成功", $"存档加密已{(_useEncryption ? "启用" : "禁用")}", "确定");
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制存档列表部分
        /// </summary>
        private void DrawSaveListSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("存档列表", EditorStyles.boldLabel);
            if (GUILayout.Button("刷新", GUILayout.Width(60)))
            {
                RefreshSaveList();
            }
            EditorGUILayout.EndHorizontal();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(150));

            if (_saveList.Count > 0)
            {
                foreach (string saveId in _saveList)
                {
                    EditorGUILayout.BeginHorizontal();

                    SaveFileInfo saveInfo = SaveManager.GetSaveInfo(saveId);
                    string saveInfoText = saveInfo != null
                        ? $"{saveId} - {saveInfo.FormattedLastWriteTime} ({saveInfo.FormattedSize})"
                        : saveId;

                    bool isSelected = _selectedSaveId == saveId;
                    bool newIsSelected = GUILayout.Toggle(isSelected, saveInfoText, "Button");

                    if (newIsSelected && !isSelected)
                    {
                        _selectedSaveId = saveId;
                    }
                    else if (!newIsSelected && isSelected)
                    {
                        _selectedSaveId = "";
                    }

                    if (GUILayout.Button("删除", GUILayout.Width(60)))
                    {
                        if (EditorUtility.DisplayDialog("确认删除", $"确定要删除存档 '{saveId}' 吗？", "删除", "取消"))
                        {
                            SaveManager.DeleteSave(saveId);
                            RefreshSaveList();
                            if (_selectedSaveId == saveId)
                            {
                                _selectedSaveId = "";
                            }
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("没有找到存档文件", MessageType.Info);
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("删除所有存档"))
            {
                if (EditorUtility.DisplayDialog("确认删除", "确定要删除所有存档吗？此操作不可撤销！", "删除", "取消"))
                {
                    SaveManager.DeleteAllSaves();
                    RefreshSaveList();
                    _selectedSaveId = "";
                }
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制选中存档部分
        /// </summary>
        private void DrawSelectedSaveSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("选中的存档", EditorStyles.boldLabel);

            if (!string.IsNullOrEmpty(_selectedSaveId))
            {
                SaveFileInfo saveInfo = SaveManager.GetSaveInfo(_selectedSaveId);
                if (saveInfo != null)
                {
                    EditorGUILayout.LabelField("存档ID", saveInfo.SaveId);
                    EditorGUILayout.LabelField("创建时间", saveInfo.FormattedCreationTime);
                    EditorGUILayout.LabelField("修改时间", saveInfo.FormattedLastWriteTime);
                    EditorGUILayout.LabelField("文件大小", saveInfo.FormattedSize);

                    EditorGUILayout.Space();

                    if (GUILayout.Button("在文件浏览器中显示"))
                    {
                        string filePath = Path.Combine(SaveManagerExtensions.GetSavePath(), _selectedSaveId + (_selectedFormat == SaveFormat.Json ? ".json" : ".sav"));
                        EditorUtility.RevealInFinder(filePath);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("无法获取存档信息", MessageType.Warning);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("未选择存档", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 刷新存档列表
        /// </summary>
        private void RefreshSaveList()
        {
            _saveList = SaveManager.GetSaveList();
        }
    }
}