using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ClocknestGames.Library.Utils
{
    public static class GameObjectExtension
    {
        public static Vector3 GetClosestPoint(this Collider self, Collider other)
        {
            Vector3 ptA = other.ClosestPoint(self.bounds.center);
            Vector3 ptB = self.ClosestPoint(other.bounds.center);
            Vector3 ptM = ptA + (ptB - ptA) / 2;
            // Answers:
            Vector3 closestAtA = self.ClosestPoint(ptM);
            return closestAtA;
        }

        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            var comp = go.GetComponent<T>();
            if (comp != null) return comp;

            return go.AddComponent<T>();
        }

        public static void RemoveAllChild(this GameObject go, bool immediate = false)
        {
            while (go.transform.childCount > 0)
            {
                if (immediate)
                    GameObject.DestroyImmediate(go.transform.GetChild(0).gameObject);
                else
                    GameObject.Destroy(go.transform.GetChild(0).gameObject);
            }
        }

        public static T GetComponentInParentRecursive<T>(this Transform obj) where T : Component
        {
            Transform objParent = obj.parent;
            while (objParent != null)
            {
                var component = objParent.GetComponent<T>();
                if (component != null)
                    return component;

                objParent = objParent.parent;
            }

            return null;
        }

        public static bool IsTrueNull(this UnityEngine.Object obj)
        {
            return (object)obj == null;
        }

        public static Color GetBaseColor(this Material material)
        {
            return material.GetColor("_BaseColor");
        }

        public static Color GetBaseColorWithMinAlpha(this Material material)
        {
            var baseColor = material.GetBaseColor();
            baseColor.a = .9f;

            return baseColor;
        }

        public static Color GetEmissionColor(this Material material)
        {
            return material.GetColor("_EmissionColor");
        }
    }

    public static class EnumerableExtension
    {
        private static readonly System.Random _rnd = new System.Random();

        public static T PickRandom<T>(this IEnumerable<T> source)
        {
            return source.PickRandom(1).Single();
        }

        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
        {
            return source.Shuffle().Take(count);
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            lock (_rnd)
            {
                return source.OrderBy(x => _rnd.Next());
            }
        }

        public static void Push<T>(this List<T> source, T item)
        {
            source.Add(item);
        }

        public static T Pop<T>(this List<T> source)
        {
            if (source.Count > 0)
            {
                T temp = source[source.Count - 1];
                source.RemoveAt(source.Count - 1);
                return temp;
            }
            else
                return default(T);
        }
    }

    public static class MathExtension
    {
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        public static T ClampMin<T>(this T val, T min) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else return val;
        }

        public static T ClampMax<T>(this T val, T max) where T : IComparable<T>
        {
            if (val.CompareTo(max) > 0) return max;
            else return val;
        }
    }

    public static class UIExtension
    {
        public static bool Intersects(this RectTransform source, RectTransform tested)
        {
            Vector3[] sourceRectCorners = new Vector3[4];
            source.GetWorldCorners(sourceRectCorners);

            Vector3[] testedRectCorners = new Vector3[4];
            tested.GetWorldCorners(testedRectCorners);


            // If one rectangle is on left side of other  
            if (sourceRectCorners[1].x > testedRectCorners[3].x || testedRectCorners[1].x > sourceRectCorners[3].x)
            {
                return false;
            }

            // If one rectangle is above other  
            if (sourceRectCorners[1].y < testedRectCorners[3].y || testedRectCorners[1].y < sourceRectCorners[3].y)
            {
                return false;
            }
            return true;
        }

        public static Toggle GetToggleOn(this ToggleGroup group)
        {
            if (group == null)
                return null;

            return group.GetComponentsInChildren<Toggle>().First(x => x.isOn);
        }

        public static void SetAlpha(this Image image, float alpha)
        {
            var color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }

    /// <summary>
    /// Game object extensions
    /// </summary>
    public static class GameObjectExtensions
    {
        static List<Component> m_ComponentCache = new List<Component>();

        /// <summary>
        /// Grabs a component without allocating memory uselessly
        /// </summary>
        /// <param name="this"></param>
        /// <param name="componentType"></param>
        /// <returns></returns>
		public static Component GetComponentNoAlloc(this GameObject @this, System.Type componentType)
        {
            @this.GetComponents(componentType, m_ComponentCache);
            var component = m_ComponentCache.Count > 0 ? m_ComponentCache[0] : null;
            m_ComponentCache.Clear();
            return component;
        }

        /// <summary>
        /// Grabs a component without allocating memory uselessly
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static T GetComponentNoAlloc<T>(this GameObject @this) where T : Component
        {
            @this.GetComponents(typeof(T), m_ComponentCache);
            var component = m_ComponentCache.Count > 0 ? m_ComponentCache[0] : null;
            m_ComponentCache.Clear();
            return component as T;
        }
    }

    public static class Extensions
    {
        /// <summary>
        /// Gets next value in enum. Only accepts enums.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <returns></returns>
        public static T Next<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<T>(Arr, src) + 1;
            return (Arr.Length == j) ? Arr[0] : Arr[j];
        }
    }
}
