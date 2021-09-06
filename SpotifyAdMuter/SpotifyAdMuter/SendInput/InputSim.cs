using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SpotifyAdMuter.SendInput
{
    /// <summary>
    /// An API for interacting with the low-level operating system input streams.
    /// </summary>
    public class InputSim
    {

        /// <summary>
        /// Synthesizes keystrokes, mouse motions, and button clicks.
        /// </summary>
        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs,
           [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs,
           int cbSize);

        /// <summary>
        /// Translate Virtual Key Code to a Scan Code.
        /// </summary>
        const uint MAPVK_VK_TO_VSC = 0;

        // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-mapvirtualkeya
        [DllImport("user32.dll")]
        private static extern uint MapVirtualKeyA(uint uCode, uint uMapType);

        /// <summary>
        /// Gets the "Scan Code" for the given key.
        /// </summary>
        private static uint GetScanCode(Keys key) => MapVirtualKeyA((uint)key, MAPVK_VK_TO_VSC);

        /// <summary>
        /// Sends the given <paramref name="keys"/> to the operating system input stream.
        /// </summary>
        /// <param name="keys"></param>
        /// <returns>The function returns the number of events that it successfully inserted into the keyboard or mouse input stream.<para/>If the function returns zero, the input was already blocked by another thread. This function fails when it is blocked by UIPI.</returns>
        public static uint SendInputKeys(params Keys[] keys)
        {
            INPUT[] inputs = new INPUT[keys.Length];
            for(int i = 0; i < keys.Length; i++)
            {
                INPUT input = new INPUT();

                input.type = InputType.Keyboard; // Keyboard Input
                input.U.ki.wScan = (ushort)GetScanCode(keys[i]); // We need to convert the virtual key code to a scan code as we are using the SCANCODE mode.
                input.U.ki.dwFlags = KeyboardEvent.KEYEVENTF_SCANCODE; // We choose SCANCODE mode

                inputs[i] = input;
            }

            return SendInput((uint)inputs.Length, inputs, INPUT.Size);
        }

        #region Struct Definitions

        [StructLayout(LayoutKind.Sequential)]
        internal struct INPUT
        {
            public InputType type;
            public InputUnion U;
            public static int Size
            {
                get => Marshal.SizeOf(typeof(INPUT));
            }
        }

        internal enum InputType : uint
        {
            Mouse = 0,
            Keyboard = 1,
            Hardware = 2
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct InputUnion
        {
            [FieldOffset(0)]
            internal MOUSEINPUT mi;

            [FieldOffset(0)]
            internal KEYBDINPUT ki;

            [FieldOffset(0)]
            internal HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct KEYBDINPUT
        {
            internal ushort wVk;
            internal ushort wScan;
            internal KeyboardEvent dwFlags;
            internal uint time;
            internal UIntPtr dwExtraInfo;
        }

        [Flags]
        internal enum KeyboardEvent : uint
        {
            KEYEVENTF_EXTENDEDKEY = 0x0001,
            KEYEVENTF_KEYUP = 0x0002,
            KEYEVENTF_SCANCODE = 0x0008,
            KEYEVENTF_UNICODE = 0x0004
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MOUSEINPUT
        {
            internal uint dx;
            internal uint dy;
            internal uint mouseData;
            internal uint dwFlags;
            internal uint time;
            internal UIntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HARDWAREINPUT
        {
            internal uint uMsg;
            internal ushort wParamL;
            internal ushort wParamH;
        }

        #endregion
    }
}
