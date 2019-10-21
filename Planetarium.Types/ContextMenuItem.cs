using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Planetarium.Types
{
    public class ContextMenuItem
    {
        public string Text { get; private set; }
        public Action Action { get; private set; }
        public Func<bool> EnabledCondition { get; private set; }
        public Func<bool> VisibleCondition { get; private set; }
        public Func<bool> CheckedCondition { get; private set; }

        public ContextMenuItem(string text, Action action, Func<bool> enabledCondition, Func<bool> visibleCondition, Func<bool> checkedCondition = null)
        {
            Text = text;
            Action = action;
            EnabledCondition = enabledCondition;
            VisibleCondition = visibleCondition;
            CheckedCondition = checkedCondition;
        }
    }
}
