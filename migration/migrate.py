#!/usr/bin/env python3
"""
Script to store clinical trial print pages in an S3 bucket.

The datafile (supplied as the only command-line parameter) is made up of
multiple records, one record per line.  Fields in the individual records
which may contain commas are surrounded by double-quotes, with any double-
quotes in the field's value replace by a pair of double-quotes.
(This is really just the output of the MS SQL Server Managent Studio's
"Save as CSV" on a query.)

Fields in the record are:

key - Unique string (a GUID) for identifying the record.
cacheDate - Timestamp identifying when the record was originally generated.
trialIDs - Comma-separated list of trial IDs.
searchParams - A string containing the JSON search criteria.
content - A blob of HTML representing the actual page.
"""

import csv
import os
import sys

import boto3
from botocore.exceptions import ClientError

S3_CLIENT = boto3.client("s3")

if len(sys.argv) != 2:
    raise RuntimeError("Please provide the input file name as a command line argument.")


if (
    "CTS_BUCKET_NAME" not in os.environ
    or os.environ["CTS_BUCKET_NAME"] is None
    or os.environ["CTS_BUCKET_NAME"].strip() == ""
):
    raise RuntimeError("The 'CTS_BUCKET_NAME' environment variable has not been set.")

BUCKET = os.environ["CTS_BUCKET_NAME"]

loadedCount = 0
errorCount = 0
totalCount = 0
with open(sys.argv[1], encoding="utf-8-sig") as datafile:
    csv.field_size_limit(sys.maxsize)
    reader = csv.reader(datafile)

    for row in reader:

        key = row[0]
        cacheDate = row[1]
        trialIDs = row[2]
        searchParams = row[3]
        content = row[4]

        metadata = {}
        metadata["migrated-data"] = "True"
        metadata["originally-generated"] = cacheDate
        metadata["search-criteria"] = searchParams
        metadata["trial-id-list"] = trialIDs

        print(key)
        try:
            S3_CLIENT.put_object(
                Key=key,
                Bucket=BUCKET,
                Metadata=metadata,
                Body=bytearray(content, "utf-8"),
                ContentType="text/html",
            )
            loadedCount += 1

        # Handle AWS-related errors.
        except ClientError as err:
            errorCount += 1
            print(err)

            # Bail completely for expired token.
            if err.response["Error"]["Code"] == "ExpiredToken":
                raise RuntimeError("\n\n\n\tFatal error - Expired token.\n\n") from err

        # Non-AWS errors
        except Exception as err:
            errorCount += 1
            print(err)

        finally:
            totalCount += 1

print(
    f"Processed {totalCount} docments: Loaded {loadedCount} with {errorCount} errors."
)
