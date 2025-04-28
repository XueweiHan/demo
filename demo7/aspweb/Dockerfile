FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG configuration=Release
WORKDIR /src

COPY aspweb.csproj .
RUN dotnet restore aspweb.csproj

COPY . .
RUN dotnet build aspweb.csproj -c $configuration -o /app/build

RUN dotnet publish aspweb.csproj -c $configuration -o /app/publish /p:UseAppHost=false



FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
EXPOSE 80
ENV HTTP_PORTS=80

COPY --from=build /app/publish .

COPY --from=build /src/start.sh .

CMD ["bash", "start.sh"]


# temporary for testing, all cert should be loaded from AKV by init Container
COPY localhost.crt /https/
COPY localhost.key /https/
COPY aspweb.hunterapp.net.crt /https/
COPY aspweb.hunterapp.net.key /https/
