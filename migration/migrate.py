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

import boto3
import csv
import sys

S3_CLIENT = boto3.client("s3")

BUCKET = 'S3 bucket name goes here!'

if len(sys.argv) != 2:
    print( "Please provide the input file name as a command line argument.")
    sys.exit(1)

## This is the wrong encoding.  Need to find out what SQL Server exports.
with open(sys.argv[1], encoding='utf-8') as datafile:
    csv.field_size_limit(sys.maxsize)
    reader = csv.reader(datafile)

    for row in reader:

        key = row[0]
        cacheDate = row[1]
        trialIDs = row[2]
        searchParams = row[3]
        content = row[4]

        metadata = {}
        metadata['migrated-data'] = 'True'
        metadata['originally-generated'] = cacheDate
        metadata['search-criteria'] = searchParams
        metadata['trial-id-list'] = trialIDs

        print(key)
        S3_CLIENT.put_object(
            Key = key,
            Bucket = BUCKET,
            Metadata = metadata,
            Body = bytearray(content, 'utf-8'),
            ContentType = 'text/html'
        )