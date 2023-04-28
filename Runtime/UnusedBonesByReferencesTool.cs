using CustomLocalization4EditorExtension;
using UnityEngine;

namespace Anatawa12.AvatarOptimizer
{
    [AddComponentMenu("Optimizer/UnusedBonesByReferencesTool")]
    [DisallowMultipleComponent]
    internal class UnusedBonesByReferencesTool : AvatarTagComponent
    {
        [CL4EELocalized("UnusedBonesByReferencesTool:prop:preserveEndBone", 
        "UnusedBonesByReferencesTool:tooltip:preserveEndBone")]
        [ToggleLeft]
        public bool preserveEndBone = true;

        [CL4EELocalized("UnusedBonesByReferencesTool:prop:detectExtraChild", 
        "UnusedBonesByReferencesTool:tooltip:detectExtraChild")]
        [ToggleLeft]
        public bool detectExtraChild;
    }
}
