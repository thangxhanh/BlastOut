using System.Globalization;
using UnityEngine;

namespace _Root.Scripts.Pattern
{
    public static class Extensions
    {
        public static void Clear(this Transform transform)
        {
            foreach (Transform child in transform)
            {
                Object.Destroy(child.gameObject);
            }
        }
        
        public static string FormatMoney(this double value)
        {
            return value.ToString("N0", CultureInfo.GetCultureInfo("de"));
        }

        public static string FormatMoney(this int value)
        {
            return value.ToString("N0", CultureInfo.GetCultureInfo("de"));
        }
    }
}