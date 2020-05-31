FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app
COPY *.sln ./
COPY IT.PostManager.Core.Contracts/*.csproj IT.PostManager.Core.Contracts/
COPY IT.PostManager.Core.Logic/*.csproj IT.PostManager.Core.Logic/
COPY IT.PostManager.Infra.TelegraphConnect/*.csproj IT.PostManager.Infra.TelegraphConnect/
COPY IT.PostManager.WebUI/*.csproj IT.PostManager.WebUI/
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out --no-restore

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 as base
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=build-env /app/out .
RUN mkdir -p logs
EXPOSE 80 443
ENTRYPOINT ["dotnet", "IT.PostManager.WebUI.dll"]
