FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine AS base
WORKDIR /app
EXPOSE 1230

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/nightly/sdk:8.0-preview-alpine AS build
ARG TARGETARCH
WORKDIR /src
COPY ["nng-api.csproj", "."]
RUN dotnet restore -a $TARGETARCH "./nng-api.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "nng-api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "nng-api.csproj" -c Release -a $TARGETARCH -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "nng-api.dll"]
