using System.Diagnostics;
using TodoListMcp.App.Tray;

namespace TodoListMcp.App;

/// <summary>
/// A small modal "About" dialog: product name, the git-derived build version (with commit hash),
/// and a link to the project. Built in code to match the rest of the app, which has no designer
/// surfaces. The version is shown in a read-only text box so it can be selected and copied into a
/// bug report.
/// </summary>
internal sealed class AboutForm : Form
{
    private const string ProjectUrl = "https://github.com/tridian-tn/TodoListMcpWin";

    public AboutForm()
    {
        SuspendLayout();

        Text = "About TodoList MCP";
        Icon = TrayIconFactory.Create();
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        AutoScaleMode = AutoScaleMode.Font;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(16);

        var logo = new PictureBox
        {
            Image = TrayIconFactory.CreateBitmap(48),
            Size = new Size(48, 48),
            SizeMode = PictureBoxSizeMode.Zoom,
            Margin = new Padding(0, 0, 16, 0),
        };

        var name = new Label
        {
            Text = "TodoList MCP",
            AutoSize = true,
            Font = new Font(Font.FontFamily, Font.Size + 3.5f, FontStyle.Bold),
            Margin = new Padding(0),
        };

        var tagline = new Label
        {
            Text = "An MCP server for AbstractSpoon ToDoList (.tdl) files.",
            AutoSize = true,
            Margin = new Padding(0, 4, 0, 10),
        };

        var versionText = $"Version {VersionInfo.Display}";
        var version = new TextBox
        {
            Text = versionText,
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            BackColor = SystemColors.Control,
            TabStop = false,
            Margin = new Padding(0, 0, 0, 10),
        };
        version.Width = TextRenderer.MeasureText(versionText, version.Font).Width + 4;

        var link = new LinkLabel
        {
            Text = ProjectUrl,
            AutoSize = true,
            Margin = new Padding(0),
        };
        link.LinkClicked += (_, _) => OpenUrl(ProjectUrl);

        var content = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0),
        };
        content.Controls.Add(name);
        content.Controls.Add(tagline);
        content.Controls.Add(version);
        content.Controls.Add(link);

        var close = new Button
        {
            Text = "Close",
            DialogResult = DialogResult.OK,
            AutoSize = true,
            Margin = new Padding(0),
        };

        var buttons = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 14, 0, 0),
        };
        buttons.Controls.Add(close);

        var root = new TableLayoutPanel
        {
            ColumnCount = 2,
            RowCount = 2,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Fill,
        };
        root.Controls.Add(logo, 0, 0);
        root.Controls.Add(content, 1, 0);
        root.Controls.Add(buttons, 0, 1);
        root.SetColumnSpan(buttons, 2);

        Controls.Add(root);

        AcceptButton = close;
        CancelButton = close;

        ResumeLayout(performLayout: true);
    }

    private static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch
        {
            // Opening the browser is best-effort, like the tray's other shell launches.
        }
    }
}
