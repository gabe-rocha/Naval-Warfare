using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Ocean
{
    /// <summary>
    /// Base class for simulation settings.
    /// </summary>
    public partial class SimSettingsBase : ScriptableObject
    {
    }

#if UNITY_EDITOR
    public partial class SimSettingsBase : IValidated
    {
        public virtual bool Validate(OceanRenderer ocean, ValidatedHelper.ShowMessage showMessage) => true;
    }
    
#endif
}
