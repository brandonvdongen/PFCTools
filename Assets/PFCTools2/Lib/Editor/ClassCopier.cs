using System.Reflection;

namespace PFCTools2.Utils
{
    public class ClassCopier
    {
        public static void Copy<T>(T sourceComp, T targetComp)
        {
            FieldInfo[] sourceFields = sourceComp.GetType().GetFields(BindingFlags.Public |
                                                             BindingFlags.NonPublic |
                                                             BindingFlags.Instance);
            int i = 0;
            for (i = 0; i < sourceFields.Length; i++)
            {
                object value = sourceFields[i].GetValue(sourceComp);
                sourceFields[i].SetValue(targetComp, value);
            }
        }
    }
}