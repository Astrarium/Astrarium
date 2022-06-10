using Astrarium.Types;
using Astrarium.Types.Themes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.ViewModels
{
    public class JournalVM : ViewModelBase
    {
        public ObservableCollection<Node> Nodes { get; set; } = new ObservableCollection<Node>();

        public JournalVM()
        {
            var node = new Node("Node 1") { IsExpanded = true };
            node.Children.Add(new Node("SubNode 1") { IsExpanded = true }) ;
            Nodes.Add(node) ;
            Nodes.Add(new Node("Node 2") { IsExpanded = true });
        }
    }
}
