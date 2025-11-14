#!/bin/bash
set -e

echo "Creating SQS queue: orderflow-orders"

aws --endpoint-url=http://localhost:4566 sqs create-queue \
  --queue-name orderflow-orders \
  --attributes VisibilityTimeout=30

echo "Queue created!"