using System.Text;
using TodoListMcp.Core;

namespace TodoListMcp.Core.Tests;

/// <summary>
/// Covers issue #30: surfacing the editable comment source for the formats this server can also
/// author (Markdown/HTML), so a read → edit → write round-trip is lossless. Exercises the
/// <see cref="CommentFormat.DecodeCustomComments"/> helper and the <see cref="Model.TodoTask.CommentsSource"/>
/// read projection against hand-written XML; the real-fixture and full round-trip coverage lives in
/// <see cref="MultiFormatCommentFileTests"/>.
/// </summary>
public class CommentSourceTests
{
    private const string HtmlId = "FE0B6B6E-2B61-4AEB-AA0D-98DBE5942F02";
    private const string MarkdownId = "BAA4E079-268B-4B9B-B7C8-6D15CCF058A2";
    private const string SpreadsheetId = "BBDCAEDF-B297-4E09-BBFB-B308358628B9";
    private const string RichId = "849CF988-79FE-418A-A40D-01FE3AFCAB2C";

    private static TodoListDocument Parse(string taskAttributesAndChildren) =>
        TodoListDocument.Parse(
            $"""
            <?xml version="1.0" encoding="utf-16"?>
            <TODOLIST PROJECTNAME="P" NEXTUNIQUEID="2"><TASK ID="1" {taskAttributesAndChildren}</TASK></TODOLIST>
            """,
            TestData.Clock);

    /// <summary>Builds a task carrying the given format id with <c>source</c> encoded into CUSTOMCOMMENTS.</summary>
    private static TodoListDocument Formatted(string typeId, string source) =>
        Parse($"""TITLE="T" COMMENTSTYPE="{typeId}"><COMMENTS>mirror</COMMENTS><CUSTOMCOMMENTS>{CommentFormat.EncodeCustomComments(source)}</CUSTOMCOMMENTS>""");

    // ---- decode helper (item 6) -------------------------------------------------------------

    [Theory]
    [InlineData("plain ascii")]
    [InlineData("# Heading\n\n- **bold**\n- _italic_")]
    [InlineData("<p>Hello <b>world</b> — ünïcödé ✓</p>")]
    [InlineData("line1\r\nline2\ttabbed")]
    public void Encode_then_decode_round_trips_the_source(string source)
    {
        // (Empty/whitespace sources never reach <CUSTOMCOMMENTS> — the write path omits the element —
        // so the decoder maps them to null instead; see Decode_returns_null_for_missing_payload.)
        Assert.Equal(source, CommentFormat.DecodeCustomComments(CommentFormat.EncodeCustomComments(source)));
    }

    [Fact]
    public void Decode_is_the_byte_exact_inverse_of_the_encoder()
    {
        const string source = "**round** _trip_";
        var encoded = CommentFormat.EncodeCustomComments(source);
        Assert.Equal(Encoding.Unicode.GetString(Convert.FromBase64String(encoded)),
                     CommentFormat.DecodeCustomComments(encoded));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Decode_returns_null_for_missing_payload(string? payload)
    {
        Assert.Null(CommentFormat.DecodeCustomComments(payload));
    }

    [Theory]
    [InlineData("not base64!!!")]
    [InlineData("====")]
    [InlineData("abc")]   // length not a multiple of 4
    public void Decode_returns_null_for_malformed_base64(string payload)
    {
        Assert.Null(CommentFormat.DecodeCustomComments(payload));
    }

    // ---- read projection (item 7) -----------------------------------------------------------

    [Fact]
    public void Markdown_task_exposes_the_decoded_source()
    {
        var source = "# Plan\n\n- **a**\n- _b_";
        var t = Formatted(MarkdownId, source).GetTask(1)!;
        Assert.Equal("markdown", t.CommentsFormat);
        Assert.Equal(source, t.CommentsSource);
        Assert.Equal("mirror", t.Comments);   // mirror retained, distinct from the source
    }

    [Fact]
    public void Html_task_exposes_the_decoded_source()
    {
        var source = "<p>Hello <b>world</b></p>";
        var t = Formatted(HtmlId, source).GetTask(1)!;
        Assert.Equal("html", t.CommentsFormat);
        Assert.Equal(source, t.CommentsSource);
        Assert.Equal("mirror", t.Comments);
    }

    [Fact]
    public void Plain_task_has_no_source()
    {
        var t = Parse("""TITLE="T" COMMENTSTYPE="PLAIN_TEXT"><COMMENTS>just text</COMMENTS>""").GetTask(1)!;
        Assert.Equal("plain", t.CommentsFormat);
        Assert.Null(t.CommentsSource);
        Assert.Equal("just text", t.Comments);
    }

    [Theory]
    [InlineData(RichId)]
    [InlineData(SpreadsheetId)]
    public void Preserve_only_formats_have_no_source(string typeId)
    {
        // Rich/spreadsheet payloads are opaque (RTF / a ReoGrid workbook) — we can't author them
        // back, so we surface only the mirror and leave CommentsSource null even though a payload
        // exists on disk.
        var t = Formatted(typeId, "irrelevant").GetTask(1)!;
        Assert.Null(t.CommentsSource);
        Assert.Equal("mirror", t.Comments);
    }

    [Fact]
    public void Unknown_content_control_has_no_source()
    {
        var t = Parse("""TITLE="T" COMMENTSTYPE="SOME-OTHER-PLUGIN-GUID"><COMMENTS>x</COMMENTS><CUSTOMCOMMENTS>blob</CUSTOMCOMMENTS>""").GetTask(1)!;
        Assert.Equal("SOME-OTHER-PLUGIN-GUID", t.CommentsFormat);
        Assert.Null(t.CommentsSource);
    }

    [Fact]
    public void Custom_payload_without_type_has_no_source()
    {
        // Formatted-but-unknown (no COMMENTSTYPE): preserve-only, so no recoverable source.
        var t = Parse("""TITLE="T" COMMENTS="mirror"><CUSTOMCOMMENTS>blob</CUSTOMCOMMENTS>""").GetTask(1)!;
        Assert.Equal("unknown", t.CommentsFormat);
        Assert.Null(t.CommentsSource);
    }

    [Fact]
    public void Task_without_comments_has_no_source()
    {
        var t = Parse("""TITLE="T">""").GetTask(1)!;
        Assert.Null(t.CommentsFormat);
        Assert.Null(t.CommentsSource);
    }
}
