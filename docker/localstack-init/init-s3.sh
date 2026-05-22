#!/bin/bash
set -e

echo "Creating S3 buckets"

aws --endpoint-url=http://localhost:4566 s3 mb s3://orderflow-failures

echo "Buckets created!"