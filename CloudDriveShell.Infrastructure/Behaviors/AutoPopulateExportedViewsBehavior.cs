

using System;
using System.ComponentModel.Composition;
using Prism.Regions;

namespace CloudDriveShell.Infrastructure.Behaviors
{
    [Export(typeof(AutoPopulateExportedViewsBehavior))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class AutoPopulateExportedViewsBehavior : RegionBehavior, IPartImportsSatisfiedNotification
    {
        protected override void OnAttach()
        {
            AddRegisteredViews();
        }

        public void OnImportsSatisfied()
        {
            AddRegisteredViews();
        }

        private void AddRegisteredViews()
        {
            if (this.Region != null)
            {
                foreach (var viewEntry in this.RegisteredViews)
                {
                    if (viewEntry.Metadata.RegionName == this.Region.Name)
                    {
                        var view = viewEntry.Value;

                        if (!this.Region.Views.Contains(view))
                        {
                            this.Region.Add(view);
                        }
                    }
                }
            }
        }
        [ImportMany(AllowRecomposition = true)]
        public Lazy<object, IViewRegionRegistration>[] RegisteredViews { get; set; }
    }
}
