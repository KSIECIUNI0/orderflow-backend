#!/bin/bash
set -e

echo "Creating SQS queue: OrderCreatedQueue"

aws --endpoint-url=http://localhost:4566 sqs create-queue \
  --queue-name OrderCreatedQueue \
  --attributes VisibilityTimeout=30

echo "Queue created!"