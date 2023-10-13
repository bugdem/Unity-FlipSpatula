using System.Collections.Generic;
using System.Linq;

namespace ClocknestGames.Library.Editor
{
    public static class StringInListData
    {
#if UNITY_EDITOR

        public static string[] AllSceneNames()
        {
            var temp = new List<string>
            {
                " "
            };

            foreach (UnityEditor.EditorBuildSettingsScene S in UnityEditor.EditorBuildSettings.scenes)
            {
                if (S.enabled)
                {
                    string name = S.path.Substring(S.path.LastIndexOf('/') + 1);
                    name = name.Substring(0, name.Length - 6);
                    temp.Add(name);
                }
            }
            return temp.ToArray();
        }

        public static string[] AllInheritanceOfBase(System.Type parent)
        {
            var temp = new List<string>
            {
                " "
            };

            System.Type[] types = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();
            System.Type[] childScriptTypes = (from System.Type type in types where type.IsSubclassOf(parent) select type).ToArray();

            foreach (var childScriptType in childScriptTypes)
            {
                temp.Add(childScriptType.ToString());
            }

            return temp.ToArray();
        }

#endif
    }
}