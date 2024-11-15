FROM mcr.microsoft.com/dotnet/sdk:8.0 AS installer-env

COPY . /src
RUN cd /src && \
    mkdir -p /app && \
    dotnet publish *.csproj -c Release -o /app



FROM mcr.microsoft.com/dotnet/aspnet:8.0

ENV AzureWebJobsScriptRoot=/home/site/wwwroot
# ENV FuctionRunnerHttpPort=80
COPY --from=installer-env ["/app", "/app"]
CMD [ "/app/FunctionRunner" ]
