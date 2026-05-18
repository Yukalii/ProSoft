FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine

WORKDIR /app

# Copy ONLY the Linux binary built by GitHub Actions
COPY docker-build/EasyLogServer ./EasyLogServer

# Make it executable
RUN chmod +x ./EasyLogServer

EXPOSE 8080
ENV LOG_DIR=/var/logs/easysave
ENV PORT=8080

ENTRYPOINT ["./EasyLogServer"]