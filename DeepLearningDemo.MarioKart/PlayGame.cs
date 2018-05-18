using CNTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;

namespace DeepLearningDemo.MarioKart
{
    class PlayGame
    {
        static Random rnd = new Random();

        public static string cntkModel = "training/LSTM.model";

        static Function m_model = null;
        static InputSimulator m_InputSimulator = new InputSimulator();

        public static void LoadModel(string modelFileName, DeviceDescriptor device)
        {
            m_model = Function.Load(modelFileName, device);
        }


        public static bool ActivateN64Emulator()
        {
            var hWnd = InterceptKeys.FindWindow("Project64 2.3.2.202", null);
            var sim = new InputSimulator();

            if (!hWnd.Equals(IntPtr.Zero) && hWnd == InterceptKeys.GetForegroundWindow())
                return true;

            return false;
        }

        private static void ReleaseKey(VirtualKeyCode key)
        {
            m_InputSimulator.Keyboard.KeyUp(key);
        }

        private static void PressKey(VirtualKeyCode key)
        {
            m_InputSimulator.Keyboard.KeyDown(key);
        }

        public static void MoveForward()
        {
            ReleaseKey(VirtualKeyCode.LEFT);
            ReleaseKey(VirtualKeyCode.RIGHT);
            ReleaseKey(VirtualKeyCode.VK_C);
            PressKey(VirtualKeyCode.VK_X);
        }

        public static void Break()
        {
            ReleaseKey(VirtualKeyCode.VK_X);
            ReleaseKey(VirtualKeyCode.LEFT);
            ReleaseKey(VirtualKeyCode.RIGHT);
            PressKey(VirtualKeyCode.VK_C);
        }

        public static void MoveForwardLeft()
        {
            ReleaseKey(VirtualKeyCode.VK_C);
            ReleaseKey(VirtualKeyCode.RIGHT);
            PressKey(VirtualKeyCode.VK_X);
            PressKey(VirtualKeyCode.LEFT);
        }

        public static void MoveForwardRight()
        {
            ReleaseKey(VirtualKeyCode.VK_C);
            ReleaseKey(VirtualKeyCode.LEFT);
            PressKey(VirtualKeyCode.VK_X);
            PressKey(VirtualKeyCode.RIGHT);
        }

        public static void None()
        {
            if (rnd.Next(0, 3) == 1)
                PressKey(VirtualKeyCode.VK_X);
            else
                ReleaseKey(VirtualKeyCode.VK_X);

            ReleaseKey(VirtualKeyCode.LEFT);
            ReleaseKey(VirtualKeyCode.RIGHT);
            ReleaseKey(VirtualKeyCode.VK_C);
        }

        public static void DeepLearningPlay(DeviceDescriptor device)
        {
            while (true)
            {
                var b = GenerateData.Capture(GenerateData.rec);

                var retValue = GenerateData.ResizeAndCHWExctraction(b).ToArray();

                Play(retValue, device);
            }
        }

        public static async Task RandomPlay()
        {
            while (true)
            {
                await Task.Delay(100);

                if (!ActivateN64Emulator())
                    continue;

                var outValue = rnd.Next(0, 4);

                if (outValue == 0)
                    MoveForward();
                else if (outValue == 1)
                    Break();
                else if (outValue == 2)
                    MoveForwardLeft();
                else if (outValue == 3)
                    MoveForwardRight();
                else
                    None();
            }
        }

        private static void Play(float[] xVal, DeviceDescriptor device)
        {
            var feature = m_model.Arguments[0];
            var label = m_model.Outputs.Last();

            var xValues = Value.CreateBatch(feature.Shape, xVal, device);

            var inputDataMap = new Dictionary<Variable, Value>
            {
                [feature] = xValues
            };
            var outputDataMap = new Dictionary<Variable, Value>
            {
                [label] = null
            };

            m_model.Evaluate(inputDataMap, outputDataMap, device);

            var outputData = outputDataMap[label].GetDenseData<float>(label);

            bool skip = true;
            foreach (var val in outputData.First())
            {
                if (!float.IsNaN(val) && val != float.MinValue && val != float.MaxValue)
                    skip = false;
            }

            if (skip)
                return;

            var outValue = outputData.Select((IList<float> l) => l.IndexOf(l.Max())).FirstOrDefault();

            if (!ActivateN64Emulator())
                return;

            if (outValue == 0)
                MoveForward();
            else if (outValue == 1)
                Break();
            else if (outValue == 2)
                MoveForwardLeft();
            else if (outValue == 3)
                MoveForwardRight();
            else
                None();
        }
    }
}
