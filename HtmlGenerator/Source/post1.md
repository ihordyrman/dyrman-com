---
date: 2023-05-17 21:22:20
page-title: This is what title should look like
url: article-1
---
### Problem:
To make a consistent mechanism for storing and receiving currency rates across all services.
Currencies should be described in one place, ideally by enums and accessed via enum.

### Why do we need enum?
When service access currency service it shoud "know" about available currencies, so there would be no "misunderstanding" between services in the system. All possible currencies are described in one place and API uses this description to provide services with actual currencies
1. Currency service needs enums to have a proper communication between processing service
2. Processing service needs enums to send request about currencies it needs to the currency service
3. Processing service needs enums to accept request from client api and understand which currency are we going to work with
4. Client API needs enums to map currency from the client request to a existed currency code and pass it to the processing service

### How to store in redis?
Integer: Not easy to read if we want to check what happens in redis dirrectly
String: easy to read
Difference in implementation? Not really
