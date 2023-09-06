FROM mcr.microsoft.com/dotnet/sdk:7.0-bookworm-slim

WORKDIR /app

EXPOSE 7096

STOPSIGNAL SIGKILL

COPY . .
#RUN git clone https://github.com/DragoQCC/HardHatC2.git .

RUN dotnet build /app/HardHatC2Client/HardHatC2Client.csproj

CMD dotnet run --project /app/HardHatC2Client/HardHatC2Client.csproj https://hardhat-server:5000 | tee --append /app/HardHatC2Client/logs/client.log
