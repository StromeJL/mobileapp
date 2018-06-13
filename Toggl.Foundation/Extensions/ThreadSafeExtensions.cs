using Toggl.Foundation.Models.Interfaces;

namespace Toggl.Foundation.Extensions
{
    public static class ThreadSafeExtensions
    {
        private const string archivedColor = "#cecece";

        public static string DisplayName(this IThreadSafeProject project)
        {
            var name = project.Name ?? "";

            switch (project.SyncStatus)
            {
                case PrimeRadiant.SyncStatus.RefetchingNeeded:
                    return name;
                default:
                    return project.Active ? name : $"{name} {Resources.ArchivedProjectDecorator}";
            }
        }

        public static string DisplayColor(this IThreadSafeProject project)
        {
            switch (project.SyncStatus)
            {
                case PrimeRadiant.SyncStatus.RefetchingNeeded:
                    return archivedColor;
                default:
                    return project.Color ?? "";
            }
        }
    }
}
