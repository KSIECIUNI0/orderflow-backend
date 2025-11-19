#!/bin/bash
set -e

echo "Creating SQS queue: order-created-queue"

aws --endpoint-url=http://localhost:4566 sqs create-queue \
  --queue-name order-created-queue \
  --attributes VisibilityTimeout=30

echo "Queue created!"