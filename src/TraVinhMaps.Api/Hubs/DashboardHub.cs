// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.SignalR;

namespace TraVinhMaps.Api.Hubs;

public class DashboardHub : Hub
{
    // Feedback notification
    public async Task SendFeedbackNotification(string feedbackId)
    {
        await Clients.Group("admin").SendAsync("ReceiveFeedback", feedbackId);
        await Clients.Group("super-admin").SendAsync("ReceiveFeedback", feedbackId);
    }

    // Charts stats realtime
    public async Task SendChartAnalytics()
    {
        await Clients.Group("admin").SendAsync("ChartAnalytics");
        await Clients.Group("super-admin").SendAsync("ChartAnalytics");
    }

    // Group join logic
    public async Task JoinAdminGroup(string role)
    {
        if (role == "super-admin")
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "super-admin");
            await Groups.AddToGroupAsync(Context.ConnectionId, "admin");
        }
        else if (role == "admin")
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "admin");
        }
    }
}
