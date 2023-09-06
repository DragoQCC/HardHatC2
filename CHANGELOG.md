# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [v0.3.0] - 2023-09-05

### Added

- Created `CHANGELOG.md` to track detailed changes
- Option to set the default admin username and password using environment variables (`HARDHAT_ADMIN_USERNAME` and `HARDHAT_ADMIN_PASSWORD`, respectively)
  - Both settings are independent (i.e., one can be set without setting the other)
  - If omitted, the username defaults to `HardHat_Admin` and the password is randomly generated and printed to STDOUT
- Docker support
  - Created `docker/client/Dockerfile` and `docker/server/Dockerfile`
  - Created `docker-compose.yaml` to manage the containers
  - Both containers `tee` their output to log files in a bind mounted volume
    - This is useful for troubleshooting and to ensure you don't lose the auto-generated admin password
    - **Note**: If you do not set the admin password using an environment variable, the plaintext admin password will be written to the teamserver log file!

### Changed

- Replaced `127.0.0.1` with `0.0.0.0` as the listening host in `./TeamServer/Properties/launchSettings.json` and `./HardHatC2Client/appsettings.*` to allow the ports to be exposed by the Docker container
- Reformatted how the default admin username and password are printed to make the credentials more clear
- Modified `README.md`
  - Made the tone slightly more formal
  - Reorganized sections to provide:
    1. A quick overview to catch reader's interest
    2. A brief invite to engage
    3. A more comprehensive list of features
    4. Quickstart instructions
    5. Existing release tracking info
        - Recommend moving information to this file
