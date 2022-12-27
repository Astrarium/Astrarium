using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Astrarium.Plugins.ASCOM
{
    [Singleton(typeof(IJoystickManager))]
    public class JoystickManager : IJoystickManager
    {
        private const string GLFW = "glfw";

        private const int GLFW_TRUE = 1;

        [DllImport(GLFW)]
        private static extern void glfwPollEvents();

        [DllImport(GLFW)]
        private static extern bool glfwInit();

        [DllImport(GLFW)]
        private static extern int glfwJoystickPresent(int jid);

        [DllImport(GLFW)]
        private static extern IntPtr glfwGetJoystickName(int jid);

        [DllImport(GLFW)]
        private static extern IntPtr glfwGetJoystickButtons(int jid, ref int count);

        [DllImport(GLFW)]
        private static extern IntPtr glfwGetJoystickAxes(int jid, ref int count);

        private ManualResetEvent isEnabledEvent = new ManualResetEvent(false);

        private readonly List<JoystickDevice> allDevices = Enumerable.Range(0, 16).Select(index => new JoystickDevice() { Index = index }).ToList();

        private bool isEnabled = false;
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                isEnabled = value;
                if (isEnabled)
                {
                    isEnabledEvent.Set();
                }
                else
                {
                    isEnabledEvent.Reset();
                }
            }
        }

        public ICollection<JoystickDevice> Devices => allDevices.Where(x => !string.IsNullOrEmpty(x.Name)).ToArray();

        private JoystickDevice selectedDevice = null;
        public JoystickDevice SelectedDevice
        {
            get => selectedDevice;
            set
            {
                if (selectedDevice != value)
                {
                    selectedDevice = value;
                }
                SelectedDeviceChanged?.Invoke();
            }
        }

        public event Action DevicesListChanged;
        public event Action<string, bool> ButtonStateChanged;
        public event Action SelectedDeviceChanged;

        public JoystickManager()
        {
            Thread joystickThread = new Thread(Poll) { IsBackground = true };
            joystickThread.SetApartmentState(ApartmentState.STA);
            joystickThread.Start();
        }

        [HandleProcessCorruptedStateExceptions]
        private void Init()
        {
            int retryCount = 0;
            while (true)
            {
                try
                {
                    glfwInit();
                    break;
                }
                catch
                {
                    retryCount++;
                    if (retryCount < 5)
                        Thread.Sleep(1000);
                    else
                        break;
                }
            }
        }
        
        private void Poll()
        {
            Init();
            
            while (true)
            {
                glfwPollEvents();
                bool needRefresh = false;

                for (int j = 0; j < allDevices.Count; j++)
                {
                    var joystick = allDevices[j];
                    bool isConnected = glfwJoystickPresent(j) == GLFW_TRUE;

                    byte[] buttons = new byte[0];
                    float[] axes = new float[0];

                    if (isConnected)
                    {
                        string name = PtrToStringUtf8(glfwGetJoystickName(j));

                        int count = 0;
                        IntPtr buttonStatesPtr = glfwGetJoystickButtons(j, ref count);
                        buttons = PtrToByteArray(buttonStatesPtr, count);

                        IntPtr axisStates = glfwGetJoystickAxes(j, ref count);
                        axes = PtrToArrayOfFloat(axisStates, count);

                        if (joystick.Name != name)
                        {
                            joystick.Name = name;
                            needRefresh = true;
                        }
                    }

                    if (joystick.IsConnected != isConnected)
                    {
                        joystick.IsConnected = isConnected;
                        joystick.Buttons.Clear();
                        needRefresh = true;
                    }

                    if (!joystick.Buttons.Any())
                    {
                        for (int a = 0; a < axes.Length; a++)
                        {
                            joystick.Buttons.Add(new JoystickButton() { Button = $"Axis {a + 1} PLUS" });
                            joystick.Buttons.Add(new JoystickButton() { Button = $"Axis {a + 1} MINUS" });
                        }

                        for (int b = 0; b < buttons.Length; b++)
                        {
                            joystick.Buttons.Add(new JoystickButton() { Button = $"Button {b + 1}" });
                        }
                    }

                    if (needRefresh)
                    {
                        DevicesListChanged?.Invoke();
                    }

                    for (int a = 0; a < axes.Length; a++)
                    {
                        var axisDec = joystick.Buttons.FirstOrDefault(x => x.Button == $"Axis {a + 1} MINUS");
                        if (axisDec != null)
                        {
                            bool isPressed = axes[a] < -0.5;
                            if (isPressed != axisDec.IsPressed)
                            {
                                axisDec.IsPressed = isPressed;
                                ButtonStateChanged?.Invoke(axisDec.Button, isPressed);
                            }
                        }

                        var axisInc = joystick.Buttons.FirstOrDefault(x => x.Button == $"Axis {a + 1} PLUS");
                        if (axisInc != null)
                        {
                            bool isPressed = axes[a] > 0.5;
                            if (isPressed != axisInc.IsPressed)
                            {
                                axisInc.IsPressed = isPressed;
                                ButtonStateChanged?.Invoke(axisInc.Button, isPressed);
                            }
                        }
                    }

                    for (int b = 0; b < buttons.Length; b++)
                    {
                        bool isPressed = buttons[b] == 1;
                        var button = joystick.Buttons.FirstOrDefault(x => x.Button == $"Button {b + 1}");
                        if (button != null)
                        {
                            if (isPressed != button.IsPressed)
                            {
                                button.IsPressed = isPressed;
                                ButtonStateChanged?.Invoke(button.Button, isPressed);
                            }
                        }
                    }
                }

                if (needRefresh)
                {
                    DevicesListChanged?.Invoke();
                }

                Thread.Sleep(10);
                isEnabledEvent.WaitOne();
            }
        }

        private static string PtrToStringUtf8(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
            {
                return null;
            }
            int len = 0;
            while (Marshal.ReadByte(ptr, len) != 0)
            {
                len++;
            }
            if (len == 0)
            {
                return null;
            }
            byte[] array = new byte[len];
            Marshal.Copy(ptr, array, 0, len);
            return Encoding.UTF8.GetString(array);
        }

        private static byte[] PtrToByteArray(IntPtr ptr, int count)
        {
            byte[] array = new byte[count];
            Marshal.Copy(ptr, array, 0, count);
            return array;
        }

        private static float[] PtrToArrayOfFloat(IntPtr ptr, int count)
        {
            float[] array = new float[count];
            Marshal.Copy(ptr, array, 0, count);
            return array;
        }
    }
}
