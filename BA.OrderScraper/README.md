# Project Name
BAM Syspro order automation

## Description
A brief description of your project.

## Arguments Input
This project accepts the following command-line arguments:

1. **jobType**: The type of job to run (import orders or create orders for example).
2. **importType**: The type of import to run (if jobType is import).

### Example Usage
BA.OrderScraper.exe "toyotaordersimport" "manifest"
BA.OrderScraper.exe "toyotaordersimport" "skid"
BA.OrderScraper.exe "sysproordercreate"