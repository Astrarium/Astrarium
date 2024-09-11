using Astrarium.Plugins.Notes.ViewModels;
using Astrarium.Plugins.Notes.Views;
using Astrarium.Types;

namespace Astrarium.Plugins.Notes
{
    public class NotesPlugin : AbstractPlugin
    {
        public NotesPlugin()
        {
            ExtendObjectInfo<ObjectNotesControl, ObjectNotesViewModel>("Notes", CreateObjectNotesViewModel);
        }

        private ObjectNotesViewModel CreateObjectNotesViewModel(SkyContext ctx, CelestialObject body)
        {
            var viewModel = ViewManager.CreateViewModel<ObjectNotesViewModel>();
            viewModel.SetObject(body);

            return viewModel;
        }
    }
}
