# build stage
FROM golang:1.22.2-alpine3.19 AS build-env

# Redundant, current golang images already include ca-certificates
RUN apk --no-cache add ca-certificates

ARG ARCH=amd64
WORKDIR /src
COPY . .
RUN CGO_ENABLED=0 GOOS=linux GOARCH=${ARCH} go build -o ./bin/go-app .


# final stage
# FROM alpine:3.19
FROM scratch 
WORKDIR /app
COPY --from=build-env /etc/ssl/certs/ca-certificates.crt /etc/ssl/certs/
COPY --from=build-env /src/bin/ ./
CMD ["./go-app"]



# FROM ubuntu:20.04
# ENV TZ=US/Pacific
# RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone
# RUN apt update && apt install -y snapd



# support multi-arch
# FROM alpine:3.19
# WORKDIR /app
# COPY --from=build-env /src/bin/ ./
# CMD ["/bin/sh", "-c", "./$(uname -m)/go-app"]
