// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;

namespace RedicionStudio
{
    [CreateAssetMenu(fileName = "GameplayItemList", menuName = "Tools/Horror Multiplayer Template by Redicion Studio/Item List")]
    public class GameplayItemList : ScriptableObject
    {
        public GameplayItem[] items;
    }
}

