using Astrarium.Plugins.Notes.ViewModels;
using Astrarium.Plugins.Notes.Views;
using Astrarium.Types;

namespace Astrarium.Plugins.Notes
{
    public class NotesPlugin : AbstractPlugin
    {
        private readonly ISky sky;
        private readonly ISkyMap map;
        private readonly NotesManager notesManager;

        public NotesPlugin(ISky sky, ISkyMap map, NotesManager notesManager)
        {
            this.sky = sky;
            this.map = map;
            this.notesManager = notesManager;
         
            ExtendObjectInfo<NotesControl, ObjectNotesVM>("$Notes.ObjectInfo.Title", CreateObjectNotesViewModel);

            var mnuNotes = new MenuItem("$Notes.ContextMenu");
            var mnuNotesBinding = new SimpleBinding(map, nameof(ISkyMap.SelectedObject), nameof(MenuItem.IsEnabled));
            mnuNotesBinding.SourceToTargetConverter = (object s) => s != null;
            mnuNotes.AddBinding(mnuNotesBinding);

            var mnuAddObjectNote = new MenuItem("$Notes.ContextMenu.AddNote", new Command(AddObjectNote));
            var mnuObjectNotes = new MenuItem("$Notes.ContextMenu.ObjectNotes", new Command(ShowObjectNotes));
            var mnuObjectNotesBinding = new SimpleBinding(this, nameof(HasSelectedObjectNotes), nameof(MenuItem.IsEnabled));
            mnuObjectNotes.AddBinding(mnuObjectNotesBinding);

            mnuNotes.SubItems.Add(mnuAddObjectNote);
            mnuNotes.SubItems.Add(mnuObjectNotes);

            MenuItems.Add(MenuItemPosition.MainMenuTools, new MenuItem("$Notes.Menu.Notes", new Command(OpenNotesWindow)));
            MenuItems.Add(MenuItemPosition.ContextMenu, mnuNotes);
        }

        private ObjectNotesVM CreateObjectNotesViewModel(SkyContext ctx, CelestialObject body)
        {
            return ViewManager.CreateViewModel<ObjectNotesVM>().ForBody(body);
        }

        private void OpenNotesWindow()
        {
            ViewManager.ShowWindow<AllNotesVM>(flags: ViewFlags.TopMost);
        }

        private void AddObjectNote()
        {
            var body = map.SelectedObject;
            if (body != null)
            {
                var vm = CreateObjectNotesViewModel(sky.Context, body);
                vm.AddNoteCommand.Execute(null);
            }
        }

        private void ShowObjectNotes()
        {
            var body = map.SelectedObject;
            if (body != null)
            {
                var vm = CreateObjectNotesViewModel(sky.Context, body);
                ViewManager.ShowWindow(vm, flags: ViewFlags.TopMost);
            }
        }

        public bool HasSelectedObjectNotes => notesManager.HasNotesForObject(map.SelectedObject);
    }
}
