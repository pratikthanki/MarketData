## Market Data Gateway

### Overview

An API based application that will allow users to store, retrieve 
and distribute these market data (e.g.: financial quotes such as 
FxQuote, Swap’s level, etc.) for different scenario.

Broadly the architecture looks like this:

```

Business users -> Market Data contribution gateway -> Market Data Validation

```

1. Business users: Individual who is contributing the market quote.
2. Market data contribution gateway -> Responsible for validating request by 
requesting the market data validation service, storing market data, and 
returning contribution responses. 
3. Market Data validation service: Allows to check the market data contribution 
rights, format (example: Negative FxQuote), legal auditing (Financial regulation 
framework validation such as MIFID, etc.) and reply with an appropriate response 
code.

