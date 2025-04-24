package service

import (
	"log"
	"os"

	"certnoti/handler"

	"github.com/gin-gonic/gin"
)

func Run() {
	router := gin.Default()
	router.LoadHTMLGlob("template/*")

	router.GET("/api/hello", handler.HelloHandlerFunc)
	router.GET("/api/cert", handler.CertHandlerFunc)

	listenAddr := ":8000"
	if val, ok := os.LookupEnv("PORT"); ok {
		listenAddr = ":" + val
	}
	log.Printf("About to listen on %s. Go to https://127.0.0.1%s/", listenAddr, listenAddr)
	log.Fatal(router.Run(listenAddr))
}
