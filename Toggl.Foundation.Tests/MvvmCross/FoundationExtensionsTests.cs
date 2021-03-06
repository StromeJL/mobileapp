﻿using System;
using System.Reactive.Concurrency;
using FluentAssertions;
using MvvmCross.Core.Navigation;
using NSubstitute;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.Login;
using Toggl.Foundation.MvvmCross;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Foundation.Services;
using Toggl.Foundation.Shortcuts;
using Toggl.Foundation.Suggestions;
using Toggl.Foundation.Tests.Generators;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Settings;
using Toggl.Ultrawave.Network;
using Xunit;

namespace Toggl.Foundation.Tests.MvvmCross
{
    public class FoundationExtensionsTests : BaseMvvmCrossTests
    {
        private readonly MvvmCrossFoundation mvvmCrossFoundation;

        private readonly Version version = Version.Parse("1.0");
        private readonly UserAgent userAgent = new UserAgent("Some Client", "1.0");
        private readonly IScheduler scheduler = Substitute.For<IScheduler>();
        private readonly IApiFactory apiFactory = Substitute.For<IApiFactory>();
        private readonly ITimeService timeService = Substitute.For<ITimeService>();
        private readonly IMailService mailService = Substitute.For<IMailService>();
        private readonly ITogglDatabase database = Substitute.For<ITogglDatabase>();
        private readonly IGoogleService googleService = Substitute.For<IGoogleService>();
        private readonly ILicenseProvider licenseProvider = Substitute.For<ILicenseProvider>();
        private readonly IAnalyticsService analyticsService = Substitute.For<IAnalyticsService>();
        private readonly IBackgroundService backgroundService = Substitute.For<IBackgroundService>();
        private readonly IPlatformConstants platformConstants = Substitute.For<IPlatformConstants>();
        private readonly IApplicationShortcutCreator applicationShortcutCreator = Substitute.For<IApplicationShortcutCreator>();
        private readonly ISuggestionProviderContainer suggestionProviderContainer = Substitute.For<ISuggestionProviderContainer>();

        private readonly IDialogService dialogService = Substitute.For<IDialogService>();
        private readonly IBrowserService browserService = Substitute.For<IBrowserService>();
        private readonly IKeyValueStorage keyValueStorage = Substitute.For<IKeyValueStorage>();
        private readonly IUserPreferences userPreferences = Substitute.For<IUserPreferences>();
        private readonly IOnboardingStorage onboardingStorage = Substitute.For<IOnboardingStorage>();
        private readonly IMvxNavigationService navigationService = Substitute.For<IMvxNavigationService>();
        private readonly IErrorHandlingService errorHandlingService = Substitute.For<IErrorHandlingService>();
        private readonly IAccessRestrictionStorage accessRestrictionStorage = Substitute.For<IAccessRestrictionStorage>();

        public FoundationExtensionsTests()
        {
            mvvmCrossFoundation =
                constructFoundation().StartRegisteringPlatformServices()
                    .WithDialogService(dialogService)
                    .WithBrowserService(browserService)
                    .WithKeyValueStorage(keyValueStorage)
                    .WithUserPreferences(userPreferences)
                    .WithOnboardingStorage(onboardingStorage)
                    .WithNavigationService(navigationService)
                    .WithErrorHandlingService(errorHandlingService)
                    .WithAccessRestrictionStorage(accessRestrictionStorage)
                    .Build();
        }

        [Theory, LogIfTooSlow]
        [ClassData(typeof(NineParameterConstructorTestData))]
        public void ThrowsIfAnyOfTheParametersIsNull(
            bool useFoundation,
            bool useDialogService,
            bool useBrowserService,
            bool useKeyValueStorage,
            bool useUserPreferences,
            bool useOnboardingStorage,
            bool useNavigationService,
            bool useApiErrorHandlingService,
            bool useAccessRestrictionStorage)
        {
            var foundation = useFoundation ? constructFoundation() : null;
            var actualDialogService = useDialogService ? Substitute.For<IDialogService>() : null;
            var actualBrowserService = useBrowserService ? Substitute.For<IBrowserService>() : null;
            var actualKeyValueStorage = useKeyValueStorage ? Substitute.For<IKeyValueStorage>() : null;
            var actualUserPreferences = useUserPreferences ? Substitute.For<IUserPreferences>() : null;
            var actualOnboardingStorage = useOnboardingStorage ? Substitute.For<IOnboardingStorage>() : null;
            var actualNavigationService = useNavigationService ? Substitute.For<IMvxNavigationService>() : null;
            var actualApiErrorHandlingService = useApiErrorHandlingService ? Substitute.For<IErrorHandlingService>() : null;
            var actualAccessRestrictionStorage = useAccessRestrictionStorage ? Substitute.For<IAccessRestrictionStorage>() : null;

            Action tryingToConstructWithEmptyParameters = () =>
                foundation.StartRegisteringPlatformServices()
                    .WithDialogService(actualDialogService)
                    .WithBrowserService(actualBrowserService)
                    .WithKeyValueStorage(actualKeyValueStorage)
                    .WithUserPreferences(actualUserPreferences)
                    .WithOnboardingStorage(actualOnboardingStorage)
                    .WithNavigationService(actualNavigationService)
                    .WithErrorHandlingService(actualApiErrorHandlingService)
                    .WithAccessRestrictionStorage(actualAccessRestrictionStorage)
                    .Build();

            tryingToConstructWithEmptyParameters
                .Should().Throw<Exception>();
        }

        [Fact]
        public void DoesNotThrowIfTheArgumentsAreValid()
        {
            var foundation = constructFoundation();
            var actualDialogService = Substitute.For<IDialogService>();
            var actualBrowserService = Substitute.For<IBrowserService>();
            var actualKeyValueStorage = Substitute.For<IKeyValueStorage>();
            var actualUserPreferences = Substitute.For<IUserPreferences>();
            var actualOnboardingStorage = Substitute.For<IOnboardingStorage>();
            var actualNavigationService = Substitute.For<IMvxNavigationService>();
            var actualApiErrorHandlingService = Substitute.For<IErrorHandlingService>();
            var actualAccessRestrictionStorage = Substitute.For<IAccessRestrictionStorage>();

            Action tryingToConstructWithEmptyParameters = () =>
                foundation.StartRegisteringPlatformServices()
                    .WithDialogService(actualDialogService)
                    .WithBrowserService(actualBrowserService)
                    .WithKeyValueStorage(actualKeyValueStorage)
                    .WithUserPreferences(actualUserPreferences)
                    .WithOnboardingStorage(actualOnboardingStorage)
                    .WithNavigationService(actualNavigationService)
                    .WithErrorHandlingService(actualApiErrorHandlingService)
                    .WithAccessRestrictionStorage(actualAccessRestrictionStorage)
                    .Build();

            tryingToConstructWithEmptyParameters.Should().NotThrow<Exception>();
        }

        [Fact, LogIfTooSlow]
        public void MarksTheUserAsNotNewWhenUsingTheAppForTheFirstTimeAfterSixtyDays()
        {
            var now = DateTimeOffset.Now;

            timeService.CurrentDateTime.Returns(now);
            onboardingStorage.GetLastOpened().Returns(now.AddDays(-60).ToString());

            mvvmCrossFoundation.RevokeNewUserIfNeeded();

            onboardingStorage.Received().SetLastOpened(now);
            onboardingStorage.Received().SetIsNewUser(false);
        }

        private TogglFoundation constructFoundation()
            => TogglFoundation.ForClient(userAgent, version)
                    .WithDatabase(database)
                    .WithScheduler(scheduler)
                    .WithApiFactory(apiFactory)
                    .WithTimeService(timeService)
                    .WithMailService(mailService)
                    .WithGoogleService(googleService)
                    .WithLicenseProvider(licenseProvider)
                    .WithAnalyticsService(analyticsService)
                    .WithBackgroundService(backgroundService)
                    .WithPlatformConstants(platformConstants)
                    .WithApplicationShortcutCreator(applicationShortcutCreator)
                    .WithSuggestionProviderContainer(suggestionProviderContainer)
                    .Build();
    }
}