//Author & Copyright: MCFX2
//Licensed under MIT, public release edition
//For updates, bug reports, and feature requests, see https://github.com/MCFX2/unity-multitag

using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent, ExecuteAlways]
public class MultitagComponent : MonoBehaviour
{
    [SerializeField] private List<string> _tags = new List<string>();
    public List<string> Tags => _tags;

    private void OnEnable()
    {
        Multitag.RegisterGameObjectTags(gameObject, Tags.ToArray());
    }

    private void OnDisable()
    {
        Multitag.UnregisterGameObjectTags(gameObject, Tags.ToArray());
    }
}