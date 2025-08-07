// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using TraVinhMaps.Application.Common.Exceptions;
using TraVinhMaps.Application.External;
using TraVinhMaps.Application.External.Models;

namespace TraVinhMaps.Infrastructure.External;

public class EmailSender : IEmailSender
{
    private readonly EmailConfiguration _emailConfig;

    public EmailSender(IOptions<EmailConfiguration> emailConfig)
    {
        _emailConfig = emailConfig.Value;

    }

    public async Task SendEmailAsync(string sendFor, string subject, string body, CancellationToken cancellationToken = default)
    {
        var email = _emailConfig.Email;
        var password = _emailConfig.Password;
        var host = _emailConfig.Host;
        var port = _emailConfig.Port;

        var smtpClient = new SmtpClient(host, port);
        smtpClient.EnableSsl = true;
        smtpClient.UseDefaultCredentials = false;

        smtpClient.Credentials = new NetworkCredential(email, password);

        var bodyEmail = MailBodyForOTP(body);

        var message = new MailMessage(
            email!, sendFor, subject, bodyEmail
        )
        {
            IsBodyHtml = true,
        };

        try
        {
            await smtpClient.SendMailAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new BadRequestException(ex.Message);
        }
    }

    public async Task SendEmailPasswordAsync(string sendFor, string subject, string body, CancellationToken cancellationToken = default)
    {
        var email = _emailConfig.Email;
        var password = _emailConfig.Password;
        var host = _emailConfig.Host;
        var port = _emailConfig.Port;

        var smtpClient = new SmtpClient(host, port)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(email, password)
        };

        var parts = body.Split('|');
        var username = parts.ElementAtOrDefault(0) ?? "Admin";
        var emailAccount = parts.ElementAtOrDefault(1) ?? sendFor;
        var tempPassword = parts.ElementAtOrDefault(2) ?? "admin123@";

        var htmlBody = MailBodyForPassword(username, emailAccount, tempPassword);

        var message = new MailMessage(email!, sendFor, subject, htmlBody)
        {
            IsBodyHtml = true
        };

        try
        {
            await smtpClient.SendMailAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new BadRequestException(ex.Message);
        }
    }

    public async Task SendEmailUnbanAsync(string sendFor, string subject, string body, CancellationToken cancellationToken = default)
    {
        var email = _emailConfig.Email;
        var password = _emailConfig.Password;
        var host = _emailConfig.Host;
        var port = _emailConfig.Port;

        var smtpClient = new SmtpClient(host, port)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(email, password)
        };

        var username = body ?? "User";

        var htmlBody = MailBodyForUnban(username);

        var message = new MailMessage(email!, sendFor, subject, htmlBody)
        {
            IsBodyHtml = true
        };

        try
        {
            await smtpClient.SendMailAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new BadRequestException(ex.Message);
        }
    }


    public async Task SendEmailBanedAsync(string sendFor, string subject, string body, CancellationToken cancellationToken = default)
    {
        var email = _emailConfig.Email;
        var password = _emailConfig.Password;
        var host = _emailConfig.Host;
        var port = _emailConfig.Port;

        var smtpClient = new SmtpClient(host, port)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(email, password)
        };

        var parts = body.Split('|');
        var username = parts.ElementAtOrDefault(0) ?? "User";
        var reason = parts.ElementAtOrDefault(1) ?? "Violation of community guidelines";

        var htmlBody = MailBodyForBaned(username, reason);

        var message = new MailMessage(email!, sendFor, subject, htmlBody)
        {
            IsBodyHtml = true
        };

        try
        {
            await smtpClient.SendMailAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new BadRequestException(ex.Message);
        }
    }


    private string MailBodyForOTP(string otp)
    {
        return $@"
    <html>
        <head>
            <style>
                .container {{
                    max-width: 600px;
                    margin: auto;
                    padding: 20px;
                    border: 1px solid #e0e0e0;
                    border-radius: 10px;
                    font-family: Arial, sans-serif;
                    background-color: #f9f9f9;
                    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
                }}
                .header {{
                    background-color: #4CAF50;
                    color: white;
                    padding: 10px 20px;
                    border-top-left-radius: 10px;
                    border-top-right-radius: 10px;
                    text-align: center;
                }}
                .content {{
                    padding: 20px;
                    text-align: center;
                }}
                .otp {{
                    font-size: 28px;
                    font-weight: bold;
                    color: #4CAF50;
                    margin: 20px 0;
                }}
                .footer {{
                    text-align: center;
                    font-size: 12px;
                    color: #777;
                    margin-top: 16px;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h2>OTP Verification</h2>
                </div>
                <div class='content'>
                    <p>Thank you for using our service!</p>
                    <p>Please use the following One-Time Password (OTP) to continue:</p>
                    <div class='otp'>{otp}</div>
                    <p>This OTP is valid for 5 minutes. Please do not share it with anyone.</p>
                </div>
                <div class='footer'>
                    &copy; {DateTime.Now.Year} TraVinhGo. All rights reserved.
                </div>
            </div>
        </body>
    </html>";
    }
    private string MailBodyForPassword(string username, string email, string tempPassword)
    {
        return $@"
    <html>
        <head>
            <style>
                .container {{
                    max-width: 600px;
                    margin: auto;
                    padding: 20px;
                    border: 1px solid #e0e0e0;
                    border-radius: 10px;
                    font-family: Arial, sans-serif;
                    background-color: #f9f9f9;
                    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
                }}
                .header {{
                    background-color: #4CAF50;
                    color: white;
                    padding: 10px 20px;
                    border-top-left-radius: 10px;
                    border-top-right-radius: 10px;
                    text-align: center;
                }}
                .content {{
                    padding: 20px;
                    line-height: 1.6;
                    color: #4CAF50;
                }}
                .password-box {{
                    font-size: 20px;
                    font-weight: bold;
                    color: #D32F2F;
                    background-color: #fbe9e7;
                    padding: 10px;
                    border-radius: 8px;
                    margin: 15px 0;
                    word-break: break-all;
                    text-align: center;
                }}
                .footer {{
                    text-align: center;
                    font-size: 12px;
                    color: #777;
                    margin-top: 16px;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h2>Welcome to TraVinhMaps</h2>
                </div>
                <div class='content'>
                    <p>Hello <strong>{username}</strong>,</p>
                    <p>Your administrator account has been successfully created.</p>
                    <p><strong>Email:</strong> {email}</p>
                    <p><strong>Temporary Password:</strong></p>
                    <div class='password-box'>{tempPassword}</div>
                    <p><strong>Note: ⚠️ For your security, please log in and change your password immediately.⚠️</strong></p>
                    <p>Thank you,<br/>TraVinhGo Team</p>
                </div>
                <div class='footer'>
                    &copy; {DateTime.Now.Year} TraVinhGo. All rights reserved.
                </div>
            </div>
        </body>
    </html>";
    }

    private string MailBodyForBaned(string username, string reason)
    {
        return $@"
        <html>
            <head>
                <style>
                    .container {{
                        max-width: 600px;
                        margin: auto;
                        padding: 20px;
                        border: 1px solid #e0e0e0;
                        border-radius: 10px;
                        font-family: Arial, sans-serif;
                        background-color: #fff3e0;
                        box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
                    }}
                    .header {{
                        background-color: #d32f2f;
                        color: white;
                        padding: 10px 20px;
                        border-top-left-radius: 10px;
                        border-top-right-radius: 10px;
                        text-align: center;
                    }}
                    .content {{
                        padding: 20px;
                        color: #d32f2f;
                        line-height: 1.6;
                    }}
                    .footer {{
                        text-align: center;
                        font-size: 12px;
                        color: #777;
                        margin-top: 16px;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h2>Account Banned Notification</h2>
                    </div>
                    <div class='content'>
                        <p>Dear <strong>{username}</strong>,</p>
                        <p>Your account has been temporarily banned due to the following reason:</p>
                        <p><em>{reason}</em></p>
                        <p>Please contact the admin team if you believe this is a mistake or need further clarification.</p>
                    </div>
                    <div class='footer'>
                        &copy; {DateTime.Now.Year} TraVinhGo. All rights reserved.
                    </div>
                </div>
            </body>
        </html>";
    }

    private string MailBodyForUnban(string username)
    {
        return $@"
    <html>
        <head>
            <style>
                .container {{
                    max-width: 600px;
                    margin: auto;
                    padding: 20px;
                    border: 1px solid #e0e0e0;
                    border-radius: 10px;
                    font-family: Arial, sans-serif;
                    background-color: #e8f5e9;
                    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
                }}
                .header {{
                    background-color: #388e3c;
                    color: white;
                    padding: 10px 20px;
                    border-top-left-radius: 10px;
                    border-top-right-radius: 10px;
                    text-align: center;
                }}
                .content {{
                    padding: 20px;
                    color: #388e3c;
                    line-height: 1.6;
                }}
                .footer {{
                    text-align: center;
                    font-size: 12px;
                    color: #777;
                    margin-top: 16px;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h2>Account Reactivated</h2>
                </div>
                <div class='content'>
                    <p>Hello <strong>{username}</strong>,</p>
                    <p>We are pleased to inform you that your account has been successfully reactivated.</p>
                    <p>You may now log in and continue using our services. Thank you for your patience!</p>
                </div>
                <div class='footer'>
                    &copy; {DateTime.Now.Year} TraVinhGo. All rights reserved.
                </div>
            </div>
        </body>
    </html>";
    }

    public async Task SendEmailBanedAdminAsync(string sendFor, string subject, string body, CancellationToken cancellationToken = default)
    {
        var email = _emailConfig.Email;
        var password = _emailConfig.Password;
        var host = _emailConfig.Host;
        var port = _emailConfig.Port;

        var smtpClient = new SmtpClient(host, port)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(email, password)
        };

        var parts = body.Split('|');
        var adminName = parts.ElementAtOrDefault(0) ?? "Administrator";
        var reason = parts.ElementAtOrDefault(1) ?? "Violation of admin policy";

        var htmlBody = MailBodyForBanedAdmin(adminName, reason);

        var message = new MailMessage(email!, sendFor, subject, htmlBody)
        {
            IsBodyHtml = true
        };

        try
        {
            await smtpClient.SendMailAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new BadRequestException(ex.Message);
        }
    }

    public async Task SendEmailUnbanAdminAsync(string sendFor, string subject, string body, CancellationToken cancellationToken = default)
    {
        var email = _emailConfig.Email;
        var password = _emailConfig.Password;
        var host = _emailConfig.Host;
        var port = _emailConfig.Port;

        var smtpClient = new SmtpClient(host, port)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(email, password)
        };

        var adminName = body ?? "Administrator";

        var htmlBody = MailBodyForUnbanAdmin(adminName);

        var message = new MailMessage(email!, sendFor, subject, htmlBody)
        {
            IsBodyHtml = true
        };

        try
        {
            await smtpClient.SendMailAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new BadRequestException(ex.Message);
        }
    }


    private string MailBodyForBanedAdmin(string adminName, string reason)
    {
        return $@"
    <html>
        <head>
            <style>
                .container {{
                    max-width: 600px;
                    margin: auto;
                    padding: 20px;
                    border: 1px solid #e0e0e0;
                    border-radius: 10px;
                    font-family: Arial, sans-serif;
                    background-color: #fff3e0;
                    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
                }}
                .header {{
                    background-color: #c62828;
                    color: white;
                    padding: 10px 20px;
                    border-top-left-radius: 10px;
                    border-top-right-radius: 10px;
                    text-align: center;
                }}
                .content {{
                    padding: 20px;
                    color: #c62828;
                    line-height: 1.6;
                }}
                .footer {{
                    text-align: center;
                    font-size: 12px;
                    color: #777;
                    margin-top: 16px;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h2>Administrator Account Banned</h2>
                </div>
                <div class='content'>
                    <p>Dear <strong>{adminName}</strong>,</p>
                    <p>Your administrator account has been banned due to the following reason:</p>
                    <p><em>{reason}</em></p>
                    <p>If you have any concerns, please contact the system administrator.</p>
                </div>
                <div class='footer'>
                    &copy; {DateTime.Now.Year} TraVinhGo Admin Panel. All rights reserved.
                </div>
            </div>
        </body>
    </html>";
    }

    private string MailBodyForUnbanAdmin(string adminName)
    {
        return $@"
    <html>
        <head>
            <style>
                .container {{
                    max-width: 600px;
                    margin: auto;
                    padding: 20px;
                    border: 1px solid #e0e0e0;
                    border-radius: 10px;
                    font-family: Arial, sans-serif;
                    background-color: #e8f5e9;
                    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
                }}
                .header {{
                    background-color: #2e7d32;
                    color: white;
                    padding: 10px 20px;
                    border-top-left-radius: 10px;
                    border-top-right-radius: 10px;
                    text-align: center;
                }}
                .content {{
                    padding: 20px;
                    color: #2e7d32;
                    line-height: 1.6;
                }}
                .footer {{
                    text-align: center;
                    font-size: 12px;
                    color: #777;
                    margin-top: 16px;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h2>Administrator Account Reactivated</h2>
                </div>
                <div class='content'>
                    <p>Hello <strong>{adminName}</strong>,</p>
                    <p>Your administrator access has been successfully restored.</p>
                    <p>You may now log in to the admin panel again. Thank you for your cooperation.</p>
                </div>
                <div class='footer'>
                    &copy; {DateTime.Now.Year} TraVinhGo Admin Panel. All rights reserved.
                </div>
            </div>
        </body>
    </html>";
    }

}
