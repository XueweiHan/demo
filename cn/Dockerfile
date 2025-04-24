# build stage
FROM mcr.microsoft.com/oss/go/microsoft/golang:1.22.2-cbl-mariner2.0 AS build-env

WORKDIR /src
COPY . .
RUN go build -o ./release/certnoti .
COPY ./template/* ./release/template/



# final stage
FROM mcr.microsoft.com/cbl-mariner/distroless/base:2.0

WORKDIR /app

COPY --from=build-env /src/release ./

ARG TAG=0.0.0
ENV TAG=$TAG
ENV PORT=8000
EXPOSE 8000

CMD [ "./certnoti" ]
