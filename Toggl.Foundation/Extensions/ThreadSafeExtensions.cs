using System;
using Toggl.Foundation.Models.Interfaces;

namespace Toggl.Foundation.Extensions
{
    public static class ThreadSafeExtensions
    {
        private const string archivedColor = "#cecece";

        public static string DisplayName(this IThreadSafeProject project)
        {
            var name = project.Name ?? "";

            if (name == Resources.InaccessibleProject)
            {
                return name;
            }

            return project.Active ? name : $"{name} (archived)";
        }

        public static string DisplayColor(this IThreadSafeProject project)
        {
            if (project.Name == Resources.InaccessibleProject)
            {
                return archivedColor;
            }
            return project.Color ?? "";
        }
    }
}
