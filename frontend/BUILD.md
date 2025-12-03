# Frontend Docker Build Guide

## Environment-Specific Builds

The frontend Dockerfile now supports building for different environments using the `ENV_CONFIG` build argument.

### Available Configurations

- **production** (default) - For production deployments
- **staging** - For staging/pre-production environments
- **development** - For development environments

### Building Locally

#### Production Build (default)

```bash
docker build -t konecta-frontend:latest ./frontend
```

#### Staging Build

```bash
docker build --build-arg ENV_CONFIG=staging -t konecta-frontend:staging ./frontend
```

#### Development Build

```bash
docker build --build-arg ENV_CONFIG=development -t konecta-frontend:dev ./frontend
```

### CI/CD Integration

The GitHub Actions workflow automatically determines the environment configuration based on the branch:

| Branch    | Environment Config | Docker Tag       |
| --------- | ------------------ | ---------------- |
| `main`    | `production`       | `latest`         |
| `staging` | `staging`          | `staging-latest` |
| `develop` | `development`      | `dev-latest`     |

### Angular Configuration Files

Make sure you have the corresponding Angular configuration files in your `angular.json`:

- `production` - Uses `src/environments/environment.prod.ts`
- `staging` - Uses `src/environments/environment.staging.ts`
- `development` - Uses `src/environments/environment.ts`

### Example: Custom Build

If you need to build for a specific environment manually:

```bash
# Build for staging environment
docker build \
  --build-arg ENV_CONFIG=staging \
  -t us-central1-docker.pkg.dev/PROJECT_ID/konecta-erp/frontend:staging \
  ./frontend

# Push to registry
docker push us-central1-docker.pkg.dev/PROJECT_ID/konecta-erp/frontend:staging
```

### Troubleshooting

**Issue**: Build fails with "Unknown configuration"

- **Solution**: Ensure the configuration name matches one defined in `angular.json`

**Issue**: Wrong environment variables in built app

- **Solution**: Verify the correct environment file is being used for the specified configuration
