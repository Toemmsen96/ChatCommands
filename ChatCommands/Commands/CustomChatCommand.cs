namespace ChatCommands.Commands
{
    internal abstract class CustomChatCommand
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string Format { get; }
        public abstract string AltFormat { get; }
        public abstract bool IsHostCommand { get; }

        public bool Handle(string message)
        {
            CommandInput command = CommandInput.Parse(message);

            if (command == null)
            {
                return false;
            }

            // Check Format and AltFormat
            if ((command.Command != this.Format.Split(' ')[0].Trim('/')) && (command.Command != this.AltFormat.Split(' ')[0].Trim('/')))
            {
                return false;
            }

            // Execute command
            this.Execute(command);
            return true;
        }

        public abstract void Execute(CommandInput message);
    }
}
