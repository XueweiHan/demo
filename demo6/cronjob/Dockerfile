# build stage
FROM golang:1.22.2-alpine3.19 AS build-env

WORKDIR /src
COPY . .
RUN go build -o ./bin/go-app .


# final stage
# FROM alpine:3.19
FROM scratch 
WORKDIR /app
COPY --from=build-env /etc/ssl/certs/ca-certificates.crt /etc/ssl/certs/
COPY --from=build-env /src/bin/ ./
CMD ["./go-app"]
