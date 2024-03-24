FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

WORKDIR /app

COPY . .

RUN dotnet restore

RUN dotnet build -c Release --no-restore

CMD ["dotnet", "test", "--logger:trx"]