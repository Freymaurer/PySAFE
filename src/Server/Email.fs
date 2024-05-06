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


let sendConfirmation(targetEmail: string) =
    let msg =
        new MailMessage(
            Environment.EmailConfig.Email,
            targetEmail,
            "CSB - Confirmation: Registration to Notification Service",
            sprintf """Dear User,

We are thrilled to confirm that your registration to our notification service has been successfully completed! 🎉

Now, whenever your data analysis is completed, you'll receive a email notification.

Here's a quick recap of what you can expect from our notification service:

Timely updates: You'll be promptly notified once your data analysis is finished.
Convenience: No need to constantly check for updates; we'll keep you in the loop.
No spam: Rest assured knowing that you'll receive exactly two emails. This confirmation email and the final notification email.
Privacy: Your data results will be stored for %s days, afterwards all data and personal information will be fully deleted. Your data stays with us, we do not use or support third party services!

Thank you for choosing our tool. We're excited to embark on this journey with you!

Warm regards,

CSB-Team"""
                (Environment.python_service_storage_timespan.TotalDays |> string)
        )
    msg.BodyEncoding <- Text.Encoding.UTF8
    SMPTClient.Send msg

let sendNotification(targetEmail: string) =
    let msg =
        new MailMessage(
            Environment.EmailConfig.Email,
            targetEmail,
            "CSB - Subject: Your Data Analysis is Ready!",
            sprintf """Dear User,

We are thrilled to inform you that your data analysis is now complete and ready for your review!

You can access your analysis results by visiting our website and clicking on the "Data Access" tab. Your data will be stored for %s days after sending this email.

We hope that these insights will be valuable to you and your project. Should you have any questions or require further assistance, please don't hesitate to reach out to us.

Thank you for trusting us with your data analysis needs. We look forward to continuing to support you in your endeavors.

Warm regards,

CSB-Team"""
                (Environment.python_service_storage_timespan.TotalDays |> string)
        )
    msg.BodyEncoding <- Text.Encoding.UTF8
    SMPTClient.Send msg