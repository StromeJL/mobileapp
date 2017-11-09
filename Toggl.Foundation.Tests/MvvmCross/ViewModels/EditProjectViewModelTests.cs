﻿using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MvvmCross.Platform.UI;
using NSubstitute;
using Toggl.Foundation.DTOs;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Tests.Generators;
using Toggl.Multivac;
using Toggl.PrimeRadiant.Models;
using Xunit;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public sealed class EditProjectViewModelTests
    {
        public abstract class EditProjectViewModelTest : BaseViewModelTests<EditProjectViewModel>
        {
            protected const long WorkspaceId = 10;
            protected const string WorkspaceName = "Some workspace name";
            protected IDatabaseWorkspace Workspace { get; } = Substitute.For<IDatabaseWorkspace>();

            protected EditProjectViewModelTest()
            {
                ViewModel.Name = "A valid name";
            }

            protected override EditProjectViewModel CreateViewModel()
                => new EditProjectViewModel(DataSource, NavigationService);
        }

        public sealed class TheConstructor : EditProjectViewModelTest
        {
            [Theory]
            [ClassData(typeof(TwoParameterConstructorTestData))]
            public void ThrowsIfAnyOfTheArgumentsIsNull(bool useDataSource, bool useNavigationService)
            {
                var dataSource = useDataSource ? DataSource : null;
                var navigationService = useNavigationService ? NavigationService : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new EditProjectViewModel(dataSource, navigationService);

                tryingToConstructWithEmptyParameters
                    .ShouldThrow<ArgumentNullException>();
            }
        }

        public sealed class TheSaveEnabledProperty : EditProjectViewModelTest
        {
            [Fact]
            public void IsFalseWhenTheNameIsEmpty()
            {
                ViewModel.Name = "";

                ViewModel.SaveEnabled.Should().BeFalse();
            }

            [Fact]
            public void IsFalseWhenTheNameIsJustWhiteSpace()
            {
                ViewModel.Name = "            ";

                ViewModel.SaveEnabled.Should().BeFalse();
            }

            [Fact]
            public void IsFalseWhenTheNameIsLongerThanTheThresholdInBytes()
            {
                ViewModel.Name = "This is a ridiculously big project name made solely with the purpose of testing whether or not Toggl apps UI has validation logic that prevents such a large name to be persisted or, even worse, pushed to the api, an event that might end up in crashes and whatnot";

                ViewModel.SaveEnabled.Should().BeFalse();
            }
        }

        public sealed class TheInitializeMethod : EditProjectViewModelTest
        {
            public TheInitializeMethod()
            {
                DataSource.Workspaces
                    .GetDefault()
                    .Returns(Observable.Return(Workspace));

                Workspace.Id.Returns(WorkspaceId);
                Workspace.Name.Returns(WorkspaceName);

                ViewModel.Prepare();
            }

            [Fact]
            public async Task SetsTheWorkspaceId()
            {
                await ViewModel.Initialize();
                await ViewModel.DoneCommand.ExecuteAsync();

                await DataSource.Projects
                    .Received()
                    .Create(Arg.Is<CreateProjectDTO>(dto => dto.WorkspaceId == WorkspaceId));
            }

            [Fact]
            public async Task SetsTheWorkspaceName()
            {
                await ViewModel.Initialize();

                ViewModel.WorkspaceName.Should().Be(WorkspaceName);
            }
        }

        public sealed class TheCloseCommand : EditProjectViewModelTest
        {
            [Fact]
            public async Task ClosesTheViewModel()
            {
                await ViewModel.CloseCommand.ExecuteAsync();

                await NavigationService.Received()
                    .Close(Arg.Is(ViewModel), Arg.Any<long?>());
            }

            [Fact]
            public async Task ReturnsNull()
            {
                ViewModel.Prepare();

                await ViewModel.CloseCommand.ExecuteAsync();

                await NavigationService.Received()
                    .Close(Arg.Is(ViewModel), Arg.Is<long?>(result => result == null));
            }

            [Fact]
            public async Task DoesNotTrySavingTheChanges()
            {
                ViewModel.Prepare();

                await ViewModel.CloseCommand.ExecuteAsync();

                await DataSource.Projects.DidNotReceive().Create(Arg.Any<CreateProjectDTO>());
            }
        }

        public sealed class TheDoneCommand : EditProjectViewModelTest
        {
            private const long proWorkspaceId = 11;
            private const long projectId = 12;

            private readonly IDatabaseProject project = Substitute.For<IDatabaseProject>();

            public TheDoneCommand()
            {
                DataSource.Workspaces
                    .WorkspaceHasFeature(WorkspaceId, WorkspaceFeatureId.Pro)
                    .Returns(Observable.Return(false));

                DataSource.Workspaces
                    .WorkspaceHasFeature(proWorkspaceId, WorkspaceFeatureId.Pro)
                    .Returns(Observable.Return(true));

                DataSource.Workspaces
                    .GetDefault()
                    .Returns(Observable.Return(Workspace));

                DataSource.Projects
                    .Create(Arg.Any<CreateProjectDTO>())
                    .Returns(Observable.Return(project));

                project.Id.Returns(projectId);
                Workspace.Id.Returns(proWorkspaceId);
            }

            [Fact]
            public async Task ClosesTheViewModel()
            {
                ViewModel.Prepare();

                await ViewModel.DoneCommand.ExecuteAsync();

                await NavigationService.Received()
                    .Close(Arg.Is(ViewModel), Arg.Any<long?>());
            }

            [Fact]
            public async Task ReturnsTheIdOfTheCreatedProject()
            {
                ViewModel.Prepare();

                await ViewModel.DoneCommand.ExecuteAsync();

                await NavigationService.Received()
                    .Close(Arg.Is(ViewModel), Arg.Is(projectId));
            }

            [Fact]
            public async Task DoesNotCallCreateIfTheProjectNameIsInvalid()
            {
                ViewModel.Prepare();
                ViewModel.Name = "";

                await ViewModel.DoneCommand.ExecuteAsync();

                await DataSource.Projects.DidNotReceive()
                    .Create(Arg.Any<CreateProjectDTO>());
            }

            [Fact]
            public async Task DoesNotCloseTheViewModelIfTheProjectNameIsInvalid()
            {
                ViewModel.Prepare();
                ViewModel.Name = "";

                await ViewModel.DoneCommand.ExecuteAsync();

                await NavigationService.DidNotReceive()
                    .Close(ViewModel, projectId);
            }

            [Fact]
            public async Task SetsBillableToNullIfTheDefaultWorkspaceIfNotPro()
            {
                Workspace.Id.Returns(WorkspaceId);
                ViewModel.Prepare();

                await ViewModel.DoneCommand.ExecuteAsync();

                await DataSource.Projects.Received().Create(
                    Arg.Is<CreateProjectDTO>(dto => dto.Billable == null)
                );
            }

            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            public async Task SetsBillableToTheValueOfTheProjectsBillableByDefaultPropertyIfTheDefaultWorkspaceIsPro(
                bool billableByDefault)
            {
                Workspace.Id.Returns(proWorkspaceId);
                Workspace.ProjectsBillableByDefault.Returns(billableByDefault);
                ViewModel.Prepare();
                await ViewModel.Initialize();

                await ViewModel.DoneCommand.ExecuteAsync();

                await DataSource.Projects.Received().Create(
                    Arg.Is<CreateProjectDTO>(dto => dto.Billable == billableByDefault)
                );
            }
        }

        public sealed class ThePickColorCommand : EditProjectViewModelTest
        {
            [Fact]
            public async Task CallsTheSelectColorViewModel()
            {
                ViewModel.Prepare();

                await ViewModel.PickColorCommand.ExecuteAsync();

                await NavigationService.Received()
                    .Navigate<MvxColor, MvxColor>(typeof(SelectColorViewModel), Arg.Any<MvxColor>());
            }

            [Fact]
            public async Task SetsTheReturnedColorAsTheColorProperty()
            {
                var expectedColor = MvxColors.AliceBlue;
                NavigationService
                    .Navigate<MvxColor, MvxColor>(typeof(SelectColorViewModel), Arg.Any<MvxColor>())
                    .Returns(Task.FromResult(expectedColor));
                ViewModel.Prepare();
                ViewModel.Color = MvxColors.Azure;

                await ViewModel.PickColorCommand.ExecuteAsync();

                ViewModel.Color.ARGB.Should().Be(expectedColor.ARGB);
            }
        }
    }
}