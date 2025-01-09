using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SmartFavorites
{
    public class FavoritesData : ScriptableObject
    {
        [Range(10, 100)] public float listItemHeight = 55f;
        [Tooltip("Recompile is required")]
        [Range(5, 15)] public int toolbarFontSize = 11;
        [Space]
        public float lastObjectSelectedTickOpen = 0.5f;
        public float lastObjectSelectedTickPing = 2f;
        [Space]
        [SerializeField] private List<FavoritesList> favoriteLists;
        
        public int CurrentListIndex 
        {
            get => _currentListIndex;
            set
            {
                if (_currentListIndex != value)
                    CurrentIndexChanged?.Invoke();

                _currentListIndex = value;
            }
        }
        public int FavoriteListsCount => favoriteLists.Count;
        public FavoritesList CurrentList {
            get
            {
                if (_currentListIndex >= favoriteLists.Count)
                    _currentListIndex = favoriteLists.Count - 1;
                else if (_currentListIndex < 0)
                    _currentListIndex = 0;
                
                return favoriteLists[CurrentListIndex];
            }
        }

        private int _currentListIndex;
        public event Action CurrentIndexChanged;

        public FavoritesData()
        {
            favoriteLists = new List<FavoritesList>();
            AddList();
        }

        public void AddList()
        {
            string listName = "Favorites 1";
            string[] names = ListNames();
            
            for (int i = 1; i < favoriteLists.Count + 1; i++)
            {
                if (names.Contains(listName))
                    listName = "Favorites " + (i + 1);
            }
            
            favoriteLists.Add(new FavoritesList(listName));
            
            CurrentListIndex = favoriteLists.Count - 1;
        }

        public void RemoveList(int index)
        {
            favoriteLists.RemoveAt(index);
            
            if (CurrentListIndex >= favoriteLists.Count)
                CurrentListIndex = favoriteLists.Count - 1;
        }

        public string[] ListNames() =>
            favoriteLists.Select(l => l.name).ToArray();
    }
}
