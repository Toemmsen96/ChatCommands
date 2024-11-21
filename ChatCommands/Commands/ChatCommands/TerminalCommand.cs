using static ChatCommands.Utils;
using HarmonyLib;
using System.Threading.Tasks;


namespace ChatCommands.Commands
{
    internal class TerminalCommand : CustomChatCommand
    {

        public override string Name => "Send Terminal Command";

        public override string Description => "Send a terminal commmand to the terminal and receive a response.";

        public override string Format => "/term [terminalcommand]";
        public override string AltFormat => "/terminal [terminalcommand]";
        public override bool IsHostCommand => false;
        public override void Execute(CommandInput message)
        { 
            if (message.Args.Count == 0)
            {
                DisplayChatError("No command entered.");
                return;
            }
            Terminal term = ChatCommands.FindObjectOfType<Terminal>();
            string argsAsString = string.Join(" ", message.Args);
            if (term == null)
            {
                DisplayChatError("Terminal not found.");
                return;
            }
            term.BeginUsingTerminal();
            term.currentText = "";
            term.screenText.text = argsAsString;
            term.OnSubmit();
            DisplayChatMessage("Command sent: " + argsAsString);
            delayedTerminalCommand(term, argsAsString);

        
        }
        private static async void delayedTerminalCommand(Terminal term, string command)
        {
            foreach (SelectableLevel level in term.moonsCatalogueList)
            {
                if (level.PlanetName.ToLower().Contains(command.ToLower()))
                {
                    LogInfo("Found Moon: " + command);
                    await Task.Delay(300);
					term.currentText = "";
					term.screenText.text = "confirm";
					term.OnSubmit();
                    break;
                }
            }
            if (command.ToLower().Contains("buy"))
            {
                LogInfo("Trying to buy");
                await Task.Delay(300);
                term.currentText = "";
                term.screenText.text = "confirm";
                term.OnSubmit();
            }
            await Task.Delay(300);
			term.QuitTerminal();
        }}}