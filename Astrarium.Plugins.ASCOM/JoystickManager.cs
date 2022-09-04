using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Astrarium.Types;
using OpenTK.Input;

namespace Astrarium.Plugins.ASCOM
{
    [Singleton(typeof(IJoystickManager))]
    public class JoystickManager : IJoystickManager
    {
        public event Action DevicesListChanged;
        public event Action<string, bool> ButtonStateChanged;

        private readonly List<JoystickDevice> allDevices = new List<JoystickDevice>()
        {
            new JoystickDevice() { Index = 0 },
            new JoystickDevice() { Index = 1 },
            new JoystickDevice() { Index = 2 },
            new JoystickDevice() { Index = 3 }
        };

        public ICollection<JoystickDevice> Devices => allDevices.Where(x => !string.IsNullOrEmpty(x.Name) && x.Buttons.Count > 0).ToArray();

        public JoystickDevice SelectedDevice { get; set; }

        public JoystickManager()
        {
            Thread joystickThread = new Thread(Poll) { IsBackground = true };
            joystickThread.Start();
        }

        private void Poll()
        {
            while (true)
            {
                RefreshDevicesList();
                RefreshButtonStates();
                Thread.Sleep(10);
            }
        }

        private void RefreshDevicesList()
        {
            bool needRefresh = false;

            for (int i = 0; i < 4; i++)
            {
                var device = allDevices[i];
                device.IsConnected = GamePad.GetState(i).IsConnected;

                Guid id = Joystick.GetGuid(i);
                if (id != device.Id)
                {
                    device.Id = id;
                    device.Name = $"Joystick #{i + 1}";

                    int buttonCount = Joystick.GetCapabilities(i).ButtonCount;
                    int axisCount = Joystick.GetCapabilities(i).AxisCount;

                    // init buttons
                    device.Buttons.Clear();

                    if (buttonCount > 2)
                    {
                        // init axis
                        for (int a = 0; a < axisCount; a++)
                        {
                            device.Buttons.Add(new JoystickButton() { Button = $"Axis {a} PLUS" });
                            device.Buttons.Add(new JoystickButton() { Button = $"Axis {a} MINUS" });
                        }

                        // init buttons
                        for (int b = 0; b < buttonCount; b++)
                        {
                            device.Buttons.Add(new JoystickButton() { Button = $"Button {b}" });
                        }
                    }

                    needRefresh = true;
                }
            }

            if (needRefresh)
            {
                DevicesListChanged?.Invoke();
            }
        }

        private void RefreshButtonStates()
        {
            var device = SelectedDevice;

            if (device != null)
            {
                int index = device.Index;
                var name = GamePad.GetName(index);
                var state = Joystick.GetState(index);
                var cap = Joystick.GetCapabilities(index);

                int axisCount = cap.AxisCount;
                int buttonsCount = cap.ButtonCount;

                // update axis
                for (int a = 0; a < axisCount; a++)
                {
                    var axisDec = device.Buttons.FirstOrDefault(x => x.Button == $"Axis {a} MINUS");
                    if (axisDec != null)
                    {
                        bool isPressed = state.GetAxis(a) < -0.5;
                        if (isPressed != axisDec.IsPressed)
                        {
                            axisDec.IsPressed = isPressed;
                            ButtonStateChanged?.Invoke(axisDec.Button, isPressed);
                        }
                    }

                    var axisInc = device.Buttons.FirstOrDefault(x => x.Button == $"Axis {a} PLUS");
                    if (axisInc != null)
                    {
                        bool isPressed = state.GetAxis(a) > 0.5;
                        if (isPressed != axisInc.IsPressed)
                        {
                            axisInc.IsPressed = isPressed;
                            ButtonStateChanged?.Invoke(axisInc.Button, isPressed);
                        }
                    }
                }

                // update buttons
                for (int b = 0; b < buttonsCount; b++)
                {
                    var button = device.Buttons.FirstOrDefault(x => x.Button == $"Button {b}");
                    if (button != null)
                    {
                        bool isPressed = state.GetButton(b) == OpenTK.Input.ButtonState.Pressed;
                        if (isPressed != button.IsPressed)
                        {
                            button.IsPressed = isPressed;
                            ButtonStateChanged?.Invoke(button.Button, isPressed);
                        }
                    }
                }
            }
        }
    }
}
