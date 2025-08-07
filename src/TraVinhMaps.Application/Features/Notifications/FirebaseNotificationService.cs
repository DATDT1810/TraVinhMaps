// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraVinhMaps.Application.Features.Notifications.Interface;
using TraVinhMaps.Application.Features.Notifications.Models;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using System.Text.RegularExpressions;

namespace TraVinhMaps.Application.Features.Notifications;
public class FirebaseNotificationService : IFirebaseNotificationService
{
    public async Task<string> PushNotificationAsync(NotificationRequest notificationRequest, string topic = "all")
    {
        var message = new Message()
        {
            Topic = topic,
            Notification = new FirebaseAdmin.Messaging.Notification()
            {
                Title = notificationRequest.IconCode.StartsWith("fa-")
            ? $"<i class='fas {notificationRequest.IconCode}'></i> {notificationRequest.Title}"
            : $"{GetEmojiFromCode(notificationRequest.IconCode)} {notificationRequest.Title ?? string.Empty}",
                Body = RemoveHtmlTags(notificationRequest.Content),
            },
            Android = new AndroidConfig()
            {
                Notification = new AndroidNotification()
                {
                    ChannelId = "HIGH_PRIORITY_NOTIFICATION"
                }
            },
            Data = new Dictionary<string, string>()
        {
            { "route", "/notification" },
            { "event_time", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString() } // tránh lỗi parse timestamp
        }
        };
        return await FirebaseMessaging.DefaultInstance.SendAsync(message);
    }

    private string GetEmojiFromCode(string code)
    {
        return code switch
        {
            "bun" => "\uD83C\uDF5C", // Emoji bat bun
            "moon" => "\uD83C\uDF1D", // Emoji the moon 
            "heart" => "\u2764\uFE0F", // Emoji the heart
            "sparkles" => "\u2728", // Emoji the star
            _ => "\uD83C\uDF5C" // Default  bat bun
        };
    }

    private string RemoveHtmlTags(string text)
    {
        List<int> openTagIndexes = Regex.Matches(text, "<").Cast<Match>().Select(m => m.Index).ToList();
        List<int> closeTagIndexes = Regex.Matches(text, ">").Cast<Match>().Select(m => m.Index).ToList();
        if (closeTagIndexes.Count > 0)
        {
            StringBuilder sb = new StringBuilder();
            int previousIndex = 0;
            foreach (int closeTagIndex in closeTagIndexes)
            {
                var openTagsSubset = openTagIndexes.Where(x => x >= previousIndex && x < closeTagIndex);
                if (openTagsSubset.Count() > 0 && closeTagIndex - openTagsSubset.Max() > 1)
                {
                    sb.Append(text.Substring(previousIndex, openTagsSubset.Max() - previousIndex));
                }
                else
                {
                    sb.Append(text.Substring(previousIndex, closeTagIndex - previousIndex + 1));
                }
                previousIndex = closeTagIndex + 1;
            }
            if (closeTagIndexes.Max() < text.Length)
            {
                sb.Append(text.Substring(closeTagIndexes.Max() + 1));
            }
            return sb.ToString();
        }
        else
        {
            return text;
        }
    }
}
