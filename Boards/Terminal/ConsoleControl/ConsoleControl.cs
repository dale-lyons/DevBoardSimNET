﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConsoleControl
{
    /// <summary>
    /// The console event handler is used for console events.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="ConsoleEventArgs"/> instance containing the event data.</param>
    public delegate void ConsoleEventHandler(object sender, ConsoleEventArgs args);
    public delegate void ConsoleKeypressHandler(char ch);

    [ToolboxBitmap(typeof(Resfinder), "ConsoleControl.ConsoleControl.bmp")]
    public partial class ConsoleControl : UserControl
    {
        public event ConsoleKeypressHandler OnConsoleKeypress;
        public ConsoleControl()
        {
            InitializeComponent();
            //  Show diagnostics disabled by default.
            ShowDiagnostics = false;

            //  Input enabled by default.
            IsInputEnabled = true;

            //  Disable special commands by default.
            SendKeyboardCommandsToProcess = false;

            //  Initialise the keymappings.
            InitialiseKeyMappings();

            //  Handle process events.
            //processInterace.OnProcessOutput += processInterace_OnProcessOutput;
            //processInterace.OnProcessError += processInterace_OnProcessError;
            //processInterace.OnProcessInput += processInterace_OnProcessInput;
            //processInterace.OnProcessExit += processInterace_OnProcessExit;

            //  Wait for key down messages on the rich text box.
            richTextBox1.KeyDown += richTextBoxConsole_KeyDown;
        }

        /// <summary>
        /// Handles the OnProcessError event of the processInterace control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="ProcessEventArgs"/> instance containing the event data.</param>
        //void processInterace_OnProcessError(object sender, ProcessEventArgs args)
        //{
        //    //  Write the output, in red
        //    WriteOutput(args.Content, Color.Red);

        //    //  Fire the output event.
        //    FireConsoleOutputEvent(args.Content);
        //}

        /// <summary>
        /// Handles the OnProcessOutput event of the processInterace control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="ProcessEventArgs"/> instance containing the event data.</param>
        //void processInterace_OnProcessOutput(object sender, ProcessEventArgs args)
        //{
        //    //  Write the output, in white
        //    WriteOutput(args.Content, Color.White);

        //    //  Fire the output event.
        //    FireConsoleOutputEvent(args.Content);
        //}

        /// <summary>
        /// Handles the OnProcessInput event of the processInterace control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="ProcessEventArgs"/> instance containing the event data.</param>
        //void processInterace_OnProcessInput(object sender, ProcessEventArgs args)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// Handles the OnProcessExit event of the processInterace control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="ProcessEventArgs"/> instance containing the event data.</param>
        //void processInterace_OnProcessExit(object sender, ProcessEventArgs args)
        //{
        //    //  Are we showing diagnostics?
        //    if (ShowDiagnostics)
        //    {
        //        WriteOutput(Environment.NewLine + processInterace.ProcessFileName + " exited.", Color.FromArgb(255, 0, 255, 0));
        //    }

        //    if (!this.IsHandleCreated)
        //        return;
        //    //  Read only again.
        //    Invoke((Action)(() =>
        //    {
        //        richTextBox1.ReadOnly = true;
        //    }));
        //}

        /// <summary>
        /// Initialises the key mappings.
        /// </summary>
        private void InitialiseKeyMappings()
        {
            //  Map 'tab'.
            keyMappings.Add(new KeyMapping(false, false, false, Keys.Tab, "{TAB}", "\t"));

            //  Map 'Ctrl-C'.
            keyMappings.Add(new KeyMapping(true, false, false, Keys.C, "^(c)", "\x03\r\n"));
        }

        /// <summary>
        /// Handles the KeyDown event of the richTextBoxConsole control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
        void richTextBoxConsole_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            OnConsoleKeypress?.Invoke((char)e.KeyValue);
            return;

            //  Check whether we are in the read-only zone.
            var isInReadOnlyZone = richTextBox1.SelectionStart < inputStart;

            //  Are we sending keyboard commands to the process?
            if (SendKeyboardCommandsToProcess && IsProcessRunning)
            {
                //  Get key mappings for this key event?
                var mappings = from k in keyMappings
                               where
                               (k.KeyCode == e.KeyCode &&
                               k.IsAltPressed == e.Alt &&
                               k.IsControlPressed == e.Control &&
                               k.IsShiftPressed == e.Shift)
                               select k;

                //  Go through each mapping, send the message.
                //foreach (var mapping in mappings)
                //{
                //SendKeysEx.SendKeys(CurrentProcessHwnd, mapping.SendKeysMapping);
                //inputWriter.WriteLine(mapping.StreamMapping);
                //WriteInput("\x3", Color.White, false);
                //}

                //  If we handled a mapping, we're done here.
                if (mappings.Any())
                {
                    e.SuppressKeyPress = true;
                    return;
                }
            }

            //  If we're at the input point and it's backspace, bail.
            if ((richTextBox1.SelectionStart <= inputStart) && e.KeyCode == Keys.Back) e.SuppressKeyPress = true;

            //  Are we in the read-only zone?
            if (isInReadOnlyZone)
            {
                //  Allow arrows and Ctrl-C.
                if (!(e.KeyCode == Keys.Left ||
                    e.KeyCode == Keys.Right ||
                    e.KeyCode == Keys.Up ||
                    e.KeyCode == Keys.Down ||
                    (e.KeyCode == Keys.C && e.Control)))
                {
                    e.SuppressKeyPress = true;
                }
            }

            //  Write the input if we hit return and we're NOT in the read only zone.
            if (e.KeyCode == Keys.Return && !isInReadOnlyZone)
            {
                //  Get the input.
                string input = richTextBox1.Text.Substring(inputStart, (richTextBox1.SelectionStart) - inputStart);

                //  Write the input (without echoing).
                WriteInput(input, Color.White, false);
            }
        }
        public void WriteOutput(char ch, Color color)
        {
            if (!this.IsHandleCreated)
                return;

            if (ch == '\r')
                return;

            Invoke((Action)(() =>
            {
                //  Write the output.
                richTextBox1.SelectionColor = color;
                richTextBox1.SelectedText += ch;
//                inputStart = richTextBox1.SelectionStart;
            }));
            Application.DoEvents();
        }

        /// <summary>
        /// Writes the output to the console control.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="color">The color.</param>
        public void WriteOutput222(string output, Color color)
        {
            if (string.IsNullOrEmpty(lastInput) == false &&
                (output == lastInput || output.Replace("\r\n", "") == lastInput))
                return;

            if (!this.IsHandleCreated)
                return;

            output.Replace("\r", "");

            Invoke((Action)(() =>
            {
                //  Write the output.
                richTextBox1.SelectionColor = color;
                richTextBox1.SelectedText += output;
                inputStart = richTextBox1.SelectionStart;
            }));
        }

        /// <summary>
        /// Clears the output.
        /// </summary>
        public void ClearOutput()
        {
            richTextBox1.Clear();
            inputStart = 0;
        }

        /// <summary>
        /// Writes the input to the console control.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="color">The color.</param>
        /// <param name="echo">if set to <c>true</c> echo the input.</param>
        public void WriteInput(string input, Color color, bool echo)
        {
            Invoke((Action)(() =>
            {
                //  Are we echoing?
                if (echo)
                {
                    richTextBox1.SelectionColor = color;
                    richTextBox1.SelectedText += input;
                    inputStart = richTextBox1.SelectionStart;
                }

                lastInput = input;

                //  Write the input.
                processInterace.WriteInput(input);

                //  Fire the event.
                FireConsoleInputEvent(input);
            }));
        }



        /// <summary>
        /// Runs a process.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="arguments">The arguments.</param>
        //public void StartProcess(string fileName, string arguments)
        //{
        //    //  Are we showing diagnostics?
        //    if (ShowDiagnostics)
        //    {
        //        WriteOutput("Preparing to run " + fileName, Color.FromArgb(255, 0, 255, 0));
        //        if (!string.IsNullOrEmpty(arguments))
        //            WriteOutput(" with arguments " + arguments + "." + Environment.NewLine, Color.FromArgb(255, 0, 255, 0));
        //        else
        //            WriteOutput("." + Environment.NewLine, Color.FromArgb(255, 0, 255, 0));
        //    }

        //    //  Start the process.
        //    processInterace.StartProcess(fileName, arguments);

        //    //  If we enable input, make the control not read only.
        //    if (IsInputEnabled)
        //        richTextBox1.ReadOnly = false;
        //}

        ///// <summary>
        ///// Runs a process.
        ///// </summary>
        ///// <param name="processStartInfo"><see cref="ProcessStartInfo"/> to pass to the process.</param>
        //public void StartProcess(ProcessStartInfo processStartInfo)
        //{
        //    //  Are we showing diagnostics?
        //    if (ShowDiagnostics)
        //    {
        //        WriteOutput("Preparing to run " + processStartInfo.FileName, Color.FromArgb(255, 0, 255, 0));
        //        if (!string.IsNullOrEmpty(processStartInfo.Arguments))
        //            WriteOutput(" with arguments " + processStartInfo.Arguments + "." + Environment.NewLine, Color.FromArgb(255, 0, 255, 0));
        //        else
        //            WriteOutput("." + Environment.NewLine, Color.FromArgb(255, 0, 255, 0));
        //    }

        //    //  Start the process.
        //    processInterace.StartProcess(processStartInfo);

        //    //  If we enable input, make the control not read only.
        //    if (IsInputEnabled)
        //        richTextBox1.ReadOnly = false;
        //}

        ///// <summary>
        ///// Stops the process.
        ///// </summary>
        //public void StopProcess()
        //{
        //    //  Stop the interface.
        //    processInterace.StopProcess();
        //}

        /// <summary>
        /// Fires the console output event.
        /// </summary>
        /// <param name="content">The content.</param>
        private void FireConsoleOutputEvent(string content)
        {
            //  Get the event.
            var theEvent = OnConsoleOutput;
            if (theEvent != null)
                theEvent(this, new ConsoleEventArgs(content));
        }

        /// <summary>
        /// Fires the console input event.
        /// </summary>
        /// <param name="content">The content.</param>
        private void FireConsoleInputEvent(string content)
        {
            //  Get the event.
            var theEvent = OnConsoleInput;
            if (theEvent != null)
                theEvent(this, new ConsoleEventArgs(content));
        }

        /// <summary>
        /// The internal process interface used to interface with the process.
        /// </summary>
        private readonly ProcessInterface processInterace = new ProcessInterface();

        /// <summary>
        /// Current position that input starts at.
        /// </summary>
        int inputStart = -1;

        /// <summary>
        /// The is input enabled flag.
        /// </summary>
        private bool isInputEnabled = true;

        /// <summary>
        /// The last input string (used so that we can make sure we don't echo input twice).
        /// </summary>
        private string lastInput;

        /// <summary>
        /// The key mappings.
        /// </summary>
        private List<KeyMapping> keyMappings = new List<KeyMapping>();

        /// <summary>
        /// Occurs when console output is produced.
        /// </summary>
        public event ConsoleEventHandler OnConsoleOutput;

        /// <summary>
        /// Occurs when console input is produced.
        /// </summary>
        public event ConsoleEventHandler OnConsoleInput;

        /// <summary>
        /// Gets or sets a value indicating whether to show diagnostics.
        /// </summary>
        /// <value>
        ///   <c>true</c> if show diagnostics; otherwise, <c>false</c>.
        /// </value>
        [Category("Console Control"), Description("Show diagnostic information, such as exceptions.")]
        public bool ShowDiagnostics
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is input enabled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is input enabled; otherwise, <c>false</c>.
        /// </value>
        [Category("Console Control"), Description("If true, the user can key in input.")]
        public bool IsInputEnabled
        {
            get { return isInputEnabled; }
            set
            {
                isInputEnabled = value;
                if (IsProcessRunning)
                    richTextBox1.ReadOnly = !value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [send keyboard commands to process].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [send keyboard commands to process]; otherwise, <c>false</c>.
        /// </value>
        [Category("Console Control"), Description("If true, special keyboard commands like Ctrl-C and tab are sent to the process.")]
        public bool SendKeyboardCommandsToProcess
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is process running.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is process running; otherwise, <c>false</c>.
        /// </value>
        [Browsable(false)]
        public bool IsProcessRunning
        {
            get { return processInterace.IsProcessRunning; }
        }

        /// <summary>
        /// Gets the internal rich text box.
        /// </summary>
        [Browsable(false)]
        public RichTextBox InternalRichTextBox
        {
            get { return richTextBox1; }
        }

        /// <summary>
        /// Gets the process interface.
        /// </summary>
        [Browsable(false)]
        public ProcessInterface ProcessInterface
        {
            get { return processInterace; }
        }

        /// <summary>
        /// Gets the key mappings.
        /// </summary>
        [Browsable(false)]
        public List<KeyMapping> KeyMappings
        {
            get { return keyMappings; }
        }

        /// <summary>
        /// Gets or sets the font of the text displayed by the control.
        /// </summary>
        /// <returns>The <see cref="T:System.Drawing.Font" /> to apply to the text displayed by the control. The default is the value of the <see cref="P:System.Windows.Forms.Control.DefaultFont" /> property.</returns>
        ///   <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
        ///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   </PermissionSet>
        //public override Font Font
        //{
        //    get
        //    {
        //        //  Return the base class font.
        //        return base.Font;
        //    }
        //    set
        //    {
        //        //  Set the base class font...
        //        base.Font = value;

        //        //  ...and the internal control font.
        //        richTextBox1.Font = value;
        //    }
        //}

        /// <summary>
        /// Gets or sets the background color for the control.
        /// </summary>
        /// <returns>A <see cref="T:System.Drawing.Color" /> that represents the background color of the control. The default is the value of the <see cref="P:System.Windows.Forms.Control.DefaultBackColor" /> property.</returns>
        ///   <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   </PermissionSet>
        public override Color BackColor
        {
            get
            {
                //  Return the base class background.
                return base.BackColor;
            }
            set
            {
                //  Set the base class background...
                base.BackColor = value;

                //  ...and the internal control background.
                richTextBox1.BackColor = value;
            }
        }
    }
    public class Resfinder { }

}