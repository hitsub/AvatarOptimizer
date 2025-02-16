using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Anatawa12.AvatarOptimizer.Test
{
    internal static class TestUtils
    {
        public static GameObject NewAvatar(string name = null)
        {
            var root = new GameObject();
            root.name = name ?? "Test Avatar";
            var animator = root.AddComponent<Animator>();
            animator.avatar = AvatarBuilder.BuildGenericAvatar(root, "");
            var descriptor = root.AddComponent<VRCAvatarDescriptor>();
            return root;
        }
    }
}
