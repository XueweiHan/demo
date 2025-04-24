package email

import (
	"certnoti/content"
	"certnoti/model"

	"github.com/go-gomail/gomail"
)

func SendEmail(to string, obj []model.InventoryCredSMARTCertInfo) error {
	body, err := content.GenerateEmailBody(obj)
	if err != nil {
		return err
	}

	if len(to) == 0 {
		to = "Hunter Han<hunter.han@microsoft.com>"
	}
	subject := "CertNoti Notification"
	return sendSMTPEmail(to, subject, body)
}

func sendSMTPEmail(to, subject, htmlBody string) error {
	m := gomail.NewMessage()
	m.SetHeader("From", "CertNoti v3<funziogree@outlook.com>")
	m.SetHeader("To", to)
	m.SetHeader("Subject", subject)
	m.SetBody("text/html", htmlBody+to)

	// Send the email to Bob
	d := gomail.NewDialer("smtp-mail.outlook.com", 587, "funziogree@outlook.com", "")
	return d.DialAndSend(m)
}
