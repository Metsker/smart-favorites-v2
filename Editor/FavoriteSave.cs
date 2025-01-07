using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SmartFavorites
{
    public class FavoriteSave : ScriptableObject
    {
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
        public FavoriteList CurrentList => favoriteLists[CurrentListIndex];
        
        [SerializeField] private List<FavoriteList> favoriteLists;

        private int _currentListIndex;
        public event Action CurrentIndexChanged;

        public FavoriteSave()
        {
            favoriteLists = new List<FavoriteList>();
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

            favoriteLists.Add(new FavoriteList(listName));
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
