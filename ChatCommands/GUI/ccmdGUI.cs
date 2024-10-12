using UnityEngine;
using BepInEx.Configuration;
using ChatCommands.Commands;  
using static ChatCommands.Utils;


namespace ChatCommands
{
    public class ccmdGUI
    {
        internal static string lastDisplayedMessage = string.Empty;
        private string userInput = string.Empty;
        private bool showMenu = false;
        private bool showPopup = false;
        private CustomChatCommand selectedCommand;
        private ConfigEntry<KeyCode> toggleKey;
        private GUIStyle menuStyle;

         public ccmdGUI InitMenu(ChatCommands instance){
            toggleKey = instance.Config.Bind<KeyCode>("Command Settings", "Toggle Key", KeyCode.F4, "Key to toggle the menu");
            menuStyle = new GUIStyle();
            menuStyle.fontSize = 20;
            menuStyle.normal.textColor = Color.white;
            LogInfo("GUI Menu initialized");
            return this;
        }
        public void Update()
        {
            if (UnityEngine.Input.GetKeyDown(toggleKey.Value))
            {
                showMenu = !showMenu;
            }
        }

        private void DrawModMenu()
        {
        // Create a simple mod menu box at the center of the screen
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        Rect menuRect = new Rect(screenWidth / 2 - 100, 0, 200, screenHeight);

        GUI.Box(menuRect, "Mod Menu");

        float buttonHeight = 30f;
        float buttonSpacing = 8f;
        float currentYPosition = 60;
        
        foreach (var command in CommandController.Commands)
        {
            if (command.Format.Split(' ').Length > 1)
            {
                if (GUI.Button(new Rect(screenWidth / 2 - 80, currentYPosition, 160, buttonHeight), command.Name))
                {
                    showMenu = false;
                    showPopup = true;
                    selectedCommand = command;
                }
            }
            else
            {
                if (GUI.Button(new Rect(screenWidth / 2 - 80, currentYPosition, 160, buttonHeight), command.Name))
                {
                    command.Execute(null);
                }
            }
        
            // Update the Y position for the next button
            currentYPosition += buttonHeight + buttonSpacing;
        }
        if (GUI.Button(new Rect(screenWidth / 2 - 80, currentYPosition, 160, buttonHeight), "Close Menu"))
        {
            showMenu = false;
        }
    }
    public void OnGUI()
    {
        // Display the current toggle key when the game starts (top left corner)
        GUI.Label(new Rect(10, 10, 300, 30), $"Press {toggleKey.Value} to toggle Mod Menu", menuStyle);
        GUI.Label(new Rect(10, 40, 300, 60), $"Last DBGMsg: {lastDisplayedMessage}", menuStyle);

        // Show mod menu if toggled
        if (showMenu)
        {
            DrawModMenu();
        }

        if (showPopup){
            ShowPopupForUserInput();
        }
    }
    // Method to show a popup and get user input
    private void ShowPopupForUserInput()
    {    
        if (selectedCommand != null)
        {
            // Create a simple popup box at the center of the screen
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            Rect popupRect = new Rect(screenWidth / 2 - 100, screenHeight / 2 - 100, 200, 200);
    
            GUI.Box(popupRect, "Enter Arguments for " + selectedCommand.Name);
    
            // Create a text field for user input
            GUI.SetNextControlName("UserInputField");
            userInput = GUI.TextField(new Rect(screenWidth / 2 - 80, screenHeight / 2 - 60, 160, 30), userInput);
            GUI.FocusControl("UserInputField");
    
            // Add a button to confirm the input
            if (GUI.Button(new Rect(screenWidth / 2 - 80, screenHeight / 2 - 20, 160, 30), "Confirm"))
            {
                string fullCommand = selectedCommand.Format.Split(' ')[0]+" "+userInput; //add command to front of arguments, not ideal
                userInput = string.Empty;
                showPopup = false;
                showMenu = true;
                selectedCommand.Execute(CommandInput.Parse(fullCommand));
                selectedCommand = null;
            }
            if (GUI.Button(new Rect(screenWidth / 2 - 80, screenHeight / 2 + 20, 160, 30), "Cancel"))
            {
                showPopup = false;
                showMenu = true;
            }
        }
    }

    }
    
}