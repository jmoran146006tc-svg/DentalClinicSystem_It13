using DentalClinicSystem.Forms;
using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Charts;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;
using System.Reflection;

namespace DentalClinicSystem.UiTests;

public sealed class ReportLayoutTests
{
    [Theory]
    [InlineData(1f)] [InlineData(1.25f)] [InlineData(1.5f)]
    public void RangeSegmentsFitMeasuredLabels(float scale) => UiThread.Run(() =>
    {
        using var toolbar = new ReportToolbar(new(), new(), new("Refresh"), new("Export CSV"));
        using var host = new Form { ClientSize = new(1280, 400) }; host.Controls.Add(toolbar); UiThread.Show(host);
        toolbar.Scale(new SizeF(scale, scale));
        foreach (AntdUI.SegmentedItem item in toolbar.Ranges.Items)
        {
            var rectangle = (Rectangle)typeof(AntdUI.SegmentedItem).GetProperty("RectText", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(item)!;
            Assert.True(TextRenderer.MeasureText(item.Text, toolbar.Ranges.Font).Width <= rectangle.Width);
            Assert.Equal(0, rectangle.Top); Assert.Equal(toolbar.Ranges.Height, rectangle.Height);
        }
        Assert.Equal(AntdUI.TAlignMini.None, toolbar.Ranges.BarPosition);
        Assert.Equal(AntdUI.TAlignMini.None, toolbar.Ranges.IconAlign); Assert.Equal(Padding.Empty, toolbar.Ranges.Margin);
    });
    [Theory]
    [InlineData(1280, 2, true)]
    [InlineData(900, 1, true)]
    [InlineData(1280, 2, false)]
    [InlineData(900, 1, false)]
    public void CardsUseResponsiveColumnsAndSingleEmptyStates(int width, int columns, bool filled) => UiThread.Run(() =>
    {
        using var page = new ucReports(filled ? Data() : ServiceStub.For<IReportService>(), ClinicFixture.Actor());
        using var host = new Form { ClientSize = new(width, 1000) }; host.Controls.Add(page); UiThread.Show(host);
        var grid = UiThread.Named<TableLayoutPanel>(page, "reportCards"); Assert.Equal(columns, grid.ColumnCount);
        var sections = UiThread.Controls(page).OfType<ReportSection>().ToArray(); Assert.Equal(4, sections.Length);
        if (columns == 2) Assert.InRange(Math.Abs(sections[0].Width - sections[1].Width), 0, 1);
        Assert.All(sections.Take(3), card => Assert.Equal(Metrics.ReportCardHeight, card.Height));
        Assert.Equal(grid.ClientSize.Width, sections[3].Width); EnglishUi.AssertTree(page);
        Assert.All(sections, card => Assert.Equal(filled ? 0 : 1, UiThread.Controls(card).OfType<EmptyState>().Count(empty => empty.Visible)));
        UiThread.Capture(host, $"F-reports-{width}-{(filled ? "filled" : "empty")}");
        page.ScrollControlIntoView(sections[3]); Application.DoEvents(); UiThread.Capture(host, $"F-reports-{width}-{filled}-bottom");
    });
    [Theory]
    [InlineData(1280)]
    [InlineData(1100)]
    public void ToolbarHasOneInputRowAndRangeSelectionTracksDates(int width) => UiThread.Run(() =>
    {
        var reports = Data(); using var page = new ucReports(reports, ClinicFixture.Actor());
        using var host = new Form { ClientSize = new(width, 1000) }; host.Controls.Add(page); UiThread.Show(host);
        var toolbar = UiThread.Named<ReportToolbar>(page, "reportToolbar");
        var centers = toolbar.Controls.Cast<Control>().Where(c => c.Visible).Select(c => c is FormField field
            ? c.Top + field.Box.Top + field.Box.Height / 2d : c.Top + c.Height / 2d).ToArray();
        Assert.True(centers.Max() - centers.Min() <= 1, string.Join("; ", toolbar.Controls.Cast<Control>().Select(c => $"{c.Name}/{c.GetType().Name}: {c.Bounds} margin {c.Margin}, available {toolbar.ClientSize.Width}")));
        Assert.Equal(toolbar.ClientSize.Width, UiThread.Named<AppButton>(page, "reportExport").Right);
        Assert.Equal(1, toolbar.Ranges.SelectIndex);
        var from = UiThread.Named<ClinicDatePicker>(page, "reportFrom"); from.Value = from.Value.AddDays(-1);
        Assert.Equal(3, toolbar.Ranges.SelectIndex);
        foreach (var index in new[] { 0, 2, 1 })
        {
            toolbar.Ranges.SelectIndex = index; Application.DoEvents(); Assert.Equal(index, toolbar.Ranges.SelectIndex);
            var expected = ReportsPresentation.Range((ReportRange)index, DateTime.Today);
            Assert.Equal(expected.From, from.Value.Date); Assert.Equal(expected.From, page.Snapshot?.From);
        }
    });
    [Fact]
    public void LegendRowsAlignAndKpiValuesFit() => UiThread.Run(() =>
    {
        using var page = new ucReports(Data(), ClinicFixture.Actor());
        using var host = new Form { ClientSize = new(1280, 1000) }; host.Controls.Add(page); UiThread.Show(host);
        var legend = UiThread.Named<ReportLegend>(page, "reportLegend"); var heights = legend.GetRowHeights();
        Assert.Equal(5, heights.Length); Assert.InRange(heights.Max() - heights.Min(), 0, 1);
        Assert.All(heights, height => Assert.True(height >= Metrics.ControlHeight));
        for (var i = 0; i < 5; i++)
        {
            var label = Assert.IsType<Label>(legend.GetControlFromPosition(1, i));
            var count = Assert.IsType<Label>(legend.GetControlFromPosition(2, i));
            Assert.Equal(AppointmentStatus.Display(AppointmentStatus.All[i]), label.Text);
            Assert.Equal(label.Top + label.Height / 2d, count.Top + count.Height / 2d);
            if (i > 0) Assert.True(label.Top >= legend.GetControlFromPosition(1, i - 1)?.Bottom);
        }
        foreach (var kpi in UiThread.Controls(page).OfType<KpiCard>())
        {
            AssertValueFits(kpi);
            var icon = UiThread.Controls(kpi).OfType<IconTile>().Single();
            Assert.True(icon.Visible); Assert.True(icon.Parent?.ClientRectangle.Contains(icon.Bounds));
        }
        Assert.Equal(4, UiThread.Controls(page).OfType<KpiCard>().Count());
    });
    [Fact]
    public void NarrowStatusCardStacksAFullSizeDonutAboveTheLegend() => UiThread.Run(() =>
    {
        using var page = new ucReports(Data(), ClinicFixture.Actor()); using var host = new Form { ClientSize = new(550, 1000) };
        host.Controls.Add(page); UiThread.Show(host);
        var group = UiThread.Named<ReportStatusContent>(page, "reportStatusGroup");
        var chart = UiThread.Named<DonutChart>(page, "reportStatus"); var legend = UiThread.Named<ReportLegend>(page, "reportLegend");
        Assert.Equal(chart.Width, chart.Height); Assert.Equal(Metrics.ChartHeight, chart.Height);
        Assert.True(chart.Bottom <= legend.Top); Assert.True(group.ClientRectangle.Contains(chart.Bounds));
        Assert.True(group.ClientRectangle.Contains(legend.Bounds));
        UiThread.Capture(host, "phase10-reports-narrow");
    });
    [Theory]
    [InlineData(1f)]
    [InlineData(1.25f)]
    [InlineData(1.5f)]
    public void KpiValueFitsScaledLayout(float scale) => UiThread.Run(() =>
    {
        using var card = new KpiCard("Billed in range", "Net billed, not collected") { Width = Metrics.FormWidth };
        card.SetValue(123456.78, value => DisplayFormat.Currency((decimal)value), animate: false);
        using var host = new Form { ClientSize = new(600, 400), AutoScaleMode = AutoScaleMode.None }; host.Controls.Add(card);
        UiThread.Show(host); card.Scale(new SizeF(scale, scale));
        using var font = new Font(Typography.KpiNumber.FontFamily, Typography.KpiNumber.Size * scale, Typography.KpiNumber.Style);
        var value = UiThread.Controls(card).OfType<Label>().Single(label => label.Font.Size == Typography.KpiNumber.Size);
        value.Font = font; card.PerformLayout(); AssertValueFits(card);
    });
    [Fact]
    public void TruncatedTopListDoesNotClaimAnExactTreatmentTotal() => UiThread.Run(() =>
    {
        using var page = new ucReports(Data(10), ClinicFixture.Actor()); using var host = new Form { ClientSize = new(1280, 1000) };
        host.Controls.Add(page); UiThread.Show(host);
        var card = UiThread.Controls(page).OfType<KpiCard>().Single(card => UiThread.Controls(card).OfType<Label>().Any(label => label.Text == "Treatments performed"));
        Assert.Contains(UiThread.Controls(card).OfType<Label>(), label => label.Text == "n/a");
        Assert.Equal(10, page.Snapshot?.Types.Count);
    });
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(264)]
    [InlineData(600)]
    public void AxisLabelsStayInsidePlotAndNeverOverlap(int width)
    {
        var plot = new Rectangle(80, 8, width, 240);
        var labels = ChartGeometry.AxisLabels(plot, 30, Metrics.ChartLabelWidth, Typography.Caption.Height);
        for (var i = 0; i < labels.Count; i++)
        {
            Assert.InRange(labels[i].Bounds.Left, plot.Left, plot.Right); Assert.InRange(labels[i].Bounds.Right, plot.Left, plot.Right);
            if (i > 0) Assert.True(labels[i].Bounds.Left >= labels[i - 1].Bounds.Right);
        }
    }
    [Fact]
    public void FirstLoadSkeletonCardsKeepTheirOuterBounds() => UiThread.Run(() =>
    {
        var ready = new TaskCompletionSource<ServiceResult<IReadOnlyList<AppointmentStatusCount>>>(); var reports = Data();
        ((ServiceStub)(object)reports).Results[nameof(IReportService.GetAppointmentStatusCountsAsync)] = ready.Task;
        using var page = new ucReports(reports, ClinicFixture.Actor()); using var host = new Form { ClientSize = new(1280, 1000) };
        host.Controls.Add(page); UiThread.Show(host);
        var sections = UiThread.Controls(page).OfType<ReportSection>().ToArray(); var bounds = sections.Select(card => card.Bounds).ToArray();
        Assert.All(sections, card => Assert.Contains(UiThread.Controls(card).OfType<Skeleton>(), skeleton => skeleton.Visible));
        ready.SetResult(ServiceResult<IReadOnlyList<AppointmentStatusCount>>.Ok([new(AppointmentStatus.Completed, 25)])); Application.DoEvents();
        Assert.NotNull(page.Snapshot); Assert.Equal(bounds, sections.Select(card => card.Bounds));
    });
    [Fact]
    public void NarrowTreatmentCardStacksChartAndTableWithoutOverlap() => UiThread.Run(() =>
    {
        using var page = new ucReports(Data(10), ClinicFixture.Actor()); using var host = new Form { ClientSize = new(700, 1000) };
        host.Controls.Add(page); UiThread.Show(host);
        var layout = UiThread.Named<ReportTypesContent>(page, "reportTypesLayout");
        Assert.Equal(1, layout.ColumnCount); Assert.Equal(2, layout.RowCount);
        var chart = UiThread.Named<BarChart>(page, "reportTypesChart"); var table = UiThread.Named<ClinicTable>(page, "reportTypes");
        Assert.True(chart.Bottom <= table.Top); Assert.Equal(10, table.Records.Count);
    });
    [Theory]
    [InlineData(1280)]
    [InlineData(900)]
    public void DashboardHeaderKpiFacesAndCalendarSharePageEdges(int width) => UiThread.Run(() =>
    {
        var services = ClinicFixture.Services();
        using var page = new ucDashboardHome(services.Appointments, services.Patients, services.Dentists,
            services.Reports, services.PatientHistory, ClinicFixture.Actor());
        using var host = new Form { ClientSize = new(width, 1000) }; host.Controls.Add(page); UiThread.Show(host);
        var header = UiThread.Controls(page).OfType<PageHeader>().Single();
        var strip = UiThread.Named<KpiStrip>(page, "dashboardKpis");
        var calendar = UiThread.Named<RoundedPanel>(page, "dashboardCalendar");
        foreach (var control in new Control[] { header, strip, calendar })
        {
            Assert.Equal(Space.Page.Left, page.PointToClient(control.PointToScreen(Point.Empty)).X);
            Assert.Equal(page.ClientSize.Width - Space.Page.Horizontal, control.Width);
        }
        var cards = UiThread.Controls(strip).OfType<KpiCard>().ToArray();
        var inset = Elevation.Padding(ElevationLevel.E1);
        Assert.Equal(strip.PointToScreen(Point.Empty).X, cards[0].PointToScreen(new(inset, inset)).X);
        Assert.Equal(strip.PointToScreen(new(strip.Width, 0)).X, cards.Max(card => card.PointToScreen(new(card.Width - inset, inset)).X));
        UiThread.Capture(host, $"F-dashboard-{width}");
    });
    private static void AssertValueFits(KpiCard card)
    {
        var value = UiThread.Controls(card).OfType<Label>().OrderByDescending(label => label.Font.Size).First();
        var measured = TextRenderer.MeasureText(value.Text, value.Font, Size.Empty, TextFormatFlags.SingleLine);
        Assert.True(value.Height >= measured.Height, $"KPI {value.Text}: height {value.Height} < text {measured.Height}");
        Assert.True(value.Width >= measured.Width, $"KPI {value.Text}: width {value.Width} < text {measured.Width}");
    }
    private static IReportService Data(int types = 5) => ServiceStub.For<IReportService>(
        (nameof(IReportService.GetAppointmentStatusCountsAsync), Task.FromResult(ServiceResult<IReadOnlyList<AppointmentStatusCount>>.Ok(
            [new(AppointmentStatus.Completed, 25), new(AppointmentStatus.Scheduled, 4), new(AppointmentStatus.Cancelled, 2)]))),
        (nameof(IReportService.GetRevenueByDayAsync), Task.FromResult(ServiceResult<IReadOnlyList<RevenueDay>>.Ok(
            Enumerable.Range(0, 25).Select(i => new RevenueDay(DateTime.Today.AddDays(-i), 1500 + i * 150)).ToArray()))),
        (nameof(IReportService.GetTopTreatmentTypesAsync), Task.FromResult(ServiceResult<IReadOnlyList<TopTreatmentType>>.Ok(
            Enumerable.Range(0, types).Select(i => new TopTreatmentType("Treatment " + (i + 1), 10 - i, (10 - i) * 800)).ToArray()))),
        (nameof(IReportService.GetDentistWorkloadAsync), Task.FromResult(ServiceResult<IReadOnlyList<DentistWorkload>>.Ok(
            [new("Maria Santos", 15, 13, 20000), new("Carlos Reyes", 16, 12, 15000), new("No activity", 0, 0, 0)]))));
}
