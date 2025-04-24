package handler

import (
	"fmt"
	"log"
	"net/http"
	"runtime"

	"certnoti/azure/kusto"
	"certnoti/config"
	"certnoti/email"

	"github.com/gin-gonic/gin"
)

func HelloHandlerFunc(ctx *gin.Context) {
	message := "This HTTP triggered function executed successfully. Pass a name in the query string for a personalized response.\n"
	name := ctx.Request.URL.Query().Get("name")
	if name != "" {
		message = fmt.Sprintf("Hello, %s. This HTTP triggered function executed successfully.\n", name)
	}

	message += fmt.Sprintf("\nBuild Version: %s\n", config.Tag)
	message += fmt.Sprintf("%s on %s\n", runtime.GOOS, runtime.GOARCH)

	ctx.String(http.StatusOK, message)
}

func CertHandlerFunc(ctx *gin.Context) {

	obj := kusto.QueryHelios()

	if ctx.Request.URL.Query().Get("format") == "html" {

		ctx.HTML(http.StatusOK, "email.html", obj)

	} else {

		err := email.SendEmail(ctx.Request.URL.Query().Get("email"), obj)
		if err != nil {
			log.Println(err)
		}

		ctx.JSON(http.StatusOK, obj)
	}
}
