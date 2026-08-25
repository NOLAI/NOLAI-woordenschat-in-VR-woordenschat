using System;
using System.Collections.Generic;

public class DashboardMessageProcessor
{
    private readonly IStreamController streamController;
    private readonly IButtonDashboard buttonDashboard;

    public List<string> ReceivedActions { get; private set; } = new List<string>();

    public DashboardMessageProcessor(IStreamController streamController, IButtonDashboard buttonDashboard)
    {
        this.streamController = streamController;
        this.buttonDashboard = buttonDashboard;
    }

    public void HandleMessage(string message)
    {
        if (string.IsNullOrEmpty(message) || message == "{\"message\":\"\"}")
            return;

        if (message == "Start stream")
        {
            streamController.EnableStream(true);
        }
        else if (message == "Stop stream")
        {
            streamController.EnableStream(false);
        }
        else if (message.Contains("data"))
        {
            ReceivedActions.Add(message);
            buttonDashboard?.ProcessJsonResponse(message);
        }
    }
}
