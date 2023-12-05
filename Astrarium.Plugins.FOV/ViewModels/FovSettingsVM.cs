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
        private ISettings _Settings;

        public FovSettingsVM(ISettings settings)
        {
            _Settings = settings;
            LoadEquipment();
            Binning = 1;
            Rotation = 0;
            Color = System.Drawing.Color.Pink;

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
                    SaveEquipment();
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
                    SaveEquipment();
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
                    SaveEquipment();
                }
            }
        }

        private void DeleteEquipment(Type equipmentType)
        {
            if (ViewManager.ShowMessageBox("$FovSettingsVM.WarningTitle", "$FovSettingsVM.DeleteEquipmentMessage", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
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
            {
                if (string.IsNullOrWhiteSpace(Label))
                {
                    ViewManager.ShowMessageBox("$FovSettingsVM.WarningTitle", "$FovSettingsVM.EmptyLabelMessage");
                    return;
                }

                Close(true);
            }
        }

        public ICollection<Telescope> Telescopes => _Equipment.Telescopes;
        public ICollection<Eyepiece> Eyepieces => _Equipment.Eyepieces;
        public ICollection<Binocular> Binoculars => _Equipment.Binoculars;
        public ICollection<Camera> Cameras => _Equipment.Cameras;
        public ICollection<Lens> Lenses => _Equipment.Lens;
        public ICollection<int> Binnings { get; } = new int[] { 1, 2, 3, 4, 5 };

        public IFieldOfView FieldOfView
        {
            get => GetValue<IFieldOfView>(nameof(FieldOfView));
            set
            {
                SetValue(nameof(FieldOfView), value);
            }
        }

        public Guid? TelescopeId
        {
            get => GetValue<Guid?>(nameof(TelescopeId));
            set
            {
                SetValue(nameof(TelescopeId), value);
                NotifyPropertyChanged(nameof(Telescope));
                Calculate();
            }
        }

        public Guid? EyepieceId
        {
            get => GetValue<Guid?>(nameof(EyepieceId));
            set
            {
                SetValue(nameof(EyepieceId), value);
                NotifyPropertyChanged(nameof(Eyepiece));
                Calculate();
            }
        }

        public Guid? BinocularId
        {
            get => GetValue<Guid?>(nameof(BinocularId));
            set 
            { 
                SetValue(nameof(BinocularId), value);
                NotifyPropertyChanged(nameof(Binocular));
                Calculate();
            }
        }

        public Guid? CameraId
        {
            get => GetValue<Guid?>(nameof(CameraId));
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

        public FovFrameRotateOrigin RotateOrigin
        {
            get => GetValue<FovFrameRotateOrigin>(nameof(RotateOrigin));
            set
            {
                SetValue(nameof(RotateOrigin), value);
                Calculate();
            }
        }

        public decimal FinderSize1
        {
            get => GetValue<decimal>(nameof(FinderSize1));
            set
            {
                SetValue(nameof(FinderSize1), value);
                Calculate();
            }
        }

        public decimal FinderSize2
        {
            get => GetValue<decimal>(nameof(FinderSize2));
            set
            {
                SetValue(nameof(FinderSize2), value);
                Calculate();
            }
        }

        public bool FinderSize2Enabled
        {
            get => GetValue<bool>(nameof(FinderSize2Enabled));
            set
            {
                SetValue(nameof(FinderSize2Enabled), value);
                Calculate();
            }
        }

        public decimal FinderSize3
        {
            get => GetValue<decimal>(nameof(FinderSize3));
            set
            {
                SetValue(nameof(FinderSize3), value);
                Calculate();
            }
        }

        public bool FinderSize3Enabled
        {
            get => GetValue<bool>(nameof(FinderSize3Enabled));
            set
            {
                SetValue(nameof(FinderSize3Enabled), value);
                Calculate();
            }
        }

        public bool FinderCrosslines
        {
            get => GetValue<bool>(nameof(FinderCrosslines));
            set
            {
                SetValue(nameof(FinderCrosslines), value);
                Calculate();
            }
        }

        public bool ShadingVisible
        {
            get => FrameType != FrameType.Camera;
        }

        public int LabelColspan
        {
            get => FrameType != FrameType.Camera ? 1 : 2;
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
            else if (FrameType == FrameType.Finder && FinderSize1 > 0)
            {
                FieldOfView = new FinderFieldOfView();
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
                    nameof(LensVisible),
                    nameof(FinderVisible),
                    nameof(ShadingVisible),
                    nameof(LabelColspan)
                );
                Calculate();
            }
        }

        public ColorSchema ColorSchema => _Settings.Get<ColorSchema>("Schema");

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

        public bool TelescopeVisible => FrameType != FrameType.Binocular && FrameType != FrameType.Finder;
        public bool EyepieceVisible => FrameType == FrameType.Telescope && FrameType != FrameType.Finder;
        public bool BinocularVisible => FrameType == FrameType.Binocular && FrameType != FrameType.Finder;
        public bool CameraVisible => FrameType == FrameType.Camera && FrameType != FrameType.Finder;
        public bool LensVisible => FrameType != FrameType.Binocular && FrameType != FrameType.Finder;
        public bool FinderVisible => FrameType == FrameType.Finder;

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
                        EyepieceId = EyepieceId ?? Guid.Empty,
                        LensId = LensId,
                        Size = telescopeFieldOfView.Size,
                        TelescopeId = TelescopeId ?? Guid.Empty
                    };
                }
                else if (FieldOfView is CameraFieldOfView cameraFieldOfView)
                {
                    return new CameraFovFrame()
                    {
                        Id = _Id,
                        Color = Color,
                        Label = Label,
                        Enabled = true,
                        LensId = LensId,
                        Width = cameraFieldOfView.Size.Width,
                        Height = cameraFieldOfView.Size.Height,
                        TelescopeId = TelescopeId ?? Guid.Empty,
                        Binning = Binning,
                        CameraId = CameraId ?? Guid.Empty,
                        Rotation = Rotation,
                        RotateOrigin = RotateOrigin
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
                        BinocularId = BinocularId ?? Guid.Empty,
                        Size = binocularFieldOfView.Size
                    };
                }
                else if (FieldOfView is FinderFieldOfView)
                {
                    return new FinderFovFrame()
                    {
                        Id = _Id,
                        Color = Color,
                        Label = Label,
                        Enabled = true,
                        Shading = Shading,
                        Sizes = new float?[] {
                            (float?)FinderSize1,
                            FinderSize2Enabled ? (float?)FinderSize2 : null,
                            FinderSize3Enabled ? (float?)FinderSize3 : null
                        }.Where(s => s != null && s.Value > 0)
                        .Cast<float>().ToArray(),
                        Crosslines = FinderCrosslines
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
                    Shading = telescopeFrame.Shading;
                    FrameType = FrameType.Telescope;
                }
                else if (value is CameraFovFrame cameraFrame)
                {
                    TelescopeId = cameraFrame.TelescopeId;
                    CameraId = cameraFrame.CameraId;
                    LensId = cameraFrame.LensId;
                    Rotation = (int)cameraFrame.Rotation;
                    RotateOrigin = cameraFrame.RotateOrigin;
                    Binning = (int)cameraFrame.Binning;
                    FrameType = FrameType.Camera;
                }
                else if (value is BinocularFovFrame binocularFrame)
                {
                    BinocularId = binocularFrame.BinocularId;
                    Shading = binocularFrame.Shading;
                    FrameType = FrameType.Binocular;                    
                }
                else if (value is FinderFovFrame finderFrame)
                {
                    FinderSize1 = (decimal)finderFrame.Sizes[0];
                    if (finderFrame.Sizes.Length > 1)
                    {
                        FinderSize2Enabled = true;
                        FinderSize2 = (decimal)finderFrame.Sizes[1];
                    }
                    if (finderFrame.Sizes.Length > 2)
                    {
                        FinderSize3Enabled = true;
                        FinderSize3 = (decimal)finderFrame.Sizes[2];
                    }
                    Shading = finderFrame.Shading;
                    FinderCrosslines = finderFrame.Crosslines;
                    FrameType = FrameType.Finder;
                }

                _Id = value.Id;
                Color = value.Color;
                Label = value.Label;
            }
        }
    }

    public enum FrameType
    {
        Telescope = 0,
        Camera = 1,
        Binocular = 2,
        Finder = 3
    }

    public class CameraResolutionConverter : ConverterBase
    {
        public override object Convert(object value)
        {
            if (value is null)
            {
                return null;
            }
            if (value is Camera camera)
            {
                return Convert(new SizeF(camera.HorizontalResolution, camera.VerticalResolution));
            }
            else if (value is SizeF resoltion)
            {
                return Convert(resoltion, Text.Get("FovSettingsVM.SecondsPerPixel"));
            }
            else
            {
                throw new ArgumentException("Unknown value type.", nameof(value));
            }
        }

        private object Convert(SizeF resolution, string units = null)
        {
            return $"{resolution.Width.ToString("0.##", culture)} x {resolution.Height.ToString("0.##", culture)} {units}";
        }
    }

    public class CameraPixelSizeConverter : ConverterBase
    {
        public override object Convert(object value)
        {
            var camera = (Camera)value;
            return camera != null ? $"{camera.PixelSizeWidth.ToString("0.##", culture)} x {camera.PixelSizeHeight.ToString("0.##", culture)} {Text.Get("FovSettingsVM.MicroMeters")}" : null;
        }
    }

    public class MillimetersConverter : ConverterBase
    {
        public override object Convert(object value)
        {
            if (value is float floatValue)
            {
                return $"{floatValue.ToString(".#", culture)} {Text.Get("FovSettingsVM.MilliMeters")}";
            }
            else if (value is int intValue)
            {
                return $"{intValue} {Text.Get("FovSettingsVM.MilliMeters")}";
            }
            else
            {
                throw new ArgumentException("Unknown value type.", nameof(value));
            }
        }
    }

    public abstract class ConverterBase : ValueConverterBase
    {
        protected static CultureInfo culture;
        static ConverterBase()
        {
            culture = new CultureInfo("");
            culture.NumberFormat = new NumberFormatInfo()
            {
                NumberDecimalSeparator = ".",
                NumberGroupSeparator = ""
            };
        }

        public abstract object Convert(object value);

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value);
        }
    }

    public class FieldOfViewConverter : ConverterBase
    {
        public override object Convert(object value)
        {
            if (value is float circularFov)
            {
                return $"{circularFov.ToString("0.##", culture)}°";
            }
            else if (value is SizeF rectFov)
            {
                return $"{rectFov.Width.ToString("0.##", culture)}° x {rectFov.Height.ToString("0.##", culture)}°";
            }
            else
            {
                throw new ArgumentException("Unknown value type.", nameof(value));
            }
        }
    }

    public class MagnificationConverter : ConverterBase
    {
        public override object Convert(object value)
        {
            return $"{((float)value).ToString("0.##", culture)}ˣ";
        }
    }

    public class FocalRatioConveter : ConverterBase
    {
        public override object Convert(object value)
        {
            return $"f/{((float)value).ToString("0.#", culture)}";
        }
    }

    public class ExitPupilConverter : ConverterBase
    {
        public override object Convert(object value)
        {
            return $"{((float)value).ToString("0.##", culture)} {Text.Get("FovSettingsVM.MilliMeters")}";
        }
    }

    public class DawesLimitConverter : ConverterBase
    {
        public override object Convert(object value)
        {
            return $"{((float)value).ToString("0.##", culture)}\"";
        }
    }

    public class FieldOfViewDetailsConverter : ValueConverterBase
    {
        private static readonly FieldOfViewConverter fovConverter = new FieldOfViewConverter();
        private static readonly MagnificationConverter magConverter = new MagnificationConverter();
        private static readonly FocalRatioConveter frConverter = new FocalRatioConveter();
        private static readonly ExitPupilConverter epConverter = new ExitPupilConverter();
        private static readonly CameraResolutionConverter crConverter = new CameraResolutionConverter();
        private static readonly DawesLimitConverter dlConverter = new DawesLimitConverter();

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var fieldOfView = (IFieldOfView)value;
            if (fieldOfView is TelescopeFieldOfView telescopeFieldOfView)
            {
                return $"{Text.Get("FovSettingsVM.FocalRatio")} = {frConverter.Convert(telescopeFieldOfView.FocalRatio)}\n{Text.Get("FovSettingsVM.Magnification")} = {magConverter.Convert(telescopeFieldOfView.Magnification)}\n{Text.Get("FovSettingsVM.FieldOfView")} = {fovConverter.Convert(telescopeFieldOfView.Size)}\n{Text.Get("FovSettingsVM.ExitPupil")} = {epConverter.Convert(telescopeFieldOfView.ExitPupil)}\n{Text.Get("FovSettingsVM.DawesLimit")} = {dlConverter.Convert(telescopeFieldOfView.DawesLimit)}";
            }
            else if (fieldOfView is CameraFieldOfView cameraFieldOfView)
            {
                return $"{Text.Get("FovSettingsVM.FocalRatio")} = {frConverter.Convert(cameraFieldOfView.FocalRatio)}\n{Text.Get("FovSettingsVM.Resolution")} = {crConverter.Convert(cameraFieldOfView.Resolution)}\n{Text.Get("FovSettingsVM.FieldOfView")} = {fovConverter.Convert(cameraFieldOfView.Size)}\n{Text.Get("FovSettingsVM.DawesLimit")} = {dlConverter.Convert(cameraFieldOfView.DawesLimit)}";
            }
            else if (fieldOfView is BinocularFieldOfView binocularFieldOfView)
            {
                return $"{Text.Get("FovSettingsVM.Magnification")} = {magConverter.Convert(binocularFieldOfView.Magnification)}\n{Text.Get("FovSettingsVM.FieldOfView")} = {fovConverter.Convert(binocularFieldOfView.Size)}\n{Text.Get("FovSettingsVM.ExitPupil")} = {epConverter.Convert(binocularFieldOfView.ExitPupil)}\n{Text.Get("FovSettingsVM.DawesLimit")} = {dlConverter.Convert(binocularFieldOfView.DawesLimit)}";
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
