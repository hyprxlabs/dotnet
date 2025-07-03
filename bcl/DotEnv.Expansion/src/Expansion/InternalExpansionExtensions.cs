namespace Hyprx.DotEnv.Expansion;

internal static class InternalExpansionExtensions
{
     public static bool EqualsFold(this string? str1, string? str2)
     {
         if (str1 is null && str2 is null)
         {
             return true;
         }

         if (str1 is null || str2 is null)
         {
             return false;
         }

         return string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
     }
}
