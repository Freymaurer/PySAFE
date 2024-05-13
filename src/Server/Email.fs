module Email

open System
open System.Net.Mail
open System.Net

let SMPTClient =
    let config = Environment.EmailConfig
    let c = new SmtpClient(config.Server, config.Port)
    c.EnableSsl <- true
    c.Credentials <- new NetworkCredential(config.AccountName,config.Password);
    c

let emailSend(mail:MailMessage) =
    async {
        try
            if not Environment.missingEmailCredentials then
                do! SMPTClient.SendMailAsync mail |> Async.AwaitTask
        with
            | e -> printfn "[Email] %s" e.Message
    }

let createMessage(from: string, toEmail: string, subject: string, body: string) =
    try
        let msg = new MailMessage(from, toEmail, subject, body)
        msg.BodyEncoding <- Text.Encoding.UTF8
        msg.IsBodyHtml <- true
        Some msg
    with
        | e ->
            printfn "[Email] %s" e.Message
            None

open Giraffe.ViewEngine

let confirmationEmailBody =
    html [] [
        body [] [
            section [] [
                h3 [] [ str "Registration to Notification Service"]
                p [] [ str "Dear User," ]

                p [] [ str "We are thrilled to confirm that your registration to our notification service has been successfully completed! 🎉"]
                p [] [ str "Now, whenever your data analysis is completed, you'll receive a email notification."]
                p [] [ str "Here's a quick recap of what you can expect from our notification service:"]
                ul [] [
                    li [] [
                        b [] [ str "Timely updates:"]
                        str " You'll be promptly notified once your data analysis is finished."
                    ]
                    li [] [
                        b  [] [ str "Convenience:"]
                        str " No need to constantly check for updates; we'll keep you in the loop."
                    ]
                    li [] [
                        b [] [ str "No spam:"]
                        str " Rest assured knowing that you'll receive exactly two emails. This confirmation email and the final notification email."
                    ]
                    li [] [
                        b [] [ str "Privacy:"]
                        rawText $" Your data results will be stored for <u>{Environment.python_service_storage_timespan.TotalDays}</u> days, afterwards all data and personal information will be fully deleted. Your data stays with us, we do not use or support third party services!"
                            
                    ]
                ]
                p [] [ str "Thank you for choosing our tool. We're excited to embark on this journey with you!"]
                p [] [ str "Warm regards,"]
                p [] [ str "CSB-Team" ]
            ]
        ]
    ]
    |> RenderView.AsString.htmlDocument

let notificationEmailBody =
    html [] [
        body [] [
            section [] [
                h3 [] [ str "Your Data Analysis is Ready!"]
                p [] [ str "Dear User," ]
                p [] [ str "Your data analysis is now complete and ready for your review!"]
                p [] [
                    str "You can access your analysis results by visiting our website and clicking on the 'Data Access' tab. Your data will be stored for "
                    u [] [ str (Environment.python_service_storage_timespan.TotalDays |> string) ]
                    str " days after sending this email."
                ]
                p [] [ str "We hope that these insights will be valuable to you and your project. Should you have any questions or require further assistance, please don't hesitate to reach out to us."]
                p [] [ str "Warm regards,"]
                p [] [ str "CSB-Team"]
            ]
        ]
    ]
    |> RenderView.AsString.htmlDocument

let sendConfirmation(targetEmail: string) =
    match createMessage(
            Environment.EmailConfig.Email,
            targetEmail,
            "CSB - Confirmation: Registration to Notification Service",
            confirmationEmailBody
        ) with
    | Some msg ->
        emailSend msg
    | None -> async.Zero()

let sendNotification(targetEmail: string) =
    match createMessage(
            Environment.EmailConfig.Email,
            targetEmail,
            "CSB - Notification: Your Data Analysis is Ready!",
            notificationEmailBody
        ) with
    | Some msg ->
        emailSend msg
    | None -> async.Zero()