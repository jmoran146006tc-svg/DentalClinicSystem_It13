using System.Reflection;
using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;
using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem.Tests;

public class FeedbackTests
{
    [Fact]
    public void EscapeClearsFocusedSearch() => PresentationTests.Sta(() =>
    {
        MotionSystem.Enabled = false;
        try
        {
            var services = TestServices.Empty;
            using var page = new DentalClinicSystem.Forms.ucPatientRecords(services.Patients, new User { Role = Roles.Admin });
            using var host = new Form { ClientSize = new(1440, 1000) }; host.Controls.Add(page); page.Dock = DockStyle.Fill; PageLayoutTests.ShowOffscreen(host);
            var search = PageLayoutTests.Descendants(page).OfType<TextBox>().Single(input => input.PlaceholderText == "Search patients");
            search.Text = "Ana"; search.Focus();
            var message = Message.Create(search.Handle, 0x100, (IntPtr)Keys.Escape, IntPtr.Zero); Application.FilterMessage(ref message);
            Assert.Equal("", search.Text);
        }
        finally { MotionSystem.UseSystemPreference(); }
    });
    [Fact]
    public void EnterInvokesLoginAndFailureKeepsGenericInlineFeedback() => PresentationTests.Sta(() =>
    {
        MotionSystem.Enabled = false;
        try
        {
            var auth = TestServices.Create<IAuthService>((nameof(IAuthService.LoginAsync), Task.FromResult(ServiceResult<User>.Fail("Internal auth detail"))));
            using var login = new frmLogin(TestServices.Empty with { Auth = auth }); PageLayoutTests.ShowOffscreen(login);
            PageLayoutTests.Named<TextBox>(login, "txtUsername").Text = "admin";
            PageLayoutTests.Named<TextBox>(login, "txtPassword").Text = "test-only";
            typeof(Form).GetMethod("ProcessDialogKey", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(login, [Keys.Enter]);
            Assert.Equal(1, ((TestServices)(object)auth).Calls.GetValueOrDefault(nameof(IAuthService.LoginAsync)));
            var alert = PageLayoutTests.Descendants(login).OfType<InlineAlert>().Single(); Assert.True(alert.Visible); Assert.DoesNotContain("Internal", alert.Text);
            Assert.True(login.Enabled); Assert.False(ButtonStyler.IsBusy(PageLayoutTests.Named<Button>(login, "btnLogin")));
        }
        finally { MotionSystem.UseSystemPreference(); }
    });
    [Fact]
    public void BusyScopePreventsDuplicateActionsAndAlwaysRestoresOwner() => PresentationTests.Sta(() =>
    {
        MotionSystem.Enabled = false;
        try
        {
            using var host = new Form(); using var owner = new Panel { Dock = DockStyle.Fill };
            host.Controls.Add(owner); var button = new Button { Text = "Save" }; ButtonStyler.Attach(button, ButtonVariant.Primary); owner.Controls.Add(button);
            var alert = UiFactory.Alert(); alert.Visible = false; owner.Controls.Add(alert); UiMessages.RegisterAlertHost(owner, alert);
            PageLayoutTests.ShowOffscreen(host); var pending = new TaskCompletionSource();
            var width = button.Width; var first = UiAction.RunAsync(owner, () => pending.Task, button);
            Assert.True(ButtonStyler.IsBusy(button)); Assert.False(owner.Enabled); Assert.Equal(width, button.Width);
            var called = false; var duplicate = UiAction.RunAsync(owner, () => { called = true; return Task.CompletedTask; }, button); Assert.True(duplicate.IsCompletedSuccessfully); Assert.False(called);
            pending.SetException(new InvalidOperationException("test exception"));
            for (var i = 0; i < 1000 && !first.IsCompleted; i++) Application.DoEvents();
            Assert.True(first.IsCompletedSuccessfully); Assert.True(owner.Enabled); Assert.False(ButtonStyler.IsBusy(button)); Assert.True(alert.Visible);
            Assert.Equal(0, MotionSystem.Animator.ActiveCount);
        }
        finally { MotionSystem.UseSystemPreference(); }
    });
}
