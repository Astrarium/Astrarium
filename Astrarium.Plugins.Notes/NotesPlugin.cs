using Astrarium.Plugins.Notes.ViewModels;
using Astrarium.Plugins.Notes.Views;
using Astrarium.Types;

namespace Astrarium.Plugins.Notes
{
    public class NotesPlugin : AbstractPlugin
    {
        public NotesPlugin()
        {
            ExtendObjectInfo<NotesControl, ObjectNotesVM>("$Notes.ObjectInfo.Title", CreateObjectNotesViewModel);
            MenuItems.Add(MenuItemPosition.MainMenuTools, new MenuItem("$Notes.Menu.Notes", new Command(OpenNotesWindow)));
        }

        private ObjectNotesVM CreateObjectNotesViewModel(SkyContext ctx, CelestialObject body)
        {
            return ViewManager.CreateViewModel<ObjectNotesVM>().ForBody(body);
        }

        private void OpenNotesWindow()
        {
            ViewManager.ShowWindow<AllNotesVM>(flags: ViewFlags.TopMost);
        }
    }
}
