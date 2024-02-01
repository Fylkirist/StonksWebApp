# Stonks web app <NAME PENDING>
#### A web app for fundamental analysis of stocks
This project serves as a platform for fundamental analysis of stocks, with easy company filings and financials lookup.
My future plan is to use data aggregation to power analysis tools such as sentiment analysis, FDA pipeline success prediction and fundamental fincancial screening.

### Technologies
- .Net7
- Postgresql
- bootstrap
- HTMX

### Environment requirements
appsettings.json needs to contain the following keys with corresponding values:
```json
"Credentials":{
   "Sql":{
      "Username":<Your SQL user>,
      "Password":<Your SQL user password>,
      "Instance":<PGSQL instance host>,
      "Database":<Database name for this project>
  },
  "Admin":{
    "Email": <Email of the initial admin user to be generated on startup>,
    "Password": <Password of inital admin>
  },
  "Finnhub": <Finnhub API key>,
  "SEC": <Your user agent string for the SEC>
}
```
### Acknowledgements
As of now, this application uses the following APIs for information gathering:

- data.sec.gov,
- api.fda.gov,
- finnhub.io/api/v1

This list is not final

