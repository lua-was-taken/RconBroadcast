# Rcon Broadcast Service

A simple **RCON-based broadcast service** designed for scheduled command execution on game servers.

## Setup & First Run

Upon the **first start**, the service will generate a **dummy `rcon-config.json`** in the same directory as the DLL. You must either:

- Run the service **once manually** to create this config:
  ```sh
  dotnet RconBroadcast.dll
  ```
- Or provide an existing config in the same directory beforehand.

## Example `rcon-config.json`

```json
{
  "Servers": [
    {
      "ServerIp": "127.0.0.1",
      "ServerPort": 25575,
      "Password": "mypassword",
      "Commands": [
        {
          "Command": "announce \"Test Broadcast\"",
          "Interval": "20m"
        }
      ]
    }
  ],
  "ReconnectInterval": "5m"
}
```

## Requirements

- **Dotnet SDK 8.0** must be installed

## Installing Dotnet SDK

```sh
sudo apt update
sudo apt install dotnet-sdk-8.0
```

## Running as a Linux Daemon

To run this **service persistently in the background**, create a **systemd service file**:
```sh
sudo nano /etc/systemd/system/rcon-broadcast.service
```

## Example service file

```ini
[Unit]
Description=Rcon Broadcast Service
After=network.target

[Service]
User=root
WorkingDirectory=/opt/rconbroadcast/
ExecStart=sudo -u root dotnet /opt/rconbroadcast/RconBroadcast.dll
Restart=always
# Restart service after 10 seconds if the dotnet service crashes:
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=rconbroadcast
Environment=DOTNET_NOLOGO=true

[Install]
WantedBy=multi-user.target
```
*(Replace /opt/rconbroadcast/ with your service directory of choice.)*

## Activate & Start the Service

```sh
sudo systemctl daemon-reload
sudo systemctl enable rcon-broadcast.service
sudo systemctl start rcon-broadcast.service
sudo systemctl status rcon-broadcast.service
```

## Important Notes

- The **first time the service runs**, it will exit unless `rcon-config.json` already exists.
  Ensure you **either run it manually once** (`dotnet RconBroadcast.dll`) or create the
  config **beforehand** in the app directory.
- Logs can be checked using:
  ```sh
  journalctl -u rcon-broadcast.service --no-pager
  ```
