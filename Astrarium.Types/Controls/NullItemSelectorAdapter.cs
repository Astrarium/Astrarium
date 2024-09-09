using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;

namespace Astrarium.Types.Controls
{
    [ContentProperty("Selector")]
    public class NullItemSelectorAdapter : ContentControl
    {
        ICollectionView _collectionView;
        /// <summary>
        /// Gets or sets the collection view associated with the internal <see cref="CompositeCollection"/>
        /// that combines the null-representing item and the <see cref="ItemsSource"/>.
        /// </summary>
        protected ICollectionView CollectionView
        {
            get { return _collectionView; }
            set { _collectionView = value; }
        }

        /// <summary>
        /// Identifies the <see cref="Selector"/> property.
        /// </summary>
        public static readonly DependencyProperty SelectorProperty = DependencyProperty.Register(
            "Selector", typeof(Selector), typeof(NullItemSelectorAdapter),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(Selector_Changed)));
        static void Selector_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            NullItemSelectorAdapter adapter = (NullItemSelectorAdapter)sender;
            adapter.Content = e.NewValue;
            Selector selector = (Selector)e.OldValue;
            if (selector != null) selector.SelectionChanged -= adapter.Selector_SelectionChanged;
            selector = (Selector)e.NewValue;
            if (selector != null)
            {
                selector.IsSynchronizedWithCurrentItem = true;
                selector.SelectionChanged += adapter.Selector_SelectionChanged;
            }
            adapter.Adapt();
        }

        /// <summary>
        /// Gets or sets the Selector control.
        /// </summary>
        public Selector Selector
        {
            get { return (Selector)GetValue(SelectorProperty); }
            set { SetValue(SelectorProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ItemsSource"/> property.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource", typeof(IEnumerable), typeof(NullItemSelectorAdapter),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(ItemsSource_Changed)));
        static void ItemsSource_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            NullItemSelectorAdapter adapter = (NullItemSelectorAdapter)sender;
            adapter.Adapt();
        }

        /// <summary>
        /// Gets or sets the data items.
        /// </summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="NullItem"/> property.
        /// </summary>
        public static readonly DependencyProperty NullItemProperty = DependencyProperty.Register(
            "NullItem", typeof(object), typeof(NullItemSelectorAdapter), new PropertyMetadata("(None)"));

        /// <summary>
        /// Gets or sets the null-representing object to display in the Selector.
        /// (The default is the string &quot;(None)&quot;.)
        /// </summary>
        public object NullItem
        {
            get { return GetValue(NullItemProperty); }
            set { SetValue(NullItemProperty, value); }
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public NullItemSelectorAdapter()
        {
            IsTabStop = false;
        }

        /// <summary>
        /// Updates the Selector control's <see cref="ItemsControl.ItemsSource"/> to include the
        /// <see cref="NullItem"/> along with the objects in <see cref="ItemsSource"/>.
        /// </summary>
        protected void Adapt()
        {
            if (CollectionView != null)
            {
                CollectionView.CurrentChanged -= CollectionView_CurrentChanged;
                CollectionView = null;
            }
            if (Selector != null && ItemsSource != null)
            {
                CompositeCollection comp = new CompositeCollection();
                comp.Add(NullItem);
                comp.Add(new CollectionContainer { Collection = ItemsSource });

                CollectionView = CollectionViewSource.GetDefaultView(comp);
                if (CollectionView != null) CollectionView.CurrentChanged += CollectionView_CurrentChanged;

                Selector.ItemsSource = comp;
            }
        }

        bool _isChangingSelection;
        /// <summary>
        /// Triggers binding sources to be updated if the <see cref="NullItem"/> is selected.
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event data</param>
        protected void Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Selector.SelectedItem == NullItem)
            {
                if (!_isChangingSelection)
                {
                    _isChangingSelection = true;
                    try
                    {
                        // Selecting the null item doesn't trigger an update to sources bound to properties
                        // like SelectedItem, so move selection away and then back to force this.
                        int selectedIndex = Selector.SelectedIndex;
                        Selector.SelectedIndex = -1;
                        Selector.SelectedIndex = selectedIndex;
                    }
                    finally
                    {
                        _isChangingSelection = false;
                    }
                }
            }
        }

        /// <summary>
        /// Selects the <see cref="NullItem"/> if the source collection's current item moved to null.
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event data</param>
        void CollectionView_CurrentChanged(object sender, EventArgs e)
        {
            if (Selector != null && ((ICollectionView)sender).CurrentItem == null && Selector.Items.Count != 0)
            {
                Selector.SelectedIndex = 0;
            }
        }
    }
}
