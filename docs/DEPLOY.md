# Homeserver Deployment

Target: Ubuntu homeserver, Docker Compose, images exposing container port 80 on the frontend service for an external reverse proxy, plus the existing `proxy_network` Docker network.

## Secrets

Never commit real values. The root `.env.example` lists every required variable:

- `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD`
- `JWT_SECRET_KEY` (at least 64 random characters, e.g. `openssl rand -base64 48`)
- `JWT_ISSUER`, `JWT_AUDIENCE`
- `SEED_ADMIN_PASSWORD`, `SEED_USERS_PASSWORD`

Copy `.env.example` to `.env` on the homeserver and fill in production values there.

## Homeserver First-Time Setup

1. Install Docker Engine and the Compose plugin.
2. Create the external network once: `docker network create proxy_network`.
3. Clone this repository to the homeserver and place a filled-in `.env` next to `docker-compose.yml`.
4. Start the stack: `docker compose up -d --build`.
5. Point an external reverse proxy at the running `web` container's port 80.

Startup applies database migrations, then the application seeds roles, demo items, lots, jobs, and users. `postgres-data` and `api-images` volumes persist the database and uploaded item images.

## Resource Limits

The whole stack is capped at 2 GB RAM and 1 CPU total:

| Service | CPU limit | Memory limit |
|---------|-----------|--------------|
| `api`   | 0.5       | 1 GB         |
| `web`   | 0.25      | 512 MB       |
| `db`    | 0.25      | 512 MB       |
| Total   | 1.0       | 2 GB         |

Adjust the `deploy.resources.limits` block in `docker-compose.yml` if the homeserver has more capacity.

## Jenkins Agent Setup

The `Jenkinsfile` assumes a Jenkins agent with network access to the homeserver over SSH; it does not assume the controller shares the homeserver Docker host. The agent needs `dotnet` 8, `node`, `npm`, `docker`, and `git` installed. The test stage starts a throwaway `postgres:16-alpine` container on port `5433` and passes its connection string via `CONNECTION_STRING`, so no external database is required.

Configure on the Jenkins controller:

- An SSH credential with ID `homeserver-deploy-ssh` (username + private key).
- `DEPLOY_HOST` (homeserver address) and `DEPLOY_DIR` (path to this repository checkout on the homeserver) as environment variables or parameters.
- Run the pipeline with the `DEPLOY` parameter checked to push a validated build to the homeserver.

Without `DEPLOY`, the pipeline stops after unit tests and the two Docker image builds.
