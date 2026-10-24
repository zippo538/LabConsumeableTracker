pipeline {
    agent any

    options {
        timestamps()
        disableConcurrentBuilds()
        buildDiscarder(logRotator(numToKeepStr: '10'))
    }

    environment {
        DOTNET_CLI_TELEMETRY_OPTOUT = '1'
        DOTNET_NOLOGO = '1'
        NPM_CONFIG_AUDIT = 'false'
        NPM_CONFIG_FUND = 'false'
    }

    parameters {
        booleanParam(name: 'DEPLOY', defaultValue: false, description: 'Deploy to the homeserver over SSH after tests and image builds pass')
    }

    // Configure in Jenkins: SSH credential ID 'homeserver-deploy-ssh' (private key),
    // plus DEPLOY_HOST and DEPLOY_DIR environment variables.
    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Test API') {
            steps {
                sh '''
                    docker run -d --rm --name ci-postgres \
                      -e POSTGRES_DB=lab_tracker_test \
                      -e POSTGRES_USER=postgres \
                      -e POSTGRES_PASSWORD=qwerty123 \
                      -p 5433:5432 postgres:16-alpine
                    trap 'docker stop ci-postgres >/dev/null 2>&1 || true' EXIT
                    for i in $(seq 1 30); do
                      if docker exec ci-postgres pg_isready -U postgres -d lab_tracker_test; then
                        break
                      fi
                      sleep 2
                    done
                    docker exec ci-postgres pg_isready -U postgres -d lab_tracker_test
                    CONNECTION_STRING="Host=localhost;Port=5433;Database=lab_tracker_test;Username=postgres;Password=qwerty123" \
                      dotnet test LabConsumableExpiryTracker.Tests/LabConsumableExpiryTracker.Tests.csproj --nologo
                '''
            }
        }

        stage('Build Web') {
            steps {
                dir('LabConsumableExpiryTracker.Web') {
                    sh 'npm ci && npm run build'
                }
            }
        }

        stage('Build API Image') {
            steps {
                sh 'docker build -t lab-consumable-expiry-tracker-api:local -f LabConsumableExpiryTracker.API/Dockerfile .'
            }
        }

        stage('Build Web Image') {
            steps {
                sh 'docker build -t lab-consumable-expiry-tracker-web:local LabConsumableExpiryTracker.Web'
            }
        }

        stage('Deploy') {
            when { expression { params.DEPLOY } }
            steps {
                withCredentials([sshUserPrivateKey(credentialsId: 'homeserver-deploy-ssh', keyFileVariable: 'DEPLOY_SSH_KEY', usernameVariable: 'DEPLOY_USER')]) {
                    sh '''
                        test -n "$DEPLOY_HOST" && test -n "$DEPLOY_DIR"
                        install -m 700 -d ~/.ssh
                        ssh-keyscan -H "$DEPLOY_HOST" >> ~/.ssh/known_hosts
                        ssh -i "$DEPLOY_SSH_KEY" "$DEPLOY_USER@$DEPLOY_HOST" "cd '$DEPLOY_DIR' && git pull --ff-only && docker compose build && docker compose up -d --remove-orphans && docker image prune -f"
                    '''
                }
            }
        }
    }

    post {
        always {
            cleanWs()
        }
    }
}
