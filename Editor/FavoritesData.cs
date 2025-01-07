using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SmartFavorites
{
    public class FavoritesData : ScriptableObject
    {
        [Range(10, 100)] public float itemHeight = 55f;
        [Range(5, 15)] public int fontSize = 10;
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
        public FavoritesList CurrentList => favoriteLists[CurrentListIndex];
        
        private int _currentListIndex;
        public event Action CurrentIndexChanged;

        public FavoritesData()
        {
            favoriteLists = new List<FavoritesList>();
            AddList();
        }

        public void AddList()
        {
            string listName = "Favorites";
            string[] names = NameList();
            for (int i = 0; i < favoriteLists.Count; i++)
            {
                if (names.Contains(listName))
                    listName = "Favorites " + (i + 1);
            }

            favoriteLists.Add(new FavoritesList(listName));
            CurrentListIndex = favoriteLists.Count - 1;
        }

        public void RemoveList(int index)
        {
            if (favoriteLists.Count <= 1)
                return;
            
            favoriteLists.RemoveAt(index);
            if (CurrentListIndex >= favoriteLists.Count)
                CurrentListIndex--;
        }

        public string[] NameList()
        {
            string[] nameList = new string[favoriteLists.Count];
            for (int i = 0; i < favoriteLists.Count; i++)
                nameList[i] = favoriteLists[i].name;
            return nameList;
        }
    }
}
