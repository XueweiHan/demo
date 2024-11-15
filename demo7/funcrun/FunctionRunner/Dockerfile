FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS installer-env

COPY . /src
RUN cd /src && \
    dotnet publish *.csproj -r linux-musl-x64 --self-contained true -c Release -o /app



# FROM mcr.microsoft.com/dotnet/aspnet:8.0
FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine

ENV AzureWebJobsScriptRoot=/home/site/wwwroot
# ENV FuctionRunnerHttpPort=80
COPY --from=installer-env ["/app", "/app"]
CMD [ "/app/FunctionRunner" ]
