using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel.Composition;

using Transitionals.Controls;
using System.Collections.Specialized;
using Prism.Regions;

namespace CloudDriveShell.Infrastructure.Extension
{
    [Export("CloudDriveShell.Infrastructure.Extension.TransitionElementAdaptor", typeof(TransitionElementAdaptor))]
    public class TransitionElementAdaptor : RegionAdapterBase<TransitionElement>
    {
        [ImportingConstructor]
        public TransitionElementAdaptor(IRegionBehaviorFactory behaviorFactory) :
            base(behaviorFactory)
        {
        }

        protected override void Adapt(IRegion region, TransitionElement regionTarget)
        {
            region.Views.CollectionChanged += (s, e) =>
            {
                Transition(regionTarget, e);
            };

            region.ActiveViews.CollectionChanged += (s, e) =>
            {
                Transition(regionTarget, e);
            };
        }

        private void Transition(TransitionElement regionTarget, NotifyCollectionChangedEventArgs e)
        {
            //Add
            if (e.Action == NotifyCollectionChangedAction.Add)
                foreach (FrameworkElement element in e.NewItems)
                    regionTarget.Content = element;

            //Removal
            if (e.Action == NotifyCollectionChangedAction.Remove)
                foreach (FrameworkElement element in e.OldItems)
                {
                    regionTarget.Content = null;
                    GC.Collect();
                }
        }




        protected override IRegion CreateRegion()
        {
            return new SingleActiveRegion();
        }

    }
}
