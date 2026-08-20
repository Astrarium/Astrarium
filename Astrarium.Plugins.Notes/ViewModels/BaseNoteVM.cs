using Astrarium.Types;
using System;

namespace Astrarium.Plugins.Notes.ViewModels
{
    public class BaseNoteVM : ViewModelBase
    {
        protected ISky sky;
        protected ISkyMap map;

        internal BaseNoteVM(ISky sky, ISkyMap map) 
        { 
            this.sky = sky;
            this.map = map;
        }

        protected void SelectDate(Note note)
        {
            if (!sky.Context.GeoLocation.Equals(note.Location))
            {
                if (ViewManager.ShowMessageBox("$Warning", "$Notes.NoteWindow.Warning.ChangeLocation", System.Windows.MessageBoxButton.YesNo) != System.Windows.MessageBoxResult.Yes)
                {
                    return;
                }

                sky.SetLocation(note.Location);
            }

            sky.SetDate(note.Date);
            var body = sky.Search(note.Body.Type, note.Body.CommonName);
            if (body != null)
            {
                map.GoToObject(body, TimeSpan.FromSeconds(1));
            }
            else
            {
                ViewManager.ShowMessageBox("$Error", "$Notes.NoteWindow.Error.ObjectNotFound", System.Windows.MessageBoxButton.OK);
            }
        }
    }
}
