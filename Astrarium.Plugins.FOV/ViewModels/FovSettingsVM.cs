using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Astrarium.Types;
using Astrarium.Types.Themes;
using Newtonsoft.Json;

namespace Astrarium.Plugins.FOV
{
    public class FovSettingsVM : ViewModelBase
    {
        private const string EquipmentFileName = "Equipment.json";
        private static readonly string CustomEquipmentDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium", "FovEquipment");
        private static readonly string DefaultEquipmentDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data");

        private Guid _Id;
        private Equipment _Equipment;

        public FovSettingsVM()
        {
            LoadEquipment();
            Binning = 1;
            Rotation = 0;
            Color = Color.Pink;

            AddEquipmentCommand = new Command<Type>((type) => EditEquipment(type, isNew: true));
            EditEquipmentCommand = new Command<Type>((type) => EditEquipment(type, isNew: false));
            DeleteEquipmentCommand = new Command<Type>((type) => DeleteEquipment(type));

            OkCommand = new Command(Ok);
            CancelCommand = new Command(Close);
        }

        public Command<Type> AddEquipmentCommand { get; }
        public Command<Type> EditEquipmentCommand { get; }
        public Command<Type> DeleteEquipmentCommand { get; }

        public Command OkCommand { get; }
        public Command CancelCommand { get; }

        private void EditEquipment(Type equipmentType, bool isNew)
        {
            if (equipmentType == typeof(Telescope))
            {
                var vm = ViewManager.CreateViewModel<TelescopeVM>();
                vm.Telescopes = Telescopes;
                if (!isNew)
                {
                    vm.Telescope.CopyFrom(Telescope);
                }
                if (ViewManager.ShowDialog(vm) ?? false)
                {
                    if (isNew)
                    {
                        Telescopes.Add(vm.Telescope);
                    }
                    TelescopeId = Guid.Empty;
                    TelescopeId = vm.Telescope.Id;
                    Telescope.CopyFrom(vm.Telescope);
                    _Equipment.Telescopes = new ObservableCollection<Telescope>(Telescopes.OrderBy(t => t.Name));
                    NotifyPropertyChanged(nameof(Telescopes), nameof(TelescopeId), nameof(Telescope));
                    Calculate();
                    SaveEquipment();
                }
            }
            else if (equipmentType == typeof(Eyepiece))
            {
                var vm = ViewManager.CreateViewModel<EyepieceVM>();
                vm.Eyepieces = Eyepieces;
                if (!isNew)
                {
                    vm.Eyepiece.CopyFrom(Eyepiece);
                }
                if (ViewManager.ShowDialog(vm) ?? false)
                {
                    if (isNew)
                    {
                        Eyepieces.Add(vm.Eyepiece);
                    }
                    EyepieceId = Guid.Empty;
                    EyepieceId = vm.Eyepiece.Id;
                    Eyepiece.CopyFrom(vm.Eyepiece);
                    _Equipment.Eyepieces = new ObservableCollection<Eyepiece>(Eyepieces.OrderBy(t => t.Name));
                    NotifyPropertyChanged(nameof(Eyepieces), nameof(EyepieceId), nameof(Eyepiece));
                    Calculate();
                }
            }
            else if (equipmentType == typeof(Camera))
            {
                var vm = ViewManager.CreateViewModel<CameraVM>();
                vm.Cameras = Cameras;
                if (!isNew)
                {
                    vm.Camera.CopyFrom(Camera);
                }
                if (ViewManager.ShowDialog(vm) ?? false)
                {
                    if (isNew)
                    {
                        Cameras.Add(vm.Camera);
                    }
                    CameraId = Guid.Empty;
                    CameraId = vm.Camera.Id;
                    Camera.CopyFrom(vm.Camera);
                    _Equipment.Cameras = new ObservableCollection<Camera>(Cameras.OrderBy(t => t.Name));
                    NotifyPropertyChanged(nameof(Cameras), nameof(CameraId), nameof(Camera));
                    Calculate();
                }
            }
            else if (equipmentType == typeof(Binocular))
            {
                var vm = ViewManager.CreateViewModel<BinocularVM>();
                vm.Binoculars = Binoculars;
                if (!isNew)
                {
                    vm.Binocular.CopyFrom(Binocular);
                }
                if (ViewManager.ShowDialog(vm) ?? false)
                {
                    if (isNew)
                    {
                        Binoculars.Add(vm.Binocular);
                    }
                    BinocularId = Guid.Empty;
                    BinocularId = vm.Binocular.Id;
                    Binocular.CopyFrom(vm.Binocular);
                    _Equipment.Binoculars = new ObservableCollection<Binocular>(Binoculars.OrderBy(t => t.Name));
                    NotifyPropertyChanged(nameof(Binoculars), nameof(BinocularId), nameof(Binocular));
                    Calculate();
                }
            }
        }

        private void DeleteEquipment(Type equipmentType)
        {
            if (ViewManager.ShowMessageBox("Warning", "Do you really want to delete the selected equipment?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (equipmentType == typeof(Telescope))
                {
                    Telescopes.Remove(Telescope);
                    TelescopeId = Guid.Empty;
                    NotifyPropertyChanged(nameof(Telescopes), nameof(TelescopeId));
                    Calculate();
                    SaveEquipment();
                }
                else if (equipmentType == typeof(Eyepiece))
                {
                    Eyepieces.Remove(Eyepiece);
                    EyepieceId = Guid.Empty;
                    NotifyPropertyChanged(nameof(Eyepieces), nameof(EyepieceId));
                    Calculate();
                    SaveEquipment();
                }
                else if (equipmentType == typeof(Camera))
                {
                    Cameras.Remove(Camera);
                    CameraId = Guid.Empty;
                    NotifyPropertyChanged(nameof(Cameras), nameof(CameraId));
                    Calculate();
                    SaveEquipment();
                }
                else if (equipmentType == typeof(Binocular))
                {
                    Binoculars.Remove(Binocular);
                    BinocularId = Guid.Empty;
                    NotifyPropertyChanged(nameof(Binoculars), nameof(BinocularId));
                    Calculate();
                    SaveEquipment();
                }
            }
        }

        private void Ok()
        {
            if (FieldOfView != null)
                Close(true);
        }

        public ICollection<Telescope> Telescopes => _Equipment.Telescopes;
        public ICollection<Eyepiece> Eyepieces => _Equipment.Eyepieces;
        public ICollection<Binocular> Binoculars => _Equipment.Binoculars;
        public ICollection<Camera> Cameras => _Equipment.Cameras;
        public ICollection<Lens> Lenses => _Equipment.Lens;
        public ICollection<int> Binnings { get; } = new int[] { 1, 2, 3, 4, 5 };

        public FieldOfView FieldOfView
        {
            get => GetValue<FieldOfView>(nameof(FieldOfView));
            set
            {
                SetValue(nameof(FieldOfView), value);
            }
        }

        public Guid TelescopeId
        {
            get => GetValue<Guid>(nameof(TelescopeId));
            set
            {
                SetValue(nameof(TelescopeId), value);
                NotifyPropertyChanged(nameof(Telescope));
                Calculate();
            }
        }

        public Guid EyepieceId
        {
            get => GetValue<Guid>(nameof(EyepieceId));
            set
            {
                SetValue(nameof(EyepieceId), value);
                NotifyPropertyChanged(nameof(Eyepiece));
                Calculate();
            }
        }

        public Guid BinocularId
        {
            get => GetValue<Guid>(nameof(BinocularId));
            set 
            { 
                SetValue(nameof(BinocularId), value);
                NotifyPropertyChanged(nameof(Binocular));
                Calculate();
            }
        }

        public Guid CameraId
        {
            get => GetValue<Guid>(nameof(CameraId));
            set 
            {
                SetValue(nameof(CameraId), value);
                NotifyPropertyChanged(nameof(Camera));
                Calculate();
            }
        }

        public Guid? LensId
        {
            get => GetValue<Guid?>(nameof(LensId));
            set 
            { 
                SetValue(nameof(LensId), value);
                Calculate();
            }
        }

        public int Binning
        {
            get => GetValue<int>(nameof(Binning));
            set
            {
                SetValue(nameof(Binning), value);
                Calculate();
            }
        }

        public int Rotation
        {
            get => GetValue<int>(nameof(Rotation));
            set
            {
                SetValue(nameof(Rotation), value);
                Calculate();
            }
        }

        public Binocular Binocular => Binoculars.FirstOrDefault(t => t.Id == BinocularId);
        public Telescope Telescope => Telescopes.FirstOrDefault(t => t.Id == TelescopeId);
        public Eyepiece Eyepiece => Eyepieces.FirstOrDefault(t => t.Id == EyepieceId);
        public Camera Camera => Cameras.FirstOrDefault(t => t.Id == CameraId);
        public Lens Lens => Lenses.FirstOrDefault(t => t.Id == LensId);

        private void Calculate()
        {
            if (FrameType == FrameType.Telescope && Telescope != null && Eyepiece != null && Lens != null)
            {
                FieldOfView = FovCalculator.GetTelescopeView(Telescope, Eyepiece, Lens);
            }
            else if (FrameType == FrameType.Binocular && Binocular != null)
            {
                FieldOfView = FovCalculator.GetBinocularView(Binocular);
            }
            else if (FrameType == FrameType.Camera && Camera != null && Telescope != null && Lens != null)
            {
                FieldOfView = FovCalculator.GetCameraView(Telescope, Camera, Lens, Binning, Rotation);
            }
            else
            {
                FieldOfView = null;
            }
        }

        private void LoadEquipment()
        {
            string equipmentFilePath = Path.Combine(DefaultEquipmentDir, EquipmentFileName);
            string customEquipmentFilePath = Path.Combine(CustomEquipmentDir, EquipmentFileName);
            if (File.Exists(customEquipmentFilePath))
            {
                equipmentFilePath = customEquipmentFilePath;
            }

            string equipmentJson = File.ReadAllText(equipmentFilePath);
            _Equipment = JsonConvert.DeserializeObject<Equipment>(equipmentJson);
        }

        private void SaveEquipment()
        {
            if (!Directory.Exists(CustomEquipmentDir))
            {
                Directory.CreateDirectory(CustomEquipmentDir);
            }
            string customEquipmentFilePath = Path.Combine(CustomEquipmentDir, EquipmentFileName);
            File.WriteAllText(customEquipmentFilePath, JsonConvert.SerializeObject(_Equipment, Formatting.Indented), Encoding.UTF8);
        }

        public FrameType FrameType
        {
            get => GetValue<FrameType>(nameof(FrameType));
            set 
            { 
                SetValue(nameof(FrameType), value);
                NotifyPropertyChanged(
                    nameof(TelescopeVisible), 
                    nameof(EyepieceVisible),
                    nameof(BinocularVisible),
                    nameof(CameraVisible),
                    nameof(LensVisible)
                    );
                Calculate();
            }
        }

        public Color Color
        {
            get => GetValue<Color>(nameof(Color));
            set => SetValue(nameof(Color), value);
        }

        public short Shading
        {
            get => GetValue<short>(nameof(Shading));
            set => SetValue(nameof(Shading), value);
        }

        public string Label
        {
            get => GetValue<string>(nameof(Label));
            set => SetValue(nameof(Label), value);
        }

        public bool TelescopeVisible => FrameType != FrameType.Binocular;
        public bool EyepieceVisible => FrameType == FrameType.Telescope;
        public bool BinocularVisible => FrameType == FrameType.Binocular;
        public bool CameraVisible => FrameType == FrameType.Camera;
        public bool LensVisible => FrameType != FrameType.Binocular;

        public FovFrame Frame
        {
            get
            {
                if (FieldOfView is TelescopeFieldOfView telescopeFieldOfView)
                {
                    return new TelescopeFovFrame()
                    {
                        Id = _Id,
                        Color = Color,
                        Label = Label,
                        Shading = Shading,
                        Enabled = true,
                        EyepieceId = EyepieceId,
                        LensId = LensId,
                        Size = telescopeFieldOfView.Size,
                        TelescopeId = TelescopeId
                    };
                }
                else if (FieldOfView is CameraFieldOfView cameraFieldOfView)
                {
                    return new CameraFovFrame()
                    {
                        Id = _Id,
                        Color = Color,
                        Label = Label,
                        Shading = Shading,
                        Enabled = true,
                        LensId = LensId,
                        Width = cameraFieldOfView.Size.Width,
                        Height = cameraFieldOfView.Size.Height,
                        TelescopeId = TelescopeId,
                        Binning = Binning,
                        CameraId = CameraId,
                        Rotation = Rotation
                    };
                }
                else if (FieldOfView is BinocularFieldOfView binocularFieldOfView)
                {
                    return new BinocularFovFrame()
                    {
                        Id = _Id,
                        Color = Color,
                        Label = Label,
                        Shading = Shading,
                        Enabled = true,
                        BinocularId = BinocularId,
                        Size = binocularFieldOfView.Size
                    };
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value is TelescopeFovFrame telescopeFrame)
                {
                    TelescopeId = telescopeFrame.TelescopeId;
                    EyepieceId = telescopeFrame.EyepieceId;
                    LensId = telescopeFrame.LensId;
                    FrameType = FrameType.Telescope;
                }
                else if (value is CameraFovFrame cameraFrame)
                {
                    TelescopeId = cameraFrame.TelescopeId;
                    CameraId = cameraFrame.CameraId;
                    LensId = cameraFrame.LensId;
                    FrameType = FrameType.Camera;
                }
                else if (value is BinocularFovFrame binocularFrame)
                {
                    BinocularId = binocularFrame.BinocularId;
                    FrameType = FrameType.Binocular;
                }

                _Id = value.Id;
                Shading = value.Shading;
                Color = value.Color;
                Label = value.Label;
            }
        }
    }

    public enum FrameType
    {
        Telescope = 0,
        Camera = 1,
        Binocular = 2
    }

    public class CameraResolutionConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var camera = (Camera)value;
            return camera != null ? $"{camera.HorizontalResolution} x {camera.VerticalResolution} mm" : null;
        }
    }

    public class CameraPixelSizeConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var camera = (Camera)value;
            return camera != null ? $"{camera.PixelSizeWidth} x {camera.PixelSizeHeight} µm" : null;
        }
    }

    public class MillimetersConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var focalLength = (int)value;
            return $"{focalLength} mm";
        }
    }

    public class FieldOfViewConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return $"{(float)value}°";
        }
    }

    public class MagnificationConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return $"x {(float)value}";
        }
    }

    public class FieldOfViewDetailsConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var fieldOfView = (FieldOfView)value;
            if (fieldOfView is TelescopeFieldOfView telescopeFieldOfView)
            {
                return $"Focal ratio: {telescopeFieldOfView.FocalRatio:0.00}, Magnification: {telescopeFieldOfView.Magnification:0.00}, Field Of View: {telescopeFieldOfView.Size}, Exit Pupil: {telescopeFieldOfView.ExitPupil}, Dawes Limit: {telescopeFieldOfView.DawesLimit}";
            }
            else if (fieldOfView is CameraFieldOfView cameraFieldOfView)
            {
                return $"Focal ratio: {cameraFieldOfView.FocalRatio}, Resolution: {cameraFieldOfView.Resolution}, Field Of View: {cameraFieldOfView.Size}, Dawes Limit: {cameraFieldOfView.DawesLimit}";
            }
            else if (fieldOfView is BinocularFieldOfView binocularFieldOfView)
            {
                return $"Magnification: {binocularFieldOfView.Magnification}, Field Of View: {binocularFieldOfView.Size}, Exit Pupil: {binocularFieldOfView.ExitPupil}, Dawes Limit: {binocularFieldOfView.DawesLimit}";
            }
            else
            {
                return null;
            }
        }
    }

    public class BinningConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int binning = (int)value;
            return $"{binning}x{binning}";
        }
    }
}
