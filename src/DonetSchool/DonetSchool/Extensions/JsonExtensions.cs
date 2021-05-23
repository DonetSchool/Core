using System.Text.Json;

namespace DonetSchool.Extensions
{
    public static class JsonExtensions
    {
        public static string ToJosn<T>(this T @object) where T : class
        {
            if (@object == null)
            {
                return string.Empty;
            }
            return JsonSerializer.Serialize(@object);
        }
    }
}