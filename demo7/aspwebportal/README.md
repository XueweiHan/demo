```
dotnet new webapp --output <project_name> --no-https
cd <project_name>
dotnet run

#-> Use VSCode command : "Docker: Add Dockerfile to workspace..." then select asp.net web ....
#-> Manully clean up the Dockerfile
or
#-> Use VisualStudio right click the project -> Add -> Container Support...

docker build -t test .
docker run --rm -it -p8080:5179 test
```