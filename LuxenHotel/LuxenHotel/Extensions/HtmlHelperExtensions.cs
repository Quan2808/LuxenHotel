using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace LuxenHotel.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static Task<Microsoft.AspNetCore.Html.IHtmlContent> PartialAsyncFromArea(
            this IHtmlHelper htmlHelper,
            string areaName,
            string relativePartialPath)
        {
            var fullPath = $"~/Areas/{areaName}/Views/Shared/{relativePartialPath}.cshtml";
            return htmlHelper.PartialAsync(fullPath);
        }
    }
}
