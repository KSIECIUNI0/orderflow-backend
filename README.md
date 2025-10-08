# orderflow-backend

```
OrderFlow/
├── src/
│    ├── OrderFlow.Api/           # main WebAPI (.NET 9)
│    ├── OrderFlow.Domain/        # entities, aggregates, ValueObjects
│    ├── OrderFlow.Application/   # CQRS, MediatR, business logic
│    ├── OrderFlow.Infrastructure/# EF Core, repositories, AWS SDK
│    └── OrderFlow.Worker/        # background service (SQS)
├── tests/
│    ├── OrderFlow.UnitTests/
│    └── OrderFlow.IntegrationTests/
├── infra/
│    └── terraform/               # IaC
├── docker/
│    └── docker-compose.yml       # LocalStack + SQL + API + Worker
└── .github/workflows/            # CI/CD pipeline
```

## Pre Requirements

* .NET 9 SDK
* Docker
* awscli
  * aws configure:
  ```
  AWS Access Key ID [None]: test
  AWS Secret Access Key [None]: test
  Default region name [None]: us-east-1
  Default output format [None]: json 
  ```

## Local development
* Run docker compose:
```
docker-compose up -d
```

## Tips
### AWS CLI
#### List buckets
```
aws --endpoint-url=http://localhost:4566 s3 ls
```