using TodoListMcp.Core;
using TodoListMcp.Core.Model;

namespace TodoListMcp.Core.Tests;

public class SearchTests
{
    [Fact]
    public void Text_search_matches_title_and_comments_case_insensitively()
    {
        var doc = TestData.Sample();

        Assert.Equal(new[] { 1 }, Ids(doc.Search(new() { Text = "parent" })));      // title + comments
        Assert.Equal(new[] { 1 }, Ids(doc.Search(new() { Text = "NOTES" })));        // comments only
        Assert.Equal(new[] { 2 }, Ids(doc.Search(new() { Text = "child a" })));
    }

    [Fact]
    public void Filter_by_category()
    {
        var doc = TestData.Sample();
        Assert.Equal(new[] { 1 }, Ids(doc.Search(new() { Category = "Work" })));
        Assert.Empty(doc.Search(new() { Category = "Nope" }));
    }

    [Fact]
    public void Filter_by_person()
    {
        var doc = TestData.Sample();
        Assert.Equal(new[] { 2 }, Ids(doc.Search(new() { Person = "Jane" })));
        Assert.Equal(new[] { 3 }, Ids(doc.Search(new() { Person = "Mary" })));
    }

    [Fact]
    public void Filter_by_completion()
    {
        var doc = TestData.Sample();
        Assert.Equal(new[] { 2 }, Ids(doc.Search(new() { Completed = true })));
        Assert.Equal(new[] { 1, 3 }, Ids(doc.Search(new() { Completed = false })));
    }

    [Fact]
    public void Filter_by_minimum_priority()
    {
        var doc = TestData.Sample();
        // Priorities: parent=5, childA=8, childB=2.
        Assert.Equal(new[] { 1, 2 }, Ids(doc.Search(new() { MinPriority = 5 })));
        Assert.Equal(new[] { 2 }, Ids(doc.Search(new() { MinPriority = 8 })));
    }

    [Fact]
    public void Filter_by_status_flag_risk_and_external_id()
    {
        var doc = TestData.Sample();
        doc.UpdateTask(3, new() { Status = "In Progress", Risk = 7, Flag = true, ExternalId = "JIRA-12" });

        Assert.Equal(new[] { 3 }, Ids(doc.Search(new() { Status = "in progress" }))); // case-insensitive
        Assert.Equal(new[] { 3 }, Ids(doc.Search(new() { Flagged = true })));
        Assert.Equal(new[] { 1, 2 }, Ids(doc.Search(new() { Flagged = false })));
        Assert.Equal(new[] { 3 }, Ids(doc.Search(new() { MinRisk = 5 })));
        Assert.Equal(new[] { 3 }, Ids(doc.Search(new() { ExternalId = "JIRA-12" })));
        Assert.Empty(doc.Search(new() { ExternalId = "nope" }));
    }

    [Fact]
    public void Filter_by_version_and_allocated_by()
    {
        var doc = TestData.Sample();
        doc.UpdateTask(3, new() { Version = "2.0", AllocatedBy = "Alice" });

        Assert.Equal(new[] { 3 }, Ids(doc.Search(new() { Version = "2.0" })));
        Assert.Equal(new[] { 3 }, Ids(doc.Search(new() { AllocatedBy = "alice" })));   // case-insensitive
        Assert.Empty(doc.Search(new() { Version = "9.9" }));
        Assert.Empty(doc.Search(new() { AllocatedBy = "nobody" }));
    }

    [Fact]
    public void Filter_by_time_estimate_normalised_to_hours()
    {
        var doc = TestData.Sample();
        doc.UpdateTask(1, new() { TimeEstimate = 1, TimeEstimateUnit = TimeUnit.Days });      // 8h
        doc.UpdateTask(2, new() { TimeEstimate = 4, TimeEstimateUnit = TimeUnit.Hours });     // 4h
        doc.UpdateTask(3, new() { TimeEstimate = 120, TimeEstimateUnit = TimeUnit.Minutes }); // 2h

        Assert.Equal(new[] { 1 }, Ids(doc.Search(new() { MinEstimateHours = 5 })));     // only the 8h task
        Assert.Equal(new[] { 1, 2 }, Ids(doc.Search(new() { MinEstimateHours = 4 }))); // inclusive
        Assert.Equal(new[] { 2, 3 }, Ids(doc.Search(new() { MaxEstimateHours = 4 }))); // 4h and 2h
        Assert.Equal(new[] { 2 }, Ids(doc.Search(new() { MinEstimateHours = 3, MaxEstimateHours = 5 })));
    }

    [Fact]
    public void Filter_by_time_spent_and_excludes_tasks_without_a_value()
    {
        var doc = TestData.Sample();
        doc.UpdateTask(2, new() { TimeSpent = 1, TimeSpentUnit = TimeUnit.Weeks }); // 40h; tasks 1 and 3 have none

        Assert.Equal(new[] { 2 }, Ids(doc.Search(new() { MinSpentHours = 40 })));
        // A max filter still excludes tasks with no recorded time spent.
        Assert.Equal(new[] { 2 }, Ids(doc.Search(new() { MaxSpentHours = 100 })));
    }

    [Fact]
    public void Criteria_combine_with_and()
    {
        var doc = TestData.Sample();
        Assert.Empty(doc.Search(new() { Completed = true, Person = "Mary" }));
        Assert.Equal(new[] { 2 }, Ids(doc.Search(new() { Completed = true, Person = "Bob" })));
    }

    private static int[] Ids(IReadOnlyList<TodoTask> tasks) => tasks.Select(t => t.Id).OrderBy(x => x).ToArray();
}
