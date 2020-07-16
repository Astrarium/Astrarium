using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Astrarium.Plugins.FOV
{
    public class FovFramesListVM : ViewModelBase
    {
        private List<FovFrame> fovFrames;
        public ObservableCollection<FovFrame> FovFrames => new ObservableCollection<FovFrame>(fovFrames);
        public bool IsEmptyList => !FovFrames.Any();

        public Command CloseCommand { get; }
        public Command<FovFrame> EditCommand { get; }
        public Command<FovFrame> DeleteCommand { get; }
        public Command AddCommand { get; }
        public Command<FovFrame> CheckedCommand { get; }

        private ISettings settings;

        public FovFramesListVM(ISettings settings)
        {
            this.settings = settings;

            fovFrames = settings.Get<List<FovFrame>>("FovFrames");
            CloseCommand = new Command(Close);
            AddCommand = new Command(AddFrame);
            EditCommand = new Command<FovFrame>(EditFrame);
            DeleteCommand = new Command<FovFrame>(DeleteFrame);
            CheckedCommand = new Command<FovFrame>(CheckedFrame);
        }

        private void CheckedFrame(FovFrame frame)
        {
            settings.Set("FovFrames", fovFrames);
            settings.Save();
        }

        private void AddFrame()
        {
            EditFrame(new TelescopeFovFrame() { Id = Guid.NewGuid(), Color = Color.Purple });
        }
    
        private void EditFrame(FovFrame frame)
        {
            var viewModel = ViewManager.CreateViewModel<FovSettingsVM>();
            viewModel.Frame = frame;
            if (ViewManager.ShowDialog(viewModel) ?? false)
            {
                frame = viewModel.Frame;
                int index = fovFrames.FindIndex(f => f.Id == frame.Id);

                if (index >= 0)
                {
                    fovFrames.RemoveAt(index);
                    fovFrames.Insert(index, frame);
                }
                else
                {
                    fovFrames.Add(frame);
                }

                settings.Set("FovFrames", fovFrames);
                settings.Save();

                NotifyPropertyChanged(nameof(FovFrames), nameof(IsEmptyList));
            }
        }

        private void DeleteFrame(FovFrame frame)
        {
            int index = fovFrames.FindIndex(f => f.Id == frame.Id);
            if (index >= 0)
            {
                if (ViewManager.ShowMessageBox(Text.Get("FovFramesListWindow.WarningTitle"), Text.Get("FovFramesListWindow.DeleteFrameWarningMessage"), MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    fovFrames.RemoveAt(index);
                    settings.Set("FovFrames", fovFrames);
                    settings.Save();
                    NotifyPropertyChanged(nameof(FovFrames), nameof(IsEmptyList));
                }
            }
        }
    }
}
