# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /app

# Copy .csproj files and restore dependencies
COPY ./StudyBuddy/StudyBuddy.csproj ./StudyBuddy/
RUN dotnet restore ./StudyBuddy/StudyBuddy.csproj

# Copy libman.json and restore client-side libraries
COPY ./StudyBuddy/libman.json ./StudyBuddy/
RUN dotnet tool install -g Microsoft.Web.LibraryManager.Cli
ENV PATH="${PATH}:/root/.dotnet/tools"
WORKDIR /app/StudyBuddy/
RUN libman restore
WORKDIR /app

# Copy the whole application and publish
COPY ./StudyBuddy/ ./StudyBuddy/
RUN dotnet publish ./StudyBuddy/StudyBuddy.csproj -c Release -o out

# Stage 2: Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app

# Copy the published output from Stage 1
COPY --from=build-env /app/out .

# Expose port 80 for the application
EXPOSE 80

# Define the entry point for the container
ENTRYPOINT ["dotnet", "StudyBuddy.dll"]
