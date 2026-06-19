using System.Text;
using System.Text.RegularExpressions;
using TodoListMcp.Core.Model;

namespace TodoListMcp.Core;

/// <summary>
/// Maps ToDoList's <c>COMMENTSTYPE</c> identifiers to friendly format names, and provides the
/// encoding used to author the text-native formats. The built-in <c>PLAIN_TEXT</c> aside, each
/// format is a "content control" identified by a GUID. The GUIDs and the <c>CUSTOMCOMMENTS</c>
/// encoding are verified against the abstractspoon/ToDoList_9.2 source (Core/RTFContentCtrl and
/// Plugins/ContentControl).
/// </summary>
public static class CommentFormat
{
    /// <summary>Friendly name for plain-text comments — the only format this server authors.</summary>
    public const string PlainText = "plain";

    /// <summary>Friendly name for the built-in Rich Text (RTF) content control.</summary>
    public const string Rich = "rich";

    /// <summary>Friendly name for the HTML content control.</summary>
    public const string Html = "html";

    /// <summary>Friendly name for the Markdown content control.</summary>
    public const string Markdown = "markdown";

    /// <summary>Friendly name for the Spreadsheet content control.</summary>
    public const string Spreadsheet = "spreadsheet";

    /// <summary>
    /// Friendly name for comments that carry a rich &lt;CUSTOMCOMMENTS&gt; payload but no recorded
    /// COMMENTSTYPE — formatted, but the specific format is not known. The plain-text mirror is not
    /// authoritative for these.
    /// </summary>
    public const string Unknown = "unknown";

    // ToDoList COMMENTSTYPE attribute values (uppercase, as ToDoList writes them).
    private const string PlainTextId = "PLAIN_TEXT";
    private const string RichId = "849CF988-79FE-418A-A40D-01FE3AFCAB2C";
    private const string HtmlId = "FE0B6B6E-2B61-4AEB-AA0D-98DBE5942F02";
    private const string MarkdownId = "BAA4E079-268B-4B9B-B7C8-6D15CCF058A2";
    private const string SpreadsheetId = "BBDCAEDF-B297-4E09-BBFB-B308358628B9";

    /// <summary>
    /// Maps a raw <c>COMMENTSTYPE</c> value to a friendly name (plain/rich/html/markdown/spreadsheet),
    /// or returns the (trimmed) raw value unchanged when it's an unrecognised content-control id.
    /// </summary>
    public static string ToFriendly(string raw) => raw.Trim().ToUpperInvariant() switch
    {
        PlainTextId => PlainText,
        RichId => Rich,
        HtmlId => Html,
        MarkdownId => Markdown,
        SpreadsheetId => Spreadsheet,
        _ => raw.Trim(),
    };

    /// <summary>The <c>COMMENTSTYPE</c> value ToDoList stores for an authorable format.</summary>
    public static string ToCommentsType(CommentContentFormat format) => format switch
    {
        CommentContentFormat.Markdown => MarkdownId,
        CommentContentFormat.Html => HtmlId,
        _ => PlainTextId,
    };

    /// <summary>
    /// Parses an authorable format name, accepting plain/markdown(md)/html. Returns false for
    /// anything else — including rich text and spreadsheet, which can be read but not authored.
    /// </summary>
    public static bool TryParseWritable(string? text, out CommentContentFormat format)
    {
        format = CommentContentFormat.Plain;
        if (string.IsNullOrWhiteSpace(text)) return false;
        switch (text.Trim().ToLowerInvariant())
        {
            case "plain": case "plaintext": case "plain_text": case "text": format = CommentContentFormat.Plain; return true;
            case "markdown": case "md": format = CommentContentFormat.Markdown; return true;
            case "html": case "htm": format = CommentContentFormat.Html; return true;
            default: return false;
        }
    }

    /// <summary>
    /// Encodes comment source the way ToDoList stores it in &lt;CUSTOMCOMMENTS&gt;: base64 of the
    /// UTF-16LE bytes (no BOM), matching the content controls' <c>GetContent()</c>.
    /// </summary>
    public static string EncodeCustomComments(string source) =>
        Convert.ToBase64String(Encoding.Unicode.GetBytes(source));

    /// <summary>
    /// A best-effort plain-text mirror for the &lt;COMMENTS&gt; element. Markdown source is already
    /// readable so it is kept as-is; HTML is tag-stripped. ToDoList regenerates this mirror from the
    /// content control on its next save, so it need only be a reasonable approximation.
    /// </summary>
    public static string ToPlainMirror(CommentContentFormat format, string source) => format switch
    {
        CommentContentFormat.Html => StripHtml(source),
        _ => source,
    };

    private static string StripHtml(string html)
    {
        // Turn common block boundaries into newlines, drop the remaining tags, then tidy whitespace.
        var withBreaks = Regex.Replace(html, @"(?i)<\s*(br|/p|/div|/li|/h[1-6]|/tr)\s*/?\s*>", "\n");
        var noTags = Regex.Replace(withBreaks, "<[^>]+>", "");
        var decoded = System.Net.WebUtility.HtmlDecode(noTags);
        decoded = Regex.Replace(decoded, @"[ \t]+", " ");
        var lines = decoded.Split('\n').Select(l => l.Trim());
        return string.Join("\n", lines).Trim();
    }
}
