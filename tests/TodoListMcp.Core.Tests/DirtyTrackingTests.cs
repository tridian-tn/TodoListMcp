using TodoListMcp.Core;

namespace TodoListMcp.Core.Tests;

/// <summary>
/// TodoListManager skips rewriting the file when an operation changed nothing, which relies on
/// every real mutation marking the document dirty (they all funnel through TouchRoot) while a
/// no-op leaves it clean. These tests guard that contract across the mutation surface.
/// </summary>
public class DirtyTrackingTests
{
    [Fact]
    public void Fresh_document_is_not_dirty()
    {
        Assert.False(TestData.Sample().IsDirty);
    }

    [Fact]
    public void Add_marks_dirty()
    {
        var doc = TestData.Sample();
        doc.AddTask(new() { Title = "New" });
        Assert.True(doc.IsDirty);
    }

    [Fact]
    public void Update_marks_dirty()
    {
        var doc = TestData.Sample();
        doc.UpdateTask(1, new() { Title = "Renamed" });
        Assert.True(doc.IsDirty);
    }

    [Fact]
    public void Complete_marks_dirty()
    {
        var doc = TestData.Sample();
        doc.CompleteTask(3);
        Assert.True(doc.IsDirty);
    }

    [Fact]
    public void Reopen_marks_dirty()
    {
        var doc = TestData.Sample();
        doc.ReopenTask(2);
        Assert.True(doc.IsDirty);
    }

    [Fact]
    public void Move_marks_dirty()
    {
        var doc = TestData.Sample();
        doc.MoveTask(3, newParentId: null);
        Assert.True(doc.IsDirty);
    }

    [Fact]
    public void Delete_existing_marks_dirty()
    {
        var doc = TestData.Sample();
        doc.DeleteTask(3);
        Assert.True(doc.IsDirty);
    }

    [Fact]
    public void Delete_missing_leaves_document_clean()
    {
        var doc = TestData.Sample();
        doc.DeleteTask(999);
        // Nothing changed, so the document stays clean and the manager can skip the save.
        Assert.False(doc.IsDirty);
    }
}
