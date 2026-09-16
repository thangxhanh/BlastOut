using System.Text;
using UnityEngine;

namespace Dacodelaac.Attributes
{
    public class NamedIdAttribute : PropertyAttribute
    {
        public NamedIdAttribute()
        {
        }

        /* Để ở đây (assembly runtime) chứ không ở PropertyDrawer, vì BaseMono.ResetId và
           BaseSO.ResetId cần gọi — mà assembly runtime không được phép tham chiếu assembly Editor. */
        public static string ToSnakeCase(string text)
        {
            if (text.Length < 2)
            {
                return text;
            }

            var sb = new StringBuilder();
            sb.Append(char.ToLowerInvariant(text[0]));
            for (var i = 1; i < text.Length; ++i)
            {
                var c = text[i];
                if (char.IsUpper(c))
                {
                    sb.Append('_');
                    sb.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }
    }
}
