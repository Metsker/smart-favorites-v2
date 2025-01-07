using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SmartFavorites
{
    public class FavoritesWindow : EditorWindow
    {
        private static FavoritesWindow _favoritesWindow;
        private static FavoritesData _favoritesData;
        private static FavoritesData FavoritesData {
            get
            {
                if (!_favoritesData)
                    InitFavoriteSave();
                return _favoritesData;
            }
        }
        private ReorderableList reorderableList;
        
        private Vector2 scrollViewPosition = Vector2.zero;
        private Object lastObjectSelected;
        private double lastObjectSelectedAt;

        private bool guiStyleDefined;
        private GUIStyle reorderableListLabelGuiStyle;
        private GUIStyle toolbarIconButtonGuiStyle;

        private bool editNameList;
        private Font _font;
        
        [MenuItem("Window/Favorites", priority = 1100)]
        public static void ShowWindow()
        {
            _favoritesWindow = GetWindow<FavoritesWindow>("Favorites");
            _favoritesWindow.titleContent = new GUIContent("Favorites", EditorGUIUtility.IconContent("d_Favorite").image);
            _favoritesWindow.Show();
        }

        public void OnEnable()
        {
            InitList();
            InitFavoriteSave();
            guiStyleDefined = false;
            _font = Resources.Load<Font>("FontAwesome");
            FavoritesData.CurrentIndexChanged += OnSelectedChanged;
        }

        private void InitList()
        {
            reorderableList = new ReorderableList(null, typeof(Object), true, false, false, false)
            {
                showDefaultBackground = false,
                headerHeight = 0F,
                footerHeight = 0F,
                drawElementCallback = OnDrawElement,
                multiSelect = true
            };
            reorderableList.onMouseUpCallback += OnMouseUp;
            reorderableList.onSelectCallback += OnSelect;
            reorderableList.onMouseDragCallback += OnMouseDrag;
        }

        private void OnDisable()
        {
            reorderableList.onMouseUpCallback -= OnMouseUp;
            reorderableList.onSelectCallback -= OnSelect;
            reorderableList.onMouseDragCallback -= OnMouseDrag;
            FavoritesData.CurrentIndexChanged -= OnSelectedChanged;
        }

        public void OnLostFocus()
        {
            if (editNameList)
                editNameList = false;
        }

        public void OnGUI()
        {
            reorderableList.elementHeight = _favoritesData.itemHeight;

            if (!guiStyleDefined)
            {
                guiStyleDefined = true;
                reorderableListLabelGuiStyle = new GUIStyle(EditorStyles.label);
                reorderableListLabelGuiStyle.focused.textColor = reorderableListLabelGuiStyle.normal.textColor;
                toolbarIconButtonGuiStyle = new GUIStyle(EditorStyles.toolbarButton)
                {
                    font = _font,
                    fontSize = _favoritesData.fontSize
                };
            }

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            
            EditorGUI.BeginDisabledGroup(editNameList || FavoritesData.FavoriteListsCount < 2);
            if (GUILayout.Button("⬅", toolbarIconButtonGuiStyle, GUILayout.ExpandWidth(false)))
            {
                FavoritesData.CurrentListIndex--;
                if (FavoritesData.CurrentListIndex < 0)
                    FavoritesData.CurrentListIndex = FavoritesData.FavoriteListsCount - 1;
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(editNameList || FavoritesData.FavoriteListsCount < 2);
            if (GUILayout.Button("➡", toolbarIconButtonGuiStyle, GUILayout.ExpandWidth(false)))
            {
                FavoritesData.CurrentListIndex++;
                if (_favoritesData.CurrentListIndex >= FavoritesData.FavoriteListsCount)
                    FavoritesData.CurrentListIndex = 0;
            }
            EditorGUI.EndDisabledGroup();

            if (editNameList)
            {
                GUI.SetNextControlName("EditNameList");
                FavoritesData.CurrentList.name = EditorGUILayout.TextField(FavoritesData.CurrentList.name, EditorStyles.toolbarTextField, GUILayout.ExpandWidth(true));
                EditorGUI.FocusTextInControl("EditNameList");
            }
            else
                FavoritesData.CurrentListIndex = EditorGUILayout.Popup(FavoritesData.CurrentListIndex, FavoritesData.NameList(), EditorStyles.toolbarPopup);

            if (editNameList && (Event.current.type == EventType.MouseUp && Event.current.button == 0 || Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Return))
                editNameList = false;

            if (GUILayout.Button("", toolbarIconButtonGuiStyle, GUILayout.ExpandWidth(false)))
                ButtonEditFavoriteList();
            
            EditorGUI.BeginDisabledGroup(editNameList);
            if (GUILayout.Button("", toolbarIconButtonGuiStyle, GUILayout.ExpandWidth(false)))
                ButtonAddFavoriteList();
            EditorGUI.EndDisabledGroup();
            
            EditorGUI.BeginDisabledGroup(FavoritesData.FavoriteListsCount <= 1);
            if (GUILayout.Button("", toolbarIconButtonGuiStyle, GUILayout.ExpandWidth(false)))
                ButtonRemoveFavoriteList();
            EditorGUI.EndDisabledGroup();

            GUILayout.EndHorizontal();

            bool mouseOnWindow = Event.current.mousePosition.x >= 0 && Event.current.mousePosition.x <= position.width && Event.current.mousePosition.y >= 20 && Event.current.mousePosition.y <= position.height;

            switch (Event.current.type)
            {
                case EventType.DragUpdated when mouseOnWindow:
                {
                    if (DragAndDrop.objectReferences.Length != 0)
                    {
                        bool isValid = DragAndDrop.objectReferences.Select(GlobalObjectId.GetGlobalObjectIdSlow).Any(obj => !FavoritesData.CurrentList.Contains(obj));
                        DragAndDrop.visualMode = isValid ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
                    }
                    else
                        DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    break;
                }
                case EventType.DragPerform when mouseOnWindow:
                    DragAndDrop.AcceptDrag();
                    AddToFavoriteDrop(DragAndDrop.objectReferences);

                    Event.current.Use();
                    break;
                case EventType.ContextClick when mouseOnWindow:
                    ShowGenericMenu();
                    Event.current.Use();
                    break;
                case EventType.KeyDown:
                    if (Event.current.keyCode == KeyCode.Delete)
                        RemoveSelected();
                    break;
            }

            scrollViewPosition = GUILayout.BeginScrollView(scrollViewPosition);

            EditorUtility.SetDirty(FavoritesData);
            reorderableList.list = FavoritesData.CurrentList.serializedIds;
            reorderableList.DoLayoutList();
            
            Rect scrollArea = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.MouseUp && mouseOnWindow && !scrollArea.Contains(Event.current.mousePosition))
            {
                reorderableList.ClearSelection();
                Repaint();
            }
            
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void OnSelectedChanged() =>
            reorderableList.ClearSelection();

        private static void OnSelect(ReorderableList list)
        {
            if (list.selectedIndices.Count == 0)
                return;

            DragAndDrop.PrepareStartDrag();
            Object selected = FavoritesData.CurrentList.Get(list.index);
            DragAndDrop.objectReferences = new[]
            {
                selected
            };
        }

        private void OnMouseDrag(ReorderableList list)
        {
            if (!mouseOverWindow.wantsMouseEnterLeaveWindow)
                return;

            int selectedIndex = list.index;
            
            DragAndDrop.StartDrag("Favorites");
            InitList();
            reorderableList.Select(selectedIndex);
            Event.current.Use();
            GUIUtility.hotControl = 0;
        }

        private void OnMouseUp(ReorderableList list)
        {
            Object currentObject = FavoritesData.CurrentList.Get(list.index);
            Selection.activeObject = currentObject;
            if (lastObjectSelected == currentObject)
            {
                if (lastObjectSelectedAt + _favoritesData.lastObjectSelectedTickOpen > EditorApplication.timeSinceStartup)
                    AssetDatabase.OpenAsset(currentObject);
                else if (lastObjectSelectedAt + _favoritesData.lastObjectSelectedTickPing > EditorApplication.timeSinceStartup)
                    EditorGUIUtility.PingObject(currentObject);
            }
            lastObjectSelected = currentObject;
            lastObjectSelectedAt = EditorApplication.timeSinceStartup;
            Event.current.Use();
        }

        private void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            Object currentObject = FavoritesData.CurrentList.Get(index);
            
            if (!currentObject)
                return;

            Rect iconRect = new (rect);
            iconRect.y += 1f;
            iconRect.height -= 4f;
            iconRect.width = iconRect.height;

            Rect labelRect = new (rect);
            labelRect.y += 2f;
            labelRect.height -= 4f;
            labelRect.x += iconRect.width + 4f;
            labelRect.width -= iconRect.width;

            Texture2D miniIcon = AssetPreview.GetMiniThumbnail(currentObject);
            GUI.DrawTexture(iconRect, miniIcon, ScaleMode.ScaleToFit, true);
            
            Texture2D preview = AssetPreview.GetAssetPreview(currentObject);
            if (preview != null)
                GUI.DrawTexture(iconRect, preview, ScaleMode.ScaleToFit, true);

            EditorGUI.LabelField(labelRect, currentObject.name, reorderableListLabelGuiStyle);
        }

        private static void InitFavoriteSave()
        {
            if (_favoritesData)
                return;
            
            string[] favoriteSaveFind = AssetDatabase.FindAssets("t:FavoriteSave");
            if (favoriteSaveFind.Length > 0)
                _favoritesData = AssetDatabase.LoadAssetAtPath<FavoritesData>(AssetDatabase.GUIDToAssetPath(favoriteSaveFind[0]));

            if (_favoritesData)
                return;
                
            string favoriteSavePath = "";
            string[] favoriteSavePathFind = AssetDatabase.FindAssets("FavoriteSave t:Script");
            if (favoriteSavePathFind.Length > 0)
                favoriteSavePath = AssetDatabase.GUIDToAssetPath(favoriteSavePathFind[0].Replace("FavoriteSave.cs", "FavoriteSave.asset"));

            if (!favoriteSavePath.Contains("FavoriteSave.asset"))
                favoriteSavePath = "Assets/FavoriteSave.asset";

            _favoritesData = CreateInstance<FavoritesData>();
            AssetDatabase.CreateAsset(_favoritesData, favoriteSavePath);
            AssetDatabase.SaveAssets();
        }

        private void ShowGenericMenu()
        {
            if (FavoritesData.CurrentList.serializedIds.Count == 0)
                return;
            
            GenericMenu genericMenu = new ();
            
            if (reorderableList.selectedIndices.Count > 0)
            {
                genericMenu.AddItem(new GUIContent("Remove Selected"), false, RemoveSelected, null);
                genericMenu.AddSeparator("");
            }
            
            genericMenu.AddItem(new GUIContent("Remove All"), false, ClearFavorite);
            genericMenu.ShowAsContext();
        }

        private void ButtonAddFavoriteList()
        {
            FavoritesData.AddList();
            editNameList = true;
        }

        private static void ButtonRemoveFavoriteList()
        {
            if (EditorUtility.DisplayDialog("Remove the list \"" + FavoritesData.CurrentList.name + "\"?", "Are you sure you want delete the list \"" + FavoritesData.CurrentList.name + "\"?", "Yes", "No"))
                FavoritesData.RemoveList(FavoritesData.CurrentListIndex);
        }

        private void ButtonEditFavoriteList() =>
            editNameList = !editNameList;

        private static void CheckObjects(IEnumerable<Object> objects, out List<GlobalObjectId> addObjects, out List<GlobalObjectId> removeObjects)
        {
            addObjects = new List<GlobalObjectId>();
            removeObjects = new List<GlobalObjectId>();
            foreach (Object iObject in objects)
            {
                GlobalObjectId obj = GlobalObjectId.GetGlobalObjectIdSlow(iObject);
               
                if (!FavoritesData.CurrentList.Contains(obj))
                    addObjects.Add(obj);
                else
                    removeObjects.Add(obj);
            }
        }

        private static void AddToFavoriteDrop(IEnumerable<Object> objects)
        {
            if (!_favoritesWindow)
                ShowWindow();

            CheckObjects(objects, out List<GlobalObjectId> addObjects, out List<GlobalObjectId> _);

            if (addObjects.Count > 0)
                FavoritesData.CurrentList.Add(addObjects);
        }

        private void RemoveSelected(object _ = default)
        {
            for (int i = reorderableList.selectedIndices.Count - 1; i >= 0; i--)
            {
                int id = reorderableList.selectedIndices[i];
                
                if (!GlobalObjectId.TryParse((string)reorderableList.list[id], out GlobalObjectId bObject))
                    continue;
                
                if (FavoritesData.CurrentList.Contains(bObject))
                    FavoritesData.CurrentList.Remove(bObject);
            }
            reorderableList.ClearSelection();
            Repaint();
        }

        private void ClearFavorite()
        {
            if (!EditorUtility.DisplayDialog("Clear the list \"" + FavoritesData.CurrentList.name + "\"?", "Are you sure you want delete all the Favorites of the list \"" + FavoritesData.CurrentList.name + "\"?", "Yes", "No"))
                return;
            
            FavoritesData.CurrentList.Clear();
            reorderableList.ClearSelection();
            Repaint();
        }
        
        /*[MenuItem("Assets/Add or Remove to Favorites %&F", false, priority = 0)]
        public static void AddRemoveToFavorite()
        {
            if (Selection.activeObject == null)
                return;

            if (!_favoriteWindow)
                ShowWindow();

            CheckObjects(Selection.objects, out List<GlobalObjectId> addObjects, out List<GlobalObjectId> removeObjects);

            if (addObjects.Count > 0)
                FavoriteSave.CurrentList.Add(addObjects);
            else
                FavoriteSave.CurrentList.Remove(removeObjects);

            _favoriteWindow.Repaint();
        }

        [MenuItem("Assets/Add or Remove to Favorites %&F", true, priority = 0)]
        public static bool AddRemoveToFavoriteValidate() =>
            Selection.activeObject != null;*/
    }
}
