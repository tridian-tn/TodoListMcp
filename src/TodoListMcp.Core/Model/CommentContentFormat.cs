namespace TodoListMcp.Core.Model;

/// <summary>
/// A comment format this server can <em>author</em>. ToDoList also has Rich Text and Spreadsheet
/// content controls, but their payloads are opaque (WordPad RTF / a ReoGrid workbook) and are not
/// writable here — they can only be read (flattened) or preserved.
/// </summary>
public enum CommentContentFormat
{
    /// <summary>Plain text (COMMENTSTYPE="PLAIN_TEXT"); no &lt;CUSTOMCOMMENTS&gt; payload.</summary>
    Plain,

    /// <summary>Markdown (the MDContentControl); source stored in &lt;CUSTOMCOMMENTS&gt;.</summary>
    Markdown,

    /// <summary>HTML (the HTMLContentControl); source stored in &lt;CUSTOMCOMMENTS&gt;.</summary>
    Html,
}
