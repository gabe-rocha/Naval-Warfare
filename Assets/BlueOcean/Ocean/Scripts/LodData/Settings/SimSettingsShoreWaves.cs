using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ocean
{
    [CreateAssetMenu(fileName = "SimSettingsShoreWaves", menuName = "Ocean Setting/Shore Wave Sim Settings", order = 10000)]
    public class SimSettingsShoreWaves : ScriptableObject
    {
        public Vector4[] waves;
    }
}


