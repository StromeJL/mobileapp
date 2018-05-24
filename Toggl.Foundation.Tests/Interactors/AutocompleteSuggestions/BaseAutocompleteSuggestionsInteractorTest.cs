﻿using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Tests.Interactors.AutocompleteSuggestions
{
    public abstract class BaseAutocompleteSuggestionsInteractorTest
    {
        protected IEnumerable<IDatabaseClient> Clients { get; }

        protected IEnumerable<IDatabaseTask> Tasks { get; }

        protected IEnumerable<IDatabaseProject> Projects { get; }

        protected IEnumerable<IDatabaseTag> Tags { get; }

        protected IEnumerable<IDatabaseTimeEntry> TimeEntries { get; }

        protected BaseAutocompleteSuggestionsInteractorTest()
        {
            Clients = Enumerable.Range(10, 10).Select(id =>
            {
                var client = Substitute.For<IDatabaseClient>();
                client.Id.Returns(id);
                client.Name.Returns(id.ToString());
                return client;
            });

            Tasks = Enumerable.Range(20, 10).Select(id =>
            {
                var task = Substitute.For<IDatabaseTask>();
                task.Id.Returns(id);
                task.Name.Returns(id.ToString());
                return task;
            }).ToList();

            Projects = Enumerable.Range(30, 10).Select(id =>
            {
                var tasks = id % 2 == 0 ? Tasks.Where(t => (t.Id == id - 10 || t.Id == id - 11)).ToList() : null;
                var project = Substitute.For<IDatabaseProject>();
                project.Id.Returns(id);
                project.Name.Returns(id.ToString());
                project.Color.Returns("#1e1e1e");
                project.Tasks.Returns(tasks);
                project.Active.Returns(id % 10 != 8);

                var client = id % 2 == 0 ? Clients.Single(c => c.Id == id - 20) : null;
                project.Client.Returns(client);

                if (tasks != null)
                {
                    foreach (var task in tasks)
                        task.ProjectId.Returns(id);
                }

                return project;
            });

            Tags = Enumerable.Range(50, 10).Select(id =>
            {
                var tag = Substitute.For<IDatabaseTag>();
                tag.Id.Returns(id);
                tag.Name.Returns(id.ToString());
                return tag;
            });

            TimeEntries = Enumerable.Range(40, 10).Select(id =>
            {
                var timeEntry = Substitute.For<IDatabaseTimeEntry>();
                timeEntry.Id.Returns(id);
                timeEntry.Description.Returns(id.ToString());

                var project = id % 2 == 0 ? Projects.Single(c => c.Id == id - 10) : null;
                timeEntry.Project.Returns(project);

                var task = id > 45 ? project?.Tasks?.First() : null;
                timeEntry.Task.Returns(task);

                timeEntry.IsDeleted.Returns(id % 10 == 9);

                return timeEntry;
            });
        }
    }
}
