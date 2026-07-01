using System;
using System.Runtime.InteropServices;
using Level2.PlateLeveler.DataFunction;

namespace Level2.PlateLeveler.Server {
    internal sealed class Program {
        //Requirements to disable the red X for closing the console
        [DllImport("user32.dll")]
        private static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        internal const uint SC_CLOSE = 0xF060;
        internal const uint MF_GRAYED = 0x00000001;

        //Use this to disable QuickEditMode
        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, int mode);
        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out int mode);
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(int handle);

        private static void Main(string[] args) {
            #region Window behaviour configuration
            //use this to disable the red X for closing the console!
            var current = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
            _ = EnableMenuItem(GetSystemMenu(current, false), SC_CLOSE, MF_GRAYED);

            DisableQuickEditMode();
            Console.CancelKeyPress += Console_CancelKeyPress;
            #endregion Window behaviour configuration

            #region Initialization
            Console.Title = "Plate Leveler";
            var controller = new PlateLeveller_Controller();
            #endregion Initialization

            #region Terminal input handling
            var quit = false;
            //Main loop
            while (!quit) {
                var input = Console.ReadLine();
                if (input.Equals("q", StringComparison.OrdinalIgnoreCase)) {
                    Console.Write("Now quiting");
                    System.Threading.Thread.Sleep(800);
                    Console.Write(".");
                    System.Threading.Thread.Sleep(800);
                    Console.Write(".");
                    System.Threading.Thread.Sleep(800);
                    Console.Write(".");
                    System.Threading.Thread.Sleep(800);
                    quit = true;
                } else if (input.Equals("test", StringComparison.OrdinalIgnoreCase)) {
                    controller.TestL3PDOInterface_Send();
                } else if (input.Equals("testl1pdi", StringComparison.OrdinalIgnoreCase)) {
                    controller.Test_L2_L1_PDI_Telegram();
                } else if (input.Equals("testl1adj", StringComparison.OrdinalIgnoreCase)) {
                    controller.Test_L2_L1_ADJ_Telegram();
                } else if (input.Equals("filldbvalues", StringComparison.OrdinalIgnoreCase)) {
                    controller.FillDBValues();
                } else if (input.ToLower(System.Globalization.CultureInfo.CurrentCulture) is "h" or "help") {
                    Console.WriteLine("-----------------------------------");
                    Console.WriteLine("Supported inputs are:");
                    Console.WriteLine("q: \t\t The application will quit after a short period of time.");
                    Console.WriteLine("-----------------------------------");
                } else {
                    Console.WriteLine("Did not recognize your input! Enter 'h' or 'help' for details.");
                }
            }
            #endregion Terminal input handling

            Logging.SendMessage("Stopping leveler application", System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Info, typeof(Program));
            Environment.Exit(0);
        }

        /// <summary>
        /// Disables Console QuickEditMode which can be a big problem especially with Windows 10
        /// </summary>
        /// <param name="DisableMouseInput"></param>
        private static void DisableQuickEditMode(bool DisableMouseInput = false) {
            const int ENABLE_QUICK_EDIT = 0x0040;
            //const int ENABLE_EXTENDED_FLAGS = 0x0080;
            const int STD_INPUT_HANDLE = -10;

            //IntPtr consoleHandle = GetConsoleWindow();
            var consoleHandle = GetStdHandle(STD_INPUT_HANDLE);

            //get current console mode
            if (!GetConsoleMode(consoleHandle, out var consoleMode)) {
                //Error: Unable to get console mode.
                return;
            }

            //Clear the quick edit bit in the mode flags
            consoleMode &= ~ENABLE_QUICK_EDIT;
            //Maybe this has to be used too
            //consoleMode |= ENABLE_EXTENDED_FLAGS;

            if (DisableMouseInput) {
                const int ENABLE_MOUSE_INPUT = 0x0010;
                consoleMode &= ~ENABLE_MOUSE_INPUT;
            }

            //set the new mode
            if (!SetConsoleMode(consoleHandle, consoleMode)) {
                //ERROR: Unable to set console mode
            }
        }

        /// <summary>
        /// Disables CTRL+C and so on
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">e.Cancel is set to true</param>
        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e) => e.Cancel = true;
    }
}
