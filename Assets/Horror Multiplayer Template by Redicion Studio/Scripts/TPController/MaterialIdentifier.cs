// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedicionStudio
{
    public class MaterialIdentifier : MonoBehaviour
    {
        public MaterialEnum material = new MaterialEnum();
    }

    public enum MaterialEnum
    {
        Metal,
        Sand,
        Stone,
        Wood,
        Character
    };
}
