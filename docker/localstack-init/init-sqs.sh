#!/bin/bash
set -e

echo "Creating SQS queues"

aws --endpoint-url=http://localhost:4566 sqs create-queue \
  --queue-name order-created-queue-dlq
echo "Created SQS queue: order-created-queue-dlq"
  
DLQ_ARN=$(aws --endpoint-url=http://localhost:4566 sqs get-queue-attributes \
  --queue-url http://localhost:4566/000000000000/order-created-queue-dlq \
  --attribute-names QueueArn \
  --query "Attributes.QueueArn" \
  --output text)
echo "Get queue ARN: $DLQ_ARN"

aws --endpoint-url=http://localhost:4566 sqs create-queue \
  --queue-name order-created-queue \
  --attributes "{\"RedrivePolicy\":\"{\\\"deadLetterTargetArn\\\":\\\"$DLQ_ARN\\\", \\\"maxReceiveCount\\\":\\\"3\\\"}\"}"
echo "Created SQS queue: order-created-queue"

echo "Queues created!"