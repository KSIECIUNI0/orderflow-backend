#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

echo "Creating S3 buckets"

aws --endpoint-url=http://localhost:4566 \
    s3 mb s3://orderflow-failures

aws --endpoint-url=http://localhost:4566 \
    s3api put-bucket-lifecycle-configuration \
    --bucket orderflow-failures \
    --lifecycle-configuration file://$SCRIPT_DIR/s3Lifecycle.json

echo "Buckets created!"