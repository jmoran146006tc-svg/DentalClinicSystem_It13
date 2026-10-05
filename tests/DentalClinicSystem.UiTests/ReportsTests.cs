using DentalClinicSystem.Forms;
using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.UiTests;

public sealed class ReportsTests
{
    [Fact]
    public void EmptyRangeHasTextEmptyStatesAndFiveZeroCounts() => UiThread.Run(() =>
    {
        using var page = new ucReports(ServiceStub.For<IReportService>(), ClinicFixture.Actor());
        using var host = new Form { ClientSize = new(900, 800) }; host.Controls.Add(page); UiThread.Show(host);
        Assert.NotNull(page.Snapshot); Assert.Equal(5, page.Snapshot.Status.Count); Assert.All(page.Snapshot.Status, row => Assert.Equal(0, row.Total));
        Assert.All(UiThread.Controls(page).OfType<ClinicTable>(), grid => { Assert.Empty(grid.Records); Assert.Contains("range", grid.EmptyText); });
        Assert.Equal(1, UiThread.Named<TableLayoutPanel>(page, "reportCards").ColumnCount);
        Assert.All(UiThread.Controls(page).OfType<Skeleton>(), skeleton => Assert.False(skeleton.Visible));
    });
    [Fact]
    public void InvalidRangeDoesNotCallServiceAndShowsAlert() => UiThread.Run(() =>
    {
        var reports = ServiceStub.For<IReportService>(); using var page = new ucReports(reports, ClinicFixture.Actor());
        UiThread.Named<ClinicDatePicker>(page, "reportFrom").Value = new(2026, 10, 12);
        UiThread.Named<ClinicDatePicker>(page, "reportTo").Value = new(2026, 10, 5);
        using var host = new Form(); host.Controls.Add(page); UiThread.Show(host);
        Assert.Empty(((ServiceStub)(object)reports).Calls); Assert.True(UiThread.Named<InlineAlert>(page, "reportAlert").Visible); Assert.Null(page.Snapshot);
    });
    [Theory]
    [InlineData(Roles.Receptionist)]
    [InlineData(Roles.Dentist)]
    public void DeniedActorSeesErrorWithoutBilledContent(string role) => UiThread.Run(() =>
    {
        var reports = ServiceStub.For<IReportService>((nameof(IReportService.GetAppointmentStatusCountsAsync), Task.FromResult(RoleAccess.Denied<IReadOnlyList<AppointmentStatusCount>>())));
        using var page = new ucReports(reports, ClinicFixture.Actor(role)); using var host = new Form(); host.Controls.Add(page); UiThread.Show(host);
        Assert.Null(page.Snapshot); Assert.True(UiThread.Named<InlineAlert>(page, "reportAlert").Visible);
        Assert.False(UiThread.Named<TableLayoutPanel>(page, "reportCards").Visible);
        Assert.False(UiThread.Named<AppButton>(page, "reportExport").Enabled);
        Assert.Single(((ServiceStub)(object)reports).Calls);
    });
    [Fact]
    public void FailedRefreshRetainsSnapshotAndRetryRecovers() => UiThread.Run(() =>
    {
        var reports = ServiceStub.For<IReportService>(); var stub = (ServiceStub)(object)reports;
        using var page = new ucReports(reports, ClinicFixture.Actor()); using var host = new Form(); host.Controls.Add(page); UiThread.Show(host);
        var snapshot = page.Snapshot;
        stub.Results[nameof(IReportService.GetDentistWorkloadAsync)] = Task.FromResult(ServiceResult<IReadOnlyList<DentistWorkload>>.Fail("Unavailable"));
        var pending = page.RefreshPageAsync(); Application.DoEvents(); Assert.True(pending.IsCompletedSuccessfully);
        Assert.Same(snapshot, page.Snapshot); Assert.True(UiThread.Named<AppButton>(page, "reportRetry").Visible);
        stub.Results.Remove(nameof(IReportService.GetDentistWorkloadAsync)); UiThread.Named<AppButton>(page, "reportRetry").PerformClick(); Application.DoEvents();
        Assert.NotSame(snapshot, page.Snapshot); Assert.False(UiThread.Named<InlineAlert>(page, "reportAlert").Visible);
    });
    [Fact]
    public void LateResponseAfterDisposalDoesNotBind() => UiThread.Run(() =>
    {
        var completion = new TaskCompletionSource<ServiceResult<IReadOnlyList<AppointmentStatusCount>>>();
        var reports = ServiceStub.For<IReportService>((nameof(IReportService.GetAppointmentStatusCountsAsync), completion.Task));
        using var page = new ucReports(reports, ClinicFixture.Actor()); using var host = new Form(); host.Controls.Add(page); UiThread.Show(host);
        page.Dispose(); completion.SetResult(ServiceResult<IReadOnlyList<AppointmentStatusCount>>.Ok([])); Application.DoEvents();
        Assert.Null(page.Snapshot); Assert.Single(((ServiceStub)(object)reports).Calls);
    });
}
