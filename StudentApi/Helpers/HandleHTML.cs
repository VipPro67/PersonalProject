using System.Net;
using System.Text.Encodings.Web;
using Ganss.Xss;
namespace StudentApi.Helpers;

public static class HandleHTML
{
    public static string ToSafeHTML(this string html)
    {
        return HtmlEncoder.Default.Encode(html);
    }

    public static string StripHTML(this string html)
    {
        var sanitizer = new HtmlSanitizer();
        sanitizer.AllowedTags.Add("b");
        sanitizer.AllowedTags.Add("i");
        sanitizer.AllowedTags.Add("u");
        sanitizer.AllowedTags.Add("strong");
        sanitizer.AllowedTags.Add("em");
        return sanitizer.Sanitize(html);
    }

    public static string SanitizeAndEncodeHTML(this string html)
    {
        return html.StripHTML().ToSafeHTML();
    }

    public static string DecodeHTMLEntities(this string html)
    {
        return WebUtility.HtmlDecode(html);
    }
}