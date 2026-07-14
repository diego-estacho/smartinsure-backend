FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/SmartInsure.Core/SmartInsure.Core.csproj src/SmartInsure.Core/
COPY src/SmartInsure.Application.UseCase/SmartInsure.Application.UseCase.csproj src/SmartInsure.Application.UseCase/
COPY src/SmartInsure.Infra.Data/SmartInsure.Infra.Data.csproj src/SmartInsure.Infra.Data/
COPY src/SmartInsure.Infra.CrossCutting/SmartInsure.Infra.CrossCutting.csproj src/SmartInsure.Infra.CrossCutting/
COPY src/SmartInsure.Infra.BackgroundServices/SmartInsure.Infra.BackgroundServices.csproj src/SmartInsure.Infra.BackgroundServices/
COPY src/SmartInsure.Integration/SmartInsure.Integration.csproj src/SmartInsure.Integration/
COPY src/SmartInsure.MailServices/SmartInsure.MailServices.csproj src/SmartInsure.MailServices/
COPY src/SmartInsure.Functions/SmartInsure.Functions.csproj src/SmartInsure.Functions/
RUN dotnet restore src/SmartInsure.Functions/SmartInsure.Functions.csproj

COPY src/ src/
RUN dotnet publish src/SmartInsure.Functions/SmartInsure.Functions.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated10.0
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY --from=build /app/publish /home/site/wwwroot
