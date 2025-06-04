using Astrarium.Plugins.Notes.ViewModels;
using Astrarium.Plugins.Notes.Views;
using Astrarium.Types;

namespace Astrarium.Plugins.Notes
{
    public class NotesPlugin : AbstractPlugin
    {
        public NotesPlugin()
        {
            ExtendObjectInfo<ObjectNotesControl, ObjectNotesVM>("Notes", CreateObjectNotesViewModel);
        }

        private ObjectNotesVM CreateObjectNotesViewModel(SkyContext ctx, CelestialObject body)
        {
            var viewModel = ViewManager.CreateViewModel<ObjectNotesVM>();
            viewModel.SetObject(body);

            return viewModel;
        }
    }
}
