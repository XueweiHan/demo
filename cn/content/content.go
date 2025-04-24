package content

import (
	"certnoti/model"

	"bytes"
	"html/template"
)

func GenerateEmailBody(certInfo []model.InventoryCredSMARTCertInfo) (string, error) {
	return generateEmailBody(`./template/email.html`, certInfo)
}

func generateEmailBody(tmplFile string, certInfo []model.InventoryCredSMARTCertInfo) (string, error) {
	html, err := template.New("email.html").ParseFiles(tmplFile)
	if err != nil {
		return "", err
	}

	var buffer bytes.Buffer

	err = html.Execute(&buffer, certInfo)
	if err != nil {
		return "", err
	}

	str := buffer.String()
	return str, nil
}


