using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClocknestGames.Library.Utils
{
    public static class CGUtils
    {
        public static void SetLayer(this GameObject parent, int layer, bool includeChildren = true)
        {
            parent.layer = layer;
            if (includeChildren)
            {
                foreach (Transform trans in parent.transform.GetComponentsInChildren<Transform>(true))
                {
                    trans.gameObject.layer = layer;
                }
            }
        }

        public static bool LayerExists(int layer, LayerMask mask)
        {
            return ((1 << layer) & mask.value) != 0;
        }

        /// <summary>
        /// Extension method to check if a layer is in a layermask
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static bool Contains(this LayerMask mask, int layer)
        {
            return mask == (mask | (1 << layer));
        }

        public static void ChangeLayerOfAllChildren(this GameObject go, int layer)
        {
            go.layer = layer;
            foreach (var child in go.GetComponentsInChildren<Transform>())
                child.gameObject.layer = layer;
        }

        public static bool HasParameter(this Animator animator, string paramName)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == paramName)
                    return true;
            }
            return false;
        }

        public static IEnumerator IDoActionWithDuration(float duration, System.Action<float> action = null, System.Action completed = null)
        {
            yield return IDoActionWithDuration(duration, new Ref<bool>(true), action, completed);
        }

        public static IEnumerator IDoActionWithDuration(float duration, Ref<bool> enabled, System.Action<float> action = null, System.Action completed = null)
        {
            float timer = 0f;
            float t = 0f;
            while (timer < duration && (enabled == null || enabled.Value))
            {
                timer += Time.deltaTime;
                t = timer / duration;

                action?.Invoke(t);

                yield return null;
            }

            completed?.Invoke();
        }
    }

    public class Ref<T>
    {
        private T _backing;
        public T Value { get { return _backing; } set { _backing = value; } }
        public Ref(T reference)
        {
            _backing = reference;
        }
    }
}