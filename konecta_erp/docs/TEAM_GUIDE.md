# Team Collaboration Guide

### DevOps \| Cloud \| Fullstack

**Version 1.0 -- Internal Document**

------------------------------------------------------------------------

## 1. Overview

This document explains how our team will collaborate across DevOps,
Cloud, and Fullstack domains using **GitHub** as the central platform.\
Our goals are: - Consistent development workflow\
- Clear task ownership\
- Automated builds and deployments through **GitHub Actions**\
- Easy tracking of progress and reviews

------------------------------------------------------------------------

## 2. Team Structure & Responsibilities

  ---------------------------------------------------------------------------
  Team            Responsibility                Folder in Repo
  --------------- ----------------------------- -----------------------------
  **DevOps**      CI/CD pipelines,              `/devops/`, (Optional)
                  containerization, monitoring, `.github/workflows/`
                  environment setup             

  **Cloud**       Infrastructure (Terraform,    `/infra/`
                  networking, IAM, S3, etc.)    

  **Fullstack**   Frontend (React/Angular) +    `/frontend/`, `/backend/`
                  Backend (Node.js, Java, etc.) 
  ---------------------------------------------------------------------------

Each team owns their area but collaborates closely during integration
and deployment.

------------------------------------------------------------------------

## 3. Repository Structure

    project-root/
    │
    ├── .github/
    │   └── workflows/           # GitHub Actions pipelines
    │
    ├── infra/                   # Cloud team: Terraform or other IaC tools
    │    
    ├── backend/                 # Backend app (dotnet)
    │
    ├── frontend/                # Frontend app (Angular)
    │
    └── README.md

------------------------------------------------------------------------

## 4. Branching Strategy

  Branch           Description
  ---------------- -------------------------------------------
  **main**         Stable, production-ready code
  **develop**      Staging/testing environment
  **feature/**\*   Used for developing new features or fixes

**Branch naming convention:**

    devops/setup-ci
    cloud/vpc-setup
    frontend/login-ui
    backend/auth-api

### Workflow

1.  Create a new branch from `develop`
2.  Commit and push your work
3.  Open a Pull Request (PR) → assign reviewers
4.  Wait for approval and successful CI checks
5.  Merge into `develop` or `main`

------------------------------------------------------------------------

## 5. GitHub Actions (CI/CD)

### Workflow 1: Lint + Test (on every Pull Request)

Ensures code quality and tests pass before merging.

``` yaml
# .github/workflows/lint-test.yml
name: Lint and Test

on:
  pull_request:
    branches: [ develop ]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '20'
      - run: cd backend && npm ci && npm test
      - run: cd frontend && npm ci && npm run lint
```

------------------------------------------------------------------------

### Workflow 2: Deploy Infrastructure (Cloud Team)

Runs when infrastructure code changes are pushed to `main`.

``` yaml
# .github/workflows/deploy-infra.yml
name: Deploy Infrastructure

on:
  push:
    branches: [ main ]
    paths:
      - 'infra/**'

jobs:
  terraform:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: hashicorp/setup-terraform@v3
      - run: cd infra && terraform init
      - run: cd infra && terraform apply -auto-approve
        env:
          AWS_ACCESS_KEY_ID: ${{ secrets.AWS_ACCESS_KEY_ID }}
          AWS_SECRET_ACCESS_KEY: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
```

------------------------------------------------------------------------

### Workflow 3: Build & Deploy App (DevOps + Fullstack)

Builds frontend and backend, then deploys automatically.

``` yaml
# .github/workflows/deploy-app.yml
name: Build and Deploy Application

on:
  push:
    branches: [ main ]
    paths:
      - 'frontend/**'
      - 'backend/**'

jobs:

# TODO WORKING ON IT
```

------------------------------------------------------------------------

## 6. Secrets & Security

All credentials are stored in **GitHub Secrets** (Settings → Secrets →
Actions).

 | Secret Name               | Used For                         |
 | ------------------------- | -----------------------------    |
 | `GCP_CREDENTIALS`         | Terraform / GCP Deployments      |    
 | `SERVER_HOST`             | SSH host for deployment          |
 | `SSH_KEY`                 | SSH private key                  |
 | `DOCKERHUB_TOKEN`         | Pushing container images         |

**Never commit credentials** in code or config files.

------------------------------------------------------------------------

## 7. Task Management & Communication

### Task Tracking: ClickUp

We use **ClickUp** for tracking: - Columns: `To Do`,
`In Progress`, `In Review`, `Done` - Each card = a GitHub Issue - Add
labels: `frontend`, `backend`, `cloud`, `devops` - Assign yourself when
you start work

### Example Workflow

1.  Open issue → "Setup CI/CD pipeline"
2.  Label `devops`
3.  Assign yourself
4.  Link PR when ready → automatically moves status in the board

------------------------------------------------------------------------

## 8. Communication

------------------------------------------------------------------------

## 9. Weekly Routine

  Day        Activity
  ---------- ---------------------------------------------------
  Sunday     Short sync call (15 min) -- align on weekly goals
  Tuesday   Review progress in GitHub Projects
  Thursday     PR review, cleanup branches, mark done tasks

------------------------------------------------------------------------

## 10. Onboarding Checklist

**Every new team member should:** 1. Clone the repo and check folder
ownership.\
2. Create their first feature branch.\
3. Understand the GitHub Actions pipeline.\
4. Link their GitHub account to Clickup (optional).\
5. Review this document before first commit.

------------------------------------------------------------------------

## 11. Summary

| Area                | Tool              | Owner       |
| --------------------| ----------------- | ----------- |
|  Codebase           |  GitHub           |  All        |
|  CI/CD              |  GitHub Actions   |  DevOps     |
|  Infrastructure     |  Terraform        |  Cloud      |
|  Frontend + Backend |  dotnet / Angular |   Fullstack |
|  Task Tracking      |  GitHub Projects  |  Everyone   |
|  Communication      |  Click / Whatsapp |   Everyone  |
